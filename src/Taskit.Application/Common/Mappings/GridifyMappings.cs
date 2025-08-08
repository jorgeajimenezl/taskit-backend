using Gridify;
using Taskit.Domain.Entities;

namespace Taskit.Application.Common.Mappings;

public static class GridifyMappings
{
    public static readonly IGridifyMapper<Project> ProjectMapper = new GridifyMapper<Project>()
        .AddMap("id", q => q.Id)
        .AddMap("name", q => q.Name)
        .AddMap("description", q => q.Description)
        .AddMap("ownerId", q => q.OwnerId)
        .AddMap("createdAt", q => q.CreatedAt)
        .AddMap("updatedAt", q => q.UpdatedAt);

    public static readonly IGridifyMapper<ProjectMember> ProjectMemberMapper = new GridifyMapper<ProjectMember>()
        .AddMap("id", q => q.Id)
        .AddMap("projectId", q => q.ProjectId)
        .AddMap("userId", q => q.UserId)
        .AddMap("username", q => q.User!.UserName)
        .AddMap("fullName", q => q.User!.FullName)
        .AddMap("role", q => q.Role)
        .AddMap("createdAt", q => q.CreatedAt)
        .AddMap("updatedAt", q => q.UpdatedAt);

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
        .AddMap("completedPercentage", q => q.CompletedPercentage)
        .AddMap("authorId", q => q.AuthorId)
        .AddMap("assignedUserId", q => q.AssignedUserId)
        .AddMap("isArchived", q => q.IsArchived)
        .AddMap("parentTaskId", q => q.ParentTaskId)
        .AddMap("createdAt", q => q.CreatedAt)
        .AddMap("updatedAt", q => q.UpdatedAt);

    public static readonly IGridifyMapper<TaskComment> TaskCommentMapper = new GridifyMapper<TaskComment>()
        .AddMap("id", q => q.Id)
        .AddMap("content", q => q.Content)
        .AddMap("taskId", q => q.TaskId)
        .AddMap("authorId", q => q.AuthorId)
        .AddMap("authorUsername", q => q.Author!.UserName)
        .AddMap("authorFullName", q => q.Author!.FullName)
        .AddMap("createdAt", q => q.CreatedAt)
        .AddMap("updatedAt", q => q.UpdatedAt);

    public static readonly IGridifyMapper<ProjectActivityLog> ProjectActivityLogMapper = new GridifyMapper<ProjectActivityLog>()
        .AddMap("id", q => q.Id)
        .AddMap("timestamp", q => q.Timestamp)
        .AddMap("eventType", q => q.EventType)
        .AddMap("userId", q => q.UserId)
        .AddMap("projectId", q => q.ProjectId)
        .AddMap("taskId", q => q.TaskId)
        .AddMap("createdAt", q => q.CreatedAt)
        .AddMap("updatedAt", q => q.UpdatedAt);

    public static readonly IGridifyMapper<Notification> NotificationMapper = new GridifyMapper<Notification>()
        .AddMap("id", q => q.Id)
        .AddMap("title", q => q.Title)
        .AddMap("message", q => q.Message)
        .AddMap("type", q => q.Type)
        .AddMap("isRead", q => q.IsRead)
        .AddMap("createdAt", q => q.CreatedAt)
        .AddMap("updatedAt", q => q.UpdatedAt);
}

