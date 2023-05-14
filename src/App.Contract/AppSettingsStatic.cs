﻿using System;

namespace Tellurian.Trains.Planning.App.Contracts
{
    
    public static class Globals
    {
        public static AppSettings AppSettings { get; private set; } = new()
        {
            LayoutId = 25,
            FontFamily="Bahnschrift",
            TrainCategory = new()
            {
                Year=1930   ,
                CountryId = 1
            }
        };
    }
}
