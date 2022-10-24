using System;

namespace Tellurian.Trains.Planning.App;


public class AppSettings
{
    
    public int LayoutId { get; init; }
    public string FontFamily { get; init; } = "Arial";
    public TrainCategorySettings TrainCategory { get; init; } = new ();
}

public class TrainCategorySettings
{
    public int Year { get; init; } = DateTime.Now.Year;
    public int CountryId { get; init; } = 1; // Sweden
}
