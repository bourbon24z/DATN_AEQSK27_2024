Hướng dẫn cho thông báo
<div align="center"> <img src="https://via.placeholder.com/800x200?text=AiStroke+Notification+System" alt="Notification System Banner"> </div>
📱 Hướng Dẫn Cho Từng Nền Tảng
<table style="width:100%; border-collapse: collapse; text-align: center;">
  <tr>
    <td style="width:50%; padding: 20px;">
      <img src="https://via.placeholder.com/100x100?text=Web" alt="Web Frontend" width="100" style="margin-bottom: 10px;"><br>
      <h3 style="margin: 10px 0;">Web Frontend</h3>
      <a href="docs/web-notifications.md">
        <img src="https://img.shields.io/badge/Xem%20Hướng%20Dẫn-blue?style=for-the-badge" alt="Xem hướng dẫn Web">
      </a>
    </td>
    <td style="width:50%; padding: 20px;">
      <img src="https://via.placeholder.com/100x100?text=Mobile" alt="Mobile Frontend" width="100" style="margin-bottom: 10px;"><br>
      <h3 style="margin: 10px 0;">Mobile Frontend</h3>
      <a href="docs/mobile-notifications.md">
        <img src="https://img.shields.io/badge/Xem%20Hướng%20Dẫn-green?style=for-the-badge" alt="Xem hướng dẫn Mobile">
      </a>
    </td>
  </tr>
</table>


🌟 Tính Năng Chung
Thông báo thời gian thực qua SignalR
Hiển thị thông báo theo nhiều cấp độ (thông tin, cảnh báo, nguy hiểm)
Lưu trữ lịch sử thông báo
Đánh dấu thông báo đã đọc
Hiển thị badge số lượng thông báo chưa đọc
🔧 Công Nghệ Sử Dụng
Backend: ASP.NET Core, SignalR Hubs
Web Frontend: JavaScript, HTML/CSS, SignalR Client
Mobile: Flutter, SignalR .NET Core Client
📊 Kiến Trúc Hệ Thống
Code
┌───────────────┐      ┌──────────────┐
│ Web Frontend  │◄────►│              │
└───────────────┘      │              │
                       │   SignalR    │
┌───────────────┐      │     Hub      │
│Mobile Frontend│◄────►│              │
└───────────────┘      └──────┬───────┘
                              │
                       ┌──────▼───────┐
                       │   Services   │
                       │  Controllers │
                       └──────────────┘
🔄 Quy Trình Thông Báo
Backend phát hiện sự kiện cần thông báo
Gửi thông báo qua SignalR Hub
Frontend nhận và hiển thị thông báo
Người dùng có thể đánh dấu thông báo đã đọc
📚 API Endpoints
Endpoint	Method	Mô tả
/notificationHub	SignalR	Hub xử lý kết nối WebSocket
/api/MobileNotifications/user/{userId}	GET	Lấy thông báo của người dùng
/api/MobileNotifications/{id}/read	PUT	Đánh dấu thông báo đã đọc
/api/MobileNotifications/test	POST	Gửi thông báo test
📅 Cập Nhật Gần Đây
2025-05-01: Thêm hướng dẫn cho Mobile Frontend
2025-04-28: Cập nhật hướng dẫn Web Frontend
📋 Kiểm Tra Môi Trường
Môi trường	API URL	SignalR URL
Development	http://localhost:5062/api	http://localhost:5062/notificationHub
Android Emulator	http://10.0.2.2:5062/api	http://10.0.2.2:5062/notificationHub
Production	http://137.59.106.46:5000/api	http://137.59.106.46:5000/notificationHub
📚 Tài Nguyên Bổ Sung
Tài Liệu ASP.NET Core SignalR
Flutter SignalR Client
JavaScript SignalR Client
Cập nhật lần cuối: 2025-05-01 15:50
Tác giả: Huy Nguyen Cute Pho Mai Que