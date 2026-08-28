using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TasksApi.Data;
using TasksApi.Models;

namespace TasksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(AppDbContext context) : ControllerBase {

    // GET: api/tasks
    [HttpGet]
    public async Task<IActionResult> GetTasks() {

        var tasks = await context.Tasks
            .Include(t => t.Category)
            .ToListAsync();

        if(tasks == null || tasks.Count == 0)
            return NotFound(new { message = "No tasks found." });

        return Ok(tasks);
    }

    // GET: api/tasks/category/3
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetTasksByCategory(int categoryId) {

        bool categoryExists = await context.Categories.AnyAsync(c => c.Id == categoryId);
        if(!categoryExists)
            return NotFound(new { message = "Category not found." });

        var tasks = await context.Tasks
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync();

        return Ok(tasks); // empty list is fine
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<IActionResult> CreateTask(TaskItem task) {

        if(string.IsNullOrWhiteSpace(task.Title))
            return BadRequest(new { message = "Task title cannot be empty." });

        bool categoryExists = await context.Categories.AnyAsync(c => c.Id == task.CategoryId);

        if(!categoryExists)
            return BadRequest(new { message = "Category does not exist." });

        var t = new TaskItem {
            Title = task.Title,
            IsCompleted = false,
            CategoryId = task.CategoryId,
            CompletedAt = null,
            Category = null,
        };

        context.Tasks.Add(t);
        await context.SaveChangesAsync();

        return Ok(t);
    }

    // PUT: api/tasks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskItem task) {

        var existingTask = await context.Tasks.FindAsync(id);

        if(existingTask == null)
            return NotFound(new { message = "Task not found." });

        existingTask.IsCompleted = task.IsCompleted;

        if(existingTask.IsCompleted) {

            existingTask.CompletedAt = DateTime.UtcNow;

            var doneCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Done");

            if(doneCategory == null)
                return BadRequest(new { message = "Done category not found." });

            existingTask.CategoryId = doneCategory.Id;

            await context.SaveChangesAsync();

            return Ok(new { message = "Task completed and moved to Done.", id });
        }

        // If not completed, update other fields if needed
        existingTask.Title = task.Title;
        existingTask.CategoryId = task.CategoryId;

        await context.SaveChangesAsync();

        return Ok(new { message = "Task updated.", id });
    }

    // PUT: api/tasks/5/move/3
    [HttpPut("{id}/move/{categoryId}")]
    public async Task<IActionResult> MoveTask(int id, int categoryId) {

        var task = await context.Tasks.FindAsync(id);

        if(task == null)
            return NotFound(new { message = "Task not found." });

        var category = await context.Categories.FindAsync(categoryId);

        if(category == null)
            return NotFound(new { message = "Target category not found." });

        // If moving to Done, set timestamp
        if(category.Name == "Done") {
            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;
        } else {
            task.IsCompleted = false;
            task.CompletedAt = null;
        }

        task.CategoryId = categoryId;

        await context.SaveChangesAsync();

        return Ok(new
        {
            message = $"Task moved to category '{category.Name}'.",
            taskId = id,
            categoryId
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id) {

        var task = await context.Tasks.FindAsync(id);

        if(task == null)
            return NotFound(new { message = "Task not found." });

        context.Tasks.Remove(task);
        await context.SaveChangesAsync();

        return Ok(new { message = "Task deleted.", id });
    }
}
