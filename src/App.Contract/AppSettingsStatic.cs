using System;

namespace Tellurian.Trains.Planning.App.Contracts
{
    
    public static class Globals
    {
        public static AppSettings AppSettings { get; private set; } = new()
        {
            LayoutId = 22,
            FontFamily="Bodoni Mt",
            TrainCategory = new()
            {
                Year=1972,
                CountryId = 1
            }
        };
    }
}
