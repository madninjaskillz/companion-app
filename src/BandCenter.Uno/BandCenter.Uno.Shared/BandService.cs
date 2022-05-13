using Microsoft.Band;
using Microsoft.Band.Admin;
using Microsoft.Band.Admin.Phone;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media;
using System.IO;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Microsoft.Band.Tiles;

namespace BandCenter.Uno
{
    public class BandService
    {
        private List<BandTile> Tiles = new List<BandTile>();
        private const string APP_NAME = "Band Center";
        public event HeartRateChangedEventHandler HeartRateChanged;
        public delegate void HeartRateChangedEventHandler(object sender, HeartRateChangedEventArgs e);
        public class HeartRateChangedEventArgs : EventArgs
        {
            public int HeartRate;
        }
        ICargoClient bandClient;
        public async Task StartUp()
        {
            IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();
            if (pairedBands == null || pairedBands.Length == 0)
            {
                return;
            }

            IBandInfo band = pairedBands[0];

            try
            {
                bandClient = await BandAdminClientManager.Instance.ConnectAsync(band);
                var version = await bandClient.GetFirmwareVersionAsync();

                Tiles = (await bandClient.TileManager.GetTilesAsync()).ToList();
                await StartHeartRate();
                await Task.Delay(5000);
                await SetupDefaultTile();
            }
            catch (BandAccessDeniedException ex)
            {
                Debug.WriteLine("Missing Bluetooth access");
            }
            catch (BandException ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public async Task SetupDefaultTile()
        {
            //await RemoveTiles();
            var existingTiles = await bandClient.TileManager.GetTilesAsync();

            if (!existingTiles.Any(x => x.Name == APP_NAME))
            {
                var tileIcon = ToBandIcon("Assets/BandCenterIcon.png");

                var bandTile = new BandTile(bandCenter.Id)
                {
                    Name = APP_NAME,
                    TileIcon = tileIcon,
                    SmallIcon = tileIcon,
                    IsBadgingEnabled = false
                };

                await bandClient.TileManager.AddTileAsync(bandTile);
                await Task.Delay(1000);
                Tiles.Add(bandTile);
            }

        }

        public static BandIcon ToBandIcon(string source)
        {
            BitmapImage bitmap = new BitmapImage();
            string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

            bitmap.UriSource = new Uri(root + "\\" + source);
            return bitmap.ToBandIcon();
        }

        //Idea behind this is we can view previous notifications - and add a tile for that app which will be used going forward.
        private async Task AddTileFromSource(NotificationSource source)
        {
            var ct = new CustomTile
            {
                AppId = source.Name,
                AppName = source.Name,
                Id = source.Id
            };
            
            int remaining = await bandClient.TileManager.GetRemainingTileCapacityAsync();
            if (remaining > 0)
            {
                BandIcon tileIcon = null;
                if (source.Image != null)
                {
                    tileIcon = source.Image.ToBandIcon();
                }

                var bandTile = new BandTile(ct.Id)
                {
                    Name = ct.AppName,
                    TileIcon = tileIcon,
                    SmallIcon = tileIcon,
                    IsBadgingEnabled = false
                };
                Tiles.Add(bandTile);
                await bandClient.TileManager.AddTileAsync(bandTile);
                Debug.WriteLine("band center tile added");

            }
        }

        public async Task RemoveTiles()
        {
            var existingTiles = await bandClient.TileManager.GetTilesAsync();
            foreach (var tile in existingTiles)
            {
                await bandClient.TileManager.RemoveTileAsync(tile);
            }
        }
        public async Task<CargoRunStatistics> GetRunStats() 
        {
            return await bandClient.GetLastRunStatisticsAsync();
        }

        public async Task StartHeartRate()
        {
            // check current user heart rate consent
            if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() != UserConsent.Granted)
            {
                // user hasn’t consented, request consent
                await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }

            // hook up to the Heartrate sensor ReadingChanged event
            bandClient.SensorManager.HeartRate.ReadingChanged += (sender, args) =>
            {
                HeartRateChanged?.Invoke(this, new HeartRateChangedEventArgs { HeartRate = args.SensorReading.HeartRate });
            };

            // start the Heartrate sensor
            await bandClient.SensorManager.HeartRate.StartReadingsAsync();
            await Task.Delay(new TimeSpan(0, 0, 30));
            await bandClient.SensorManager.HeartRate.StopReadingsAsync();
        }
        public class NotificationSource
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public BitmapImage Image { get; set; }

            public Guid NotificationTile { get; set; }

        }

        public List<NotificationSource> NotificationSources { get; set; } = new List<NotificationSource>();

        CustomTile bandCenter = new CustomTile
        {
            AppId = "b6c55a8c-03b9-489a-8fb2-6bad457a62e4",
            Id = Guid.Parse("b6c55a8c-03b9-489a-8fb2-6bad457a62e4"),
            AppName = "Band Center"
        };

        internal async Task<NotificationSource> FetchSource(string appId, string appDisplayName, RandomAccessStreamReference icon)
        {       
            var source = NotificationSources.FirstOrDefault(x => x.Name == appDisplayName);
            if (source == null)
            {
                BitmapImage theIcon;
                if (icon != null)
                {
                    theIcon = ImageFromStream(icon);
                }
                else
                {
                    theIcon = null;
                }

                source = new NotificationSource
                {
                    Name = appDisplayName,
                    NotificationTile = bandCenter.Id,
                    Image = theIcon,
                    Id = Guid.NewGuid()
                };

                NotificationSources.Add(source);
            }

            return source;
        }
        internal async void SendNotification(string appId, string appDisplayName, RandomAccessStreamReference icon, string titleText, string bodyText)
        {
            await DispatchAsync(async () =>
            {
                var source = await FetchSource(appId, appDisplayName, icon);

                BandTile destTile = Tiles.FirstOrDefault(x => x.Name == source.Name);
                if (destTile == null)
                {
                    destTile = Tiles.FirstOrDefault(x => x.TileId == bandCenter.Id);
                }

                if (destTile != null)
                {
                    bandClient.SendTileMessage(destTile.TileId, new TileMessage(titleText, bodyText));
                }

            });
        }
        protected CoreDispatcher Dispatcher => CoreApplication.MainView.Dispatcher;
        protected async Task DispatchAsync(DispatchedHandler callback)
        {
            // As WASM is currently single-threaded, and Dispatcher.HasThreadAccess always returns false for broader compatibility  reasons
            // the following code ensures the app always directly invokes the callback on WASM.
            var hasThreadAccess = Dispatcher.HasThreadAccess;

            if (hasThreadAccess)
            {
                callback.Invoke();
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, callback);
            }
        }

        private BitmapImage ImageFromStream(RandomAccessStreamReference reference)
        {
            var stream = reference.OpenReadAsync().AsTask().Result;

            var image = new BitmapImage();
            image.SetSource(stream);
            return image;
        }

        private class CustomTile
        {
            public Guid Id { get; set; }
            public string AppId { get; set; }
            public string AppName { get; set; }
        }
    }
}
