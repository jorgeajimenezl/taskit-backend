using AutoMapper;
using Gridify;
using Gridify.EntityFramework;
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

    public async Task<Paging<TaskDto>> GetAllAsync(IGridifyQuery query)
    {
        return await _taskRepository.Query()
            .GridifyToAsync<AppTask, TaskDto>(_mapper, query);
    }
}