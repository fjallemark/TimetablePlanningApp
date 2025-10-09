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
    public int? ToYear { get; set; }
    public int? CountryId { get; set; }

    public override string ToString() => ResourceCode;

    public static TrainCategory Empty => new ();

}

public static class TrainCategoryExtensions
{
    public static TrainCategory Category(this IEnumerable<TrainCategory> items, string resourceCode) => 
        items.SingleOrDefault(items => 
        items.ResourceCode.Equals(resourceCode, StringComparison.OrdinalIgnoreCase)) ?? 
        new TrainCategory { ResourceCode = resourceCode };
}

public class TrainCategories (IEnumerable<TrainCategory> categories, int categoryYear, int defaultCountryId = 1)
{
    private readonly IEnumerable<TrainCategory> _categories = categories;
    private readonly int? _defaultCountryId = defaultCountryId;
    private readonly int? _categoryYear = categoryYear ;

    public TrainCategory Category(string resourceCode, int countryId)
    {
        var specific = _categories.Where(c =>  c.CountryId == countryId &&  c.ResourceCode.Equals(resourceCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (specific is not null) return specific;
        var fallback = _categories.Where(c => c.CountryId== _defaultCountryId || c.CountryId==0 && c.ResourceCode.Equals(resourceCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (fallback is not null) return fallback;
        return TrainCategory.Empty;
    }

    public TrainCategory Category(int categoryId, int countryId)
    {
        var specific = _categories.Where(c => c.CountryId == countryId && c.Id == categoryId).FirstOrDefault();
        if (specific is not null) return specific;
        var fallback = _categories.Where(c => c.CountryId == _defaultCountryId || c.CountryId == 0 && (c.Id == categoryId)).FirstOrDefault();
        if (fallback is not null) return fallback;
        return TrainCategory.Empty;
    }
}
