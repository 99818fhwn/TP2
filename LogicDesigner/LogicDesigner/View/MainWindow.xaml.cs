//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="FH">
//     Company copyright tag.
// </copyright>
//-----------------------------------------------------------------------
namespace LogicDesigner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    using LogicDesigner.ViewModel;
    using Microsoft.Win32;

    /// <summary>
    /// WPF Logic for the main window.
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
        /// The last pressed pin.
        /// </summary>
        private Button lastPressedPin = new Button();

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
            this.ProgramMngVM = new ProgramMngVM();
            this.MainDocPanel.DataContext = this;
            this.MainGrid.DataContext = this.ProgramMngVM;
            var selectBind = new Binding("SelectedFieldComponent");
            selectBind.Source = (ProgramMngVM)this.MainGrid.DataContext;
            this.CurrentSelectedComponentView.SetBinding(MainWindow.DataContextProperty, selectBind);

            this.InputBindings.Add(new InputBinding(this.ProgramMngVM.CopyCommand, new KeyGesture(Key.C, ModifierKeys.Control)));
            this.InputBindings.Add(new InputBinding(this.ProgramMngVM.PasteCommand, new KeyGesture(Key.V, ModifierKeys.Control)));

            this.ProgramMngVM.FieldComponentAdded += this.OnComponentAdded;
            this.ProgramMngVM.FieldComponentRemoved += this.OnComponentDeleted;

            this.ProgramMngVM.PinsConnected += this.OnPinsConnected;
            this.ProgramMngVM.PinsDisconnected += this.OnPinsDisconnected;
            this.ProgramMngVM.ConnectionVMUpdated += this.OnConnectionUpdated;

            this.ComponentWindow.PreviewMouseDown += new MouseButtonEventHandler(this.ComponentMouseDown);
            this.ComponentWindow.PreviewMouseUp += new MouseButtonEventHandler(this.ComponentMouseUp);
            this.ComponentWindow.PreviewMouseMove += new MouseEventHandler(this.ComponentMouseMovePre);

            //// change button icons on click

            foreach (var child in this.ButtonsGrid.Children)
            {
                if (child.GetType() == typeof(Button))
                {
                    var button = (Button)child;

                    if (button.Name == "StopButton")
                    {
                        button.Command = new Command(obj =>
                        {
                            this.SetPressedStopPicture(button, null);
                            this.ProgramMngVM.StopCommand.Execute(obj);
                            this.SetDefaultStopPicture(button, null);
                        });
                    }

                    if (button.Name == "StepButton")
                    {
                        button.Command = new Command(obj =>
                        {
                            this.SetPressedStepPicture(button, null);
                            this.ProgramMngVM.StepCommand.Execute(obj);
                            this.SetDefaultStepPicture(button, null);
                        });
                    }
                }
            }       
        }

        /// <summary>
        /// Gets or sets the program manager view model.
        /// </summary>
        /// <value>
        /// The program manager view model.
        /// </value>
        public ProgramMngVM ProgramMngVM { get; set; }

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
            get
            {
                return this.ProgramMngVM.UndoCommand;
            }
        }

        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// <value>
        /// The save command that can be used to save the current state.
        /// </value>
        public Command SaveCommand
        {
            // TODO: Make serialized path relative, so projects can be shared!!!
            get => new Command(new Action<object>((input) =>
            {
                try
                {
                    SaveFileDialog filepicker = new SaveFileDialog();

                    // filepicker.CheckFileExists = false;
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
                catch (Exception e)
                {
                    MessageBox.Show($"File could not be saved, please try again. \r{e.Message}");
                }
            }));
        }

        /// <summary>
        /// Gets the clear command.
        /// </summary>
        /// <value>
        /// The clear command.
        /// </value>
        public Command ClearCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                var manager = (ProgramMngVM)this.ComponentWindow.DataContext;
                manager.ClearField();
            }));
        }

        /// <summary>
        /// Gets a grid as the background.
        /// </summary>
        /// <value>
        /// The toggle grid command that is used to show the grid.
        /// </value>
        public Command ToggleGridCommand
        {
            get => new Command(new Action<object>(param =>
            {
                DrawingBrush brush = new DrawingBrush();
                brush.TileMode = TileMode.Tile;
                brush.ViewportUnits = BrushMappingMode.Absolute;
                brush.Viewport = new Rect(-15, -15, 15, 15);

                GeometryDrawing geometry = new GeometryDrawing();
                geometry.Geometry = new RectangleGeometry(new Rect(0, 0, 50, 50));
                geometry.Pen = new Pen(new SolidColorBrush(Color.FromArgb(100, 41, 49, 51)), 1);

                brush.Drawing = geometry;

                if (this.ComponentWindow.Background != null)
                {
                    this.ComponentWindow.Background = null;
                }
                else
                {
                    this.ComponentWindow.Background = brush;
                }

                this.ComponentWindow.UpdateLayout();
            }));
        }

        /// <summary>
        /// Gets the load command.
        /// </summary>
        /// <value>
        /// The load command is used for loading an logic designer file into the work environment.
        /// </value>
        public Command LoadCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                try
                {
                    OpenFileDialog filepicker = new OpenFileDialog();

                    // filepicker.CheckFileExists = false;
                    filepicker.Filter = "LogicDesigner files (*.ldf)|*.ldf|All files (*.*)|*.*";
                    filepicker.DefaultExt = ".ldf";
                    filepicker.ShowDialog();

                    string filename = filepicker.FileName;
                    if (File.Exists(filename))
                    {
                        var manager = (ProgramMngVM)this.ComponentWindow.DataContext;

                        MessageBoxResult messageBoxResult = MessageBoxResult.Yes;
                        if (this.ComponentWindow.Children.Count > 0)
                        {
                            messageBoxResult = MessageBox.Show("Created Progress will be lost. \rLoad anyways?", "Load overwrite warning", System.Windows.MessageBoxButton.YesNo);
                        }

                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            this.ComponentWindow.Children.Clear();
                            var loadResult = manager.LoadStatus(filename);

                            App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                foreach (var loadedComponent in loadResult.Item2)
                                {
                                    manager.AddLoadedComponent(loadedComponent);
                                }
                            });

                            foreach (var conn in loadResult.Item1)
                            {
                                manager.AddLoadedConnection(conn);
                            }
                        }
                    }

                    this.ProgramMngVM.UpdateUndoHistory();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"File could not be loaded, please try again. \r{e.Message}");
                }
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
        /// Gets the open config command.
        /// </summary>
        /// <value>
        /// The open config command.
        /// </value>
        public Command OpenConfCommand
        {
            get => new Command(new Action<object>((input) =>
                {
                    if (File.Exists("config.json"))
                    {
                        Process.Start("notepad.exe", "config.json");
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
            get
            {
                return this.ProgramMngVM.RedoCommand;
            }
        }

        /// <summary>
        /// Sets the default step picture.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void SetDefaultStepPicture(object sender, MouseButtonEventArgs e)
        {
            Task.Run(() =>
            {
                Task.Delay(30000);
                this.ProgramMngVM.StepButtonPath = @"\ButtonPictures\step.png";
            });
        }

        /// <summary>
        /// Sets the default stop picture.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void SetDefaultStopPicture(object sender, MouseButtonEventArgs e)
        {
            Task.Run(() =>
            {
                Task.Delay(30000);
                this.ProgramMngVM.StopButtonPath = @"\ButtonPictures\stop.png";
            });
        }

        /// <summary>
        /// Sets the pressed stop picture.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void SetPressedStopPicture(object sender, MouseButtonEventArgs e)
        {
            this.ProgramMngVM.StopButtonPath = @"\ButtonPictures\stop_pressed.png";
        }

        /// <summary>
        /// Sets the pressed step picture.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void SetPressedStepPicture(object sender, MouseButtonEventArgs e)
        {
            this.ProgramMngVM.StepButtonPath = @"\ButtonPictures\step_pressed.png";
        }

        /// <summary>
        /// Called when connection gets updated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnConnectionUpdated(object sender, PinVMConnectionChangedEventArgs e)
        {
            // find the line with name == id -> change its color
            this.Dispatcher.Invoke(() =>
            {
                foreach (var child in this.ComponentWindow.Children)
                {
                    if (child.GetType() == typeof(Line))
                    {
                        Line l = (Line)child;

                        if (l.Name == e.Connection.ConnectionId)
                        {
                            l.Stroke = new SolidColorBrush(Color.FromRgb(e.Connection.LineColor.R, e.Connection.LineColor.G, e.Connection.LineColor.B));
                            break;
                        }
                    }
                }
            });
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
                                this.OnPinsDisconnected(this, new PinVMConnectionChangedEventArgs(connectionVM));
                                this.OnPinsConnected(this, new PinVMConnectionChangedEventArgs(connectionVM));
                                return;
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
        }

        /// <summary>
        /// Disposes bitmap correctly.
        /// </summary>
        /// <param name="hObject"> The bitmap reference. </param>
        /// <returns> Nuffin'. </returns>
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

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
                                        var map = e.Component.Picture.GetHbitmap();
                                        ImageBrush imageBrush = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(map, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));

                                        imageBrush.Stretch = Stretch.Fill;
                                        compToChange.Background = imageBrush;
                                        compToChange.UpdateLayout();
                                        DeleteObject(map);

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
            this.Dispatcher.Invoke(() =>
            {
                var compOld = this.ComponentWindow.Children; // FindName(e.Component.Name);

                foreach (var child in compOld)
                {
                    if (child.GetType() == typeof(Grid))
                    {
                        var grid = (Grid)child;

                        if (grid.Name == e.Component.Name)
                        {
                            this.ComponentWindow.Children.Remove(grid);
                            break;
                        }
                    }
                }
            });

            e.Component.ComponentPropertyChanged -= this.OnComponentChanged; // Unsubscribes from the deleted component

            // this.UndoHistory.Push(currentMan);
            // this.RedoHistory.Clear();
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

            Style style = this.FindResource("btnComponent") as Style;
            sampleBody.Style = style;

            newComponent.Name = componentVM.Name;
            newComponent.RenderTransform = new TranslateTransform(componentVM.XCoord, componentVM.YCoord);
            sampleBody.Name = componentVM.Name + "Body";
            sampleBody.Height = componentVM.Picture.Height; // Can throw an exception i no picture is set the manager has to check for valid, is now solved(21-01-2019) by validator
            sampleBody.Width = componentVM.Picture.Width;

            ImageBrush imageBrush = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(componentVM.Picture.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            imageBrush.Stretch = Stretch.Fill;
            sampleBody.Background = imageBrush;
            sampleBody.Background.Opacity = 0.97;

            // Remove command
            this.ProgramMngVM.FieldComponentRemoved += this.OnComponentDeleted;
            sampleBody.InputBindings.Add(
                new MouseBinding(
                    new Command(obj =>
                    {
                        this.OnComponentRightClick(newComponent);
                        componentVM.RemoveComponentCommand.Execute(componentVM);
                    }),
                new MouseGesture(MouseAction.RightClick)));

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
            Panel.SetZIndex(sampleBody, 3);
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

                var passiveColor = componentVM.InputPinsVM[i].PassiveColor;
                var activeColor = componentVM.InputPinsVM[i].ActiveColor;

                pinButton.Background = new SolidColorBrush(Color.FromRgb(passiveColor.R, passiveColor.G, passiveColor.B));
                pinButton.ToolTip = "PinID: " + componentVM.InputPinsVM[i].Pin.Label;

                var pinVM = componentVM.InputPinsVM[i];

                pinButton.Command = new Command(x =>
                {
                    pinVM.SetPinCommand.Execute(pinVM);

                    if (pinVM.Active == false)
                    {
                        pinButton.Background = new SolidColorBrush(Color.FromRgb(passiveColor.R, passiveColor.G, passiveColor.B));
                    }
                    else
                    {
                        pinButton.Background = new SolidColorBrush(Color.FromRgb(activeColor.R, activeColor.G, activeColor.B));
                    }

                    if (pinButton != this.lastPressedPin)
                    {
                        this.lastPressedPin.Background = new SolidColorBrush(Color.FromRgb(passiveColor.R, passiveColor.G, passiveColor.B));
                    }

                    this.lastPressedPin = pinButton;
                });

                pinButton.RenderTransform = new TranslateTransform((-componentVM.Picture.Width / 2) - 10, yOffset);

                componentVM.InputPinsVM[i].XPosition = (newComponent.Width / 2) - (componentVM.Picture.Width / 2) - 10;
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
                pinButton.Width = 20;
                pinButton.Height = 10;

                var passiveColor = componentVM.OutputPinsVM[i].PassiveColor;
                var activeColor = componentVM.OutputPinsVM[i].ActiveColor;

                pinButton.Background = new SolidColorBrush(Color.FromRgb(passiveColor.R, passiveColor.G, passiveColor.B));
                pinButton.ToolTip = "PinID: " + componentVM.OutputPinsVM[i].Pin.Label;

                var pinVM = componentVM.OutputPinsVM[i];

                pinButton.Command = new Command(x =>
                {
                    pinVM.SetPinCommand.Execute(pinVM);

                    if (pinVM.Active == false)
                    {
                        pinButton.Background = new SolidColorBrush(Color.FromRgb(passiveColor.R, passiveColor.G, passiveColor.B));
                    }
                    else
                    {
                        pinButton.Background = new SolidColorBrush(Color.FromRgb(activeColor.R, activeColor.G, activeColor.B));
                    }

                    if (pinButton != this.lastPressedPin)
                    {
                        this.lastPressedPin.Background = new SolidColorBrush(Color.FromRgb(passiveColor.R, passiveColor.G, passiveColor.B));
                    }

                    this.lastPressedPin = pinButton;
                });

                pinButton.RenderTransform = new TranslateTransform((componentVM.Picture.Width / 2) + 10, yOffset);
                yOffset += offsetStepValue;

                componentVM.OutputPinsVM[i].XPosition = (newComponent.Width / 2) + (componentVM.Picture.Width / 2) + 10;
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

            Panel.SetZIndex(newComponent, 100);
        }

        /// <summary>
        /// Called when component is right clicked.
        /// </summary>
        /// <param name="component">The component.</param>
        private void OnComponentRightClick(object component)
        {
            // Die Methode ist komisch, warum wird ein object übergeben, obwohl es sowies ein grid sein muss?
            // Und es ist ja auch kein wirkliches Event, ich find den Namen etwas verwirrend
            // Des weiteren sollte es fixer Teil der Remove-Eventchain sein
            // LG Moe :3
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
            line.Stroke = new SolidColorBrush(
                Color.FromRgb(
                    e.Connection.LineColor.R,
                    e.Connection.LineColor.G,
                    e.Connection.LineColor.B));
            line.X1 = inputPin.XPosition;
            line.X2 = outputPin.XPosition;
            line.Y1 = inputPin.YPosition;
            line.Y2 = outputPin.YPosition;

            Panel.SetZIndex(line, 2);

            this.ComponentWindow.Children.Add(line);
            this.ComponentWindow.UpdateLayout();
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
    }
}
