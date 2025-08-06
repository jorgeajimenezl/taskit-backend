using AutoMapper;
using AutoMapper.QueryableExtensions;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.DTOs;
using Taskit.Application.Interfaces;
using Taskit.Domain.Entities;
using Taskit.Domain.Messages;

namespace Taskit.Application.Services;

public class RecommendationService(
    ITaskRepository taskRepository,
    IRequestClient<RelatedTasksQuery> client,
    UserManager<AppUser> userManager,
    IMapper mapper)
{
    private readonly ITaskRepository _tasks = taskRepository;
    private readonly IRequestClient<RelatedTasksQuery> _client = client;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IMapper _mapper = mapper;

    private async Task<IEnumerable<AppTask>?> GetRelatedTasksAsync(int taskId, string userId, int count = 5)
    {
        var response = await _client.
            GetResponse<IOperationInProgress, IOperationSucceeded<RelatedTasksQueryResult>>
            (new RelatedTasksQuery
            {
                TaskId = taskId,
                Count = count
            });

        IReadOnlyCollection<int> taskIds = [];

        if (response.Is<IOperationInProgress>(out var _))
        {
            return null;
        }
        else if (response.Is<Fault<RelatedTasksQuery>>(out var fault))
        {
            throw new Exception(fault.Message.Exceptions.FirstOrDefault()?.Message ??
                "An error occurred while processing the request.");
        }
        else if (response.Is<IOperationSucceeded<RelatedTasksQueryResult>>(out var result))
        {
            taskIds = result.Message.Result.TaskIds;
        }
        else
        {
            throw new InvalidOperationException();
        }

        var tasks = await _tasks.QueryForUser(userId)
            .Where(t => taskIds.Contains(t.Id))
            .ToListAsync();

        return tasks;
    }

    public async Task<IEnumerable<UserProfileDto>?> GetAssigneeSuggestionsAsync(int taskId, string userId, int count = 5)
    {
        var relatedTasks = await GetRelatedTasksAsync(taskId, userId, count);

        if (relatedTasks == null || !relatedTasks.Any())
        {
            return null;
        }

        var assigneeIds = relatedTasks
            .Select(t => t.AssignedUserId)
            .Where(id => id != null && id != userId)
            .Distinct()
            .Take(count);

        var users = await _userManager.Users
            .Where(u => assigneeIds.Contains(u.Id))
            .ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return users;
    }
}
