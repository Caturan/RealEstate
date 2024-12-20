using Microsoft.AspNetCore.Mvc.Rendering;

namespace RealEstate.Models
{
    public class SearchViewModel
    {
        public string? SearchString { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? Bedrooms { get; set; }
        public string? City { get; set; }
        public PropertyType? PropertyType { get; set; }
        public IEnumerable<Property>? Properties { get; set; }
    }
}