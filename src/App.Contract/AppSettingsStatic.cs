using System;

namespace Tellurian.Trains.Planning.App.Contracts
{
    
    public static class Globals
    {
        public static AppSettings AppSettings { get; private set; } = new()
        {
            LayoutId = 25,
            FontFamily="Bodoni Mt",
            TrainCategory = new()
            {
                Year=1967,
                CountryId = 1
            }
        };
    }
}
