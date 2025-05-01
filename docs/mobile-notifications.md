Hướng Dẫn Tích Hợp Thông Báo Mobile - Flutter
Tổng Quan
Tài liệu này hướng dẫn cách tích hợp hệ thống thông báo thời gian thực từ backend AiStroke vào ứng dụng Flutter mobile. Hệ thống sử dụng SignalR để gửi cảnh báo và thông báo theo thời gian thực tới người dùng trên mobile.
By Huy Nguyen Cute Pho Mai Que

Các Tính Năng
Nhận thông báo thời gian thực từ server
Lưu trữ thông báo cục bộ khi không có kết nối
Hiển thị thông báo theo 3 cấp độ khác nhau (thông tin, cảnh báo, nguy hiểm)
Hỗ trợ thông báo trong và ngoài ứng dụng
Hiển thị badge số lượng thông báo chưa đọc
Cài Đặt
1. Add Dependency
Thêm denpendency sau vào file pubspec.yaml:

```yaml
dependencies:
  flutter:
    sdk: flutter
  provider: ^6.0.5         
  signalr_netcore: ^1.3.3  # Client SignalR
  shared_preferences: ^2.1.1  
  http: ^0.13.6            # HTTP requests
  intl: ^0.18.1    
  ###   
  ```     
Update dependency: flutter pub get
####
Code
2. Tạo Model Thông Báo
Tạo file lib/models/notification_model.dart:

```Dart
class NotificationModel {
  final String id;
  final String title;
  final String message;
  final String type;
  final String timestamp;
  final bool isRead;

  NotificationModel({
    required this.id,
    required this.title,
    required this.message,
    required this.type,
    required this.timestamp,
    this.isRead = false,
  });

  factory NotificationModel.fromJson(Map<String, dynamic> json) {
    return NotificationModel(
      id: json['id']?.toString() ?? '',
      title: json['title'] ?? 'Thông báo',
      message: json['message'] ?? '',
      type: json['type']?.toString().toLowerCase() ?? 'info',
      timestamp: json['timestamp'] ?? DateTime.now().toIso8601String(),
      isRead: json['isRead'] ?? false,
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'title': title,
    'message': message,
    'type': type,
    'timestamp': timestamp,
    'isRead': isRead,
  };

  String getFormattedTime() {
    try {
      final dateTime = DateTime.parse(timestamp);
      return '${dateTime.day}/${dateTime.month}/${dateTime.year} ${dateTime.hour}:${dateTime.minute}';
    } catch (e) {
      return timestamp;
    }
  }
}
```
3. Thiết Lập API Service
Tạo hoặc cập nhật file lib/services/api_service.dart:

```dart
class ApiEndpoints {
  // Đường dẫn API đã có...
  
  // SignalR Hub URL
  static String get notificationHub {
    final domainBase = baseUrl.substring(0, baseUrl.lastIndexOf('/api'));
    return "$domainBase/notificationHub";
  }
  
  // Thông báo Endpoints
  static const String mobileNotifications = "$baseUrl/MobileNotifications";
  static String mobileNotificationsForUser(int userId) => "$mobileNotifications/user/$userId";
  static String markAsRead(String notificationId) => "$mobileNotifications/$notificationId/read";
}
```
4. Tạo SignalR Service
Tạo file lib/services/signalr_service.dart:

