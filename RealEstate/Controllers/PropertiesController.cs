using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Data;
using RealEstate.Models;

namespace RealEstate.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropertiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Properties
        public async Task<IActionResult> Index(string searchString, string city, decimal? minPrice, decimal? maxPrice, int? bedrooms, PropertyType? propertyType)
        {
            try
            {
                var query = _context.Properties.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(p =>
                        p.Title.Contains(searchString) ||
                        p.Description.Contains(searchString) ||
                        p.Address.Contains(searchString) ||
                        p.City.Contains(searchString));
                }

                if (!string.IsNullOrEmpty(city))
                {
                    query = query.Where(p => p.City.Contains(city));
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                if (bedrooms.HasValue)
                {
                    query = query.Where(p => p.Bedrooms >= bedrooms.Value);
                }

                if (propertyType.HasValue)
                {
                    query = query.Where(p => p.Type == propertyType.Value);
                }

                // Order by listing date (newest first)
                query = query.OrderByDescending(p => p.ListingDate);

                var properties = await query.ToListAsync();

                // Store search parameters in ViewBag
                ViewBag.CurrentSearch = searchString;
                ViewBag.CurrentCity = city;
                ViewBag.CurrentMinPrice = minPrice;
                ViewBag.CurrentMaxPrice = maxPrice;
                ViewBag.CurrentBedrooms = bedrooms;
                ViewBag.CurrentPropertyType = propertyType;

                // Store result count
                ViewBag.ResultCount = properties.Count;

                return View(properties);
            }
            catch (Exception ex)
            {
                // Log the error (consider using a logging framework)
                TempData["Error"] = "An error occurred while searching for properties.";
                return View(new List<Property>());
            }
        }

        // GET: Properties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .FirstOrDefaultAsync(m => m.Id == id);
            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // GET: Properties/Create
        public IActionResult Create()
        {
            var property = new Property
            {
                ListingDate = DateTime.Now,
                IsAvailable = true
            };
            return View(property);
        }

        // POST: Properties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Price,Bedrooms,Bathrooms,SquareFootage,Address,City,State,ZipCode,Type,IsAvailable,ImageUrl")] Property property)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    property.ListingDate = DateTime.Now;
                    _context.Add(property);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Property created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Log the error (consider using a logging framework)
                TempData["Error"] = "An error occurred while creating the property.";
            }
            return View(property);
        }

        // GET: Properties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }
            return View(property);
        }

        // POST: Properties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,Bedrooms,Bathrooms,SquareFootage,Address,City,State,ZipCode,Type,ListingDate,IsAvailable,ImageUrl")] Property property)
        {
            // Check if the ID in the route matches the ID in the property object
            if (id != property.Id)
            {
                return NotFound(); // Return 404 if IDs do not match
            }

            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(property); // Update the property in the context
                    await _context.SaveChangesAsync(); // Save changes to the database
                    TempData["Success"] = "Property updated successfully!";
                    return RedirectToAction(nameof(Index)); // Redirect to the Index action
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency issues
                    if (!PropertyExists(property.Id))
                    {
                        return NotFound(); // Return 404 if the property no longer exists
                    }
                    else
                    {
                        throw; // Rethrow the exception if it's a different issue
                    }
                }
                catch (Exception ex)
                {
                    // Log the error (consider using a logging framework)
                    TempData["Error"] = "An error occurred while updating the property.";
                }
            }
            return View(property); // Return the view with the property if the model state is invalid
        }

        // GET: Properties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .FirstOrDefaultAsync(m => m.Id == id);
            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // POST: Properties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var property = await _context.Properties.FindAsync(id);
                if (property != null)
                {
                    _context.Properties.Remove(property);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Property deleted successfully!";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error (consider using a logging framework)
                TempData["Error"] = "An error occurred while deleting the property.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }

        // AJAX Search
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<object>());
            }

            var suggestions = await _context.Properties
                .Where(p =>
                    p.Title.Contains(term) ||
                    p.City.Contains(term) ||
                    p.Address.Contains(term))
                .Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    price = p.Price.ToString("C0"),
                    city = p.City,
                    bedrooms = p.Bedrooms,
                    imageUrl = p.ImageUrl ?? "/images/default-property.jpg"
                })
                .Take(5)
                .ToListAsync();

            return Json(suggestions);
        }

        // Clear Search
        public IActionResult ClearSearch()
        {
            return RedirectToAction(nameof(Index));
        }
    }
}