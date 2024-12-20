using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstate.Models
{
    public class Property
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        public double SquareFootage { get; set; }

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string ZipCode { get; set; } = string.Empty;

        public PropertyType Type { get; set; }

        public DateTime ListingDate { get; set; }

        public bool IsAvailable { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
    }

    public enum PropertyType
    {
        House,
        Apartment,
        Condo,
        TownHouse,
        Land,
        Villa,
        Office
    }
}