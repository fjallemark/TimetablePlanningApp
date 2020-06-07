using System;
using System.Collections.Generic;
using System.Text;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class Train
    {
        public string Number { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public IList<StationCall> Calls { get; } = new List<StationCall>();
    }
}
