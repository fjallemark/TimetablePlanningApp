using System;

namespace Tellurian.Trains.Planning.App.Contracts
{
    
    public static class Globals
    {
        public static AppSettings AppSettings { get; private set; } = new()
        {
            LayoutId = 24,
            FontFamily="Bahnschrift",
            TrainCategory = new()
            {
                Year=1993   ,
                CountryId = 1
            }
        };
    }
}
