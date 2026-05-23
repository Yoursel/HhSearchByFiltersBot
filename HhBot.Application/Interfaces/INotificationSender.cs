namespace HhBot.Application.Interfaces;

public interface INotificationSender
{
    Task SendAsync(string message, CancellationToken cancellationToken);
}