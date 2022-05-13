using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BandCenter.Uno
{
    public static class ServiceManager
    {
        public static BandService BandService = new BandService();

        public static async Task StartUp()
        {
            await BandService.StartUp();
        }
    }
}
