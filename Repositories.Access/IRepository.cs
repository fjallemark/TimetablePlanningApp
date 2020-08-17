using System.Collections.Generic;
using Tellurian.Trains.Planning.App.Shared;

namespace Tellurian.Trains.Planning.Repositories
{
    public interface IRepository
    {
        IEnumerable<Waybill> GetWaybills(string? timetableName);
        IEnumerable<VehicleSchedule> GetLocoSchedules(string? scheduleName);
        IEnumerable<VehicleSchedule> GetTrainsetSchedules(string? scheduleName);
        DriverDutyBooklet? GetDriverDutyBooklet(string scheduleName);
        IEnumerable<ManualTrainCallNote> GetManualTrainStationCallNotes(string? scheduleName);
        IEnumerable<TrainsetsCallNote> GetDepartureTrainsetsCallNotes(string? scheduleName);
        IEnumerable<TrainsetsCallNote> GetArrivalTrainsetsCallNotes(string? scheduleName);
    }

    public class RepositoryOptions
    {
        public string? ConnectionString { get; set; }
    }
}