# Hướng Dẫn Tích Hợp Thông Báo Web - Frontend

## Tổng Quan

Hướng dẫn cách tích hợp và hiển thị thông báo từ backend của kao. Sử dùng SignalR

## Các Tính Năng

- Nhận thông báo thời gian thực từ hệ thống
- Hiển thị thông báo với nhiều cấp độ khác nhau (thông thường, cảnh báo, nguy hiểm)
- Hỗ trợ hiển thị định dạng HTML trong thông báo
- Khả năng hiển thị vị trí trên bản đồ

## Cài Đặt

### 1. Thêm SignalR Client vào Dự Án

```html
<!-- Thêm SignalR Client từ CDN -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.min.js"></script>
```

Hoặc sử dụng npm:

```bash
npm install @microsoft/signalr
```

```javascript
// Import trong JavaScript
import * as signalR from "@microsoft/signalr";
```

### 2. Thêm CSS cho Thông Báo

```html
<link rel="stylesheet" href="css/notifications.css">
```

```css
/* notifications.css */
.notifications-container {
    position: fixed;
    top: 20px;
    right: 20px;
    width: 350px;
    max-width: 80%;
    max-height: 80vh;
    overflow-y: auto;
    z-index: 1000;
}

.notification {
    margin-bottom: 15px;
    padding: 15px;
    border-radius: 5px;
    box-shadow: 0 2px 5px rgba(0,0,0,0.2);
    animation: slideIn 0.3s ease-out forwards;
}

@keyframes slideIn {
    from { transform: translateX(100%); opacity: 0; }
    to { transform: translateX(0); opacity: 1; }
}

.notification h3 {
    margin-top: 0;
    margin-bottom: 10px;
    font-size: 18px;
    font-weight: bold;
}

.notification-content {
    white-space: pre-wrap;
    line-height: 1.5;
}

.notification-time {
    font-size: 12px;
    color: #888;
    display: block;
    text-align: right;
    margin-top: 10px;
}

/* Màu sắc cho các loại thông báo */
.notification.info {
    background-color: #d1ecf1;
    border-left: 5px solid #17a2b8;
}

.notification.warning {
    background-color: #fff3cd;
    border-left: 5px solid #ffc107;
}

.notification.risk {
    background-color: #f8d7da;
    border-left: 5px solid #dc3545;
}
```

### 3. Thêm Container HTML để Hiển Thị Thông Báo

```html
<div id="notifications-container" class="notifications-container"></div>
```

## Kết Nối và Xử Lý Thông Báo

### 1. Thiết Lập Kết Nối SignalR

```javascript
// notifications.js
class NotificationService {
    constructor(userId) {
        this.userId = userId;
        this.connection = null;
        this.soundEnabled = true;
        this.initialize();
    }

    initialize() {
        // Tạo kết nối SignalR
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Thử kết nối lại sau các khoảng thời gian
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Lắng nghe sự kiện nhận thông báo
        this.connection.on("ReceiveNotification", (title, message, notificationType) => {
            this.displayNotification(title, message, notificationType);
        });

        // Bắt đầu kết nối
        this.connection.start()
            .then(() => {
                console.log("Kết nối SignalR thành công");
                // Đăng ký nhận thông báo cho người dùng hiện tại
                this.connection.invoke("RegisterForNotifications", this.userId);
            })
            .catch(err => {
                console.error("Lỗi khi kết nối SignalR:", err);
                // Thử kết nối lại sau 5 giây
                setTimeout(() => this.initialize(), 5000);
            });

        // Xử lý kết nối đóng
        this.connection.onclose(() => {
            console.log("Kết nối SignalR bị đóng");
        });
    }

    displayNotification(title, message, type = 'info') {
        // Tạo phần tử thông báo
        const notificationElement = document.createElement("div");
        notificationElement.className = `notification ${type}`;
        
        // Định dạng thời gian
        const timeString = new Date().toLocaleTimeString('vi-VN', { 
            hour: '2-digit', 
            minute: '2-digit',
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
        
        // HTML cho thông báo
        notificationElement.innerHTML = `
            <h3>${title}</h3>
            <div class="notification-content">${message}</div>
            <span class="notification-time">${timeString}</span>
            <button class="notification-close">&times;</button>
        `;
        
        // Thêm vào container
        const container = document.getElementById("notifications-container");
        container.appendChild(notificationElement);
        
        // Thêm event listener cho nút đóng
        const closeButton = notificationElement.querySelector('.notification-close');
        closeButton.addEventListener('click', () => {
            notificationElement.remove();
        });
        
        // Tự động đóng sau 1 phút nếu không phải thông báo nguy hiểm
        if (type !== 'risk') {
            setTimeout(() => {
                if (notificationElement.parentNode) {
                    notificationElement.classList.add('fade-out');
                    setTimeout(() => notificationElement.remove(), 500);
                }
            }, 60000);
        }
        
        
}
```

### 2. Khởi Tạo Dịch Vụ Thông Báo

