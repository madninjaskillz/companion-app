﻿using System;
using Windows.UI.Xaml;

namespace BandCenter.Uno.Wasm
{
    public class Program
    {
        private static App _app;

        static int Main(string[] args)
        {
            Windows.UI.Xaml.Application.Start(_ => _app = new App());

            return 0;
        }
    }
}
