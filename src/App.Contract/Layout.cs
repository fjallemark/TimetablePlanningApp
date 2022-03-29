using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tellurian.Trains.Planning.App.Contracts;
public class Layout
{
    public string Name { get; set; }=string.Empty;
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; } 
}
