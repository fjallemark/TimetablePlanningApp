using Microsoft.Extensions.Options;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.Repositories.Access;

/// <summary>
/// Gets data from the Access Database.
/// This is a temporary implementation that will be removed when the
/// Access database is replaced by a SQL Server database.
/// </summary>
public class AccessPrintedReportsStore(IOptions<RepositoryOptions> options) : IPrintedReportsStore
{
    private readonly RepositoryOptions Options = options.Value;
    private OdbcConnection CreateConnection => new(Options.ConnectionString);

    public async Task<int> UpdateTrainAsync(int trainId, int minutes)
    {
        using var connection = CreateConnection;
        var sql = $"UPDATE TrainStationCall SET ArrivalTime = DATEADD(\'n\', {minutes}, [ArrivalTime]), DepartureTime = DATEADD(\'n\', {minutes}, [DepartureTime]) WHERE IsTrain = {trainId}";
        if (minutes < 0)
        {
            minutes = Math.Abs(minutes);
            sql = $"UPDATE TrainStationCall SET ArrivalTime = DATEADD(\'n\', -{minutes}, [ArrivalTime]), DepartureTime = DATEADD(\'n\', -{minutes}, [DepartureTime]) WHERE IsTrain = {trainId}";
        }
        var command = new OdbcCommand(sql, connection);
        connection.Open();
        return await command.ExecuteNonQueryAsync();
    }

    public Task<Layout?> GetLayout(int layoutId)
    {
        using var connection = CreateConnection;
        var sql = layoutId == 0 ? "SELECT * FROM Layout WHERE IsCurrent <> 0" : $"SELECT * FROM Layout WHERE Id = {layoutId}";
        var reader = ExecuteReader(connection, sql);
        if (reader.Read())
        {
            return Task.FromResult(reader.ToLayout());
        }
        return Task.FromResult((Layout?)null);
    }

    private record DutyToRenumber(int Id, byte OperationDays);
    public ValueTask RenumberDuties(int layoutId)
    {
        var duties = new List<DutyToRenumber>(200);
        using var connection = CreateConnection;
        var sql = $"SELECT * FROM DutyNumbering WHERE LayoutId = {layoutId}";
        var reader = ExecuteReader(connection, sql);
        while (reader.Read())
        {
            var duty = new DutyToRenumber(reader.GetInt("Id"), reader.GetByte("OperatingDays"));
            duties.Add(duty);
        }
        connection.Close();
        using var updateConnection = CreateConnection;
        updateConnection.Open();
        var dutyNumber = 0;
        DutyToRenumber? last = null;
        foreach (var duty in duties)
        {
            if (last is null || last?.OperationDays.IsAllDays()==true || duty.OperationDays.IsAllDays()) dutyNumber++;
            using var command = updateConnection.CreateCommand();
            command.CommandText = $"UPDATE [Duty] SET [Number] = {dutyNumber} WHERE [Id] = {duty.Id};";
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
            last = duty;
        }
        return ValueTask.CompletedTask;
    }

    public Task<StationDutyBooklet?> GetStationDutyBookletAsync(int layoutId) =>
        ReadDutyBooklet<StationDutyBooklet>(layoutId);

