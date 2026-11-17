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
    Task<IEnumerable<ShuntingLoco>> GetShuntingLocosAsync(int layoutId);
    Task<StationDutyBooklet?> GetStationDutyBookletAsync(int layoutId);
    Task<IEnumerable<StationDutyData>> GetStationDutiesDataAsync(int layoutId, string? countryCode = null);
    Task<IEnumerable<TimetableStretch>> GetTimetableStretchesAsync(int layoutId, string? stretchNumber);
    Task<int> UpdateTrainTimesAsync(int trainId, int minutes);
    Task<IEnumerable<Train>> GetTrainsAsync(int layoutId, string? operatorSignature = null, int? id = null);
    Task<IEnumerable<VehicleSchedule>> GetTrainsetSchedulesAsync(int layoutId);
    Task<IEnumerable<TrainDeparture>> GetTrainDeparturesAsync(int layoutId, bool onlyItitialTrains = false);
    Task<IEnumerable<TrainCallNote>> GetTrainCallNotesAsync(int layoutId, bool forStations = false);
    Task<Layout?> GetLayout(int layoutId);
    Task<IEnumerable<TrainCategory>> GetTrainCategoriesAsync(int layoutId);
    Task<IEnumerable<StationTrainOrder>> GetStationsTrainOrder(int layoutId, string? countryCode = null);
    IEnumerable<LayoutVehicle> GetLayoutVehicles(int layoutId);
    Task<IEnumerable<VehicleStartInfo>> GetVehicleStartInfosAsync(int layoutId);
    Task<IEnumerable<StationInstruction>> GetStationInstructionsAsync(int layoutId);
    Task<IEnumerable<VehicleSchedule>> GetTrainsetWagonCardsAsync(int layoutId);
    Task<IEnumerable<TrainComposition>> GetTrainCompositionsAsync(int layoutId, string? operatorSignature = null);
    Task<IEnumerable<AdjacentDispatchPlace>> GetAdjacentDispatchPlacesAsync(int layoutId);
    Task<int> CloneTrainAsync(int trainId, int minutes);
    ValueTask RenumberDuties(int layoutId);

}

public class RepositoryOptions
{
    public string? ConnectionString { get; set; }
}


