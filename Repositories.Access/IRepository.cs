using System.Collections.Generic;
using Tellurian.Trains.Planning.App.Shared;

namespace Tellurian.Trains.Planning.Repositories
{
    public interface IRepository
    {
        IEnumerable<Waybill> GetWaybills(string? timetableName);
        IEnumerable<VehicleSchedule> GetLocoSchedules(string? scheduleName);
        IEnumerable<VehicleSchedule> GetTrainsetSchedules(string? scheduleName);
    }

    public class RepositoryOptions
    {
        public string? ConnectionString { get; set; }
    }
}