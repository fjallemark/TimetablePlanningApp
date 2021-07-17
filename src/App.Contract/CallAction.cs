﻿namespace Tellurian.Trains.Planning.App.Contracts
{
    public class CallAction
    {
        public StationInfo? Station { get; set; }
        public string Track { get; set; } = string.Empty;
        public CallTime? Time { get; set; }
        public override string ToString() => $"{Station} {Track} {Time}";
    }
}
