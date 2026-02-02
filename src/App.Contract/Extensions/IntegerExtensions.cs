
namespace Tellurian.Trains.Planning.App.Contracts.Extensions;

public static class IntegerExtensions
{
    extension(int value)
    {
        public bool IsValueOrZero(int other) => value == 0 || value == other;
    }
}
