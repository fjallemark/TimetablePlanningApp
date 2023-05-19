using Microsoft.Extensions.Options;
using System.Data.Odbc;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;
public class AccessTrainStore : ITrainsStore
{
    public AccessTrainStore(IOptions<RepositoryOptions> options)
    {
        Options = options.Value;
    }
    private readonly RepositoryOptions Options;
    private OdbcConnection CreateConnection => new(Options.ConnectionString);

    public Task<int> UpdateTrainTimes( int trainId, int minutes)
    {
        using var connection = CreateConnection;
        var sql = $"UPDATE TrainStationCall SET ArrivalTime = DATEADD(\'n\', {minutes}, [ArrivalTime]), DepartureTime = DATEADD(\'n\', {minutes}, [DepartureTime]) WHERE IsTrain = {trainId}";
        var command = new OdbcCommand(sql, connection);
        connection.Open();
        return command.ExecuteNonQueryAsync();
    }
}
