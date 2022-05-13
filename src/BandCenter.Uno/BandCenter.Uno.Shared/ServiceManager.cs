using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BandCenter.Uno
{
    //this isnt a service manager, its a dumb singleton - but we can fix that later ;)
    public static class ServiceManager
    {
        public static BandService BandService = new BandService();

        public static async Task StartUp()
        {
            await BandService.StartUp();
        }
    }
}
