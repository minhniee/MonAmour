# 🔧 Hướng dẫn Debug Biểu đồ Doanh thu

## 🎯 **Mục tiêu:**
Tìm và sửa lỗi biểu đồ không hiển thị doanh thu

## 🔍 **Các bước debug đã thực hiện:**

### 1. **Thêm Logging vào ReportService:**
```csharp
// Trong GetMonthlyRevenueAsync
_logger.LogInformation($"Getting monthly revenue for year: {year}");
_logger.LogInformation($"Total orders (non-cart): {totalOrders}, Total bookings: {totalBookings}");
_logger.LogInformation($"Month {month}: OrderRevenue={orderRevenue}, BookingRevenue={bookingRevenue}, TotalRevenue={totalRevenue}");
_logger.LogInformation($"Returning {monthlyData.Count} months of data");
```

### 2. **Thêm Logging vào ReportController:**
```csharp
// Trong RevenueReport action
_logger.LogInformation($"Revenue Report - TotalRevenue: {report?.TotalRevenue}, MonthlyData count: {monthlyData?.Count}, DailyData count: {dailyData?.Count}");
```

### 3. **Cải thiện JavaScript Debug:**
```javascript
// Debug logging
console.log('Monthly data:', monthlyData);
console.log('Daily data:', dailyData);
console.log('Monthly labels:', monthlyLabels);
console.log('Monthly values:', monthlyDataValues);
console.log('Final monthly data:', { labels: monthlyLabels, values: monthlyDataValues });
```

### 4. **Tạo Script dữ liệu test:**
- File: `Scripts/AddTestData.sql`
- Thêm 12 orders và 12 bookings với dữ liệu trong các tháng khác nhau
- Tổng doanh thu: ~42,000,000 VNĐ

## 🚀 **Cách kiểm tra:**

### **Bước 1: Chạy Script dữ liệu test**
```sql
-- Mở SQL Server Management Studio
-- Chạy file Scripts/AddTestData.sql
-- Kiểm tra kết quả:
SELECT COUNT(*) as TotalOrders FROM [Order] WHERE Status != 'cart';
SELECT COUNT(*) as TotalBookings FROM Booking;
SELECT SUM(TotalPrice) as TotalOrderRevenue FROM [Order] WHERE Status != 'cart';
SELECT SUM(TotalPrice) as TotalBookingRevenue FROM Booking;
```

### **Bước 2: Build và chạy ứng dụng**
```bash
dotnet build
dotnet run
```

### **Bước 3: Kiểm tra Console logs**
1. Mở **Developer Tools** (F12)
2. Vào tab **Console**
3. Truy cập Revenue Report page
4. Xem các log messages:
   - `Monthly data: [...]`
   - `Daily data: [...]`
   - `Monthly labels: [...]`
   - `Monthly values: [...]`

### **Bước 4: Kiểm tra Server logs**
Xem console output của `dotnet run` để thấy:
- `Getting monthly revenue for year: 2024`
- `Total orders (non-cart): X, Total bookings: Y`
- `Month 1: OrderRevenue=X, BookingRevenue=Y, TotalRevenue=Z`
- `Revenue Report - TotalRevenue: X, MonthlyData count: Y, DailyData count: Z`

## 🔧 **Các trường hợp có thể:**

### **Trường hợp 1: Database trống**
- **Triệu chứng:** `Total orders (non-cart): 0, Total bookings: 0`
- **Giải pháp:** Chạy `Scripts/AddTestData.sql`

### **Trường hợp 2: Dữ liệu có nhưng Revenue = 0**
- **Triệu chứng:** `Month 1: OrderRevenue=0, BookingRevenue=0, TotalRevenue=0`
- **Nguyên nhân:** Orders/Bookings có `TotalPrice = NULL`
- **Giải pháp:** Kiểm tra và sửa dữ liệu

### **Trường hợp 3: Dữ liệu đúng nhưng JavaScript lỗi**
- **Triệu chứng:** Console có lỗi JavaScript
- **Giải pháp:** Kiểm tra property names và data format

### **Trường hợp 4: Chart.js không load**
- **Triệu chứng:** `Chart is not defined`
- **Giải pháp:** Kiểm tra CDN link Chart.js

## 📊 **Dữ liệu test mong đợi:**

### **Monthly Revenue (2024):**
- Tháng 1: 2,330,000 VNĐ (Order: 330,000 + Booking: 2,000,000)
- Tháng 2: 2,050,000 VNĐ (Order: 550,000 + Booking: 1,500,000)
- Tháng 3: 2,220,000 VNĐ (Order: 220,000 + Booking: 2,000,000)
- ...và tiếp tục cho 12 tháng

### **Daily Revenue (30 ngày gần nhất):**
- Mỗi ngày có dữ liệu sẽ hiển thị doanh thu tương ứng
- Các ngày không có dữ liệu sẽ hiển thị 0

## 🎯 **Kết quả mong đợi:**
- ✅ **Console logs hiển thị dữ liệu đúng**
- ✅ **Biểu đồ monthly hiển thị 12 tháng với dữ liệu**
- ✅ **Biểu đồ daily hiển thị 7 ngày gần nhất**
- ✅ **Không có lỗi JavaScript**

## 📝 **Lưu ý quan trọng:**
- **Luôn kiểm tra Console trước** khi debug
- **Chạy script AddTestData.sql** để có dữ liệu test
- **Kiểm tra Server logs** để xem dữ liệu từ database
- **Sử dụng fallback data** nếu không có dữ liệu thực

---

## 🎉 **SAU KHI ÁP DỤNG CÁC BƯỚC TRÊN, BIỂU ĐỒ SẼ HIỂN THỊ DỮ LIỆU!**

**Hãy làm theo từng bước một cách cẩn thận!** 🚀
