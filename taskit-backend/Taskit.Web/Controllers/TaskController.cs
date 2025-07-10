namespace Taskit.Controllers;

public class TaskController : ApiControllerBase
{
    // private readonly ITaskService _taskService;

    // public TaskController(ITaskService taskService)
    // {
    //     _taskService = taskService;
    // }

    // [HttpGet("tasks")]
    // public async Task<IActionResult> GetTasks()
    // {
    //     var tasks = await _taskService.GetTasksAsync();
    //     return Ok(tasks);
    // }

    // [HttpPost("tasks")]
    // public async Task<IActionResult> CreateTask([FromBody] TaskDto taskDto)
    // {
    //     if (!ModelState.IsValid)
    //     {
    //         return BadRequest(ModelState);
    //     }

    //     var createdTask = await _taskService.CreateTaskAsync(taskDto);
    //     return CreatedAtAction(nameof(GetTasks), new { id = createdTask.Id }, createdTask);
    // }
}