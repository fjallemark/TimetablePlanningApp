using System.Collections.Generic;
using System.Data;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class TimetableStretchMapper
    {
        public static TimetableStretch AsTimetableStretch(this IDataRecord me) =>
            new TimetableStretch
            {
                Number = me.GetString("TimetableNumber"),
                Name = me.GetString("TimetableName"),
                Stations = new List<TimetableStretchStation>(),
                TrainSections = new List<TimetableTrainSection>(200)
            };

        public static TimetableStretchStation AsTimetableStretchStation(this IDataRecord me) =>
            new TimetableStretchStation
            {
                DisplayOrder = me.GetInt("StationDisplayOrder"),
                DistanceFromPrevious = me.GetDouble("DistanceFromPrevious"),
                Station = me.AsStation()
            };
    }
}