```javascript
// Thêm vào file main.js hoặc script chính của trang
document.addEventListener('DOMContentLoaded', () => {
    // Lấy user ID từ localStorage hoặc từ biến toàn cục đã được định nghĩa
    const userId = window.currentUserId || localStorage.getItem('userId');
    
    if (userId) {
        // Khởi tạo dịch vụ thông báo
        window.notificationService = new NotificationService(userId);
        
        // Thêm nút bật/tắt âm thanh nếu cần
        const soundToggle = document.getElementById('toggle-notification-sound');
        if (soundToggle) {
            soundToggle.addEventListener('click', function() {
                const enabled = this.classList.toggle('active');
                window.notificationService.toggleSound(enabled);
                this.textContent = enabled ? 'Tắt âm thanh' : 'Bật âm thanh';
            });
        }
    } else {
        console.warn("Không tìm thấy ID người dùng, không thể khởi tạo thông báo");
    }
});
```

## Hiển Thị Thông Báo Đúng Cách


### 2. Xử Lý HTML Trong Thông Báo

Thông báo từ server có thể chứa HTML để định dạng nội dung tốt hơn. Đảm bảo:

1. Thuộc tính innerHTML được sử dụng thay vì textContent
2. Áp dụng CSS phù hợp cho nội dung HTML bên trong thông báo

```css
/* Thêm vào CSS */
.notification-content ul {
    margin: 5px 0;
    padding-left: 20px;
}

.notification-content a {
    color: #0066cc;
    text-decoration: none;
}

.notification-content a:hover {
    text-decoration: underline;
}
```

### 3. Xử Lý Liên Kết Bản Đồ

```javascript
// Thêm vào NotificationService
handleMapLink(notificationElement) {
    const mapLinks = notificationElement.querySelectorAll('a[href*="openstreetmap.org"]');
    mapLinks.forEach(link => {
        link.setAttribute('target', '_blank');
        link.setAttribute('rel', 'noopener noreferrer');
        
        // Tùy chọn: thay đổi văn bản liên kết
        link.textContent = 'Xem vị trí trên bản đồ';
        
        // Thêm biểu tượng bản đồ
        const icon = document.createElement('i');
        icon.className = 'map-icon';
        link.prepend(icon);
    });
}
```

## Demo Thông Báo HTML From Huy Nguyen Cute


```html
<div style='font-weight: bold; font-size: 1.2em; margin-bottom: 10px;'>❗ CẢNH BÁO ❗</div>
<div style='margin-bottom: 10px;'>⏰ Thời gian phát hiện: 28/04/2025 14:00</div>
<div style='margin-bottom: 10px;'>
    <div><strong>📊 CHI TIẾT:</strong></div>
    <div style='margin-left: 20px;'>• Nhiệt độ: 38.5°C (bình thường: 37 ±0.5°C, Nguy hiểm)</div>
    <div style='margin-left: 20px;'>• Huyết áp tâm thu: 170 mmHg (bình thường: ≤140, Nguy hiểm)</div>
    <div style='margin-left: 20px;'>• Nhịp tim: 95 bpm (bình thường: 60–90, Cảnh báo)</div>
    <div style='margin-left: 20px;'>• SPO2: 93% (bình thường: ≥95%, Cảnh báo)</div>
    <div style='margin-left: 20px;'>• Độ pH máu: 7.32 (bình thường: 7.4 ±0.05, Cảnh báo)</div>
</div>
<div style='margin-bottom: 10px;'>
    <div><strong>📍 VỊ TRÍ:</strong></div>
    <a href='https://www.openstreetmap.org/?mlat=10.823099&mlon=106.62966&zoom=15' target='_blank'>Xem bản đồ</a>
</div>
<div style='margin-top: 10px; font-style: italic;'>⚠️ Vui lòng kiểm tra sức khỏe hoặc liên hệ với bác sĩ nếu tình trạng kéo dài.</div>
```

## Loại Thông Báo

- **info**: Thông báo thông thường, thông tin chung
- **risk**: Cảnh báo sức khỏe
- **warning**: Cảnh báo nghiêm trọng, nguy hiểm

## Xử Lý Lỗi Thường Gặp

### 1. Không nhận được thông báo

- Kiểm tra kết nối SignalR trong Console trình duyệt
- Đảm bảo đã đăng ký đúng userId
- Kiểm tra lại endpoint của SignalR hub có đúng không

```javascript
// Kiểm tra trạng thái kết nối
console.log("Trạng thái kết nối:", window.notificationService.connection.state);
```

## Danh Sách Kiểm Tra

- [ ] Thêm SignalR Client vào dự án
- [ ] Thêm container HTML để hiển thị thông báo
- [ ] Khởi tạo dịch vụ thông báo với userId
- [ ] Kiểm tra hiển thị thông báo với các loại khác nhau
- [ ] Kiểm tra xem thông báo có xuống dòng đúng không
- [ ] Kiểm tra liên kết bản đồ có hoạt động không