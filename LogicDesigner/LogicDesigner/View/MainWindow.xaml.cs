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
    using System.IO;
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
    using System.Windows.Threading;
    using LogicDesigner.Commands;

    // Will be restructured
    using LogicDesigner.Model;
    //

    using LogicDesigner.ViewModel;
    using Microsoft.Win32;

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

        //private List<Tuple<Line, ConnectionVM>> connectionLines;
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.Scale = 1;
            this.InitializeComponent();
            //this.connectionLines = new List<Tuple<Line, ConnectionVM>>();
            this.UndoHistory = new Stack<ProgramMngVM>();
            this.RedoHistory = new Stack<ProgramMngVM>();

            this.DataContext = this;
            ProgramMngVM programMngVM = new ProgramMngVM();
            this.MainGrid.DataContext = programMngVM;
            var selectBind = new Binding("SelectedFieldComponent");
            selectBind.Source = (ProgramMngVM)this.MainGrid.DataContext;
            this.CurrentSelectedComponentView.SetBinding(DataContextProperty, selectBind);

            //this.UndoHistory.Push(new ProgramMngVM(programMngVM));

            this.InputBindings.Add(new InputBinding(programMngVM.CopyCommand, new KeyGesture(Key.C, ModifierKeys.Control)));
            this.InputBindings.Add(new InputBinding(programMngVM.PasteCommand, new KeyGesture(Key.V, ModifierKeys.Control)));

            programMngVM.FieldComponentAdded += this.OnComponentAdded;
            programMngVM.FieldComponentRemoved += this.OnComponentDeleted;

            programMngVM.PinsConnected += this.OnPinsConnected;
            programMngVM.PinsDisconnected += this.OnPinsDisconnected;

            programMngVM.PreFieldComponentAdded += this.PreComponentAdded;

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

                    var current = (ProgramMngVM)this.MainGrid.DataContext;
                    if (history.NodesVMInField == current.NodesVMInField)
                    {
                        this.RedoHistory.Push(history);
                        history = this.UndoHistory.Pop();
                    }

                    this.ComponentWindow.Children.Clear();
                    this.MainGrid.DataContext = history;
                    foreach (var component in history.NodesVMInField)
                    {
                        this.DrawNewComponent(component);
                    }

                    this.RedoHistory.Push(history);
                }
            }));
        }

        public Command SaveCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                try
                {
                    SaveFileDialog filepicker = new SaveFileDialog();
                    //filepicker.CheckFileExists = false;
                    filepicker.Filter = "LogicDesigner files (*.ldf)|*.ldf|All files (*.*)|*.*";
                    filepicker.DefaultExt = ".ldf";
                    filepicker.ShowDialog();

                    string filename = filepicker.FileName;

                    if (Directory.Exists(System.IO.Path.GetDirectoryName(filename)))
                    {
                        var manager = (ProgramMngVM)this.ComponentWindow.DataContext;
                        manager.SaveStatus(filename);
                    }
                }
                catch
                { }
            }));
        }

        public Command LoadCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                try
                {
                    OpenFileDialog filepicker = new OpenFileDialog();
                    //filepicker.CheckFileExists = false;
                    filepicker.Filter = "LogicDesigner files (*.ldf)|*.ldf|All files (*.*)|*.*";
                    filepicker.DefaultExt = ".ldf";
                    filepicker.ShowDialog();

                    string filename = filepicker.FileName;
                    if (File.Exists(filename))
                    {
                        var manager = (ProgramMngVM)this.ComponentWindow.DataContext;

                        var loadResult = manager.LoadStatus(filename);

                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            foreach (var existingComponent in manager.NodesVMInField)
                            {
                                manager.NodesVMInField.Remove(existingComponent);
                            // Insert visual remove
                        }
                        });


                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            foreach (var loadedComponent in loadResult.Item2)
                            {
                                manager.NodesVMInField.Add(loadedComponent);
                                this.DrawNewComponent(loadedComponent);
                            }
                        });
                    }
                }
                catch
                { }
            }));
        }

        /// <summary>
        /// Handles zooming with mouse wheel.
        /// </summary>
        private void MouseWheelZoom(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Delta > 0)
            {
                this.ZoomInCommand.Execute(null);
            }

            if (e.Delta < 0)
            {
                this.ZoomOutCommand.Execute(null);
            }
        }

        /// <summary>
        /// Gets the zoom in command.
        /// </summary>
        /// <value>
        /// The zoom in command.
        /// </value>
        public Command ZoomInCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                if (this.Scale < 2)
                {
                    this.Scale = this.Scale + 0.05;
                    var scaleTransform = new ScaleTransform(this.Scale, this.Scale);
                    this.ComponentWindow.RenderTransform = scaleTransform;
                }
                //this.ComponentWindow.RenderTransform = scaleTransform;
            }));
        }

        private double Scale { get; set; }

        /// <summary>
        /// Gets the zoom out command.
        /// </summary>
        /// <value>
        /// The zoom out command.
        /// </value>
        public Command ZoomOutCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                if (this.Scale > 0.2)
                {
                    this.Scale = this.Scale - 0.05;
                    var scaleTransform = new ScaleTransform(this.Scale, this.Scale);
                    this.ComponentWindow.RenderTransform = scaleTransform;
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

                    var current = (ProgramMngVM)this.MainGrid.DataContext;
                    if (history.NodesVMInField == current.NodesVMInField)
                    {
                        this.UndoHistory.Push(history);
                        history = this.RedoHistory.Pop();
                    }

                    this.ComponentWindow.Children.Clear();
                    this.MainGrid.DataContext = history;
                    foreach (var component in history.NodesVMInField)
                    {
                        this.DrawNewComponent(component);
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
                var temp = this.GetParentGridComponent(pressedComponent);

                if (temp == null)
                {
                    return;
                }

                ((ProgramMngVM)this.MainGrid.DataContext).SelectedFieldComponent = temp;

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
                var componentToMove = this.GetParentGridComponent((UIElement)e.Source);
                componentToMove.XCoord += this.CurrentMove.X;
                componentToMove.YCoord += this.CurrentMove.Y;
            }

            this.isMoving = false;
            this.CurrentMove = new Point(0, 0);
            this.CurrentMouse = new Point(0, 0);
        }

        /// <summary>
        /// Gets the parent grid component.
        /// </summary>
        /// <param name="uIElement">The ui element.</param>
        /// <returns></returns>
        private ComponentVM GetParentGridComponent(UIElement uIElement)
        {
            var parent = (UIElement)VisualTreeHelper.GetParent(uIElement);

            if (parent.GetType() == typeof(Grid))
            {
                var parentgrid = (Grid)parent;
                var dataContext = (ProgramMngVM)this.MainGrid.DataContext;
                var component = dataContext.NodesVMInField.FirstOrDefault(x => x.Name == parentgrid.Name);
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
            //Not sure if I broke it or not, maybe was a different event
            e.Component.ComponentPropertyChanged += this.OnComponentChanged;

            this.DrawNewComponent(e.Component);

            var updatedCurrentMan = new ProgramMngVM((ProgramMngVM)this.ComponentWindow.DataContext);
            this.RedoHistory.Clear();
            this.RedoHistory.Push(updatedCurrentMan);
        }

        /// <summary>
        /// Called when [component changed] and changes the visual of the component.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FieldComponentEventArgs"/> instance containing the event data.</param>
        private void OnComponentChanged(object sender, FieldComponentEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
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
                                    if (compToChange.Name == (e.Component.Name + "Body"))
                                    {
                                        ImageBrush imageBrush = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(e.Component.Picture.GetHbitmap(),
                                            IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                                        imageBrush.Stretch = Stretch.Fill;
                                        compToChange.Background = imageBrush;
                                        compToChange.UpdateLayout();
                                    }
                                }
                            }
                        }
                    }
                }
            });
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
            Grid newComponent = new Grid();

            // Component Body
            Button sampleBody = new Button();

            newComponent.Name = componentVM.Name;
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

            newComponent.Height = sampleBody.Height + label.Height + 20;
            newComponent.Width = sampleBody.Width + label.Width + 20;
            Panel.SetZIndex(sampleBody, 100);
            newComponent.Children.Add(sampleBody);
            newComponent.Children.Add(label);

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

                componentVM.InputPinsVM[i].XPosition = (newComponent.Width / 2) - (componentVM.Picture.Width / 2);
                componentVM.InputPinsVM[i].YPosition = (newComponent.Height / 2) + yOffset;

                yOffset += offsetStepValue;

                newComponent.Children.Add(pinButton);
            }

            yOffset = -(componentVM.Picture.Height / 2) + 10;

            offsetStepValue = 0;

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

                componentVM.OutputPinsVM[i].XPosition = (newComponent.Width / 2) + (componentVM.Picture.Width / 2);
                componentVM.OutputPinsVM[i].YPosition = (newComponent.Height / 2) + yOffset;

                newComponent.Children.Add(pinButton);
            }

            if (componentVM.XCoord != 0 && componentVM.YCoord == 0)
            {
                newComponent.RenderTransform = new TranslateTransform(componentVM.XCoord, 0);
            }

            if (componentVM.YCoord != 0 && componentVM.XCoord == 0)
            {
                newComponent.RenderTransform = new TranslateTransform(0, componentVM.YCoord);
            }

            if (componentVM.YCoord != 0 && componentVM.XCoord != 0)
            {
                newComponent.RenderTransform = new TranslateTransform(componentVM.XCoord, componentVM.YCoord);
            }

            this.ComponentWindow.Children.Add(newComponent);
        }

        // OnPinsDisconnected with ConnectionVM
        public void OnPinsDisconnected(object sender, PinVMConnectionChangedEventArgs e)
        {
            var inputPin = e.Connection.InputPin;
            var outputPin = e.Connection.OutputPin;

            //var connectionToRemove = this.connectionLines.Where(l => l.Item2.ConnectionId == e.Connection.ConnectionId).First();
            //Line lineToRemove = connectionToRemove.Item1;

            foreach (var child in this.ComponentWindow.Children)
            {
                //try
                //{
                //Grid grid = child as Grid;

                //foreach (var gridChild in grid.Children)
                //{
                try
                {
                    Line l = (Line)child;
                    //Line l = (Line)gridChild;
                    if (l.Name == e.Connection.ConnectionId)
                    {
                        //this.connectionLines.Remove(connectionToRemove);
                        //grid.Children.Remove((Line)gridChild);
                        this.ComponentWindow.Children.Remove((Line)child);
                        break;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                //    }
                //}
                //catch (Exception)
                //{
                //    continue;
                //}
            }
        }

        //new OnPinsConnected with ConnectionVM
        public void OnPinsConnected(object sender, PinVMConnectionChangedEventArgs e)
        {
            var inputPin = e.Connection.InputPin;
            var outputPin = e.Connection.OutputPin;

            Line line = new Line();
            line.Name = e.Connection.ConnectionId;
            line.Visibility = Visibility.Visible;
            line.StrokeThickness = 4;
            line.Stroke = Brushes.Black;
            line.X1 = inputPin.XPosition;
            line.X2 = outputPin.XPosition;
            line.Y1 = inputPin.YPosition;
            line.Y2 = outputPin.YPosition;

            Panel.SetZIndex(line, -100);

            this.ComponentWindow.Children.Add(line);
        }

        ///// <summary>
        ///// Called when pins disconnected.
        ///// </summary>
        ///// <param name="sender">The sender.</param>
        ///// <param name="e">The <see cref="PinsConnectedEventArgs"/> instance containing the event data.</param>
        //public void OnPinsDisconnected(object sender, PinsConnectedEventArgs e)
        //{

        //    Grid lineBody = new Grid();

        //    lineBody.Children.Add(line);

        //    this.ComponentWindow.Children.Add(lineBody);

        //    //this.connectionLines.Add(new Tuple<Line, ConnectionVM>(line, e.Connection));
        //}
        // Commented by Katja, connectionVM 

        /// <summary>
        /// Called when [pins connected].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PinsConnectedEventArgs"/> instance containing the event data.</param>
        //public void OnPinsConnected(object sender, PinsConnectedEventArgs e)
        //{
        //var inputPin = e.InputPinVM;
        //var outputPin = e.OutputPinVM;

        //Line line = new Line();
        //line.Visibility = Visibility.Visible;
        //line.StrokeThickness = 4;
        //line.Stroke = Brushes.Black;
        //line.X1 = inputPin.XPosition;
        //line.X2 = outputPin.XPosition;
        //line.Y1 = inputPin.YPosition;
        //line.Y2 = outputPin.YPosition;

        //Grid lineBody = new Grid();

        //lineBody.Children.Add(line);

        //this.ComponentWindow.Children.Add(lineBody);            
        //}

        ///// <summary>
        ///// Handles the Loaded event of the ScrollViewer control. Sets the view to the middle. 
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        ////private void ScrollViewerLoaded(object sender, RoutedEventArgs e)
        ////{
        ////    var scrollbar = (ScrollViewer)e.Source;
        ////    scrollbar.ScrollToVerticalOffset(scrollbar.ScrollableHeight / 2);
        ////    scrollbar.ScrollToHorizontalOffset(scrollbar.ScrollableWidth / 2);
        ////}

        private void PreComponentAdded(object sender, EventArgs e)
        {
            var currentMan = new ProgramMngVM((ProgramMngVM)this.ComponentWindow.DataContext);
            this.UndoHistory.Push(currentMan);
            this.RedoHistory.Clear();
        }
    }
}
