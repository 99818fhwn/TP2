namespace LogicDesigner
{
    using System;
    using System.Collections.Generic;
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
            // Set datacontext specifically to MainGrid, else Undo/Redo wouldn't work in current structure - Moe
            this.MainGrid.DataContext = new ProgramMngVM();

            this.ComponentWindow.PreviewMouseDown += new MouseButtonEventHandler(ComponentMouseDown);
            ComponentWindow.PreviewMouseUp += new MouseButtonEventHandler(ComponentMouseUp);
            ComponentWindow.PreviewMouseMove += new MouseEventHandler(ComponentMouseMovePre);

            DrawNewComponent(null, 150);
            DrawNewComponent(null);
        }

        /// <summary>
        /// Called when the button is pressed down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ComponentMouseDown(object sender, MouseButtonEventArgs e)
        {
            var pressedComponent = (UIElement)e.Source;
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
            CurrentMove = new Point(0, 0);
            CurrentMouse = new Point(0, 0);
        }

        // needed for translation calculation - Moe
        private Point CurrentMouse { get; set; }
        private Point CurrentMove { get; set; }
        /// <summary>
        /// Called when the button is moved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void ComponentMouseMovePre(object sender, MouseEventArgs e)
        {
            // Components are being reset, because their translation is not persistent and the temp variables are reset after letting the mouse go
            var pressedComponent = (UIElement)e.Source;

            if (!this.isMoving)
            {
                return;
            }
            else
            {
                var previousMouse = CurrentMouse;
                CurrentMouse = Mouse.GetPosition(this.ComponentWindow);
                //Point relativePoint = pressedComponent.TransformToAncestor(ComponentWindow).Transform(new Point(0, 0));
                if (previousMouse != new Point(0, 0) && CurrentMouse != previousMouse)
                {
                    Point movepoint = new Point(CurrentMouse.X - previousMouse.X, CurrentMouse.Y - previousMouse.Y);
                    CurrentMove = new Point(CurrentMove.X + movepoint.X, CurrentMove.Y + movepoint.Y);
                    pressedComponent.RenderTransform = new TranslateTransform(CurrentMove.X,CurrentMove.Y);
                }
            }
        }

        /// <summary>
        /// Called when when a component is added.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnComponentAdded(object sender, EventArgs e)
        {
            // Add a component.
        }

        /// <summary>
        /// Called when a component is deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnComponentDeleted(object sender, EventArgs e)
        {
            // Delete component.
        }

        /// <summary>
        /// Draws a new component.
        /// </summary>
        private void DrawNewComponent(ComponentVM componentVM, int debugShift = 0)
        {
            // New component
            Grid sampleComponent = new Grid();

            // Component Body
            Button sampleBody = new Button();

            sampleBody.Name = "NewComponent";
            sampleBody.Height = Properties.Resources.And.Height;
            sampleBody.Width = Properties.Resources.And.Width;

            ImageBrush imageBrush = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.And.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
            imageBrush.Stretch = Stretch.Fill;
            sampleBody.Background = imageBrush;

            Button middle = new Button();
            middle.Height = 5;
            middle.Width = 5;
            this.ComponentWindow.Children.Add(middle);

            sampleComponent.Children.Add(sampleBody);
            this.ComponentWindow.Children.Add(sampleComponent);

            sampleComponent.RenderTransform = new TranslateTransform(debugShift, debugShift);
        }


        // Undo and Redo functionality -> to be tested
        private Stack<ProgramMngVM> UndoHistory { get; set; }
        private Stack<ProgramMngVM> RedoHistory { get; set; }
        public Command UndoCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                if (this.UndoHistory.Count > 0)
                {
                    ProgramMngVM history = this.UndoHistory.Pop();
                    this.MainGrid.DataContext = history;
                    this.RedoHistory.Push(history);
                }
            }));
        }

        public Command RedoCommand
        {
            get => new Command(new Action<object>((input) =>
            {
                if (this.RedoHistory.Count > 0)
                {
                    ProgramMngVM history = this.RedoHistory.Pop();
                    this.MainGrid.DataContext = history;
                    this.UndoHistory.Push(history);
                }

            }));
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollbar = (ScrollViewer)e.Source;
            scrollbar.ScrollToVerticalOffset(scrollbar.ScrollableHeight / 2);
            scrollbar.ScrollToHorizontalOffset(scrollbar.ScrollableWidth / 2);
        }
    }

}
