using Microsoft.Band;
using Microsoft.Band.Admin;
using Microsoft.Band.Admin.Phone;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using static BandCenter.Uno.BandService;

namespace BandCenter.Uno.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        ICargoClient bandClient;
  
     public async Task StartUp()
        {
            ServiceManager.BandService.HeartRateChanged += BandService_HeartRateChanged;
            var lastRun = await ServiceManager.BandService.GetRunStats();
            RunEndTime = lastRun.EndTime;
            RunDistance = lastRun.Distance;
        }

        private void BandService_HeartRateChanged(object sender, HeartRateChangedEventArgs e)
        {
            HeartRate = e.HeartRate;
        }

        public int HeartRate { get => Get<int>(); set => Set<int>(value); }
        public DateTime RunEndTime { get => Get<DateTime>(); set => Set<DateTime>(value); }
        public uint RunDistance { get => Get<uint>(); set => Set<uint>(value); }

    }
}
