using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using Tellurian.Trains.Planning.App.Shared;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    public class AccessRepository : IRepository
    {
        public AccessRepository(IOptions<RepositoryOptions> options)
        {
            Options = options.Value;
        }

        private readonly RepositoryOptions Options;
        private OdbcConnection CreateConnection => new OdbcConnection(Options.ConnectionString);

        public DriverDutyBooklet? GetDriverDutyBooklet(string scheduleName)
        {
            var duties = GetDriverDuties(scheduleName);
            if (duties is null) return null;
            foreach (var d in duties)
            {
                d.Parts = d.Parts.OrderBy(p => p.Calls().First().Departure?.Time).ToArray();
            }
            return new DriverDutyBooklet
            {
                ScheduleName = scheduleName,
                Duties = duties.OrderBy(d => d.Number).ToArray()
            };
        }

        private IEnumerable<DriverDuty>? GetDriverDuties(string scheduleName)
        {
            using var connection = CreateConnection;
            var sql = $"SELECT * FROM DutyBookletReport WHERE LayoutName = '{scheduleName}' ORDER BY DutyId, LocoScheduleTrainId, DepartureTime";
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
                        if (duty != null) yield return duty;
                        duty = reader.AsDuty();
                    }
                    duty?.Parts.Add(reader.AsDutyPart(train));
                    lastLocoScheduleTrainId = currentLocoScheduleTrainId;
                }
                train?.Calls.Add(reader.AsStationCall(++sequenceNumber));
            }
            if (duty != null) yield return duty;
        }

        public IEnumerable<Waybill>? GetWaybills(string? timetableName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, "SELECT * FROM WaybillReport ORDER BY ToRegionName, ToStationName");
            while (reader.Read())
            {
                yield return WaybillMapper.AsWaybill(reader);
            }
        }

        public IEnumerable<VehicleSchedule>? GetLocoSchedules(string scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, "SELECT * FROM LocoTurnusReport ORDER BY LocoNumber, LocoDays, DepartureTime");
            var lastUnique = "";
            LocoSchedule? locoSchedule = null;
            while (reader.Read())
            {
                var currentUnique = $"{reader.GetInt16("LocoNumber")}{reader.GetString("LocoDays")}";
                if (currentUnique != lastUnique)
                {
                    if (locoSchedule != null) yield return locoSchedule;
                    locoSchedule = reader.AsLocoSchedule();
                }
                if (locoSchedule != null) reader.AddTrainPart(locoSchedule);
                lastUnique = currentUnique;
            }
            if (locoSchedule != null) yield return locoSchedule;
        }

        public IEnumerable<VehicleSchedule>? GetTrainsetSchedules(string scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, "SELECT * FROM TrainsetTurnusReport WHERE PrintSchedule = TRUE ORDER BY TrainsetNumber, TrainsetDays, DepartureTime");
            var lastUnique = "";
            TrainsetSchedule? schedule = null;
            while (reader.Read())
            {
                var currentUnique = $"{reader.GetInt16("TrainsetNumber")}{reader.GetString("TrainsetDays")}";
                if (currentUnique != lastUnique)
                {
                    if (schedule != null) yield return schedule;
                    schedule = reader.AsTrainsetSchedule();
                }
                if (schedule != null) reader.AddTrainPart(schedule);
                lastUnique = currentUnique;
            }
            if (schedule != null) yield return schedule;
        }

        public IEnumerable<TrainCallNote> GetTrainCallNotes(string scheduleName)
        {
            foreach (var n in GetManualTrainStationCallNotes(scheduleName)) yield return n;
            foreach (var n in GetDepartureTrainsetsCallNotes(scheduleName)) yield return n;
            foreach (var n in GetArrivalTrainsetsCallNotes(scheduleName)) yield return n;
            foreach (var n in GetTrainContinuationNumberCallNotes(scheduleName)) yield return n;
            foreach (var n in GetLocoExchangeCallNotes(scheduleName)) yield return n;
            foreach (var n in GetLocoDepartureCallNotes(scheduleName)) yield return n;
            foreach (var n in GetLocoArrivalCallNotes(scheduleName)) yield return n;
            foreach (var n in GetBlockDestinationsCallNotes(scheduleName)) yield return n;
            foreach (var n in GetBlockArrivalCallNotes(scheduleName)) yield return n;
        }

        public IEnumerable<ManualTrainCallNote> GetManualTrainStationCallNotes(string scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM ManualTrainStationCallNotes WHERE LayoutName = '{scheduleName}' ORDER BY CallId, Row");
            while (reader.Read()) {
                yield return reader.AsManualTrainCallNote();
            }
        }

        public IEnumerable<TrainsetsCallNote> GetDepartureTrainsetsCallNotes(string scheduleName)
        {
            var sql= $"SELECT * FROM TrainsetsDepartureNotes WHERE LayoutName = '{scheduleName}' ORDER BY CallId, OrderInTrain";
            return GetTrainsetsCallNotes(sql, true, false);
        }
        public IEnumerable<TrainsetsCallNote> GetArrivalTrainsetsCallNotes(string scheduleName)
        {
            var sql = $"SELECT * FROM TrainsetsArrivalNotes WHERE LayoutName = '{scheduleName}' ORDER BY CallId, OrderInTrain";
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

        public IEnumerable<TrainContinuationNumberCallNote> GetTrainContinuationNumberCallNotes(string scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM TrainNumberChangeNotes WHERE LayoutName = '{scheduleName}' ORDER BY CallId");
            while (reader.Read())
            {
                yield return reader.AsTrainContinuationNumberCallNote();
            }
        }

        public IEnumerable<LocoExchangeCallNote> GetLocoExchangeCallNotes(string scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM LocoExchangeCallNotes WHERE LayoutName = '{scheduleName}' ORDER BY CallId");
            while (reader.Read())
            {
                yield return reader.AsLocoExchangeCallNote();
            }
        }

        public IEnumerable<LocoDepartureCallNote> GetLocoDepartureCallNotes(string scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM LocoDepartureCallNotes WHERE LayoutName = '{scheduleName}' ORDER BY CallId, LocoOperationDaysFlag");
            while (reader.Read())
            {
                yield return reader.AsLocoDepartureCallNote();
            }
        }

        public IEnumerable<LocoArrivalCallNote> GetLocoArrivalCallNotes(string scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM LocoArrivalCallNotes WHERE LayoutName = '{scheduleName}' ORDER BY CallId, LocoOperationDaysFlag");
            while (reader.Read())
            {
                yield return reader.AsLocoArrivalCallNote();
            }
        }

        internal IEnumerable<BlockDestinationsCallNote> GetBlockDestinationsCallNotes(string scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM BlockDestinationsCallNotes WHERE LayoutName = '{scheduleName}' ORDER BY CallId, TrainsetScheduleId, OrderInTrain DESC");
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
                current?.BlockDestinations.Add(reader.AsBlockDestination());
                lastCallId = currentCallId;
            }
            if (current != null) yield return current;
        }

        internal IEnumerable<BlockArrivalCallNote> GetBlockArrivalCallNotes(string scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, $"SELECT * FROM BlockArrivalCallNotes WHERE LayoutName = '{scheduleName}' ORDER BY CallId, TrainsetScheduleId, OrderInTrain DESC");
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
