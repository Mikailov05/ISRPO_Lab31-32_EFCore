using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskDb.Data;
using TaskDb.Models;
namespace TaskDb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null)
            return NotFound(new { Message = $"Задача с id={id} не найдена" });
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create([FromBody] CreateTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { Message = "Поле Title обязательно для заполнения" });

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Priority = dto.Priority,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<TaskItem>> ToggleComplete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null)
            return NotFound(new { Message = $"Задача с id={id} не найдена" });
        task.IsCompleted = !task.IsCompleted;
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null)
            return NotFound(new { Message = $"Задача с id={id} не найдена" });
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskItem>> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null)
            return NotFound(new { Message = $"Задача с id={id} не найдена" });
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { Message = "Поле Title не может быть пустым" });

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim() ?? string.Empty;
        task.IsCompleted = dto.IsCompleted;
        task.Priority = dto.Priority;
        await _db.SaveChangesAsync();
        return Ok(task);
    }
}