    public Task<IEnumerable<StationDutyData>> GetStationDutiesDataAsync(int layoutId)
    {
        var result = new List<StationDutyData>(30);
        using var connection = CreateConnection;
        var sql = $"SELECT * FROM StationDutyBookletReport WHERE LayoutId = {layoutId}";
        var reader = ExecuteReader(connection, sql);
        while (reader.Read())
        {
            var data = reader.ToStationDutyData();
            reader.AddStationInstructions(data);
            result.Add(data);
        }
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<DriverDutyBooklet?> GetDriverDutyBookletAsync(int layoutId) =>
        ReadDutyBooklet<DriverDutyBooklet>(layoutId);

    private Task<T?> ReadDutyBooklet<T>(int layoutId) where T : DutyBooklet
    {
        using var connection = CreateConnection;
        var sql = $"SELECT Name AS LayoutName, ValidFromDate, ValidToDate, StartHour, EndHour FROM Layout WHERE Id = {layoutId}";
        var reader = ExecuteReader(connection, sql);
        DutyBooklet? booklet = null;
        if (reader.Read())
        {
            booklet = Activator.CreateInstance<T>();
            reader.ToDutyBooklet(booklet);
            booklet.Instructions = LayoutInstructions(layoutId);
        }
        return Task.FromResult((T?)booklet);

    }
    private IEnumerable<Instruction> LayoutInstructions(int layoutId)
    {
        using var connection = CreateConnection;
        var sql = $"SELECT * FROM DutyInstructionsReport WHERE LayoutId = {layoutId}";
        var reader = ExecuteReader(connection, sql);
        while (reader.Read())
        {
            yield return reader.ToInstruction("Markdown");
        }
    }

    public Task<IEnumerable<StationInstruction>> GetStationInstructionsAsync(int layoutId)
    {
        var result = new List<StationInstruction>(50);
        using var connection = CreateConnection;
        var sql = $"SELECT * FROM StationInstructionsReport WHERE LayoutId = {layoutId} ORDER BY DisplayOrder";
        var reader = ExecuteReader(connection, sql);
        while (reader.Read())
        {
            result.Add(reader.ToStationInstruction());
        }
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<DriverDuty>> GetDriverDutiesAsync(int layoutId)
    {
        var result = new List<DriverDuty>(100);
        using var connection = CreateConnection;

        var sql = $"SELECT * FROM DutyBookletReport WHERE LayoutId = {layoutId} ORDER BY DutyId, LocoScheduleTrainId, DepartureTime";
        var reader = ExecuteReader(connection, sql);
        var lastDutyId = 0;
        var lastLocoScheduleTrainId = 0;
        int sequenceNumber = 0;
        DriverDuty? duty = null;
        Train? train = null;
        while (reader.Read())
        {
            var currentDutyId = reader.GetInt("DutyId");
            var currentLocoScheduleTrainId = reader.GetInt("LocoScheduleTrainId");
            if (currentDutyId != lastDutyId || currentLocoScheduleTrainId != lastLocoScheduleTrainId)
            {
                sequenceNumber = 0;
                train = reader.ToTrain();
                if (currentDutyId != lastDutyId)
                {
                    lastDutyId = currentDutyId;
                    if (duty != null) result.Add(duty);
                    duty = reader.ToDuty();
                }
                duty?.Parts.Add(reader.ToDutyPart(train));
                //if (duty.Parts.Last().IsReinforcement) Debugger.Break();
                lastLocoScheduleTrainId = currentLocoScheduleTrainId;
            }
            train?.Calls.Add(reader.ToStationCall(++sequenceNumber));
        }
        if (duty != null) result.Add(duty);
        return Task.FromResult(result.AsEnumerable());
    }

    public IEnumerable<LayoutVehicle> GetLayoutVehicles(int layoutId)
    {
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM LocoAndTrainsetStartReport WHERE LayoutId = {layoutId} ORDER BY StartStationName, DepartureTrack, DepartureTime, Number, OperationDaysFlag, ReplaceOrder");
        while (reader.Read())
        {
            yield return new LayoutVehicle()
            {
                Id = reader.GetInt("Id"),
                StartStationName = reader.GetString("StartStationName"),
                StartTrack = reader.GetString("DepartureTrack"),
                VehicleScheduleNumber = reader.GetInt("Number", 0).ToString(),
                OperatorSignature = reader.GetString("Operator"),
                OperatingDays = reader.GetByte("OperationDaysFlag").OperationDays().ShortName,
                Class = reader.GetString("Class"),
                VehicleNumber = reader.GetString("VehicleNumber"),
                DepartureTime = reader.GetString("DepartureTime"),
                OwnerName = reader.GetString("Owner"),
                LocoAddress = reader.GetIntOrNull("Address"),
                MaxNumberOfWagons = reader.GetInt("MaxNumberOfWagons"),
                Note = reader.GetString("Note"),
                VehicleType = reader.GetString("Type")
            };
        }
    }

    public Task<IEnumerable<LocoSchedule>> GetLocoSchedulesAsync(int layoutId)
    {
        var result = new List<LocoSchedule>(100);
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM LocoTurnusReport WHERE LayoutId = {layoutId} ORDER BY LocoOperator, LocoNumber, ReplaceOrder, LocoDays, DepartureTime");
        var lastUnique = "";
        LocoSchedule? locoSchedule = null;
        while (reader.Read())
        {
            var currentUnique = $"{reader.GetString("LocoOperator")}{reader.GetInt("LocoNumber")}{reader.GetString("LocoDays")}{reader.GetInt("ReplaceOrder")}";
            if (currentUnique != lastUnique)
            {
                if (locoSchedule != null) result.Add(locoSchedule);
                locoSchedule = reader.ToLocoSchedule();
            }
            if (locoSchedule != null) reader.AddTrainPart(locoSchedule);
            lastUnique = currentUnique;
        }
        if (locoSchedule != null) result.Add(locoSchedule);
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<ShuntingLoco>> GetShuntingLocosAsync(int layoutId)
    {
        var result = new List<ShuntingLoco>(30);
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM ShuntingLocoReport WHERE LayoutId = {layoutId} ORDER BY HomeStationName");
        while (reader.Read())
        {
            result.Add(reader.ToShuntingLoco());
        }
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<VehicleSchedule>> GetTrainsetSchedulesAsync(int layoutId)
    {
        var result = new List<VehicleSchedule>(100);
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM TrainsetTurnusReport WHERE LayoutId = {layoutId} AND PrintSchedule = TRUE ORDER BY TrainsetOperator, TrainsetNumber, TrainsetDays, DepartureTime");
        var lastUnique = "";
        VehicleSchedule? schedule = null;
        try
        {
            while (reader.Read())
            {
                var currentUnique = $"{reader.GetInt("TrainsetNumber")}{reader.GetString("TrainsetDays")}";
                if (currentUnique != lastUnique)
                {
                    if (schedule != null) result.Add(schedule);
                    var isCargoWagon = reader.GetBool("IsCargo");
                    var isLoadOnly = reader.GetBool("IsLoadOnly");

                    schedule = isLoadOnly ? reader.ToCargoOnlySchedule() : isCargoWagon ? reader.ToCargoWagonSchedule() : reader.ToPassengerWagonSchedule();
                }
                if (schedule != null) reader.AddTrainPart(schedule);
                lastUnique = currentUnique;
            }
            if (schedule != null) result.Add(schedule);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Debugger.Break();
            throw;
        }
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<VehicleStartInfo>> GetVehicleStartInfosAsync(int layoutId)
    {
        var result = new List<VehicleStartInfo>(200);
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM LocoAndTrainsetStartReport WHERE LayoutId = {layoutId} ORDER BY SortOrder, StartStationName, DepartureTrack, DepartureTime");
        while (reader.Read())
        {
            var info = reader.ToVehicleStartInfo();
            result.Add(info);
        }

        return Task.FromResult(result.AsEnumerable());
    }

    public async Task<IEnumerable<BlockDestinations>> GetBlockDestinationsAsync(int layoutId)
    {
        var result = new List<BlockDestinations>(100);
        var categories = await GetTrainCategories(layoutId);
        {
            using var connection1 = CreateConnection;
            var reader = ExecuteReader(connection1, $"SELECT * FROM TrainBlockDestinations WHERE LayoutId = {layoutId} ORDER BY OriginStationName, TrackDisplayOrder, DepartureTime, TrainNumber, OrderInTrain, DisplayOrder, DestinationStationName, TransferDestinationName");
            Read(result, categories, reader);
        }

        return result;

        static void Read(List<BlockDestinations> result, IEnumerable<TrainCategory> categories, IDataReader reader)
        {
            BlockDestinations? current = null;
            var lastOriginStationName = "";
            var lastTrackNumber = "";
            var lastTrainNumber = "";
            var lastPositionInTrain = -1;
            while (reader.Read())

            {
                var currentOriginStationName = reader.GetString("OriginStationName");
                var currentTrackNumber = currentOriginStationName + reader.GetString("TrackNumber");
                var currentTrainNumber = currentOriginStationName + reader.GetString("TrainOperatorName") + reader.GetInt("TrainNumber");
                var currentPositionInTrain = reader.GetInt("OrderInTrain");
                //var isTrainset = reader.GetBool("IsTrainset");
                //var isScheduled = reader.GetBool("IsScheduled");
                if (currentOriginStationName != lastOriginStationName)
                {
                    if (current is not null) result.Add(current);
                    current = reader.ToBlockDestinations();
                }
                if (current is not null)
                {
                    if (currentTrackNumber != lastTrackNumber)
                    {
                        current.Tracks.Add(reader.ToTrackDestination());
                    }
                    if (currentTrainNumber != lastTrainNumber)
                    {
                        var trainBlocking = reader.ToTrainBlocking();
                        trainBlocking.Train.Prefix = categories.Category(trainBlocking.Train.CategoryResourceCode).Prefix;
                        current.Tracks.Last().TrainBlocks.Add(trainBlocking);
                    }
                    var destination = reader.ToBlockDestination();
                    if (destination.HasCouplingNote) current.Tracks.Last().TrainBlocks.Last().BlockDestinations.Add(destination);
                }
                lastPositionInTrain = currentPositionInTrain;
                lastTrainNumber = currentTrainNumber;
                lastTrackNumber = currentTrackNumber;
                lastOriginStationName = currentOriginStationName;
            }
            if (current != null) result.Add(current);
        }
    }

    public Task<IEnumerable<TimetableStretch>> GetTimetableStretchesAsync(int layoutId, string? stretchNumber)
    {
        var result = new List<TimetableStretch>();
        TimetableStretch? current = null;
        var lastTimetableNumber = "";
        var lastStationId = 0;
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, SQL(layoutId, stretchNumber));
        while (reader.Read())
        {
            var currentTimetableNumber = reader.GetString("TimetableNumber");
            var currentStationId = reader.GetInt("StationDisplayOrder");
            if (currentTimetableNumber != lastTimetableNumber)
            {
                if (current is not null) { result.Add(current); lastStationId = 0; }
                current = reader.ToTimetableStretch();

            }
            if (current is not null && currentStationId != lastStationId)
            {
                current.Stations.Add(reader.ToTimetableStretchStation());
            }
            if (current?.Stations.Any() == true)
                current?.Stations.Last().Station.Tracks.Add(reader.ToStationStrack());
            lastStationId = currentStationId;
            lastTimetableNumber = currentTimetableNumber;
        }
        if (current != null) { result.Add(current); }

        return Task.FromResult(result.AsEnumerable());

        static string SQL(int layoutId, string? stretchNumber) => string.IsNullOrWhiteSpace(stretchNumber) ?
            $"SELECT * FROM TimetableStretchesReport WHERE LayoutId = {layoutId} ORDER BY SortOrder, StationDisplayOrder, TrackDisplayOrder" :
            $"SELECT * FROM TimetableStretchesReport WHERE LayoutId = {layoutId} AND TimetableNumber = '{stretchNumber}' ORDER BY SortOrder, StationDisplayOrder, TrackDisplayOrder";
    }

    public async Task<IEnumerable<Train>> GetTrainsAsync(int layoutId, string? operatorSignature = null)
    {
        var result = new List<Train>(100);
        var categories = await GetTrainCategories(layoutId);
        Train? current = null;
        var lastTrainNumber = string.Empty;
        var sequenceNumber = 0;
        using var connection = CreateConnection;
        var sql = operatorSignature.HasValue() ?
            $"SELECT * FROM TrainsReport WHERE LayoutId = {layoutId} AND TrainOperator = '{operatorSignature}' AND DepartureTime IS NOT NULL ORDER BY TrainOperator, TrainNumber, DepartureTime" :
            $"SELECT * FROM TrainsReport WHERE LayoutId = {layoutId} AND DepartureTime IS NOT NULL ORDER BY TrainOperator, TrainNumber, DepartureTime";

        var reader = ExecuteReader(connection, sql);
        while (reader.Read())
        {
            var currentTrainNumber = $"{reader.GetString("TrainOperator")}{reader.GetInt("TrainNumber")}";
            if (currentTrainNumber != lastTrainNumber)
            {
                if (current != null) result.Add(current);
                sequenceNumber = 0;
                current = reader.ToTrain();
                current.Prefix = categories.Category(current.CategoryResourceCode).Prefix;
            }
            current?.Calls.Add(reader.ToStationCall(++sequenceNumber));
            lastTrainNumber = currentTrainNumber;
        }
        if (current != null) result.Add(current);
        return result.AsEnumerable();
    }

    public async Task<IEnumerable<StationTrainOrder>> GetStationsTrainOrder(int layoutId)
    {
        var sql = $"SELECT * FROM StationTrainOrder WHERE LayoutId = {layoutId} ORDER BY StationDisplayOrder, SortTime, IsArrival";
        var result = new List<StationTrainOrder>();
        var categories = await GetTrainCategories(layoutId);
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, sql);
        var stationName = "";
        StationTrainOrder? item = null;
        while (reader.Read())
        {
            var currentStationName = reader.GetString("FullName");
            if (stationName != currentStationName)
            {
                if (item is not null) result.Add(item);
                item = reader.ToStationTrainOrder();
                stationName = currentStationName;
            }
            var stationTrain = reader.ToStationTrain(categories);

            item?.Trains.Add(stationTrain);
        }
        if (item is not null) result.Add(item);
        return result.AsEnumerable();
    }


    public async Task<IEnumerable<TrainDeparture>> GetTrainDeparturesAsync(int layoutId, bool onlyInitialTrains = true)
    {
        var sql = onlyInitialTrains ? $"SELECT * FROM TrainStartReport WHERE LayoutId = {layoutId} AND IsInitial <> 0 ORDER BY StationName, TrackNumber" :
        $"SELECT * FROM TrainStartReport WHERE LayoutId = {layoutId} ORDER BY StationName, TrackNumber";
        var result = new List<TrainDeparture>(100);
        var categories = await GetTrainCategories(layoutId);
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, sql);
        while (reader.Read())
        {
            var departure = reader.ToTrainDeparture();
            departure.Train.Prefix = categories.Category(departure.Train.CategoryResourceCode).Prefix;
            result.Add(departure);
        }
        return result.AsEnumerable();
    }

    public Task<IEnumerable<TrainCategory>> GetTrainCategories(int layoutId)
    {
        var result = new List<TrainCategory>(20);
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM HistoricalTrainCategory WHERE LayoutId = {layoutId}");

        while (reader.Read())
        {
            result.Add(reader.ToTrainCategory());
        }
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<TrainCallNote>> GetTrainCallNotesAsync(int layoutId)
    {
        var result = new List<TrainCallNote>(500);
        result.AddRange(GetManualTrainStationCallNotes(layoutId));
        result.AddRange(GetTrainsetsDepartureCallNotes(layoutId));
        result.AddRange(GetTrainsetsArrivalCallNotes(layoutId));
        result.AddRange(GetTrainContinuationNumberCallNotes(layoutId));
        result.AddRange(GetTrainMeetCallNotes(layoutId));
        result.AddRange(GetLocoExchangeCallNotes(layoutId));
        result.AddRange(GetLocoDepartureCallNotes(layoutId));
        result.AddRange(GetLocoArrivalCallNotes(layoutId));
        result.AddRange(GetBlockDestinationsCallNotes(layoutId));
        result.AddRange(GetBlockArrivalCallNotes(layoutId));
        result.AddRange(GetLocoTurnOrReverseCallNotes(layoutId));
        result.AddRange(GetBlockOriginCallNotes(layoutId));
        return Task.FromResult(result.AsEnumerable());
    }

    private IEnumerable<ManualTrainCallNote> GetManualTrainStationCallNotes(int layoutId)
    {
        ManualTrainCallNote? note = null;
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM ManualTrainStationCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, ParentId, Row");
        var lastId = 0;
        while (reader.Read())
        {
            var noteId = reader.GetInt("NoteId");
            if (noteId > 0)
            {
                if (note is not null && noteId != lastId) yield return note;
                note = reader.ToManualTrainCallNote();
            }
            else if (note is not null)
            {
                var localizedNote = reader.ToLocalizedManualTrainCallNote();
                note.AddLocalizedManualTrainCallNote(localizedNote);
            }
            if (noteId > 0) lastId = noteId;
        }
        if (note is not null) yield return note;
    }

    private IEnumerable<TrainsetsCallNote> GetTrainsetsArrivalCallNotes(int layoutId)
    {
        using var connection = CreateConnection;
        var sql = $"SELECT * FROM TrainsetsArrivalNotes WHERE LayoutId = {layoutId} ORDER BY CallId, IsLoadOnly, OrderInTrain";
        var reader = ExecuteReader(connection, sql);
        string lastCallKey = string.Empty;
        TrainsetsCallNote? current = null;
        while (reader.Read())
        {
            var currentCallKey = $"{reader.GetInt("CallId")}{reader.GetBool("IsLoadOnly")}";
            if (currentCallKey != lastCallKey)
            {
                if (current != null) yield return current;
                current = reader.ToTrainsetArrivalCallNote();
            }
            current?.AddTrainset(reader.ToTrainset());
            lastCallKey = currentCallKey;
        }
        if (current != null) yield return current;
    }

    private IEnumerable<TrainsetsCallNote> GetTrainsetsDepartureCallNotes(int layoutId)
    {
        using var connection = CreateConnection;
        var sql = $"SELECT * FROM TrainsetsDepartureNotes WHERE LayoutId = {layoutId} ORDER BY CallId, IsLoadOnly, OrderInTrain";
        var reader = ExecuteReader(connection, sql);
        string lastCallKey = string.Empty;
        TrainsetsCallNote? current = null;
        while (reader.Read())
        {
            var currentCallKey = $"{reader.GetInt("CallId")}{reader.GetBool("IsLoadOnly")}";
            if (currentCallKey != lastCallKey)
            {
                if (current != null) yield return current;
                current = reader.ToTrainsetDepartureCallNote();
            }
            current?.AddTrainset(reader.ToTrainset());
            lastCallKey = currentCallKey;
        }
        if (current != null) yield return current;
    }

    private IEnumerable<TrainMeetCallNote> GetTrainMeetCallNotes(int layoutId)
    {
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM TrainMeetCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId");
        var lastCallId = 0;
        TrainMeetCallNote? current = null;
        while (reader.Read())
        {
            var currentCallId = reader.GetInt("CallId");
            if (currentCallId != lastCallId)
            {
                if (current != null) yield return current;
                current = reader.ToTrainMeetCallNote();
            }
            current?.MeetingTrains.Add(reader.ToMeetingTrainCall());
            lastCallId = currentCallId;
        }
        if (current != null) yield return current;
    }

    private IEnumerable<TrainContinuationNumberCallNote> GetTrainContinuationNumberCallNotes(int layoutId)
    {
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM TrainNumberChangeNotes WHERE LayoutId = {layoutId} ORDER BY CallId");
        TrainContinuationNumberCallNote? last = null;
        while (reader.Read())
        {
            var current = reader.ToTrainContinuationNumberCallNote();
            var dayFlag = current.LocoOperationDaysFlag;
            if (current.CallId == last?.CallId)
            {
                current.LocoOperationDaysFlag |= last.LocoOperationDaysFlag;
                last = current;
            }
            else
            {
                last = current;
                yield return last;
            }
        }
    }

    private IEnumerable<LocoExchangeCallNote> GetLocoExchangeCallNotes(int layoutId)
    {
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM LocoExchangeCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId");
        while (reader.Read())
        {
            yield return reader.ToLocoExchangeCallNote();
        }
    }

    private IEnumerable<LocoDepartureCallNote> GetLocoDepartureCallNotes(int layoutId)
    {
        var result = new List<LocoDepartureCallNote>(100);
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM LocoDepartureCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, LocoOperationDaysFlag");
        while (reader.Read())
        {
            var note = reader.ToLocoDepartureCallNote();
            if (ShouldAdd(note)) result.Add(note);
        }
        return result.AggregateOperationDays();

        static bool ShouldAdd(LocoDepartureCallNote note) => note.IsFromParking || note.UseNote;
    }

    private IEnumerable<LocoArrivalCallNote> GetLocoArrivalCallNotes(int layoutId)
    {
        var result = new List<LocoArrivalCallNote>(100);
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM LocoArrivalCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, LocoOperationDaysFlag");
        while (reader.Read())
        {

            var note = reader.ToLocoArrivalCallNote();
            if (note.CirculateLoco && !note.ArrivingLoco.IsRailcar) result.Add(new LocoCirculationNote(note.CallId)
            {
                ArrivingLoco = note.ArrivingLoco,
                TrainInfo = note.TrainInfo,
                CirculateLoco = true,
            });
            if (note.TurnLoco && !note.ArrivingLoco.IsRailcar) result.Add(new LocoTurnNote(note.CallId)
            {
                ArrivingLoco = note.ArrivingLoco,
                TrainInfo = note.TrainInfo,
                TurnLoco = true
            });
            if (note.IsToParking) result.Add(note);
        }
        return result.AggregateOperationDays();
    }

    private IEnumerable<BlockOriginCallNote> GetBlockOriginCallNotes(int layoutId)
    {
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM BlockOriginCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, OriginName");
        var lastCallId = 0;
        BlockOriginCallNote? current = null;
        while (reader.Read())
        {
            var currentCallId = reader.GetInt("CallId");
            string originName = reader.GetString("OriginName");
            if (currentCallId != lastCallId)
            {
                if (current != null) yield return current;
                current = new BlockOriginCallNote(currentCallId);
            }
            current?.AddOriginName(originName);
            lastCallId = currentCallId;
        }
        if (current != null) yield return current;
    }

    private IEnumerable<BlockDestinationsCallNote> GetBlockDestinationsCallNotes(int layoutId)
    {
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT DISTINCT * FROM BlockDestinationsCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, TrainsetScheduleId, DestinationStationName, OrderInTrain DESC");
        var lastCallId = 0;
        BlockDestinationsCallNote? current = null;
        while (reader.Read())
        {
            var currentCallId = reader.GetInt("CallId");
            var maxNumberOfWagons = reader.GetInt("MaxNumberOfWagons");
            if (currentCallId != lastCallId)
            {
                if (current != null) yield return current;
                current = reader.ToBlockDestinationsCallNote();
            }
            var blockDestination = reader.ToBlockDestination();
            current?.BlockDestinations.Add(blockDestination);
            lastCallId = currentCallId;
        }
        if (current != null) yield return current;
    }

    private IEnumerable<BlockArrivalCallNote> GetBlockArrivalCallNotes(int layoutId)
    {
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM BlockArrivalCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, TrainsetScheduleId, IsTransfer DESC");
        BlockArrivalCallNote? current = null;
        string? lastKey = null;
        while (reader.Read())
        {
            var currentKey = $"{reader.GetInt("CallId")}{reader.GetBool("AlsoSwitch")}";
            if (currentKey != lastKey)
            {
                if (current is not null) yield return current;
                current = reader.ToBlockArrivalCallNote();
            }
            if (current is not null)
            {
                var stationName = reader.GetString("ArrivalStationName");
                if (!current.StationNames.Contains(stationName))
                    current.StationNames.Add(reader.GetString("ArrivalStationName"));
                lastKey = currentKey;
            }
        }
        if (current != null) yield return current;
    }

    private IEnumerable<LocoTurnOrReverseCallNote> GetLocoTurnOrReverseCallNotes(int layoutId)
    {
        using var connection = CreateConnection;
        var reader = ExecuteReader(connection, $"SELECT * FROM ReverseOrTurnLocoCallNote WHERE LayoutId = {layoutId}");
        while (reader.Read()) { yield return reader.ToLocoReverseOrTurnCallNote(); }
    }

    private static OdbcDataReader ExecuteReader(OdbcConnection connection, string sql)
    {
        var command = new OdbcCommand(sql, connection);
        try
        {
            connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);

        }
        catch (Exception ex)
        {
            _ = ex.Message;
            Debugger.Break();
            throw;
        }
    }

}
