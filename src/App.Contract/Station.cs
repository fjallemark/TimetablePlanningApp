using System.Collections;

namespace Tellurian.Trains.Planning.App.Contract
{
    public class Station
    {
        public string Name { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public override string ToString() => Signature;
    }
}
