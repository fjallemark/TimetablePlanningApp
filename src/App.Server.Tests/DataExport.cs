using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Server.Services;
using Tellurian.Trains.Planning.Repositories.Access;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Server.Tests;

[TestClass]
public class DataExport
{
    [Ignore("Use only for current plan.")]
    [TestMethod]
    public async Task ExportStationDutyTrains()
    {
        CultureInfo.CurrentCulture = new CultureInfo("sv");
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
        var options =
             Options.Create(new RepositoryOptions
             {
                 ConnectionString = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2022\\2022-03 Grimslöv\\Trafikplanering\\Timetable.accdb;Uid=Admin;Pwd=;"
             });
        var store = new AccessPrintedReportsStore(options, Options.Create(Globals.AppSettings));
        var target = new PrintedReportsService(store);
        var booklet = await target.GetStationDutyBookletAsync(17, includeAllTrains: true);

        using var stream = new FileStream("C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2022\\2022-03 Grimslöv\\Trafikplanering\\Station trains Cda.csv", FileMode.Create);
        using var output = 
            new StreamWriter(stream, System.Text.Encoding.UTF8);
        Assert.IsNotNull(booklet);
        output.WriteLine($"\"{Notes.Station}\";\"{Notes.Track}\";\"{Notes.Train}\";\"{Notes.Days}\";\"{Notes.From}\";\"{Notes.To}\";\"{Notes.IsStopping}\";\"{Notes.IsThroughpassing}\";\"{Notes.Arrival}\";\"{Notes.Departure}\";\"{Notes.Remarks}\""); ;
        foreach (var duty in booklet.Duties)
        {
            foreach(var call in duty.Calls.Where(c=> c.Station.Signature=="Cda" && ! c.IsShuntingOnly))
            {
                output.WriteLine($"\"{call.Station.Signature}\";\"{call.Call.TrackNumber}\";\"{call.Train.Prefix} {call.Train.Number}\";\"{call.Train.OperationDays().ShortName}\";\"{call.Train.Origin}\";\"{call.Train.Destination}\";\"{call.Call.IsStop}\";\"{IsPassingThroug(call, call.Station.Name)}\";\"{call.ArrivalTime}\";\"{call.DepartureTime}\";\"{call.Train.CategoryName}. {string.Join(" ",call.Notes)}\"");
            }
        }

        static bool IsPassingThroug(StationCallWithAction call, string currentStationName) =>
            !call.Train.Destination.Equals(currentStationName, System.StringComparison.OrdinalIgnoreCase) &&
            !call.Train.Origin.Equals(currentStationName, System.StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void ExportVehicleBookingList()
    {
        CultureInfo.CurrentCulture = new CultureInfo("sv");
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
        var options =
             Options.Create(new RepositoryOptions
             {
                 ConnectionString = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2023\\2023-01 Halmstad\\Trafikplanering\\Timetable.accdb;Uid=Admin;Pwd=;"
             });
        
        using var stream = new FileStream("C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2023\\2023-01 Halmstad\\Trafikplanering\\LayoutVehicles.csv", FileMode.Create);
        using var output =
            new StreamWriter(stream, System.Text.Encoding.UTF8);        
        
        var store = new AccessPrintedReportsStore(options, Options.Create(Globals.AppSettings));
        var items = store.GetLayoutVehicles(23);

        output.WriteLine($"\"Station\";\"Spår\";\"Avgångstid\";\"Dagar\";\"Littera\";\"Fordonsnr\";\"Ägare\"");
        foreach (var item in items)
        {
            output.WriteLine($"\"{item.StartStationName}\";\"{item.StartTrack}\";\"{item.DepartureTime}\";\"{item.OperatingDays}\";\"{item.OperatorSignature} {item.Class}\";\"{item.VehicleNumber}\";\"{item.OwnerName}\"");
        }
    }
}
