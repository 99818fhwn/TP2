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
    using System.Windows.Threading;
    using LogicDesigner.Commands;
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
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.Scale = 1;
            this.InitializeComponent();
            this.UndoHistory = new Stack<ProgramMngVM>();
            this.RedoHistory = new Stack<ProgramMngVM>();

            this.DataContext = this;
            ProgramMngVM programMngVM = new ProgramMngVM();
            this.MainGrid.DataContext = programMngVM;
            var selectBind = new Binding("SelectedFieldComponent");
            selectBind.Source = (ProgramMngVM)this.MainGrid.DataContext;
            this.CurrentSelectedComponentView.SetBinding(MainWindow.DataContextProperty, selectBind);
            
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
                        DrawNewComponent(component);
                    }

                    this.RedoHistory.Push(history);
                }
            }));
        }

        /// <summary>
        /// Gets the undo command.
        /// </summary>
        /// <value>
        /// The undo command.
        /// </value>
        public Command SaveCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                SaveFileDialog filepicker = new SaveFileDialog();

                filepicker.DefaultExt = ".ldf";

                filepicker.ShowDialog();

                string filename = filepicker.FileName;
                var manager = (ProgramMngVM)this.ComponentWindow.DataContext;
                manager.SaveStatus(filename);

                manager.LoadStatus(filename);
            }));
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
            }));
        }

        /// <summary>
        /// Gets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public double Scale { get; private set; }

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
            this.isMoving = false;
            this.CurrentMouse = new Point(0, 0);
        }

        /// <summary>
        /// Gets the parent grid component.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The parent.</returns>
        private ComponentVM GetParentGridComponent(UIElement element)
        {
            var parent = (UIElement)VisualTreeHelper.GetParent(element);

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
            var componentToMove = this.GetParentGridComponent((UIElement)e.Source);
            this.CurrentMove = new Point(0, 0);
            var pressedComponent = (UIElement)e.Source;

            var parent = (UIElement)VisualTreeHelper.GetParent(pressedComponent);

            if (parent.GetType() == typeof(Grid))
            {
                var parentgrid = (Grid)parent;
                var dataContext = (ProgramMngVM)this.MainGrid.DataContext;
                componentToMove = dataContext.NodesVMInField.FirstOrDefault(x => x.Name == parentgrid.Name);

                if (componentToMove == null)
                {
                    return;
                }
                
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
                        
                        componentToMove.XCoord += this.CurrentMove.X;
                        componentToMove.YCoord += this.CurrentMove.Y;
                    }

                    var currentDataContext = (ProgramMngVM)this.MainGrid.DataContext;

                    // Check if connection exists, redraw it if it does.
                    foreach (var uiElement in this.ComponentWindow.Children)
                    {
                        if (uiElement.GetType() == typeof(Line))
                        {
                            var line = (Line)uiElement;
                            var connectionVM = currentDataContext.ConnectionsVM.FirstOrDefault(cVM => cVM.ConnectionId == line.Name);

                            if (connectionVM != null)
                            {
                                if (connectionVM.InputPin.Parent == componentToMove || connectionVM.OutputPin.Parent == componentToMove)
                                {
                                    this.OnPinsDisconnected(this, new PinVMConnectionChangedEventArgs(connectionVM));
                                    this.OnPinsConnected(this, new PinVMConnectionChangedEventArgs(connectionVM));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the zoom.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs"/> instance containing the event data.</param>
        private void MouseWheelZoom(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                return;
            }                

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
        /// Called when when a component is added.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FieldComponentEventArgs"/> instance containing the event data.</param>
        private void OnComponentAdded(object sender, FieldComponentEventArgs e)
        {
            // Not sure if I broke it or not, maybe was a different event
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
            Dispatcher.Invoke(() =>
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

            // remove command 
            sampleBody.InputBindings.Add(new MouseBinding(new Command(obj =>
            {
                this.OnComponentRightClick(newComponent);
                componentVM.RemoveComponentCommand.Execute(componentVM);
            }), new MouseGesture(MouseAction.RightClick)));

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
            Panel.SetZIndex(sampleBody, 0);
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
                pinButton.Width = 20;
                pinButton.Height = 10;
                pinButton.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                pinButton.CommandParameter = componentVM.InputPinsVM[i];
                pinButton.Command = componentVM.InputPinsVM[i].SetPinCommand;

                pinButton.RenderTransform = new TranslateTransform((-componentVM.Picture.Width / 2) - 10, yOffset);

                componentVM.InputPinsVM[i].XPosition = (newComponent.Width / 2) - (componentVM.Picture.Width / 2) - 10;
                componentVM.InputPinsVM[i].YPosition = (newComponent.Height / 2) + yOffset;

                yOffset += offsetStepValue;

                Panel.SetZIndex(pinButton, 100);

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
                pinButton.Width = 20;
                pinButton.Height = 10;
                pinButton.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                pinButton.CommandParameter = componentVM.OutputPinsVM[i];
                pinButton.Command = componentVM.OutputPinsVM[i].SetPinCommand;

                pinButton.RenderTransform = new TranslateTransform((componentVM.Picture.Width / 2) + 10, yOffset);
                yOffset += offsetStepValue;

                componentVM.OutputPinsVM[i].XPosition = (newComponent.Width / 2) + (componentVM.Picture.Width / 2) + 10;
                componentVM.OutputPinsVM[i].YPosition = (newComponent.Height / 2) + yOffset;

                Panel.SetZIndex(pinButton, 100);

                newComponent.Children.Add(pinButton);
            }

            this.ComponentWindow.Children.Add(newComponent);
        }

        private void OnComponentRightClick(object component)
        {
            if (component.GetType() == typeof(Grid))
            {
                this.ComponentWindow.Children.Remove((Grid)component);
            }
        }

        /// <summary>
        /// Called when pins get disconnected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PinVMConnectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnPinsDisconnected(object sender, PinVMConnectionChangedEventArgs e)
        {
            var inputPin = e.Connection.InputPin;
            var outputPin = e.Connection.OutputPin;

            foreach (var child in this.ComponentWindow.Children)
            {
                if (child.GetType() == typeof(Line))
                {
                    Line l = (Line)child;
                    
                    if (l.Name == e.Connection.ConnectionId)
                    {
                        this.ComponentWindow.Children.Remove((Line)child);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Called when when pins get connected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PinVMConnectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnPinsConnected(object sender, PinVMConnectionChangedEventArgs e)
        {
            var inputPin = e.Connection.InputPin;
            var outputPin = e.Connection.OutputPin;

            Line line = new Line();
            line.Name = e.Connection.ConnectionId;
            line.MouseRightButtonUp += this.OnConnectionLineClicked;
            line.Visibility = Visibility.Visible;
            line.StrokeThickness = 4;
            line.Stroke = Brushes.Black;
            line.X1 = inputPin.XPosition;
            line.X2 = outputPin.XPosition;
            line.Y1 = inputPin.YPosition;
            line.Y2 = outputPin.YPosition;

            Panel.SetZIndex(line, 50);

            this.ComponentWindow.Children.Add(line);
        }

        /// <summary>
        /// Called when the connection is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnConnectionLineClicked(object sender, MouseButtonEventArgs e)
        {
            var manager = (ProgramMngVM)this.ComponentWindow.DataContext;

            if (sender.GetType() == typeof(Line))
            {
                Line l = (Line)sender;
                manager.RemoveConnectionVM(l.Name);

                this.ComponentWindow.Children.Remove(l);
            }
        }

        /// <summary>
        /// On component added.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PreComponentAdded(object sender, EventArgs e)
        {
            var currentMan = new ProgramMngVM((ProgramMngVM)this.ComponentWindow.DataContext);
            this.UndoHistory.Push(currentMan);
            this.RedoHistory.Clear();
        }
    }
}