```Dart
import 'dart:async';
import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:signalr_netcore/hub_connection.dart';
import 'package:signalr_netcore/hub_connection_builder.dart';
import '../models/notification_model.dart';
import 'api_service.dart';

class SignalRService {
  static final SignalRService _instance = SignalRService._internal();
  factory SignalRService() => _instance;

  SignalRService._internal();

  HubConnection? _hubConnection;
  String _baseUrl = ApiEndpoints.notificationHub;
  int? _userId;
  final _notificationStreamController = StreamController<NotificationModel>.broadcast();
  
  // Getter để các widget có thể lắng nghe thông báo mới
  Stream<NotificationModel> get notificationStream => _notificationStreamController.stream;

  Future<void> initialize(int userId) async {
    _userId = userId;
    debugPrint('Khởi tạo SignalR service cho user: $userId');
    
    // Lưu userId hiện tại
    final prefs = await SharedPreferences.getInstance();
    await prefs.setInt('current_user_id', userId);
  }

  Future<void> connect() async {
    if (_hubConnection != null && _hubConnection!.state == HubConnectionState.Connected) {
      debugPrint('SignalR đã được kết nối');
      return;
    }

    if (_userId == null) {
      throw Exception('Bạn phải khởi tạo service với userId trước');
    }

    try {
      debugPrint('Đang kết nối đến SignalR: $_baseUrl?userId=$_userId');
      
      // Tạo hub connection
      _hubConnection = HubConnectionBuilder()
        .withUrl('$_baseUrl?userId=$_userId')
        .build();

      // Thiết lập xử lý sự kiện
      _setupSignalRCallbacks();

      // Bắt đầu kết nối
      await _hubConnection!.start();
      debugPrint('✅ Kết nối SignalR thành công');
    } catch (e) {
      debugPrint('❌ Lỗi kết nối SignalR: $e');
      rethrow;
    }
  }

  void _setupSignalRCallbacks() {
    _hubConnection!.on('ReceiveNotification', _handleReceiveNotification);
  }

  void _handleReceiveNotification(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      debugPrint('Nhận thông báo: ${data['title']}');

      // Tạo notification model
      final notification = NotificationModel.fromJson(data);

      // Lưu notification
      _saveNotification(notification);

      // Thêm vào stream để UI cập nhật
      _notificationStreamController.add(notification);
    } catch (e) {
      debugPrint('Lỗi xử lý thông báo: $e');
    }
  }

  Future<void> _saveNotification(NotificationModel notification) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      
      // Lấy thông báo đã lưu
      final notificationsJson = prefs.getString('notifications_$_userId') ?? '[]';
      List<dynamic> notificationsList = json.decode(notificationsJson);
      
      // Thêm thông báo mới vào đầu
      notificationsList.insert(0, notification.toJson());
      
      // Giới hạn số lượng thông báo lưu trữ
      if (notificationsList.length > 50) {
        notificationsList = notificationsList.take(50).toList();
      }
      
      // Lưu lại vào shared preferences
      await prefs.setString('notifications_$_userId', json.encode(notificationsList));
    } catch (e) {
      debugPrint('Lỗi lưu thông báo: $e');
    }
  }

  Future<void> disconnect() async {
    if (_hubConnection != null && 
        _hubConnection!.state == HubConnectionState.Connected) {
      await _hubConnection!.stop();
      debugPrint('SignalR đã ngắt kết nối');
    }
  }

  Future<List<NotificationModel>> getStoredNotifications() async {
    try {
      if (_userId == null) return [];
      
      final prefs = await SharedPreferences.getInstance();
      final notificationsJson = prefs.getString('notifications_$_userId') ?? '[]';
      List<dynamic> notificationsList = json.decode(notificationsJson);
      
      return notificationsList
          .map<NotificationModel>((jsonData) => NotificationModel.fromJson(jsonData))
          .toList();
    } catch (e) {
      debugPrint('Lỗi tải thông báo: $e');
      return [];
    }
  }
}
```
5. Tạo Notification Provider
Tạo file lib/providers/notification_provider.dart:

```Dart
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';
import '../models/notification_model.dart';
import '../services/signalr_service.dart';
import '../services/api_service.dart';

class NotificationProvider with ChangeNotifier {
  final SignalRService _signalRService = SignalRService();
  List<NotificationModel> _notifications = [];
  bool _isConnected = false;
  bool _isLoading = false;
  String? _error;
  int? _userId;

  List<NotificationModel> get notifications => _notifications;
  bool get isConnected => _isConnected;
  bool get isLoading => _isLoading;
  String? get error => _error;
  int? get userId => _userId;
  
  // Số thông báo chưa đọc
  int get unreadCount => _notifications.where((n) => !n.isRead).length;

  NotificationProvider() {
    // Lắng nghe thông báo mới
    _signalRService.notificationStream.listen((notification) {
      _notifications.insert(0, notification);
      notifyListeners();
    });
  }

  Future<void> initialize(int userId) async {
    try {
      _isLoading = true;
      _userId = userId;
      notifyListeners();

      await _signalRService.initialize(userId);
      
      _notifications = await _signalRService.getStoredNotifications();
      
      await _signalRService.connect();
      _isConnected = true;
      
      await fetchNotifications(userId);
      
      _error = null;
    } catch (e) {
      _error = e.toString();
      debugPrint('Lỗi khởi tạo thông báo: $_error');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> fetchNotifications(int userId) async {
    try {
      final response = await http.get(
        Uri.parse(ApiEndpoints.mobileNotificationsForUser(userId)),
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = json.decode(response.body);
        
        _notifications = data
            .map<NotificationModel>((item) => NotificationModel.fromJson(item))
            .toList();
            
        notifyListeners();
      } else {
        throw Exception('Không thể tải thông báo: ${response.statusCode}');
      }
    } catch (e) {
      debugPrint('Lỗi tải thông báo: $e');
    }
  }

  Future<void> markAsRead(String notificationId) async {
    try {
      final response = await http.put(
        Uri.parse(ApiEndpoints.markAsRead(notificationId)),
      );

      if (response.statusCode == 200) {
        final index = _notifications.indexWhere((n) => n.id == notificationId);
        if (index >= 0) {
          final updatedNotification = NotificationModel(
            id: _notifications[index].id,
            title: _notifications[index].title,
            message: _notifications[index].message,
            type: _notifications[index].type,
            timestamp: _notifications[index].timestamp,
            isRead: true,
          );
          
          _notifications[index] = updatedNotification;
          notifyListeners();
        }
      } else {
        throw Exception('Không thể đánh dấu đã đọc');
      }
    } catch (e) {
      debugPrint('Lỗi đánh dấu đã đọc: $e');
    }
  }

  Future<void> reconnect() async {
    if (_userId != null) {
      await _signalRService.connect();
      _isConnected = true;
      notifyListeners();
    }
  }

  Future<void> disconnect() async {
    await _signalRService.disconnect();
    _isConnected = false;
    notifyListeners();
  }
}
```
6. Đăng Ký Provider trong main.dart
Cập nhật file lib/main.dart:

