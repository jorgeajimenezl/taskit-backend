using Microsoft.Extensions.Options;
using MimeKit;
using Taskit.Domain.Enums;
using Taskit.Domain.Events;
using Taskit.Notification.Worker.Interfaces;
using Taskit.Notification.Worker.Settings;

namespace Taskit.Notification.Worker.Services.MessageGenerators.Email;

public class ProjectActivityLogEmailMessageGenerator(IOptions<EmailSettings> options) : IEmailMessageGenerator<ProjectActivityLogCreated>
{
    private readonly EmailSettings _settings = options.Value;

    public Task<MimeMessage> GenerateAsync(ProjectActivityLogCreated @event, string recipientEmail, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_settings.From));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = GetSubject(@event);
        message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
        {
            Text = GetBody(@event)
        };
        return Task.FromResult(message);
    }

    private static string GetSubject(ProjectActivityLogCreated evt)
    {
        return evt.EventType switch
        {
            ProjectActivityLogEventType.TaskCreated => "Task created",
            ProjectActivityLogEventType.TaskAssigned => "Task assigned",
            ProjectActivityLogEventType.TaskUpdated => "Task updated",
            ProjectActivityLogEventType.TaskDeleted => "Task deleted",
            ProjectActivityLogEventType.TaskStatusChanged => "Task status changed",
            ProjectActivityLogEventType.CommentAdded => "Comment added",
            ProjectActivityLogEventType.UserJoinedProject => "User joined project",
            ProjectActivityLogEventType.UserLeftProject => "User left project",
            ProjectActivityLogEventType.FileAttached => "File attached",
            ProjectActivityLogEventType.FileUploaded => "File uploaded",
            ProjectActivityLogEventType.FileDeleted => "File deleted",
            ProjectActivityLogEventType.ProjectCreated => "Project created",
            ProjectActivityLogEventType.ProjectUpdated => "Project updated",
            ProjectActivityLogEventType.ProjectDeleted => "Project deleted",
            _ => "Project activity"
        };
    }

    private static string GetBody(ProjectActivityLogCreated evt)
    {
        return $"Event {@evt.EventType} occurred for project {@evt.ProjectId} task {@evt.TaskId}.";
    }
}
