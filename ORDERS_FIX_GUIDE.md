# Hướng dẫn sửa lỗi trang Orders.cshtml

## Vấn đề
Trang Orders.cshtml không hiển thị được dữ liệu từ database.

## Nguyên nhân
1. Logic lọc trong OrderService quá nghiêm ngặt - chỉ lấy orders đã thanh toán
2. Có thể không có dữ liệu mẫu trong database
3. Thiếu một số trạng thái đơn hàng

## Giải pháp đã thực hiện

### 1. Sửa OrderService.cs
- **File**: `Services/Implements/OrderService.cs`
- **Thay đổi**: 
  - Bỏ điều kiện `o.PaymentDetails.Any()` trong `GetOrdersAsync()`
  - Chỉ lọc bỏ orders có status "cart"
  - Thêm logging để debug
  - Cập nhật `GetOrderStatisticsAsync()` và `GetRecentOrdersAsync()`
  - Thêm trạng thái "pending" và "cancelled"

### 2. Cập nhật Orders.cshtml
- **File**: `Views/Admin/Orders.cshtml`
- **Thay đổi**:
  - Thêm hiển thị cho trạng thái "pending" và "cancelled"
  - Thêm thống kê bổ sung (Chờ xử lý, Đã hủy, Chưa thanh toán, Đã thanh toán)
  - Cập nhật JavaScript để refresh các thống kê mới

### 3. Thêm dữ liệu mẫu
- **File**: `Scripts/AddSampleOrders.sql`
- **Mục đích**: Tạo dữ liệu mẫu để test trang Orders

## Cách chạy

### Bước 1: Chạy script SQL
```sql
-- Mở SQL Server Management Studio hoặc Azure Data Studio
-- Kết nối đến database MonAmourDb
-- Chạy file Scripts/AddSampleOrders.sql
```

### Bước 2: Kiểm tra kết quả
1. Chạy ứng dụng: `dotnet run`
2. Đăng nhập với tài khoản admin
3. Truy cập trang Orders: `/Admin/Orders`
4. Kiểm tra:
   - Có hiển thị danh sách đơn hàng
   - Thống kê hiển thị đúng
   - Có thể tìm kiếm và lọc
   - Có thể cập nhật trạng thái đơn hàng

## Các trạng thái đơn hàng được hỗ trợ

| Trạng thái | Mô tả | Badge Color |
|------------|-------|-------------|
| pending | Chờ xử lý | Secondary (xám) |
| confirmed | Đã xác nhận | Success (xanh lá) |
| shipping | Đang giao hàng | Warning (vàng) |
| completed | Hoàn thành | Info (xanh dương) |
| cancelled | Đã hủy | Danger (đỏ) |

## Tính năng mới

### Thống kê bổ sung
- **Chờ xử lý**: Số đơn hàng đang chờ xử lý
- **Đã hủy**: Số đơn hàng đã bị hủy
- **Chưa thanh toán**: Số đơn hàng chưa có thanh toán
- **Đã thanh toán**: Số đơn hàng đã có thanh toán

### Debug logging
- Thêm logging trong OrderService để theo dõi số lượng orders
- Có thể xem log trong console để debug

## Lưu ý
- Script SQL sẽ tạo dữ liệu mẫu nếu chưa có
- Có thể chạy script nhiều lần mà không bị duplicate
- Dữ liệu mẫu bao gồm: Users, Products, Orders, OrderItems, Payments, PaymentDetails

## Troubleshooting

### Nếu vẫn không hiển thị dữ liệu:
1. Kiểm tra connection string trong `appsettings.json`
2. Kiểm tra log trong console để xem số lượng orders
3. Kiểm tra database có dữ liệu không:
   ```sql
   SELECT COUNT(*) FROM [Order] WHERE status != 'cart';
   ```

### Nếu có lỗi SQL:
1. Kiểm tra database có tồn tại không
2. Kiểm tra quyền truy cập database
3. Chạy từng phần của script SQL

## Kết quả mong đợi
- Trang Orders hiển thị đầy đủ danh sách đơn hàng
- Thống kê hiển thị chính xác
- Có thể quản lý đơn hàng (xem, cập nhật trạng thái)
- Giao diện đẹp và responsive
