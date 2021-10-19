using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public class StationInstruction
    {
        public StationInfo? StationInfo { get; set; }
        public IEnumerable<StationCall> Calls { get; set; } = Array.Empty<StationCall>();

    }
}
