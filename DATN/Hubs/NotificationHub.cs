using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace DATN.Hubs
{
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Xử lý sự kiện khi client kết nối đến hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            try
            {
                // Lấy userId từ query string
                var userId = Context.GetHttpContext().Request.Query["userId"].ToString();

                if (!string.IsNullOrEmpty(userId))
                {
                    // Thêm user vào group có tên là userId để gửi thông báo riêng
                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                    Console.WriteLine($"User {userId} connected with connection ID: {Context.ConnectionId}");

                    // Gửi thông báo xác nhận kết nối thành công
                    await Clients.Client(Context.ConnectionId).SendAsync("ConnectionConfirmed",
                        new { userId = userId, connectionId = Context.ConnectionId, timestamp = DateTime.UtcNow });
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

        /// <summary>
        /// Xử lý sự kiện khi client ngắt kết nối
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                // Lấy userId từ query string
                var userId = Context.GetHttpContext().Request.Query["userId"].ToString();

                if (!string.IsNullOrEmpty(userId))
                {
                    // Xóa user khỏi group khi ngắt kết nối
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

        /// <summary>
        /// Phương thức cho phép client tham gia vào group cụ thể
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Client {Context.ConnectionId} joined group: {groupName}");
            await Clients.Caller.SendAsync("GroupJoined", groupName);
        }

        /// <summary>
        /// Phương thức cho phép client rời khỏi group
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Client {Context.ConnectionId} left group: {groupName}");
            await Clients.Caller.SendAsync("GroupLeft", groupName);
        }
    }
}