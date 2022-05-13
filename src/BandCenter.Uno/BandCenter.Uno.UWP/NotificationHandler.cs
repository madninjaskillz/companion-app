using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace BandCenter.Uno
{
    internal class NotificationHandler : INotificationHandler
    {
        UserNotificationListener listener = UserNotificationListener.Current;
        public async void StartListening()
        {
            UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

            switch (accessStatus)
            {
                // This means the user has granted access.
                case UserNotificationListenerAccessStatus.Allowed:
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
