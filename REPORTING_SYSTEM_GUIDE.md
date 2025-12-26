# Hướng dẫn Hệ thống Báo cáo Thống kê

## Tổng quan
Hệ thống báo cáo thống kê MonAmour cung cấp các báo cáo chi tiết về doanh thu, người dùng, sản phẩm, đơn hàng và hiệu suất đối tác.

## Các tính năng chính

### 1. Báo cáo Doanh thu (`/Report/RevenueReport`)
- **Tổng doanh thu**: Doanh thu từ đơn hàng và đặt chỗ
- **Phân tích theo tháng**: Biểu đồ doanh thu theo từng tháng
- **Phân tích theo ngày**: Biểu đồ doanh thu theo từng ngày
- **Tăng trưởng**: Tỷ lệ tăng trưởng so với kỳ trước
- **Phân bố doanh thu**: Tỷ lệ doanh thu từ đơn hàng vs đặt chỗ

**Tính năng:**
- Lọc theo khoảng thời gian
- Xuất Excel
- Làm mới dữ liệu real-time
- Biểu đồ tương tác

### 2. Thống kê Người dùng (`/Report/UserStatistics`)
- **Tổng người dùng**: Số lượng người dùng đã đăng ký
- **Người dùng hoạt động**: Số người dùng có trạng thái active
- **Người dùng mới**: Số người đăng ký trong 30 ngày gần nhất
- **Phân bố giới tính**: Thống kê theo giới tính (Nam/Nữ/Khác)
- **Đăng ký theo ngày**: Biểu đồ đăng ký mới theo thời gian
- **Hoạt động người dùng**: Số đơn hàng và đặt chỗ mới

**Tính năng:**
- Lọc theo khoảng thời gian
- Biểu đồ đăng ký và hoạt động
- Xuất Excel
- Thống kê chi tiết

### 3. Phân tích Dữ liệu (`/Report/DataAnalysis`)
- **Thống kê đơn hàng**: Tổng quan về đơn hàng theo trạng thái
- **Thống kê sản phẩm**: Tổng quan về sản phẩm và tồn kho
- **Thống kê đặt chỗ**: Tổng quan về đặt chỗ theo trạng thái
- **Sản phẩm bán chạy**: Top sản phẩm bán được nhiều nhất
- **Sản phẩm sắp hết hàng**: Cảnh báo sản phẩm cần nhập thêm
- **Concept phổ biến**: Top concept được đặt nhiều nhất

**Tính năng:**
- Lọc theo loại báo cáo và thời gian
- Bảng dữ liệu chi tiết
- Biểu đồ phân bố
- Cảnh báo tồn kho

### 4. Hiệu suất Đối tác (`/Report/PartnerPerformance`)
- **Tổng quan đối tác**: Số lượng đối tác theo trạng thái
- **Bảng hiệu suất**: Chi tiết hiệu suất từng đối tác
- **Doanh thu theo đối tác**: Biểu đồ doanh thu
- **Số đặt chỗ theo đối tác**: Biểu đồ số lượng đặt chỗ
- **Đánh giá đối tác**: Hệ thống rating (nếu có)

**Tính năng:**
- Sắp xếp theo doanh thu
- Lọc theo thời gian
- Xuất Excel
- Xem chi tiết đối tác

## Cấu trúc dữ liệu

### ViewModels chính:
- `RevenueReportViewModel`: Báo cáo doanh thu
- `UserStatisticsViewModel`: Thống kê người dùng
- `OrderStatisticsViewModel`: Thống kê đơn hàng
- `ProductStatisticsViewModel`: Thống kê sản phẩm
- `BookingStatisticsViewModel`: Thống kê đặt chỗ
- `PartnerStatisticsViewModel`: Thống kê đối tác
- `DashboardSummaryViewModel`: Tổng quan dashboard

### Service Interface:
- `IReportService`: Interface chính cho tất cả báo cáo
- `ReportService`: Implementation với logic xử lý dữ liệu

### Controller:
- `ReportController`: Xử lý các request báo cáo
- API endpoints cho AJAX calls
- Export functionality

## Cách sử dụng

