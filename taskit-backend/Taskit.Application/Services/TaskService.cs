using AutoMapper;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;

namespace Taskit.Web.Services;

public class TaskService(ITaskRepository taskRepository, IMapper mapper)
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();        
        return _mapper.Map<IEnumerable<TaskDto>>(tasks);
    }

    public async Task<TaskDto> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return _mapper.Map<TaskDto>(task);
    }

    // public async Task CreateTaskAsync(TaskDto taskDto)
    // {
    //     var task = _mapper.Map<Task>(taskDto);
    //     await _taskRepository.AddAsync(task);
    // }

    // public async Task UpdateTaskAsync(TaskDto taskDto)
    // {
    //     var task = _mapper.Map<Task>(taskDto);
    //     await _taskRepository.UpdateAsync(task);
    // }

    // public async Task DeleteTaskAsync(int id)
    // {
    //     await _taskRepository.DeleteAsync(id);
    // }
}