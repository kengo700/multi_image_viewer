using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MultiImageViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            const double scale = 1.1;

            var mouse_pos = e.GetPosition((IInputElement)sender);

            TransformGroup transformGroup = ((Image)sender).RenderTransform as TransformGroup;
            ScaleTransform scaleTransform = transformGroup.Children[0] as ScaleTransform;

            if (e.Delta > 0)
            {
                // 拡大処理
                scaleTransform.ScaleX *= scale;
                scaleTransform.ScaleY *= scale;
            }
            else
            {
                // 縮小処理
                scaleTransform.ScaleX /= scale;
                scaleTransform.ScaleY /= scale;
            }

            transformGroup.Children[0] = scaleTransform;
            ((Image)sender).RenderTransform = transformGroup;

        }

        private Point _start;

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Image)sender).CaptureMouse();
            _start = e.GetPosition(MainGrid);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (((Image)sender).IsMouseCaptured)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Vector v = _start - e.GetPosition(MainGrid);

                    TransformGroup transformGroup = ((Image)sender).RenderTransform as TransformGroup;
                    TranslateTransform translateTransform = transformGroup.Children[2] as TranslateTransform;
                    translateTransform.X += -v.X;
                    translateTransform.Y += -v.Y;
                    transformGroup.Children[2] = translateTransform;
                    ((Image)sender).RenderTransform = transformGroup;

                    _start = e.GetPosition(MainGrid);
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    Vector v = _start - e.GetPosition(MainGrid);

                    TransformGroup transformGroup = ((Image)sender).RenderTransform as TransformGroup;
                    RotateTransform rotateTransform = transformGroup.Children[1] as RotateTransform;

                    rotateTransform.Angle += -v.X / 3.0;

                    transformGroup.Children[1] = rotateTransform;
                    ((Image)sender).RenderTransform = transformGroup;

                    _start = e.GetPosition(MainGrid);

                }
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((Image)sender).ReleaseMouseCapture();
        }

        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TransformGroup transformGroup = ((Image)sender).RenderTransform as TransformGroup;
                ScaleTransform scaleTransform = transformGroup.Children[0] as ScaleTransform;

                scaleTransform.ScaleX *= -1;

                transformGroup.Children[0] = scaleTransform;
                ((Image)sender).RenderTransform = transformGroup;
            }
            else
            {
                ((Image)sender).CaptureMouse();
                _start = e.GetPosition(MainGrid);
            }
        }

        private void Image_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((Image)sender).ReleaseMouseCapture();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                foreach (var image in ((Canvas)sender).Children.OfType<Image>())
                {
                    TransformGroup transformGroup = image.RenderTransform as TransformGroup;
                    ScaleTransform scaleTransform = transformGroup.Children[0] as ScaleTransform;
                    RotateTransform rotateTransform = transformGroup.Children[1] as RotateTransform;
                    TranslateTransform translateTransform = transformGroup.Children[2] as TranslateTransform;

                    rotateTransform.Angle = 0;
                    translateTransform.X = 0;
                    translateTransform.Y = 0;

                    var matrix = image.RenderTransform.Value;

                    double canvas_width = ((Canvas)image.Parent).ActualWidth;
                    double canvas_height = ((Canvas)image.Parent).ActualHeight;
                    double image_width = image.ActualWidth;
                    double image_height = image.ActualHeight;

                    double scale_x = canvas_width / image_width;
                    double scale_y = canvas_height / image_height;

                    double scale = Math.Min(scale_x, scale_y);

                    if(scaleTransform.ScaleX == scale || scaleTransform.ScaleY == scale)
                    {
                        scaleTransform.ScaleX = 1.0;
                        scaleTransform.ScaleY = 1.0;

                        translateTransform.X = 0.0;
                        translateTransform.Y = 0.0;
                    }
                    else
                    {
                        scaleTransform.ScaleX = scale;
                        scaleTransform.ScaleY = scale;

                        translateTransform.X = image_width / 2.0 * scale - image_width / 2.0;
                        translateTransform.Y = image_height / 2.0 * scale - image_height / 2.0;

                        transformGroup.Children[0] = scaleTransform;
                        transformGroup.Children[1] = rotateTransform;
                        transformGroup.Children[2] = translateTransform;
                        image.RenderTransform = transformGroup;

                    }

                }

            }

        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (var image in ((Canvas)sender).Children.OfType<Image>())
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    var bmpImage = new BitmapImage();
                    bmpImage.BeginInit();
                    bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                    bmpImage.CreateOptions = BitmapCreateOptions.None;
                    bmpImage.UriSource = new Uri(files[0]);
                    bmpImage.EndInit();
                    bmpImage.Freeze();

                    image.Source = bmpImage;
                }
            }
        }

    }
}
