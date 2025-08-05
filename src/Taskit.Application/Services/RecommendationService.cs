using AutoMapper;
using AutoMapper.QueryableExtensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Messages;

namespace Taskit.Application.Services;

public class RecommendationService(
    ITaskRepository taskRepository,
    IRequestClient<RelatedTasksQuery> client,
    IMapper mapper)
{
    private readonly ITaskRepository _tasks = taskRepository;
    private readonly IRequestClient<RelatedTasksQuery> _client = client;
    private readonly IMapper _mapper = mapper;

    public async Task<RelatedTasksDto> GetRelatedTasksAsync(int taskId, string userId, int count = 5)
    {
        var response = await _client.GetResponse<OperationResult<RelatedTasksQueryResult>>(new RelatedTasksQuery
        {
            TaskId = taskId,
            Count = count
        });

        if (response.Message.IsProcessing)
        {
            return new RelatedTasksDto { IsProcessing = true, Tasks = [] };
        }

        var taskIds = response.Message.Result?.TaskIds ?? [];

        var tasks = await _tasks.QueryForUser(userId)
            .Where(t => taskIds.Contains(t.Id))
            .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new RelatedTasksDto { IsProcessing = false, Tasks = tasks };
    }
}
