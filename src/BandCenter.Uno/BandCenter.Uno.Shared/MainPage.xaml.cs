using BandCenter.Uno.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BandCenter.Uno
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MainPageViewModel vm;
        public MainPage()
        {
            Loaded += MainPage_Loaded;

            vm = new MainPageViewModel();
            this.DataContext = vm;
            this.InitializeComponent();
        }

        UserNotificationListener listener = UserNotificationListener.Current;
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await vm.StartUp();
            UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

            switch (accessStatus)
            {
                // This means the user has granted access.
                case UserNotificationListenerAccessStatus.Allowed:
                    await ServiceManager.StartUp();
                    listener.NotificationChanged += Listener_NotificationChanged;
                    break;
            }
        }

        //this all feels very windows specific - not ideal. should abstract it out.
        private void Listener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            UserNotification notification = listener.GetNotification(args.UserNotificationId);
            if (notification != null)
            {
                Windows.Storage.Streams.RandomAccessStreamReference icon = notification.AppInfo.DisplayInfo.GetLogo(new Size(64, 64));
                string appDisplayName = notification.AppInfo.DisplayInfo.DisplayName;
                string appId = notification.AppInfo.AppUserModelId;
                NotificationBinding toastBinding = notification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);

                if (toastBinding != null)
                {
                    IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();
                    string titleText = textElements.FirstOrDefault()?.Text;
                    string bodyText = string.Join("\n", textElements.Skip(1).Select(t => t.Text));
                    ServiceManager.BandService.SendNotification(appId, appDisplayName, icon, titleText, bodyText);
                }
            }
        }
    }
}
