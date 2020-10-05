using Microsoft.Extensions.Options;
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
                var currentDutyId = reader.GetInt32("DutyId");
                var currentLocoScheduleTrainId = reader.GetInt32("LocoScheduleTrainId");
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
                var currentUnique = $"{reader.GetInt16("LocoNumber")}{reader.GetString("LocoDays")}";
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
                var currentUnique = $"{reader.GetInt16("TrainsetNumber")}{reader.GetString("TrainsetDays")}";
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

        public Task<IEnumerable<BlockDestinations>> GetBlockDestinations(int layoutId)
        {
            var result = new List<BlockDestinations>(100);
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM ShadowStationBlockDestinations WHERE LayoutId = {layoutId} ORDER BY OriginStationName, TrackDisplayOrder, OrderInTrain");
            BlockDestinations? current = null;
            var lastOriginStationName = "";
            var lastTrackNumber = "";
            while (reader.Read())
            {
                var currentOriginStationName = reader.GetString("OriginStationName");
                var currentTrackNumber = reader.GetString("TrackNumber");
                if (currentOriginStationName != lastOriginStationName)
                {
                    if (current != null) result.Add(current);
                    current = reader.AsBlockDestinations();
                }
                if (current != null && currentTrackNumber != lastTrackNumber)
                {
                    current.TrackDestinations.Add(reader.AsTrackDestination());
                }
                if (current != null)
                {
                    current.TrackDestinations.Last().BlockDestinations.Add(reader.AsBlockDestination());
                }
                lastTrackNumber = currentTrackNumber;
                lastOriginStationName = currentOriginStationName;
            }
            if (current != null) result.Add(current);
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
                var currentCallId = reader.GetInt32("CallId");
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
                var currentCallId = reader.GetInt32("CallId");
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
                var currentCallId = reader.GetInt32("CallId");
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
