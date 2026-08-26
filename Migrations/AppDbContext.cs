using Microsoft.EntityFrameworkCore;

using TasksApi.Models;

namespace TasksApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {

    public DbSet<TaskItem> Tasks { get; set; }

    public DbSet<Category> Categories { get; set; }
}
