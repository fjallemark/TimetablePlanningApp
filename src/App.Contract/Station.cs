using System;
using System.Collections.Generic;

#pragma warning disable CA2227 // Collection properties should be read only, but then deserialization does not work.

namespace Tellurian.Trains.Planning.App.Contract
{
    public class StationInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public override string ToString() => Signature;
        public override bool Equals(object obj) => obj is Station other && other.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
    public class Station : StationInfo
    {
        public IList<StationTrack> Tracks { get; set; } = Array.Empty<StationTrack>();
    }
}
