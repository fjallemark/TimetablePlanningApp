using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;
internal static class PassengerCallNoteMapper
{
    public static PassengerDepartureCallNote ToPassengerDepartureCallNote(this IDataRecord record) =>
        new(record.GetInt("CallId"));

    public static PassengerInterchangeCallNote ToPassengerInterchangeCallNote(this IDataRecord record) =>
        new(record.GetInt("CallId"));
}
