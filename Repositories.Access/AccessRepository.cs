using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Net;
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

        public IEnumerable<Waybill> GetWaybills(string? timetableName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, "SELECT * FROM WaybillReport ORDER BY ToRegionName, ToStationName");
            while (reader.Read())
            {
                yield return WaybillMapper.AsWaybill(reader);
            }
        }

        public IEnumerable<VehicleSchedule> GetLocoSchedules(string? scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, "SELECT * FROM LocoTurnusReport ORDER BY LocoNumber, OperatingDayShortName, DepartureTime");
            var lastUnique = "";
            LocoSchedule? locoSchedule = null;
            while (reader.Read())
            {
                var currentUnique = string.Format("{0}{1}", reader.GetInt16("LocoNumber"), reader.GetString("OperatingDayShortName"));
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

        public IEnumerable<VehicleSchedule> GetTrainsetSchedules(string? scheduleName)
        {
            using var connection = CreateConnection;
            var reader = ExecuteReader(connection, "SELECT * FROM TrainsetReport WHERE PrintSchedule = TRUE ORDER BY Number, OperatingDayShortName, DepartureTime");
            var lastUnique = "";
            TrainsetSchedule? schedule = null;
            while (reader.Read())
            {
                var currentUnique = string.Format("{0}{1}", reader.GetInt32("Number"), reader.GetString("OperatingDayShortName"));
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

        private IDataReader ExecuteReader(OdbcConnection connection, string sql)
        {
            var command = new OdbcCommand(sql, connection);
            connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}