```Dart
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'providers/notification_provider.dart';
// Import các provider khác nếu có

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  
  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => NotificationProvider()),
        // Thêm các provider khác nếu có
      ],
      child: const MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'AiStroke',
      theme: ThemeData(
        primarySwatch: Colors.blue,
        visualDensity: VisualDensity.adaptivePlatformDensity,
      ),
      home: const LoginScreen(), // Hoặc màn hình chính của ứng dụng
    );
  }
}
```
Tạo Giao Diện Thông Báo
1. Tạo Màn Hình Thông Báo
Tạo file lib/screens/notifications_screen.dart:

```Dart
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/notification_provider.dart';
import '../models/notification_model.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({Key? key}) : super(key: key);

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Thông báo'),
        actions: [
          // Hiển thị trạng thái kết nối
          Consumer<NotificationProvider>(
            builder: (context, provider, _) => Icon(
              provider.isConnected ? Icons.wifi : Icons.wifi_off,
              color: provider.isConnected ? Colors.green : Colors.grey,
            ),
          ),
          
          // Nút làm mới
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () {
              final provider = Provider.of<NotificationProvider>(context, listen: false);
              if (provider.userId != null) {
                provider.fetchNotifications(provider.userId!);
              }
            },
          ),
        ],
      ),
      body: Consumer<NotificationProvider>(
        builder: (context, provider, _) {
          if (provider.isLoading) {
            return const Center(child: CircularProgressIndicator());
          }
          
          if (provider.error != null) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    'Đã xảy ra lỗi: ${provider.error}',
                    style: const TextStyle(color: Colors.red),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () {
                      if (provider.userId != null) {
                        provider.initialize(provider.userId!);
                      }
                    },
                    child: const Text('Thử lại'),
                  ),
                ],
              ),
            );
          }
          
          if (provider.notifications.isEmpty) {
            return const Center(
              child: Text('Không có thông báo nào'),
            );
          }
          
          return RefreshIndicator(
            onRefresh: () async {
              if (provider.userId != null) {
                await provider.fetchNotifications(provider.userId!);
              }
            },
            child: ListView.builder(
              itemCount: provider.notifications.length,
              itemBuilder: (context, index) {
                final notification = provider.notifications[index];
                return NotificationTile(
                  notification: notification,
                  onTap: () {
                    provider.markAsRead(notification.id);
                  },
                );
              },
            ),
          );
        },
      ),
    );
  }
}

class NotificationTile extends StatelessWidget {
  final NotificationModel notification;
  final VoidCallback onTap;

  const NotificationTile({
    Key? key,
    required this.notification,
    required this.onTap,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      child: ListTile(
        onTap: onTap,
        leading: _getNotificationIcon(),
        title: Text(
          notification.title,
          style: TextStyle(
            fontWeight: notification.isRead ? FontWeight.normal : FontWeight.bold,
          ),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              notification.message,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
            ),
            const SizedBox(height: 4),
            Text(
              notification.getFormattedTime(),
              style: TextStyle(
                fontSize: 12,
                color: Colors.grey[600],
              ),
            ),
          ],
        ),
        isThreeLine: true,
      ),
    );
  }

  Widget _getNotificationIcon() {
    IconData iconData;
    Color color;
    
    switch (notification.type.toLowerCase()) {
      case 'warning':
        iconData = Icons.warning_amber_rounded;
        color = Colors.red;
        break;
      case 'risk':
        iconData = Icons.warning_rounded;
        color = Colors.orange;
        break;
      default:
        iconData = Icons.info_outline;
        color = Colors.blue;
    }
    
    return CircleAvatar(
      backgroundColor: color.withOpacity(0.2),
      child: Icon(iconData, color: color),
    );
  }
}
```
2. Tạo Widget Button Thông Báo với Badge
Tạo file lib/widgets/notification_button.dart:

