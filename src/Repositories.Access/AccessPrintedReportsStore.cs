using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    /// <summary>
    /// Gets data from the Access Database.
    /// This is a temporary implementation that will be removed when the
    /// Access database is replaced by a SQL Server database.
    /// </summary>
    public class AccessPrintedReportsStore : IPrintedReportsStore
    {
        public AccessPrintedReportsStore(IOptions<RepositoryOptions> options)
        {
            Options = options.Value;
        }

        private readonly RepositoryOptions Options;
        private OdbcConnection CreateConnection => new(Options.ConnectionString);

        public Task<int?> GetCurrentLayoutId()
        {
            using var connection = CreateConnection;
            var sql = $"SELECT Id FROM Layout WHERE IsCurrent <> 0";
            var command = new OdbcCommand(sql, connection);
            var result = command.ExecuteScalar();
            return Task.FromResult((int?)result);

        }
        public Task<Layout?> GetLayout(int layoutId)
        {
            using var connection = CreateConnection;
            var sql = $"SELECT * FROM Layout WHERE Id = {layoutId}";
            var reader = ExecuteReader(connection, sql);
            if (reader.Read())
            {
                return Task.FromResult(reader.AsLayout());
            }
            return Task.FromResult((Layout?)null);
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
                var data = reader.AsStationDutyData();
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
                reader.AsDutyBooklet(booklet);
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
                yield return reader.AsInstruction("Markdown");
            }
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
                    train = reader.AsTrain();
                    if (currentDutyId != lastDutyId)
                    {
                        lastDutyId = currentDutyId;
                        if (duty != null) result.Add(duty);
                        duty = reader.AsDuty();
                    }
                    duty?.Parts.Add(reader.AsDutyPart(train));
                    lastLocoScheduleTrainId = currentLocoScheduleTrainId;
                }
                train?.Calls.Add(reader.AsStationCall(++sequenceNumber));
            }
            if (duty != null) result.Add(duty);
            return Task.FromResult(result.AsEnumerable());
        }

        public IEnumerable<LayoutVehicle> GetLayoutVehicles(int layoutId)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM LocoAndTrainsetStartReport WHERE LayoutId = {layoutId} ORDER BY StartStationName, DepartureTrack, DepartureTime, Number, OperatingDays, ReplaceOrder");
            while (reader.Read())
            {
                yield return new LayoutVehicle()
                {
                    Id = reader.GetInt("Id"),
                    StartStationName = reader.GetString("StartStationName"),
                    StartTrack = reader.GetString("DepartureTrack"),
                    VehicleScheduleNumber = reader.GetString("Number"),
                    OperatorSignature = reader.GetString("Operator"),
                    OperatingDays = reader.GetString("OperatingDays"),
                    Class = reader.GetString("Class"),
                    VehicleNumber = reader.GetString("VehicleNumber"),
                    DepartureTime = reader.GetString("DepartureTime"),
                    OwnerName = reader.GetString("Owner")
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
                    locoSchedule = reader.AsLocoSchedule();
                }
                if (locoSchedule != null) reader.AddTrainPart(locoSchedule);
                lastUnique = currentUnique;
            }
            if (locoSchedule != null) result.Add(locoSchedule);
            return Task.FromResult(result.AsEnumerable());
        }

        public Task<IEnumerable<VehicleSchedule>> GetTrainsetSchedulesAsync(int layoutId)
        {
            var result = new List<VehicleSchedule>(100);
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM TrainsetTurnusReport WHERE LayoutId = {layoutId} AND PrintSchedule = TRUE ORDER BY TrainsetNumber, TrainsetDays, DepartureTime");
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

                        schedule = isLoadOnly ? reader.AsCargoOnlySchedule() : isCargoWagon ? reader.AsCargoWagonSchedule() : reader.AsPassengerWagonSchedule();
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

        public async Task<IEnumerable<BlockDestinations>> GetBlockDestinationsAsync(int layoutId)
        {
            var result = new List<BlockDestinations>(100);
            var categories = await GetTrainCategories(layoutId);
            {
                using var connection1 = CreateConnection;
                var reader = ExecuteReader(connection1, $"SELECT * FROM TrainBlockDestinations WHERE LayoutId = {layoutId} ORDER BY OriginStationName, TrackDisplayOrder, DepartureTime, TrainNumber, OrderInTrain, TransferDestinationName");
                Read(result, categories, reader);
            }

            return result;

            static void Read(List<BlockDestinations> result, IEnumerable<TrainCategory> categories, IDataReader reader)
            {
                BlockDestinations? current = null;
                var lastOriginStationName = "";
                var lastTrackNumber = "";
                var lastTrainNumber = "";
                while (reader.Read())
                {
                    var currentOriginStationName = reader.GetString("OriginStationName");
                    var currentTrackNumber = currentOriginStationName + reader.GetString("TrackNumber");
                    var currentTrainNumber = currentOriginStationName + reader.GetString("TrainOperatorName") + reader.GetInt("TrainNumber");
                    var isTrainset = reader.GetBool("IsTrainset");
                    if (currentOriginStationName != lastOriginStationName)
                    {
                        if (current is not null) result.Add(current);
                        current = reader.AsBlockDestinations();
                    }
                    if (current is not null)
                    {
                        if (currentTrackNumber != lastTrackNumber)
                        {
                            current.Tracks.Add(reader.AsTrackDestination());
                        }
                        if (currentTrainNumber != lastTrainNumber)
                        {
                            var trainBlocking = reader.AsTrainBlocking();
                            trainBlocking.Train.Prefix = categories.Category(trainBlocking.Train.CategoryResourceCode).Prefix;
                            current.Tracks.Last().TrainBlocks.Add(trainBlocking);
                        }
                        var destination = reader.AsBlockDestination();
                        if (destination.HasCouplingNote) current.Tracks.Last().TrainBlocks.Last().BlockDestinations.Add(destination);
                    }
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
                    current = reader.AsTimetableStretch();

                }
                if (current is not null && currentStationId != lastStationId)
                {
                    current.Stations.Add(reader.AsTimetableStretchStation());
                }
                if (current?.Stations.Any() == true)
                    current?.Stations.Last().Station.Tracks.Add(reader.AsStationStrack());
                lastStationId = currentStationId;
                lastTimetableNumber = currentTimetableNumber;
            }
            if (current != null) { result.Add(current); }

            return Task.FromResult(result.AsEnumerable());

            static string SQL(int layoutId, string? stretchNumber) => string.IsNullOrWhiteSpace(stretchNumber) ?
                $"SELECT * FROM TimetableStretchesReport WHERE LayoutId = {layoutId} ORDER BY SortOrder, StationDisplayOrder, TrackDisplayOrder" :
                $"SELECT * FROM TimetableStretchesReport WHERE LayoutId = {layoutId} AND TimetableNumber = '{stretchNumber}' ORDER BY SortOrder, StationDisplayOrder, TrackDisplayOrder";
        }

        public async Task<IEnumerable<Train>> GetTrainsAsync(int layoutId)
        {
            var result = new List<Train>(100);
            var categories = await GetTrainCategories(layoutId);
            Train? current = null;
            var lastTrainNumber = string.Empty;
            var sequenceNumber = 0;
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM TrainsReport WHERE LayoutId = {layoutId} AND DepartureTime IS NOT NULL ORDER BY TrainOperator, TrainNumber, DepartureTime");
            while (reader.Read())
            {
                var currentTrainNumber = $"{reader.GetString("TrainOperator")}{reader.GetInt("TrainNumber")}";
                if (currentTrainNumber != lastTrainNumber)
                {
                    if (current != null) result.Add(current);
                    sequenceNumber = 0;
                    current = reader.AsTrain();
                    current.Prefix = categories.Category(current.CategoryResourceCode).Prefix;
                }
                current?.Calls.Add(reader.AsStationCall(++sequenceNumber));
                lastTrainNumber = currentTrainNumber;
            }
            if (current != null) result.Add(current);
            return result.AsEnumerable();
        }

        public Task<IEnumerable<StationTrainOrder>> GetStationsTrainOrder(int layoutId)
        {
            var sql = $"SELECT * FROM StationTrainOrder WHERE Id = {layoutId} ORDER BY StationDisplayOrder, SortTime";
            var result = new List<StationTrainOrder>();
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
                    item = reader.AsStationTrainOrder();
                    stationName = currentStationName;
                }
                item?.Trains.Add(reader.AsStationTrain());
            }
            if (item is not null) result.Add(item);
            return Task.FromResult(result.AsEnumerable());
        }


        public async Task<IEnumerable<TrainDeparture>> GetTrainDeparturesAsync(int layoutId, bool onlyItitialTrains = true)
        {
            var sql = $"SELECT * FROM TrainInitialDeparturesReport WHERE LayoutId = {layoutId} ORDER BY StationName, TrackNumber";
            var result = new List<TrainDeparture>(100);
            var categories = await GetTrainCategories(layoutId);
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, sql);
            while (reader.Read())
            {
                var departure = reader.AsTrainDeparture();
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
                result.Add(reader.AsTrainCategory());
            }
            return Task.FromResult(result.AsEnumerable());
        }

        [Obsolete("Waybills are available in MOdule Registry")]
        public Task<IEnumerable<Waybill>> GetWaybillsAsync(int layoutId)
        {
            var result = new List<Waybill>(100);
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM WaybillsReport WHERE LayoutId = {layoutId} ORDER BY ToRegionName, ToStationName");
            while (reader.Read())
            {
                result.Add(WaybillMapper.AsWaybill(reader));
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
                    note = reader.AsManualTrainCallNote();
                }
                else if (note is not null)
                {
                    var localizedNote = reader.AsLocalizedManualTrainCallNote();
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
                    current = reader.AsTrainsetArrivalCallNote();
                }
                current?.AddTrainset(reader.AsTrainset());
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
                    current = reader.AsTrainsetDepartureCallNote();
                }
                current?.AddTrainset(reader.AsTrainset());
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
                    current = reader.AsTrainMeetCallNote();
                }
                current?.MeetingTrains.Add(reader.AsMeetingTrainCall());
                lastCallId = currentCallId;
            }
            if (current != null) yield return current;
        }

        private IEnumerable<TrainContinuationNumberCallNote> GetTrainContinuationNumberCallNotes(int layoutId)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM TrainNumberChangeNotes WHERE LayoutId = {layoutId} ORDER BY CallId");
            while (reader.Read())
            {
                yield return reader.AsTrainContinuationNumberCallNote();
            }
        }

        private IEnumerable<LocoExchangeCallNote> GetLocoExchangeCallNotes(int layoutId)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM LocoExchangeCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId");
            while (reader.Read())
            {
                yield return reader.AsLocoExchangeCallNote();
            }
        }

        private IEnumerable<LocoDepartureCallNote> GetLocoDepartureCallNotes(int layoutId)
        {
            var result = new List<LocoDepartureCallNote>(100);
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM LocoDepartureCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, LocoOperationDaysFlag");
            while (reader.Read())
            {
                result.Add(reader.AsLocoDepartureCallNote());
            }
            return result.AggregateOperationDays();
        }

        private IEnumerable<LocoArrivalCallNote> GetLocoArrivalCallNotes(int layoutId)
        {
            var result = new List<LocoArrivalCallNote>(100);
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM LocoArrivalCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, LocoOperationDaysFlag");
            while (reader.Read())
            {
                var note = reader.AsLocoArrivalCallNote();
                if (note.CirculateLoco && !note.ArrivingLoco.IsRailcar) result.Add(new LocoCirculationNote(note.CallId)
                {
                    ArrivingLoco = note.ArrivingLoco,
                    TrainInfo = note.TrainInfo,
                    CirculateLoco = true

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
                    current = reader.AsBlockDestinationsCallNote();
                }
                var blockDestination = reader.AsBlockDestination();
                current?.BlockDestinations.Add(blockDestination);
                lastCallId = currentCallId;
            }
            if (current != null) yield return current;
        }

        private IEnumerable<BlockArrivalCallNote> GetBlockArrivalCallNotes(int layoutId)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM BlockArrivalCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, TrainsetScheduleId, IsTransfer DESC");
            var lastCallId = 0;
            BlockArrivalCallNote? current = null;
            while (reader.Read())
            {
                var currentCallId = reader.GetInt("CallId");
                if (currentCallId != lastCallId || reader.GetBool("IsTransfer"))
                {
                    if (current is not null) yield return current;
                    current = reader.AsBlockArrivalCallNote();
                }
                if (current is not null)
                {
                    var stationName = reader.GetString("ArrivalStationName");
                    if (!current.StationNames.Contains(stationName))
                        current.StationNames.Add(reader.GetString("ArrivalStationName"));
                    lastCallId = currentCallId;
                }
            }
            if (current != null) yield return current;
        }

        private static IDataReader ExecuteReader(OdbcConnection connection, string sql)
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
}
