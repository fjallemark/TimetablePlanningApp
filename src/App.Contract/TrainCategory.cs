namespace Tellurian.Trains.Planning.App.Contracts;
public class TrainCategory
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsCargo { get; set; }
    public bool IsPassenger { get; set; }
    public string Prefix { get; set; } = string.Empty;

    public override string ToString() => Type;

    public static TrainCategory Empty => new ();

}

public class TrainCategories (IEnumerable<TrainCategory> categories)
{
    private readonly IEnumerable<TrainCategory> _categories = categories;

    public TrainCategory Category(int id)
    {
        var category = _categories.Where(c =>  c.Id == id).FirstOrDefault();
                return category ?? TrainCategory.Empty;
    }

    [Obsolete()]
    public TrainCategory Category(int categoryId, int countryId)
    {
        //var specific = _categories.Where(c => c.CountryId == countryId && c.Id == categoryId).FirstOrDefault();
        //if (specific is not null) return specific;
        //var fallback = _categories.Where(c => c.CountryId == _defaultCountryId || c.CountryId == 0 && (c.Id == categoryId)).FirstOrDefault();
        //if (fallback is not null) return fallback;
        return TrainCategory.Empty;
    }
}
