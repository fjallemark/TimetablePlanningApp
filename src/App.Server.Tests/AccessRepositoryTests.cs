using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        var result = await target.GetStationDutiesDataAsync(TestData.AppSettings.LayoutId);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task UpdateTrainTimes()
    {
        var options =
            Options.Create(new RepositoryOptions
            {
                ConnectionString = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2024\\2024-09 Nässjö\\Timetable.accdb;Uid=Admin;Pwd=;"
            });
        var target = new AccessTrainStore(options);
        int[] trainIds =  [1709, 1713];
        foreach (var trainId in trainIds)
        {
            _ = await target.UpdateTrainTimes(trainId, 60);
        }

    }
}