```Dart
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/notification_provider.dart';
import '../screens/notifications_screen.dart';

class NotificationButton extends StatelessWidget {
  const NotificationButton({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Consumer<NotificationProvider>(
      builder: (context, provider, _) {
        final unreadCount = provider.unreadCount;
        
        return Stack(
          children: [
            IconButton(
              icon: const Icon(Icons.notifications),
              onPressed: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const NotificationsScreen()),
              ),
            ),
            if (unreadCount > 0)
              Positioned(
                top: 5,
                right: 5,
                child: Container(
                  padding: const EdgeInsets.all(2),
                  decoration: BoxDecoration(
                    color: Colors.red,
                    borderRadius: BorderRadius.circular(10),
                  ),
                  constraints: const BoxConstraints(
                    minWidth: 16,
                    minHeight: 16,
                  ),
                  child: Text(
                    unreadCount > 99 ? '99+' : unreadCount.toString(),
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 10,
                    ),
                    textAlign: TextAlign.center,
                  ),
                ),
              )
          ],
        );
      },
    );
  }
}
```
3. Thêm Button Thông Báo vào AppBar
Trong màn hình chính của ứng dụng:

```Dart
AppBar(
  title: const Text('AiStroke'),
  actions: [
    
    const NotificationButton(),
  ],
)
Khởi Tạo Kết Nối Thông Báo
Thêm đoạn code dưới đây vào lib/screens/login_screen.dart sau khi đăng nhập thành công:
```
```Dart
// Sau khi đăng nhập thành công
final userId = response.userId; // Hoặc ID của người dùng đã đăng nhập
await Provider.of<NotificationProvider>(context, listen: false).initialize(userId);

Navigator.pushAndRemoveUntil(
  context,
  MaterialPageRoute(builder: (_) => const HomeScreen()),
  (route) => false,
);
```
Kiểm Tra và Gỡ Lỗi
1. Không kết nối được
Đảm bảo:

URL server đúng
Đối với máy ảo Android, thay localhost bằng 10.0.2.2
Đối với thiết bị thực, sử dụng địa chỉ IP hoặc tên miền thực
```Dart
// Kiểm tra URL
print("SignalR URL: ${ApiEndpoints.notificationHub}?userId=$userId");
```
2. Không nhận được thông báo
Thêm logging để kiểm tra:

```Dart
// Trong SignalRService._handleReceiveNotification
debugPrint('Nhận thông báo raw data: $arguments');
```
3. Test gửi thông báo
Tạo hàm test trong NotificationProvider:

```Dart
Future<bool> sendTestNotification() async {
  if (_userId == null) return false;
  
  try {
    final response = await http.post(
      Uri.parse('${ApiEndpoints.mobileNotifications}/test'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'userId': _userId,
        'title': 'Test Notification',
        'message': 'Đây là thông báo test từ Flutter app',
        'type': 'info'
      }),
    );
    
    return response.statusCode == 200;
  } catch (e) {
    debugPrint('Lỗi gửi thông báo test: $e');
    return false;
  }
}
```
Mức Độ Thông Báo
info: Thông báo thông thường, thông tin chung
risk: Cảnh báo mức trung bình
warning: Cảnh báo mức cao, nguy hiểm
Danh Sách Kiểm Tra
 Thêm các dependency cần thiết vào pubspec.yaml
 Tạo model thông báo (notification_model.dart)
 Tạo service SignalR (signalr_service.dart)
 Tạo provider thông báo (notification_provider.dart)
 Đăng ký provider trong main.dart
 Tạo màn hình thông báo (notifications_screen.dart)
 Tạo widget badge thông báo (notification_button.dart)
 Khởi tạo kết nối thông báo sau khi đăng nhập
 Kiểm tra hiển thị thông báo với các loại khác nhau
 Kiểm tra đánh dấu đã đọc thông báo
 Kiểm tra hiển thị badge số lượng thông báo chưa đọc
Kiểm Tra URL Cho Từng Môi Trường
Môi trường	API URL	SignalR URL
Development	http://localhost:5062/api	http://localhost:5062/notificationHub
Android Emulator	http://10.0.2.2:5062/api	http://10.0.2.2:5062/notificationHub
Production	http://137.59.106.46:5000/api	http://137.59.106.46:5000/notificationHub
Cập nhật lần cuối: 2025-05-01 15:29:54
Tác giả: Huy Nguyen Cute Pho Mai Que