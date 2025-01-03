using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Controllers;
using RealEstate.Data;
using RealEstate.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RealEstate.Tests
{
    public class PropertiesControllerTests : IDisposable
    {
        private readonly PropertiesController _controller;
        private readonly ApplicationDbContext _context;

        public PropertiesControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new PropertiesController(_context);
        }

        private void SeedDatabase()
        {
            _context.Properties.AddRange(new List<Property>
            {
                new Property { Id = 1, Title = "Luxury Villa", Price = 1500000, Bedrooms = 4, City = "Beverly Hills" },
                new Property { Id = 2, Title = "Modern Apartment", Price = 500000, Bedrooms = 2, City = "Los Angeles" },
                new Property { Id = 3, Title = "Cozy Cottage", Price = 300000, Bedrooms = 3, City = "Santa Monica" }
            });
            _context.SaveChanges();
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithFilteredProperties()
        {
            SeedDatabase();

            var result = await _controller.Index("Luxury", null, null, null, null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Property>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Luxury Villa", model.First().Title);
        }

        [Fact]
        public async Task Create_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Title", "Required");

            var newProperty = new Property { Price = 150000, Bedrooms = 3, City = "New City" };

            var result = await _controller.Create(newProperty);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(newProperty, viewResult.Model);
        }

        [Fact]
        public async Task Create_ReturnsRedirectToActionResult_WhenModelStateIsValid()
        {
            var newProperty = new Property
            {
                Title = "New Property",
                Price = 150000,
                Bedrooms = 3,
                City = "New City"
            };

            var result = await _controller.Create(newProperty);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.True(_context.Properties.Any(p => p.Title == "New Property"));
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var result = await _controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            SeedDatabase();
            var propertyToEdit = await _context.Properties.FirstOrDefaultAsync();
            Assert.NotNull(propertyToEdit);

            _controller.ModelState.AddModelError("Title", "Required");

            var result = await _controller.Edit(propertyToEdit.Id, propertyToEdit);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(propertyToEdit, viewResult.Model);
        }

        [Fact]
        public async Task Edit_ReturnsRedirectToActionResult_WhenModelStateIsValid()
        {
            SeedDatabase();
            var propertyToEdit = await _context.Properties.FirstOrDefaultAsync();
            Assert.NotNull(propertyToEdit);

            propertyToEdit.Title = "Updated Property";
            propertyToEdit.Price = 1600000;
            propertyToEdit.Bedrooms = 5;
            propertyToEdit.City = "Updated City";

            var result = await _controller.Edit(propertyToEdit.Id, propertyToEdit);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Updated Property", _context.Properties.Find(propertyToEdit.Id).Title);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesProperty_AndRedirectsToIndex()
        {
            SeedDatabase();

            var result = await _controller.DeleteConfirmed(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Null(_context.Properties.Find(1));
        }

        [Fact]
        public async Task Delete_ReturnsViewResult_WithProperty()
        {
            SeedDatabase();

            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Property>(viewResult.Model);
            Assert.Equal("Luxury Villa", model.Title);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }

    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropertiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder, string city, int? minPrice, int? maxPrice, int? bedrooms)
        {
            var properties = _context.Properties.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                properties = properties.Where(p => p.Title.Contains(searchString));

            if (!string.IsNullOrEmpty(city))
                properties = properties.Where(p => p.City == city);

            if (minPrice.HasValue)
                properties = properties.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                properties = properties.Where(p => p.Price <= maxPrice);

            if (bedrooms.HasValue)
                properties = properties.Where(p => p.Bedrooms == bedrooms);

            return View(await properties.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Property property)
        {
            if (!ModelState.IsValid)
            {
                return View(property);
            }

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
                return NotFound();

            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Property property)
        {
            if (id != property.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(property);

            _context.Properties.Update(property);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
                return NotFound();

            return View(property);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property != null)
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}