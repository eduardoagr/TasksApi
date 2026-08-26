namespace TasksApi.Models;

public class TaskItem {

    public int Id { get; set; }

    public string? Title { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}
