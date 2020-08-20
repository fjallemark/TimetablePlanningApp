using System.Collections.Generic;
using Tellurian.Trains.Planning.App.Shared;

namespace Tellurian.Trains.Planning.Repositories
{
    /// <summary>
    /// Operations to get data from the data store.
    /// </summary>
    public interface IRepository
    {
        IEnumerable<Waybill>? GetWaybills(string timetableName);
        IEnumerable<VehicleSchedule>? GetLocoSchedules(string scheduleName);
        IEnumerable<VehicleSchedule>? GetTrainsetSchedules(string scheduleName);
        DriverDutyBooklet? GetDriverDutyBooklet(string scheduleName);

        IEnumerable<TrainCallNote> GetTrainCallNotes(string scheduleName);
    }

    public class RepositoryOptions
    {
        public string? ConnectionString { get; set; }
    }
}