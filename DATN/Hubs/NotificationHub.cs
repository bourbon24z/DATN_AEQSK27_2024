using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace DATN.Hubs
{
    public class NotificationHub : Hub
    {
        
        public override async Task OnConnectedAsync()
        {
            try
            {
               
                var userId = Context.GetHttpContext().Request.Query["userId"].ToString();

                if (!string.IsNullOrEmpty(userId))
                {
                    
                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                    Console.WriteLine($"User {userId} connected with connection ID: {Context.ConnectionId}");

                   
                    await Clients.Client(Context.ConnectionId).SendAsync("ConnectionConfirmed",
                        new { userId = userId, connectionId = Context.ConnectionId, timestamp = DateTime.Now });
                }
                else
                {
                    Console.WriteLine($"Connection established but no userId provided. ConnectionId: {Context.ConnectionId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
            }

            await base.OnConnectedAsync();
        }

       
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = Context.GetHttpContext().Request.Query["userId"].ToString();

                if (!string.IsNullOrEmpty(userId))
                {
                   
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                    Console.WriteLine($"User {userId} disconnected");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Client {Context.ConnectionId} joined group: {groupName}");
            await Clients.Caller.SendAsync("GroupJoined", groupName);
        }

      
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Client {Context.ConnectionId} left group: {groupName}");
            await Clients.Caller.SendAsync("GroupLeft", groupName);
        }
    }
}