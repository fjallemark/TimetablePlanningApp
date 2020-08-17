using System;
using System.Collections.Generic;
using System.Text;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class CallAction
    {
        public Station? Station { get; set; }
        public string Track { get; set; } = string.Empty;
        public CallTime? Time { get; set; }
        public override string ToString() => $"{Station} {Track} {Time}";
    }
}
