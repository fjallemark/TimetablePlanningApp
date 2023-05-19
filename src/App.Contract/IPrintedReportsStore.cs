namespace Tellurian.Trains.Planning.App.Contracts;

/// <summary>
/// Operations to get data from the data store.
/// </summary>
public interface IPrintedReportsStore
{
    Task<IEnumerable<BlockDestinations>> GetBlockDestinationsAsync(int layoutId);
    Task<DriverDutyBooklet?> GetDriverDutyBookletAsync(int layoutId);
    Task<IEnumerable<DriverDuty>> GetDriverDutiesAsync(int layoutId);
    Task<IEnumerable<LocoSchedule>> GetLocoSchedulesAsync(int layoutId);
    Task<StationDutyBooklet?> GetStationDutyBookletAsync(int layoutId);
    Task<IEnumerable<StationDutyData>> GetStationDutiesDataAsync(int layoutId);
    Task<IEnumerable<TimetableStretch>> GetTimetableStretchesAsync(int layoutId, string? stretchNumber);
    Task<IEnumerable<Train>> GetTrainsAsync(int layoutId);
    Task<IEnumerable<VehicleSchedule>> GetTrainsetSchedulesAsync(int layoutId);
    Task<IEnumerable<TrainDeparture>> GetTrainDeparturesAsync(int layoutId, bool onlyItitialTrains = false);
    Task<IEnumerable<TrainCallNote>> GetTrainCallNotesAsync(int layoutId);
    Task<Layout?> GetLayout(int layoutId);
    Task<IEnumerable<TrainCategory>> GetTrainCategories(int layoutId);
    Task<int?> GetCurrentLayoutId();
    Task<IEnumerable<StationTrainOrder>> GetStationsTrainOrder(int layoutId);
    IEnumerable<LayoutVehicle> GetLayoutVehicles(int layoutId);
}

public class RepositoryOptions
{
    public string? ConnectionString { get; set; }
}

public interface ITrainsStore
{
    Task<int> UpdateTrainTimes(int trainId, int minutes);
}