using Microsoft.AspNetCore.SignalR;
using MonAmour.Helpers;

namespace MonAmour.Hubs;

public class CommentHub : Hub<ICommentHubClient>
{
    public async Task JoinBlogGroup(string blogId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"blog_{blogId}");
    }

    public async Task LeaveBlogGroup(string blogId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"blog_{blogId}");
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}

public interface ICommentHubClient
{
    Task ReceiveComment(object comment);
    Task CommentDeleted(int commentId);
    Task CommentUpdated(object comment);
}
