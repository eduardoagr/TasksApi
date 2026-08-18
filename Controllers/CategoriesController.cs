using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TasksApi.Data;
using TasksApi.Models;

namespace TasksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(AppDbContext context) : ControllerBase {

    [HttpGet]
    public async Task<IActionResult> GetCategories() {

        var categories = await context.Categories
            .Include(t => t.Tasks)
            .ToListAsync();

        return Ok(categories);

    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id) {

        var category = await context.Categories
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if(category == null) {
            return NotFound("Category not found. ");
        }

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(Category category) {

        if(string.IsNullOrEmpty(category.Name)) {
            return BadRequest("Category name is required. ");
        }

        bool exiist = await context.Categories.AnyAsync(c => c.Name == category.Name);
        if(exiist) {
            return Conflict("Category with this name already exists. ");
        }

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return Ok(category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, Category updated) {
        var category = await context.Categories.FindAsync(id);

        if(category == null)
            return NotFound(new { message = "Category not found." });

        if(string.IsNullOrWhiteSpace(updated.Name))
            return BadRequest(new { message = "Category name cannot be empty." });

        category.Name = updated.Name;
        await context.SaveChangesAsync();

        return Ok(new { message = "Category updated.", id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id) {

        var category = await context.Categories
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if(category == null)
            return NotFound(new { message = "Category not found." });

        context.Tasks.RemoveRange(category.Tasks);

        context.Categories.Remove(category);

        await context.SaveChangesAsync();

        return Ok(new { message = "Category deleted.", id });
    }

    [HttpDelete("reset")]
    public async Task<IActionResult> ResetCategories() {
        // Delete all tasks
        var allTasks = await context.Tasks.ToListAsync();
        context.Tasks.RemoveRange(allTasks);

        // Delete all categories
        var allCategories = await context.Categories.ToListAsync();
        context.Categories.RemoveRange(allCategories);

        await context.SaveChangesAsync();

        // Recreate default categories
        var defaultCategories = new List<Category>
        {
             new() { Name = "Backlog" },
             new() { Name = "Planned" },
             new() { Name = "Doing" },
             new() { Name = "Done" },
             new() { Name = "Bug" },
        };

        context.Categories.AddRange(defaultCategories);
        await context.SaveChangesAsync();

        return Ok(new { message = "All categories deleted and recreated. IDs reset." });
    }

}
