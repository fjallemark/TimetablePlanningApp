using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;

internal static class TimetableStretchMapper
{
    public static TimetableStretch AsTimetableStretch(this IDataRecord me) =>
        new()
        {
            Number = me.GetString("TimetableNumber"),
            Name = me.GetString("TimetableName"),
            Stations = new List<TimetableStretchStation>(),
            TrainSections = new List<TimetableTrainSection>(200),
            StartHour = me.GetInt("StartHour"),
            EndHour = me.GetInt("EndHour"),
            ShowTrainOperatorSignature = me.GetBool("ShowOperator"),
        };

    public static TimetableStretchStation AsTimetableStretchStation(this IDataRecord me) =>
        new()
        {
            DisplayOrder = me.GetInt("StationDisplayOrder"),
            DistanceFromPrevious = me.GetDouble("DistanceFromPrevious"),
            Station = me.AsStation(),
        };
}
