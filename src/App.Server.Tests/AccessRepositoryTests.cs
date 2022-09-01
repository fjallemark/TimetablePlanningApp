using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.Repositories.Access;

namespace Tellurian.Trains.Planning.App.Server.Tests;

[TestClass]
public class AccessRepositoryTests
{
    [TestMethod]
    public async Task ReadStationDutyData()
    {
        var options =
            Options.Create(new RepositoryOptions
            {
                ConnectionString = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2022\\2022-03 Grimslöv\\Trafikplanering\\Timetable.accdb;Uid=Admin;Pwd=;"
            });
        var target = new AccessPrintedReportsStore(options);
        var result = await target.GetStationDutiesDataAsync(Constants.LayoutId);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task UpdateTrainTimes()
    {
        var options =
            Options.Create(new RepositoryOptions
            {
                ConnectionString = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2022\\2022-09 Nässjö\\Trafikplanering\\Timetable.accdb;Uid=Admin;Pwd=;"
            });
        var target = new AccessTrainStore(options);
        var result = await target.UpdateTrainTimes(917, 10);

    }
}
