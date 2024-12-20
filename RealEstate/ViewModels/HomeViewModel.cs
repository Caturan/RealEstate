using RealEstate.Models;  // Add this to reference the Property model

namespace RealEstate.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Property> FeaturedProperties { get; set; } = new List<Property>();
        public int TotalProperties { get; set; }
        public int PropertiesForSale { get; set; }
        public int RecentlyAdded { get; set; }
    }
}