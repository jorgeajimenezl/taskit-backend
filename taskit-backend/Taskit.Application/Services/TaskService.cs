using AutoMapper;
using Taskit.Application.Common.Mappings;
using Taskit.Application.Common.Models;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;

namespace Taskit.Application.Services;

public class TaskService(ITaskRepository taskRepository, IMapper mapper)
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<PaginatedList<TaskDto>> GetTasksByAssignedUserIdAsync(string assignedUserId, int pageIndex, int pageSize)
    {
        var tasks = await _taskRepository.GetTasksByAssignedUserIdAsync(assignedUserId, pageIndex, pageSize);
        return _mapper.Map<PaginatedList<TaskDto>>(tasks);
    }
}