### 1. Truy cập báo cáo
- Đăng nhập với tài khoản admin
- Vào menu "BÁO CÁO & THỐNG KÊ"
- Chọn loại báo cáo cần xem

### 2. Lọc dữ liệu
- Sử dụng bộ lọc ở đầu mỗi trang
- Chọn khoảng thời gian
- Chọn loại báo cáo (nếu có)
- Nhấn "Lọc" để cập nhật dữ liệu

### 3. Xuất dữ liệu
- Nhấn nút "Xuất Excel" để tải file Excel
- Dữ liệu sẽ được xuất theo bộ lọc hiện tại

### 4. Làm mới dữ liệu
- Nhấn nút "Làm mới" để cập nhật dữ liệu real-time
- Dữ liệu sẽ được tải lại từ database

## API Endpoints

### Revenue Data
```
GET /Report/GetRevenueData?fromDate=2024-01-01&toDate=2024-12-31
```

### User Data
```
GET /Report/GetUserData?fromDate=2024-01-01&toDate=2024-12-31
```

### Order Data
```
GET /Report/GetOrderData?fromDate=2024-01-01&toDate=2024-12-31
```

### Product Data
```
GET /Report/GetProductData
```

### Booking Data
```
GET /Report/GetBookingData?fromDate=2024-01-01&toDate=2024-12-31
```

### Partner Data
```
GET /Report/GetPartnerData?fromDate=2024-01-01&toDate=2024-12-31
```

## Cấu hình

### 1. Đăng ký Service
Trong `Program.cs`:
```csharp
builder.Services.AddScoped<IReportService, ReportService>();
```

### 2. Database Requirements
- Cần có dữ liệu trong các bảng: Orders, Bookings, Users, Products, Partners
- Các bảng cần có đầy đủ foreign keys
- Cần có dữ liệu mẫu để test

### 3. Chart.js
- Sử dụng Chart.js cho các biểu đồ
- CDN: `https://cdn.jsdelivr.net/npm/chart.js`

## Troubleshooting

### 1. Không hiển thị dữ liệu
- Kiểm tra kết nối database
- Kiểm tra dữ liệu trong các bảng
- Xem log trong console để debug

### 2. Lỗi biểu đồ
- Kiểm tra dữ liệu JSON được truyền vào
- Kiểm tra console browser để xem lỗi JavaScript
- Đảm bảo Chart.js được load đúng

### 3. Lỗi xuất Excel
- Kiểm tra quyền ghi file
- Kiểm tra cấu hình export
- Xem log server để debug

## Mở rộng

### 1. Thêm báo cáo mới
1. Tạo ViewModel mới trong `ReportViewModel.cs`
2. Thêm method trong `IReportService`
3. Implement trong `ReportService`
4. Thêm action trong `ReportController`
5. Tạo view mới

### 2. Thêm biểu đồ mới
1. Thêm canvas element trong view
2. Tạo Chart.js instance
3. Cập nhật dữ liệu qua AJAX
4. Thêm styling phù hợp

### 3. Thêm export format
1. Thêm method export trong controller
2. Sử dụng thư viện như EPPlus cho Excel
3. Tạo template export
4. Thêm nút export trong view

## Performance

### 1. Tối ưu database
- Sử dụng index cho các cột thường query
- Sử dụng pagination cho dữ liệu lớn
- Cache kết quả query nếu cần

### 2. Tối ưu frontend
- Lazy load cho biểu đồ
- Debounce cho filter
- Cache dữ liệu trong session

### 3. Monitoring
- Log performance metrics
- Monitor memory usage
- Track query execution time

## Bảo mật

### 1. Authorization
- Chỉ admin mới được truy cập báo cáo
- Kiểm tra quyền trong controller
- Validate input parameters

### 2. Data Privacy
- Không hiển thị thông tin nhạy cảm
- Mã hóa dữ liệu nếu cần
- Audit log cho các truy cập

## Kết luận

Hệ thống báo cáo thống kê MonAmour cung cấp đầy đủ các tính năng cần thiết cho việc quản lý và phân tích dữ liệu. Với kiến trúc modular, dễ dàng mở rộng và tùy chỉnh theo nhu cầu cụ thể.
