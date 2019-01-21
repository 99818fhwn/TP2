//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="FH">
//     Company copyright tag.
// </copyright>
//-----------------------------------------------------------------------
namespace LogicDesigner
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using LogicDesigner.Commands;
    using LogicDesigner.ViewModel;

    /// <summary>
    /// WPF Logic
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window
    {
        /// <summary>
        /// If a component is being dragged.
        /// </summary>
        private bool isMoving;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.UndoHistory = new Stack<ProgramMngVM>();
            this.RedoHistory = new Stack<ProgramMngVM>();

            this.DataContext = this;
            ProgramMngVM programMngVM = new ProgramMngVM();
            this.MainGrid.DataContext = programMngVM;

            this.UndoHistory.Push(new ProgramMngVM(programMngVM));

            programMngVM.FieldComponentAdded += this.OnComponentAdded;
            programMngVM.FieldComponentRemoved += this.OnComponentDeleted;
            programMngVM.PinsConnected += this.OnPinsConnected;

            this.ComponentWindow.PreviewMouseDown += new MouseButtonEventHandler(this.ComponentMouseDown);
            this.ComponentWindow.PreviewMouseUp += new MouseButtonEventHandler(this.ComponentMouseUp);
            this.ComponentWindow.PreviewMouseMove += new MouseEventHandler(this.ComponentMouseMovePre);
        }

        /// <summary>
        /// Gets the undo history.
        /// </summary>
        /// <value>
        /// The undo history.
        /// </value>
        public Stack<ProgramMngVM> UndoHistory { get; private set; }

        /// <summary>
        /// Gets the redo history.
        /// </summary>
        /// <value>
        /// The redo history.
        /// </value>
        public Stack<ProgramMngVM> RedoHistory { get; private set; }

        /// <summary>
        /// Gets the current mouse.
        /// </summary>
        /// <value>
        /// The current mouse.
        /// </value>
        public Point CurrentMouse { get; private set; }

        /// <summary>
        /// Gets the current move.
        /// </summary>
        /// <value>
        /// The current move.
        /// </value>
        public Point CurrentMove { get; private set; }

        /// <summary>
        /// Gets the undo command.
        /// </summary>
        /// <value>
        /// The undo command.
        /// </value>
        public Command UndoCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                if (this.UndoHistory.Count > 0)
                {
                    ProgramMngVM history = this.UndoHistory.Pop();

                    if (history == (ProgramMngVM)this.MainGrid.DataContext)
                    {
                        this.RedoHistory.Push(history);
                        history = this.UndoHistory.Pop();
                    }

                    this.ComponentWindow.Children.Clear();
                    this.MainGrid.DataContext = history;
                    foreach (var component in history.NodesVMInField)
                    {
                        DrawNewComponent(component);
                    }

                    this.RedoHistory.Push(history);
                }
            }));
        }

        /// <summary>
        /// Gets the redo command.
        /// </summary>
        /// <value>
        /// The redo command.
        /// </value>
        public Command RedoCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                if (this.RedoHistory.Count > 0)
                {
                    ProgramMngVM history = this.RedoHistory.Pop();

                    if (history == (ProgramMngVM)this.MainGrid.DataContext)
                    {
                        this.UndoHistory.Push(history);
                        history = this.RedoHistory.Pop();
                    }

                    this.ComponentWindow.Children.Clear();
                    this.MainGrid.DataContext = history;
                    foreach (var component in history.NodesVMInField)
                    {
                        DrawNewComponent(component);
                    }

                    this.UndoHistory.Push(history);
                }
            }));
        }

        /// <summary>
        /// Called when the button is pressed down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ComponentMouseDown(object sender, MouseButtonEventArgs e)
        {
            var pressedComponent = (UIElement)e.Source;
            var parentType = VisualTreeHelper.GetParent(pressedComponent).GetType();

            if (parentType == typeof(Grid))
            {
                var parent = (Grid)VisualTreeHelper.GetParent(pressedComponent);

                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    var dataContext = (ProgramMngVM)this.MainGrid.DataContext;
                    var componentToActivate = dataContext.NodesVMInField.First(x => x.Name == parent.Name);

                    componentToActivate.Activate();

                    return;
                }

                this.isMoving = true;
            }
        }

        /// <summary>
        /// Called when the button is pressed up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ComponentMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.isMoving)
            {
                var componentToMove = this.GetParentGrid((UIElement)e.Source);
                componentToMove.XCoord += this.CurrentMove.X;
                componentToMove.YCoord += this.CurrentMove.Y;
            }

            this.isMoving = false;
            this.CurrentMove = new Point(0, 0);
            this.CurrentMouse = new Point(0, 0);
        }

        private ComponentVM GetParentGrid(UIElement uIElement)
        {
            var parent = (UIElement)VisualTreeHelper.GetParent(uIElement);

            if (parent.GetType() == typeof(Grid))
            {
                var parentgrid = (Grid)parent;
                var dataContext = (ProgramMngVM)this.MainGrid.DataContext;
                var component = dataContext.NodesVMInField.First(x => x.Name == parentgrid.Name);
                return component;
            }

            return null;
        }

        /// <summary>
        /// Called when the button is moved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void ComponentMouseMovePre(object sender, MouseEventArgs e)
        {
            // Components are being reset, because their translation is not persistent and the temp variables are reset after letting the mouse go
            var pressedComponent = (UIElement)e.Source;

            var parent = (UIElement)VisualTreeHelper.GetParent(pressedComponent);
            Point newPoint;

            if (parent.GetType() == typeof(Grid))
            {
                var parentgrid = (Grid)parent;
                var dataContext = (ProgramMngVM)this.MainGrid.DataContext;
                var componentToMove = dataContext.NodesVMInField.FirstOrDefault(x => x.Name == parentgrid.Name);

                if (componentToMove == null)
                {
                    return;
                }

                newPoint = new Point(componentToMove.XCoord, componentToMove.YCoord);

                if (!this.isMoving)
                {
                    return;
                }
                else
                {
                    var previousMouse = this.CurrentMouse;
                    this.CurrentMouse = Mouse.GetPosition(this.ComponentWindow);

                    // Point relativePoint = pressedComponent.TransformToAncestor(ComponentWindow).Transform(new Point(0, 0));
                    if (previousMouse != new Point(0, 0) && this.CurrentMouse != previousMouse)
                    {
                        Point movepoint = new Point(this.CurrentMouse.X - previousMouse.X, this.CurrentMouse.Y - previousMouse.Y);
                        this.CurrentMove = new Point(this.CurrentMove.X + movepoint.X, this.CurrentMove.Y + movepoint.Y);

                        parent.RenderTransform = new TranslateTransform(this.CurrentMove.X + componentToMove.XCoord, this.CurrentMove.Y + componentToMove.YCoord);
                    }
                }
            }
        }

        /// <summary>
        /// Called when when a component is added.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FieldComponentEventArgs"/> instance containing the event data.</param>
        private void OnComponentAdded(object sender, FieldComponentEventArgs e)
        {
            var currentMan = new ProgramMngVM((ProgramMngVM)this.ComponentWindow.DataContext);
            this.UndoHistory.Push(currentMan);
            this.RedoHistory.Clear();
            e.Component.ComponentPropertyChanged += this.OnComponentChanged;
            this.DrawNewComponent(e.Component);
            var updatedCurrentMan = new ProgramMngVM((ProgramMngVM)this.ComponentWindow.DataContext);
            this.RedoHistory.Push(updatedCurrentMan);
        }

        /// <summary>
        /// Called when [component changed] and changes the visual of the component.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FieldComponentEventArgs"/> instance containing the event data.</param>
        private void OnComponentChanged(object sender, FieldComponentEventArgs e)
        {
            var compOld = this.ComponentWindow.Children; // FindName(e.Component.Name);

            foreach (var child in compOld)
            {
                if (child.GetType() == typeof(Grid))
                {
                    var grids = (Grid)child;

                    if (grids.Name == e.Component.Name)
                    {
                        foreach (var item in grids.Children)
                        {
                            if (item.GetType() == typeof(Button))
                            {
                                var compToChange = (Button)item;
                                if (compToChange.Name == (e.Component.Name+"Body"))
                                {
                                    ImageBrush imageBrush = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(e.Component.Picture.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                                    imageBrush.Stretch = Stretch.Fill; 
                                    compToChange.Background = imageBrush;
                                    compToChange.UpdateLayout();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when a component is deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FieldComponentEventArgs"/> instance containing the event data.</param>
        private void OnComponentDeleted(object sender, FieldComponentEventArgs e)
        {
            var currentMan = new ProgramMngVM((ProgramMngVM)this.ComponentWindow.DataContext);
            e.Component.ComponentPropertyChanged -= this.OnComponentChanged; // Unsubscribes from the deleted component
            this.UndoHistory.Push(currentMan);
            this.RedoHistory.Clear();
        }

        /// <summary>
        /// Draws the new component.
        /// </summary>
        /// <param name="componentVM">The component.</param>
        private void DrawNewComponent(ComponentVM componentVM)
        {
            // New component
            Grid sampleComponent = new Grid();

            int yOffset = -(componentVM.Picture.Height / 2) + 10;
            int offsetStepValue = 0;

            if (componentVM.InputPinsVM.Count > 1)
            {
                offsetStepValue = (componentVM.Picture.Height - 20) / (componentVM.InputPinsVM.Count - 1);
            }

            // Draw input pins
            for (int i = 0; i < componentVM.InputPinsVM.Count; i++)
            {
                Button pinButton = new Button();
                pinButton.Width = 30;
                pinButton.Height = 10;
                pinButton.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                pinButton.CommandParameter = componentVM.InputPinsVM[i];
                pinButton.Command = componentVM.InputPinsVM[i].SetPinCommand;

                pinButton.RenderTransform = new TranslateTransform(-componentVM.Picture.Width / 2, yOffset);

                componentVM.InputPinsVM[i].XPosition = -componentVM.Picture.Width / 2;
                componentVM.InputPinsVM[i].YPosition = yOffset;

                yOffset += offsetStepValue;

                sampleComponent.Children.Add(pinButton);
            }

            yOffset = -(componentVM.Picture.Height / 2) + 10;

            if (componentVM.OutputPinsVM.Count > 1)
            {
                offsetStepValue = (componentVM.Picture.Height - 20) / (componentVM.OutputPinsVM.Count - 1);
            }

            // Draw output pins
            for (int i = 0; i < componentVM.OutputPinsVM.Count; i++)
            {
                Button pinButton = new Button();
                pinButton.Width = 30;
                pinButton.Height = 10;
                pinButton.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                pinButton.CommandParameter = componentVM.OutputPinsVM[i];
                pinButton.Command = componentVM.OutputPinsVM[i].SetPinCommand;

                pinButton.RenderTransform = new TranslateTransform(componentVM.Picture.Width / 2, yOffset);
                yOffset += offsetStepValue;
                
                componentVM.OutputPinsVM[i].XPosition = componentVM.Picture.Width / 2;
                componentVM.OutputPinsVM[i].YPosition = yOffset;

                sampleComponent.Children.Add(pinButton);
            }

            // Component Body
            Button sampleBody = new Button();

            sampleComponent.Name = componentVM.Name;
            sampleBody.Name = componentVM.Name + "Body"; 
            sampleBody.Height = componentVM.Picture.Height; ////Can throw an exception i no picture is set the manager has to check for valid, is now solved(21-01-2019) by validator
            sampleBody.Width = componentVM.Picture.Width;

            ImageBrush imageBrush = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(componentVM.Picture.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            imageBrush.Stretch = Stretch.Fill;
            sampleBody.Background = imageBrush;

            // Add the label
            string text = componentVM.Label;

            Typeface myTypeface = new Typeface("Helvetica");

            FormattedText ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, myTypeface, 16, Brushes.Red);

            TextBlock label = new TextBlock
            {
                Width = ft.Width,
                Height = ft.Height,
                Text = componentVM.Label
            };

            label.TextAlignment = TextAlignment.Center;

            label.RenderTransform = new TranslateTransform(0, (-componentVM.Picture.Height / 2) - 10);

            sampleComponent.Height = sampleBody.Height + label.Height + 20;
            sampleComponent.Width = sampleBody.Width + label.Width + 20;
            sampleComponent.Children.Add(sampleBody);
            sampleComponent.Children.Add(label);
            
            this.ComponentWindow.Children.Add(sampleComponent);
        }

        /// <summary>
        /// Called when [pins connected].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PinsConnectedEventArgs"/> instance containing the event data.</param>
        public void OnPinsConnected(object sender, PinsConnectedEventArgs e)
        {
            var inputPin = e.InputPinVM;
            var outputPin = e.OutputPinVM;

            Line line = new Line();
            line.Visibility = Visibility.Visible;
            line.StrokeThickness = 4;
            line.Stroke = Brushes.Black;
            line.X1 = inputPin.XPosition;
            line.X2 = outputPin.YPosition;
            line.Y1 = inputPin.YPosition;
            line.Y2 = outputPin.YPosition;

            //line.RenderTransform = new TranslateTransform(line.X1, line.Y1);

            this.ComponentWindow.Children.Add(line);

            this.ComponentWindow.UpdateLayout();
        }

        /// <summary>
        /// Handles the Loaded event of the ScrollViewer control. Sets the view to the middle. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void ScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            var scrollbar = (ScrollViewer)e.Source;
            scrollbar.ScrollToVerticalOffset(scrollbar.ScrollableHeight / 2);
            scrollbar.ScrollToHorizontalOffset(scrollbar.ScrollableWidth / 2);
        }
    }
}
