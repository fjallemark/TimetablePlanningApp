using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CA2227 // Collection properties should be read only

namespace Tellurian.Trains.Planning.App.Shared
{
    public class CallTime
    {
        public string Time { get; set; } = string.Empty;
        public bool IsHidden { get; set; } = true;
        public bool IsStop { get; set; }
        public IList<Note> Notes { get;  set; } = new List<Note>();

        public static CallTime Create(string time, params string[] notes) =>
            new CallTime { Time = time, IsStop = true, IsHidden = false, Notes = notes.Select(n => new Note { Text = n }).ToArray() };
        public static CallTime Create(string time, bool isStop =true, bool isHidden = false, params string[] notes) =>
            new CallTime { Time = time, IsStop = isStop, IsHidden = isHidden, Notes=notes.Select(n => new Note { Text = n }).ToArray() };

        public override string ToString() => IsHidden || Time is null ? string.Empty : Time;
    }
}
