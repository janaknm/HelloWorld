using Microsoft.Phone.Shell;
using PixPresenter.ViewModels;
using PixPresenterPortableLib;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Tasks;

namespace PixPresenter
{
    /// <summary>
    /// Code behind for AlbumsView.xaml
    /// </summary>
    public partial class AlbumsView : ViewBase
    {
        private bool _initialised = false;

        // Constructor
        public AlbumsView()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Make sure no album is selected, so that when we come back we can select again
            AlbumsList.SelectedItem = null;
            base.OnNavigatedFrom(e);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_initialised)
                App.AlbumsViewModel = new AlbumsViewModel(App.AlbumsService);
            // Need to await since this modifies the data context on a background thread
            await App.AlbumsViewModel.LoadAlbumsAsync();
            this.DataContext = App.AlbumsViewModel;

            _initialised = true;
        }

        void ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ListBox albums = sender as ListBox;
            if (albums.SelectedItem == null)
            return;

            // User has selected an album, so navigate to the PicturePresenter page.
            AlbumViewModel avm = albums.SelectedItem as AlbumViewModel;
            NavigationService.Navigate(new Uri(String.Format("/Views/AlbumView.xaml?albumname={0}", avm.Name), UriKind.Relative));
        }

        void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            ApplicationBar.BackgroundColor = Color.FromArgb(255, 127, 186, 0);
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.connection.bluetooth.png", UriKind.Relative));
            appBarButton.Text = "Settings";
            //Strings.Caption_AppBarConnect;
            appBarButton.Click += Settings_Click;
            ApplicationBar.Buttons.Add(appBarButton);

            appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.elevator.up.png", UriKind.Relative));
            appBarButton.Text = Strings.Caption_AppBarConnect;
            appBarButton.Click += Share_Click;
            ApplicationBar.Buttons.Add(appBarButton);

            ApplicationBar.Mode = ApplicationBarMode.Default;
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            ShowBluetoothcControlPanel();
        }
    }
}
