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
    }
}