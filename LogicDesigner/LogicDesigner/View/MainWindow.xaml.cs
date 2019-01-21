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
        /// If a component is beeing dragged.
        /// </summary>
        private bool isMoving;

        /// <summary>
        /// The component position.
        /// </summary>
        private Point? componentPosition;

        /// <summary>
        /// The delta of the clicked x position on the component and the mouse x position.
        /// </summary>
        private double deltaX;

        /// <summary>
        /// The delta of the clicked y position on the component and the mouse y position.
        /// </summary>
        private double deltaY;

        /// <summary>
        /// The translated transform.
        /// </summary>
        private TranslateTransform translateTransform;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.UndoHistory = new Stack<ProgramMngVM>();
            this.RedoHistory = new Stack<ProgramMngVM>();

            this.DataContext = this;
            ProgramMngVM programMngVM = new ProgramMngVM();
            this.MainGrid.DataContext = programMngVM;

            programMngVM.FieldComponentAdded += OnComponentAdded;
            programMngVM.FieldComponentRemoved += OnComponentDeleted;

            this.ComponentWindow.PreviewMouseDown += new MouseButtonEventHandler(ComponentMouseDown);
            this.ComponentWindow.PreviewMouseUp += new MouseButtonEventHandler(ComponentMouseUp);
            this.ComponentWindow.PreviewMouseMove += new MouseEventHandler(ComponentMouseMovePre);

            var currentMan = new ProgramMngVM((ProgramMngVM)this.ComponentWindow.DataContext);
            this.UndoHistory.Push(currentMan);
        }

        /// <summary>
        /// Called when the button is pressed down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ComponentMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.isMoving = true;
        }

        /// <summary>
        /// Called when the button is pressed up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ComponentMouseUp(object sender, MouseButtonEventArgs e)
        {
            var pressedComponent = (UIElement)e.Source;
            this.isMoving = false;
            this.CurrentMove = new Point(0, 0);
            this.CurrentMouse = new Point(0, 0);
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

            if (!this.isMoving)
            {
                return;
            }
            else
            {
                var previousMouse = this.CurrentMouse;
                this.CurrentMouse = Mouse.GetPosition(this.ComponentWindow);
                //Point relativePoint = pressedComponent.TransformToAncestor(ComponentWindow).Transform(new Point(0, 0));
                if (previousMouse != new Point(0, 0) && this.CurrentMouse != previousMouse)
                {
                    Point movepoint = new Point(this.CurrentMouse.X - previousMouse.X, this.CurrentMouse.Y - previousMouse.Y);
                    this.CurrentMove = new Point(this.CurrentMove.X + movepoint.X, this.CurrentMove.Y + movepoint.Y);
                    pressedComponent.RenderTransform = new TranslateTransform(this.CurrentMove.X, this.CurrentMove.Y);
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
            DrawNewComponent(e.Component);
        }

        /// <summary>
        /// Called when a component is deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FieldComponentEventArgs"/> instance containing the event data.</param>
        private void OnComponentDeleted(object sender, FieldComponentEventArgs e)
        {
            var currentMan = new ProgramMngVM((ProgramMngVM)this.ComponentWindow.DataContext);
            this.UndoHistory.Push(currentMan);
            this.RedoHistory.Clear();
            // Delete component.
        }

        /// <summary>
        /// Draws a new component.
        /// </summary>
        private void DrawNewComponent(ComponentVM componentVM)
        {
            // New component
            Grid sampleComponent = new Grid();

            // Component Body
            Button sampleBody = new Button();

            sampleBody.Name = componentVM.Label;
            sampleBody.Height = componentVM.Picture.Height;
            sampleBody.Width = componentVM.Picture.Width;

            ImageBrush imageBrush = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(componentVM.Picture.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            imageBrush.Stretch = Stretch.Fill;
            sampleBody.Background = imageBrush;

            // Add the label
            string text = componentVM.Label;
            Typeface myTypeface = new Typeface("Helvetica");
            FormattedText ft = new FormattedText(text, CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, myTypeface, 16, Brushes.Red);

            TextBlock label = new TextBlock
            {
                Width = ft.Width,
                Height = ft.Height,
                Text = componentVM.Label
            };

            label.TextAlignment = TextAlignment.Center;

            label.RenderTransform = new TranslateTransform(0, -componentVM.Picture.Height / 2 - 10);

            sampleComponent.Children.Add(sampleBody);
            sampleComponent.Children.Add(label);

            this.ComponentWindow.Children.Add(sampleComponent);

            sampleComponent.RenderTransform = new TranslateTransform(0, 100);
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

        /// <summary>
        /// Gets or sets the undo history.
        /// </summary>
        /// <value>
        /// The undo history.
        /// </value>
        public Stack<ProgramMngVM> UndoHistory { get; private set; }

        /// <summary>
        /// Gets or sets the redo history.
        /// </summary>
        /// <value>
        /// The redo history.
        /// </value>
        public Stack<ProgramMngVM> RedoHistory { get; private set; }

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
                    //if (RedoHistory.Count == 0)
                    //{
                    //    this.RedoHistory.Push(new ProgramMngVM((ProgramMngVM)this.MainGrid.DataContext));
                    //}
                    //else
                    //{
                    //    this.RedoHistory.Push(history);
                    //}
                    //MessageBox.Show($"{UndoHistory.Count()} undo count - {RedoHistory.Count} redo count");

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
                    //MessageBox.Show($"{UndoHistory.Count()} undo count - {RedoHistory.Count} redo count");

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
        /// Gets or sets the current mouse.
        /// </summary>
        /// <value>
        /// The current mouse.
        /// </value>
        public Point CurrentMouse { get; private set; }

        /// <summary>
        /// Gets or sets the current move.
        /// </summary>
        /// <value>
        /// The current move.
        /// </value>
        public Point CurrentMove { get; private set; }
    }

}
