using BandCenter.Uno.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BandCenter.Uno
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        NotificationHandler notificationHandler = new NotificationHandler();
        MainPageViewModel vm;
        public MainPage()
        {
            Loaded += MainPage_Loaded;

            vm = new MainPageViewModel();
            this.DataContext = vm;
            this.InitializeComponent();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await vm.StartUp();
            notificationHandler.StartListening(); 
        }
    }
}
