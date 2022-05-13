using Microsoft.Band;
using Microsoft.Band.Admin;
using Microsoft.Band.Admin.Phone;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BandCenter.Uno
{
    public class BandService
    {
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
                return;

            IBandInfo band = pairedBands[0];
            // TODO: Make sure band is available

            try
            {
                bandClient = await BandAdminClientManager.Instance.ConnectAsync(band);
                var version = await bandClient.GetFirmwareVersionAsync();

                await StartHeartRate();
                bandClient.SendEmailNotification("test", "Hello", DateTime.Now);

            }
            catch (BandAccessDeniedException ex)
            {
                // Handle a Band connection exception
                System.Diagnostics.Debug.WriteLine("Missing Bluetooth access");
            }
            catch (BandException ex)
            {
                // Handle a Band connection exception
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public void SendNotification(string from, string title, string body)
        {
            bandClient.SendEmailNotification(from, title, DateTime.Now);
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
    }
}
