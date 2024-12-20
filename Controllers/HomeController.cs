using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Data;
using RealEstate.Models;
using RealEstate.ViewModels;  // Add this for HomeViewModel
using System.Diagnostics;

namespace RealEstate.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
public async Task<IActionResult> Index()
        {
            // Get featured properties
            var featuredProperties = await _context.Properties
                .Where(p => p.IsAvailable)
                .OrderByDescending(p => p.ListingDate)
                .Take(6)
                .ToListAsync();

            // Get property statistics
            var stats = new HomeViewModel
            {
                FeaturedProperties = featuredProperties,
                TotalProperties = await _context.Properties.CountAsync(),
                PropertiesForSale = await _context.Properties.CountAsync(p => p.IsAvailable),
                RecentlyAdded = await _context.Properties
                    .Where(p => p.ListingDate >= DateTime.Now.AddDays(-30))
                    .CountAsync()
            };

            return View(stats);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string location, decimal? minPrice, decimal? maxPrice, PropertyType? propertyType)
        {
            var query = _context.Properties.AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(p =>
                    p.City.Contains(location) ||
                    p.State.Contains(location) ||
                    p.ZipCode.Contains(location));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (propertyType.HasValue)
            {
                query = query.Where(p => p.Type == propertyType.Value);
            }

            var results = await query
                .Where(p => p.IsAvailable)
                .OrderByDescending(p => p.ListingDate)
                .ToListAsync();

            return View("SearchResults", results);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Implement email sending logic
                TempData["Message"] = "Thank you for your message. We'll get back to you soon!";
                return RedirectToAction(nameof(Contact));
            }
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}