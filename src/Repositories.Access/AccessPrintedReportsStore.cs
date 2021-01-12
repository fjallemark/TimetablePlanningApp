using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contract;

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
        private OdbcConnection CreateConnection => new OdbcConnection(Options.ConnectionString);

        public Task<DriverDutyBooklet?> GetDriverDutyBookletAsync(int layoutId)
        {
            using var connection = CreateConnection;
            var sql = $"SELECT * FROM DutyInstructionsReport WHERE LayoutId = {layoutId}";
            var reader = ExecuteReader(connection, sql);
            DriverDutyBooklet? booklet = null;
            while (reader.Read())
            {
                if (booklet is null)
                {
                    booklet = reader.AsDriverDutyBooklet();
                    booklet.Instructions.Add(reader.AsLayoutInstruction());
                }
                else
                {
                    booklet.Instructions.Add(reader.AsLayoutInstruction());
                }
            }
            return Task.FromResult(booklet);
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
                if (currentLocoScheduleTrainId != lastLocoScheduleTrainId)
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

        public Task<IEnumerable<LocoSchedule>> GetLocoSchedulesAsync(int layoutId)
        {
            var result = new List<LocoSchedule>(100);
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM LocoTurnusReport WHERE LayoutId = {layoutId} ORDER BY LocoNumber, LocoDays, DepartureTime");
            var lastUnique = "";
            LocoSchedule? locoSchedule = null;
            while (reader.Read())
            {
                var currentUnique = $"{reader.GetInt("LocoNumber")}{reader.GetString("LocoDays")}";
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

        public Task<IEnumerable<TrainsetSchedule>> GetTrainsetSchedulesAsync(int layoutId)
        {
            var result = new List<TrainsetSchedule>(100);
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM TrainsetTurnusReport WHERE LayoutId = {layoutId} AND PrintSchedule = TRUE ORDER BY TrainsetNumber, TrainsetDays, DepartureTime");
            var lastUnique = "";
            TrainsetSchedule? schedule = null;
            while (reader.Read())
            {
                var currentUnique = $"{reader.GetInt("TrainsetNumber")}{reader.GetString("TrainsetDays")}";
                if (currentUnique != lastUnique)
                {
                    if (schedule != null) result.Add(schedule);
                    schedule = reader.AsTrainsetSchedule();
                }
                if (schedule != null) reader.AddTrainPart(schedule);
                lastUnique = currentUnique;
            }
            if (schedule != null) result.Add(schedule);
            return Task.FromResult(result.AsEnumerable());
        }

        public Task<IEnumerable<BlockDestinations>> GetBlockDestinationsAsync(int layoutId)
        {
            var result = new List<BlockDestinations>(100);
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM TrainBlockDestinations WHERE LayoutId = {layoutId} ORDER BY OriginStationName, TrackDisplayOrder, TrainNumber, OrderInTrain");
            BlockDestinations? current = null;
            var lastOriginStationName = "";
            var lastTrackNumber = "";
            var lastTrainNumber = "";
            while (reader.Read())
            {
                var currentOriginStationName = reader.GetString("OriginStationName");
                var currentTrackNumber = currentOriginStationName + reader.GetString("TrackNumber");
                var currentTrainNumber = currentOriginStationName + reader.GetInt("TrainNumber");
                if (currentOriginStationName != lastOriginStationName)
                {
                    if (current != null) result.Add(current);
                    current = reader.AsBlockDestinations();
                }
                if (current != null && currentTrackNumber != lastTrackNumber)
                {
                    current.Tracks.Add(reader.AsTrackDestination());
                }
                if(current != null && currentTrainNumber != lastTrainNumber)
                {
                    current.Tracks.Last().TrainBlocks.Add(reader.AsTrainBlocking());
                }
                if (current != null)
                {
                    current.Tracks.Last().TrainBlocks.Last().BlockDestinations.Add(reader.AsBlockDestination());
                }
                lastTrainNumber = currentTrainNumber;
                lastTrackNumber = currentTrackNumber;
                lastOriginStationName = currentOriginStationName;
            }
            if (current != null) result.Add(current);
            return Task.FromResult(result.AsEnumerable());
        }

        public Task<IEnumerable<TimetableStretch>> GetTimetableStretchesAsync(int layoutId)
        {
            var result = new List<TimetableStretch>();
            TimetableStretch? current = null;
            var lastTimetableNumber = "";
            var lastStationId = 0;
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM TimetableStretchesReport WHERE LayoutId = {layoutId} ORDER BY TimetableNumber, StationDisplayOrder, TrackDisplayOrder");
            while (reader.Read())
            {
                var currentTimetableNumber = reader.GetString("TimetableNumber");
                var currentStationId = reader.GetInt("StationDisplayOrder");
                if (currentTimetableNumber != lastTimetableNumber)
                {
                    if (current != null) { result.Add(current); }
                    current = reader.AsTimetableStretch();
                }
                if (current != null && currentStationId != lastStationId)
                {
                    current.Stations.Add(reader.AsTimetableStretchStation());
                }
                if (current != null)
                {
                    current.Stations.Last().Station.Tracks.Add(reader.AsStationStrack());
                }
                lastStationId = currentStationId;
                lastTimetableNumber = currentTimetableNumber;
            }
            if (current != null) { result.Add(current); }

            return Task.FromResult(result.AsEnumerable());
        }

        public Task<IEnumerable<Train>> GetTrainsAsync(int layoutId)
        {
            var result = new List<Train>(100);
            Train? current = null;
            var lastTrainNumber = 0;
            var sequenceNumber = 0;
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM TrainsReport WHERE LayoutId = {layoutId} ORDER BY TrainNumber, DepartureTime");
            while (reader.Read())
            {
                var currentTrainNumber = reader.GetInt("TrainNumber");
                if (currentTrainNumber != lastTrainNumber)
                {
                    if (current != null) result.Add(current);
                    sequenceNumber = 0;
                    current = reader.AsTrain();
                }
                if (current != null) current.Calls.Add(reader.AsStationCall(++sequenceNumber));
                lastTrainNumber = currentTrainNumber;
            }
            if (current != null) result.Add(current);
            return Task.FromResult(result.AsEnumerable());
        }

        public Task<IEnumerable<TrainDeparture>> GetTrainDeparturesAsync(int layoutId)
        {
            var result = new List<TrainDeparture>(100);
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM TrainInitialDeparturesReport WHERE LayoutId = {layoutId} ORDER BY StationName, TrackNumber");
            while (reader.Read())
            {
                result.Add(reader.AsTrainDeparture());
            }
            return Task.FromResult(result.AsEnumerable());
        }

        public Task<IEnumerable<TrainCallNote>> GetTrainCallNotesAsync(int layoutId)
        {
            var result = new List<TrainCallNote>(500);
            result.AddRange(GetManualTrainStationCallNotes(layoutId));
            result.AddRange(GetDepartureTrainsetsCallNotes(layoutId));
            result.AddRange(GetArrivalTrainsetsCallNotes(layoutId));
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
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM ManualTrainStationCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, Row");
            while (reader.Read())
            {
                yield return reader.AsManualTrainCallNote();
            }
        }

        private IEnumerable<TrainsetsCallNote> GetDepartureTrainsetsCallNotes(int layoutId)
        {
            var sql = $"SELECT * FROM TrainsetsDepartureNotes WHERE LayoutId = {layoutId} ORDER BY CallId, OrderInTrain";
            return GetTrainsetsCallNotes(sql, true, false);
        }
        private IEnumerable<TrainsetsCallNote> GetArrivalTrainsetsCallNotes(int layoutId)
        {
            var sql = $"SELECT * FROM TrainsetsArrivalNotes WHERE LayoutId = {layoutId} ORDER BY CallId, OrderInTrain";
            return GetTrainsetsCallNotes(sql, false, true);
        }

        private IEnumerable<TrainsetsCallNote> GetTrainsetsCallNotes(string sql, bool isForDeparture, bool isForArrival)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, sql);
            var lastCallId = 0;
            TrainsetsCallNote? current = null;
            while (reader.Read())
            {
                var currentCallId = reader.GetInt("CallId");
                if (currentCallId != lastCallId)
                {
                    if (current != null) yield return current;
                    current = reader.AsTrainsetsCallNote(isForDeparture, isForArrival);
                }
                current?.AddTrainset(reader.AsTrainset());
                lastCallId = currentCallId;
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
                current?.MeetingTrains.Add(reader.AsMeetingTrain());
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
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM LocoDepartureCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, LocoOperationDaysFlag");
            while (reader.Read())
            {
                yield return reader.AsLocoDepartureCallNote();
            }
        }

        private IEnumerable<LocoArrivalCallNote> GetLocoArrivalCallNotes(int layoutId)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM LocoArrivalCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, LocoOperationDaysFlag");
            while (reader.Read())
            {
                yield return reader.AsLocoArrivalCallNote();
            }
        }

        private IEnumerable<BlockDestinationsCallNote> GetBlockDestinationsCallNotes(int layoutId)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM BlockDestinationsCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, TrainsetScheduleId, OrderInTrain DESC");
            var lastCallId = 0;
            BlockDestinationsCallNote? current = null;
            while (reader.Read())
            {
                var currentCallId = reader.GetInt("CallId");
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
            var reader = ExecuteReader(connection, $"SELECT * FROM BlockArrivalCallNotes WHERE LayoutId = {layoutId} ORDER BY CallId, TrainsetScheduleId, OrderInTrain DESC");
            while (reader.Read())
            {
                yield return reader.AsBlockArrivalCallNote();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "n/a")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Is taken care in other place.")]
        private static IDataReader ExecuteReader(OdbcConnection connection, string sql)
        {
            var command = new OdbcCommand(sql, connection);
            connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}
