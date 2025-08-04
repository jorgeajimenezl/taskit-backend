using Gridify;
using Taskit.Domain.Entities;

namespace Taskit.Application.Common.Mappings;

public static class GridifyMappings
{
    public static readonly IGridifyMapper<Project> ProjectMapper = new GridifyMapper<Project>()
        .AddMap("id", q => q.Id)
        .AddMap("name", q => q.Name)
        .AddMap("description", q => q.Description)
        .AddMap("ownerId", q => q.OwnerId);

    public static readonly IGridifyMapper<ProjectMember> ProjectMemberMapper = new GridifyMapper<ProjectMember>()
        .AddMap("id", q => q.Id)
        .AddMap("projectId", q => q.ProjectId)
        .AddMap("userId", q => q.UserId)
        .AddMap("username", q => q.User!.UserName)
        .AddMap("fullName", q => q.User!.FullName)
        .AddMap("role", q => q.Role);

    public static readonly IGridifyMapper<AppTask> TaskMapper = new GridifyMapper<AppTask>()
        .AddMap("id", q => q.Id)
        .AddMap("title", q => q.Title)
        .AddMap("description", q => q.Description)
        .AddMap("generatedSummary", q => q.GeneratedSummary)
        .AddMap("projectId", q => q.ProjectId)
        .AddMap("projectName", q => q.Project!.Name)
        .AddMap("dueDate", q => q.DueDate)
        .AddMap("completedAt", q => q.CompletedAt)
        .AddMap("status", q => q.Status)
        .AddMap("priority", q => q.Priority)
        .AddMap("complexity", q => q.Complexity)
        .AddMap("completedPercentage", q => q.CompletedPercentage);

    public static readonly IGridifyMapper<ProjectActivityLog> ProjectActivityLogMapper = new GridifyMapper<ProjectActivityLog>()
        .AddMap("id", q => q.Id)
        .AddMap("timestamp", q => q.Timestamp)
        .AddMap("eventType", q => q.EventType)
        .AddMap("userId", q => q.UserId)
        .AddMap("projectId", q => q.ProjectId)
        .AddMap("taskId", q => q.TaskId);
}

