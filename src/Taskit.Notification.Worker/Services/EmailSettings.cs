namespace Taskit.Notification.Worker.Services;

public class EmailSettings
{
    public required string Host { get; init; }
    public int Port { get; init; } = 587;
    public string? Username { get; init; }
    public string? Password { get; init; }
    public bool UseSsl { get; init; } = true;
    public required string From { get; init; }
}
