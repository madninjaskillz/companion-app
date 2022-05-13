using BandCenter.Uno.Controls;
using BandCenter.Uno.ViewModels;
using Microsoft.Band;
using Microsoft.Band.Admin.Phone;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

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
            

            // And request access to the user's notifications (must be called from UI thread)
            UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

            switch (accessStatus)
            {
                // This means the user has granted access.
                case UserNotificationListenerAccessStatus.Allowed:
                    listener.NotificationChanged += Listener_NotificationChanged;
                    break;

            }
    }

        private void Listener_NotificationChanged(UserNotificationListener sender, Windows.UI.Notifications.UserNotificationChangedEventArgs args)
        {
            Debug.WriteLine(args.UserNotificationId);
            UserNotification notification = listener.GetNotification(args.UserNotificationId);
            string appDisplayName = notification.AppInfo.DisplayInfo.DisplayName;
            NotificationBinding toastBinding = notification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);

            if (toastBinding != null)
            {
                // And then get the text elements from the toast binding
                IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();

                // Treat the first text element as the title text
                string titleText = textElements.FirstOrDefault()?.Text;

                // We'll treat all subsequent text elements as body text,
                // joining them together via newlines.
                string bodyText = string.Join("\n", textElements.Skip(1).Select(t => t.Text));

                ServiceManager.BandService.SendNotification(appDisplayName, titleText, bodyText);
            }
        }
        }
    }
