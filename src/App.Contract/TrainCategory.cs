using System;
using System.Collections.Generic;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Contracts;
public class TrainCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResourceCode { get; set; } = string.Empty;
    public bool IsCargo { get; set; }
    public bool IsPassenger { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public int? FromYear { get; set; }
    public int? ToYear  { get; set; }
    public int? CountryId { get; set; }

}

public static class TrainCategoryExtensions
{
    public static TrainCategory Category(this IEnumerable<TrainCategory> items, string resourceCode)
    {
        try
        {
            return items.Single(items => items.ResourceCode.Equals(resourceCode, System.StringComparison.OrdinalIgnoreCase));

        }
        catch (System.Exception ex)
        {

            throw new ArgumentOutOfRangeException(nameof(resourceCode), resourceCode);
        }
    }
}
