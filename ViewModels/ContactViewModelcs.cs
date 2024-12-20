using System.ComponentModel.DataAnnotations;

namespace RealEstate.ViewModels
{
    public class ContactViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? PropertyId { get; set; }
    }
}