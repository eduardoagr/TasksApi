using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TasksApi.Data;
using TasksApi.Models;

namespace TasksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase {

    private readonly AppDbContext _context;

    public TasksController(AppDbContext context) => _context = context;

    // GET: api/tasks
    [HttpGet]
    public async Task<IActionResult> GetTasks() {

        var tasks = await _context.Tasks
            .Include(t => t.Category)
            .ToListAsync();

        if(tasks == null || tasks.Count == 0)
            return NotFound(new { message = "No tasks found." });

        return Ok(tasks);
    }

    // GET: api/tasks/category/3
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetTasksByCategory(int categoryId) {

        var tasks = await _context.Tasks
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync();
        
        if(tasks == null || tasks.Count == 0)
            return NotFound(new { message = "No tasks found for the specified category." });

        return Ok(tasks);
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<IActionResult> CreateTask(TaskItem task) {

        if(string.IsNullOrWhiteSpace(task.Title))
            return BadRequest(new { message = "Task title cannot be empty." });

        bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == task.CategoryId);

        if(!categoryExists)
            return BadRequest(new { message = "Category does not exist." });

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return Ok(task);
    }

    // PUT: api/tasks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskItem task) {
        var existingTask = await _context.Tasks.FindAsync(id);

        if(existingTask == null)
            return NotFound(new { message = "Task not found." });

        existingTask.IsCompleted = task.IsCompleted;

        if(existingTask.IsCompleted) {
         
            _context.Tasks.Remove(existingTask);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task completed and removed.", id });
        }

        return Ok();
    }
}
