/* 
    Copyright (c) 2012 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see all Code Samples for Windows Phone, visit http://go.microsoft.com/fwlink/?LinkID=219604 
  
*/
using PixPresenter.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;
using System.IO;
using Microsoft.Xna.Framework.Media;
using System.IO.IsolatedStorage;
using System.Windows.Media;

namespace PixPresenter
{
    public partial class PictureView : ViewBase
    {
        const double MaxScale = 10;

        double _scale = 1.0;
        double _minScale;
        double _coercedScale;
        double _originalScale;

        Size _viewportSize;
        bool _pinching;
        Point _screenMidpoint;
        Point _relativeMidpoint;

        BitmapImage _bitmap;

        /// <summary>
        /// This is a very simple page. We simply bind to the CurrentPicture property on the AlbumsViewModel
        /// </summary>
        public PictureView()
        {
            InitializeComponent();
            // Create AppBar
            BuildLocalizedApplicationBar();
            this.DataContext = SharingViewModel.Instance;
        }

        /// <summary>
        /// Either the user has manipulated the image or the size of the viewport has changed. We only
        /// care about the size.
        /// </summary>
        void viewport_ViewportChanged(object sender, System.Windows.Controls.Primitives.ViewportChangedEventArgs e)
        {
            Size newSize = new Size(viewport.Viewport.Width, viewport.Viewport.Height);
            if (newSize != _viewportSize)
            {
                _viewportSize = newSize;
                CoerceScale(true);
                ResizeImage(false);
            }
        }

        /// <summary>
        /// Handler for the ManipulationStarted event. Set initial state in case
        /// it becomes a pinch later.
        /// </summary>
        void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _pinching = false;
            _originalScale = _scale;
        }

        /// <summary>
        /// Handler for the ManipulationDelta event. It may or may not be a pinch. If it is not a 
        /// pinch, the ViewportControl will take care of it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation != null)
            {
                e.Handled = true;

                if (!_pinching)
                {
                    _pinching = true;
                    Point center = e.PinchManipulation.Original.Center;
                    _relativeMidpoint = new Point(center.X / ImageReceived.ActualWidth, center.Y / ImageReceived.ActualHeight);

                    var xform = ImageReceived.TransformToVisual(viewport);
                    _screenMidpoint = xform.Transform(center);
                }

                _scale = _originalScale * e.PinchManipulation.CumulativeScale;

                CoerceScale(false);
                ResizeImage(false);
            }
            else if (_pinching)
            {
                _pinching = false;
                _originalScale = _scale = _coercedScale;
            }
        }

        /// <summary>
        /// The manipulation has completed (no touch points anymore) so reset state.
        /// </summary>
        void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _pinching = false;
            _scale = _coercedScale;
        }


        /// <summary>
        /// When a new image is opened, set its initial scale.
        /// </summary>
        void OnImageOpened(object sender, RoutedEventArgs e)
        {
            _bitmap = (BitmapImage)ImageReceived.Source;
           
            // Set scale to the minimum, and then save it.
            _scale = 0;
            CoerceScale(true);
            _scale = _coercedScale;

            ResizeImage(true);
        }

        /// <summary>
        /// Adjust the size of the image according to the coerced scale factor. Optionally
        /// center the image, otherwise, try to keep the original midpoint of the pinch
        /// in the same spot on the screen regardless of the scale.
        /// </summary>
        /// <param name="center"></param>
        void ResizeImage(bool center)
        {
            if (_coercedScale != 0 && _bitmap != null)
            {
                double newWidth = canvas.Width = Math.Round(_bitmap.PixelWidth * _coercedScale);
                double newHeight = canvas.Height = Math.Round(_bitmap.PixelHeight * _coercedScale);

                xform.ScaleX = xform.ScaleY = _coercedScale;

                viewport.Bounds = new Rect(0, 0, newWidth, newHeight);

                if (center)
                {
                    viewport.SetViewportOrigin(
                        new Point(
                            Math.Round((newWidth - viewport.ActualWidth) / 2),
                            Math.Round((newHeight - viewport.ActualHeight) / 2)
                            ));
                }
                else
                {
                    Point newImgMid = new Point(newWidth * _relativeMidpoint.X, newHeight * _relativeMidpoint.Y);
                    Point origin = new Point(newImgMid.X - _screenMidpoint.X, newImgMid.Y - _screenMidpoint.Y);
                    viewport.SetViewportOrigin(origin);
                }
            }
        }

        /// <summary>
        /// Coerce the scale into being within the proper range. Optionally compute the constraints 
        /// on the scale so that it will always fill the entire screen and will never get too big 
        /// to be contained in a hardware surface.
        /// </summary>
        /// <param name="recompute">Will recompute the min max scale if true.</param>
        void CoerceScale(bool recompute)
        {
            if (recompute && _bitmap != null && viewport != null)
            {
                // Calculate the minimum scale to fit the viewport
                double minX = viewport.ActualWidth / _bitmap.PixelWidth;
                double minY = viewport.ActualHeight / _bitmap.PixelHeight;

                _minScale = Math.Min(minX, minY);
            }

            _coercedScale = Math.Min(MaxScale, Math.Max(_scale, _minScale));

        }


        void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = Color.FromArgb(255, 127, 186, 0);
            ApplicationBar.Mode = ApplicationBarMode.Default;

            //ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.home.variant.png", UriKind.Relative));
            //appBarButton.Text = "Home";
            //appBarButton.Click += Home_Click;
            //ApplicationBar.Buttons.Add(appBarButton);

            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/save.png", UriKind.Relative));
            appBarButton.Text = "Save";
            appBarButton.Click += Save_Click;
            ApplicationBar.Buttons.Add(appBarButton);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            WriteableBitmap picLibaryImage = new WriteableBitmap(_bitmap);
            String tempJPEG = DateTime.Now.ToBinary().ToString();
            //Create virtual store and file stream. Check for duplicate tempJPEG files.
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStore.FileExists(tempJPEG))
            {
                myStore.DeleteFile(tempJPEG);
            }
           
            IsolatedStorageFileStream myFileStream = myStore.CreateFile(tempJPEG);
            //Encode the WriteableBitmap into JPEG stream and place into isolated storage.
            Extensions.SaveJpeg(picLibaryImage, myFileStream, picLibaryImage.PixelWidth, picLibaryImage.PixelHeight, 0, 85);
            myFileStream.Close();
            myFileStream = myStore.OpenFile(tempJPEG, FileMode.Open, FileAccess.Read);
            //Add the JPEG file to the photos library on the device.
            MediaLibrary library = new MediaLibrary();
            Picture pic = library.SavePicture(tempJPEG, myFileStream);
            myFileStream.Close();
            MessageBox.Show("Received File Successfully Saved.");
            NavigationService.GoBack();
        }
    }
}
