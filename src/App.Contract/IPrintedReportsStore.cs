using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellurian.Trains.Planning.App.Contracts
{
    /// <summary>
    /// Operations to get data from the data store.
    /// </summary>
    public interface IPrintedReportsStore
    {
        Task<IEnumerable<Waybill>> GetWaybillsAsync(int layoutId);
        Task<IEnumerable<LocoSchedule>> GetLocoSchedulesAsync(int layoutId);
        Task<IEnumerable<TrainsetSchedule>> GetTrainsetSchedulesAsync(int layoutId);
        Task<IEnumerable<DriverDuty>> GetDriverDutiesAsync(int layoutId);
        Task<IEnumerable<TrainCallNote>> GetTrainCallNotesAsync(int layoutId);
        Task<IEnumerable<BlockDestinations>> GetBlockDestinationsAsync(int layoutId);
        Task<DriverDutyBooklet?> GetDriverDutyBookletAsync(int layoutId);
        Task<IEnumerable<TimetableStretch>> GetTimetableStretchesAsync(int layoutId);
        Task<IEnumerable<Train>> GetTrainsAsync(int layoutId);
        Task<IEnumerable<TrainDeparture>> GetTrainDeparturesAsync(int layoutId);
    }

    public class RepositoryOptions
    {
        public string? ConnectionString { get; set; }
    }
}