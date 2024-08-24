using Markdig.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Odbc;
using System.Globalization;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Extensions;
using Tellurian.Trains.Planning.App.Contracts.Resources;
using Tellurian.Trains.Planning.App.Server.Services;
using Tellurian.Trains.Planning.Repositories.Access;
using Tellurian.Trains.Timetables.Models;
using Tellurian.Trains.Timetables.Models.Extensions;


namespace Tellurian.Trains.Planning.App.Server.Tests;

[TestClass]
public class DataExport
{
    const string ConnectionString = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2024\\2024-08 Värnamo\\Timetable.accdb;Uid=Admin;Pwd=;";
    const int LayoutId = 32;
    const string OutputPath = "C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2024\\2024-08 Värnamo\\";

    //[Ignore("Use only for current plan.")]
    [TestMethod]
    public async Task ExportStationDutyTrains()
    {
        CultureInfo.CurrentCulture = new CultureInfo("sv");
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
        var options =
             Options.Create(new RepositoryOptions
             {
                 ConnectionString = ConnectionString
             }); ;
        var store = new AccessPrintedReportsStore(options);
        var target = new PrintedReportsService(store);
        var booklet = await target.GetStationDutyBookletAsync(LayoutId, includeAllTrains: true);

        using var stream = new FileStream(OutputPath + "Station trains Cda.csv", FileMode.Create);
        using var output =
            new StreamWriter(stream, System.Text.Encoding.UTF8);
        Assert.IsNotNull(booklet);
        output.WriteLine($"\"{Notes.Station}\";\"{Notes.Track}\";\"{Notes.Train}\";\"{Notes.Days}\";\"{Notes.From}\";\"{Notes.To}\";\"{Notes.IsStopping}\";\"{Notes.IsThroughpassing}\";\"{Notes.Arrival}\";\"{Notes.Departure}\";\"{Notes.Remarks}\""); ;
        var stationCalls = new Dictionary<int, StationCallWithAction>(300);
        foreach (var duty in booklet!.Duties)
        {
            foreach (var call in duty.Calls.Where(c => (c.Station.Signature == "Cda") && !c.IsShuntingOnly))
            {
                stationCalls[call.Train.Number] = call;
            }
            foreach (var call in duty.Calls.Where(c => (c.Station.Signature == "Cdr") && !c.IsShuntingOnly))
            {
                stationCalls.TryAdd(call.Train.Number, call);
            }
        }
        foreach (var call in stationCalls.Values.OrderBy(c => c.SortTime))
        {
            output.WriteLine($"\"{call.Station.Signature}\";\"{call.Call.TrackNumber}\";\"{call.Train.Prefix} {call.Train.Number}\";\"{call.Train.OperationDays().ShortName}\";\"{call.Train.Origin}\";\"{call.Train.Destination}\";\"{call.Call.IsStop}\";\"{IsPassingThroug(call, call.Station.Name)}\";\"{WithoutParenteses(call.ArrivalTime)}\";\"{call.DepartureTime}\";\"{call.Train.CategoryName}. {string.Join(" ", call.Notes.Select(n => n.Text.WithHtmlRemoved()))}\"");
        }

        static bool IsPassingThroug(StationCallWithAction call, string currentStationName) =>
            !call.Train.Destination.Equals(currentStationName, StringComparison.OrdinalIgnoreCase) &&
            !call.Train.Origin.Equals(currentStationName, StringComparison.OrdinalIgnoreCase);
        static string WithoutParenteses(string time) => time.Length == 7 ? time.Substring(1, 5) : time;
    }

    [TestMethod]
    public void ExportVehicleBookingList()
    {
        CultureInfo.CurrentCulture = new CultureInfo("sv");
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
        var options =
             Options.Create(new RepositoryOptions
             {
                 ConnectionString = ConnectionString
             });

        using var stream = new FileStream(OutputPath + "LayoutVehicles.csv", FileMode.Create);
        using var output =
            new StreamWriter(stream, System.Text.Encoding.UTF8);

        var store = new AccessPrintedReportsStore(options);
        var items = store.GetLayoutVehicles(LayoutId);

        output.WriteLine($"\"Station\";\"Spår\";\"Avgångstid\";\"Omlopp\";\"Dagar\";\"Littera\";\"Notera\";\"Fordonsnr\";\"Adress\";\"Ägare\"");
        foreach (var item in items)
        {
            for (var i = 0; i < NumberOfWagons(item); i++)
            {
                output.WriteLine($"\"{item.StartStationName}\";\"{item.StartTrack}\";\"{item.DepartureTime}\";\"{item.VehicleScheduleNumber}\";\"{item.OperatingDays}\";\"{item.OperatorSignature} {item.Class}\";\"{item.Note}\";\"{item.VehicleNumber}\";\"{LocoAddress(item)}\";\"{item.OwnerName}\"");
            }
        }
    }

    [TestMethod]
    public void ExportJsonToNewTimetableApp()
    {
        Timetable Result = new();
        CultureInfo.CurrentCulture = new CultureInfo("sv");
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
        using var connection = new OdbcConnection(ConnectionString);
        var sql = $"SELECT * FROM Layout WHERE Id = {LayoutId}";
        var command = new OdbcCommand(sql, connection);
        connection.Open();
        var reader = command.ExecuteReader();
        while (reader.Read()) {
            Result.Name = reader.GetString("Name");
            Result.InitialYear = (short)(reader.GetIntOrNull("FirstYear") ?? 1900);
            Result.FinalYear = (short)(reader.GetIntOrNull("LastYear") ?? DateTime.Now.Year);
            Result.StartTime = new TimeOnly(reader.GetInt("StartHour"), 0);
            Result.EndTime = new TimeOnly(reader.GetInt("EndHour"), 0);
            Result.BreakTime = GetBreakTimeOrNull(reader.GetIntOrNull("BreakHour"));
            Result.FirstDay = (DayOfWeek)(reader.GetInt("StartWeekday"));
            Result.MinNumberOfDrivers = (short)reader.GetInt("MaxLocoDriversCount");
            Result.ValidFrom = reader.GetDate("ValidFromDate").ToDateOnly();
            Result.ValidUntil = reader.GetDate("ValidToDate").ToDateOnly();
        }
        
        using var writer = new StreamWriter($"{OutputPath}{Result.Name}.json", System.Text.Encoding.Unicode, new FileStreamOptions() { Access = FileAccess.Write, Mode= FileMode.OpenOrCreate});
        writer.Write(Result.Serialize());



        static TimeOnly? GetBreakTimeOrNull(int? breakHour) => breakHour.HasValue ? new TimeOnly(breakHour.Value, 0) : null;
    }

    static int NumberOfWagons(LayoutVehicle vehicle) => (vehicle.Note?.Length > 0 && vehicle.Note?.First().IsDigit() == true) || vehicle.MaxNumberOfWagons < 1 ? 1 : vehicle.MaxNumberOfWagons;
    static string LocoAddress(LayoutVehicle vehicle) =>
        vehicle.IsLoco ? vehicle.LocoAddress.HasValue ? vehicle.LocoAddress.Value == 0 ? "Ange!" : vehicle.LocoAddress.Value.ToString() : "Ange!" : string.Empty;

}
