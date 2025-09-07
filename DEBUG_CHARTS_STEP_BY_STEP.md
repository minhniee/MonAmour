# 🔧 Hướng dẫn Debug Biểu đồ Doanh thu - Từng bước

## ✅ **Đã hoàn thành:**
1. ✅ Thêm dữ liệu test vào database (26 orders, 3 bookings)
2. ✅ Thêm logging chi tiết vào ReportService
3. ✅ Cải thiện JavaScript debug
4. ✅ Build và chạy ứng dụng

## 🚀 **Bước tiếp theo - Kiểm tra biểu đồ:**

### **Bước 1: Truy cập Revenue Report**
1. Mở trình duyệt
2. Truy cập: `http://localhost:5012/Report/RevenueReport`
3. Mở **Developer Tools** (F12)
4. Vào tab **Console**

### **Bước 2: Kiểm tra Console Logs**
Tìm các log messages sau trong Console:
```
Monthly data: [...]
Daily data: [...]
Monthly labels: [...]
Monthly values: [...]
Final monthly data: { labels: [...], values: [...] }
```

### **Bước 3: Kiểm tra Server Logs**
Trong terminal chạy `dotnet run`, tìm các log messages:
```
Getting monthly revenue for year: 2024
Total orders (non-cart): 26, Total bookings: 3
Month 1: OrderRevenue=X, BookingRevenue=Y, TotalRevenue=Z
Month 2: OrderRevenue=X, BookingRevenue=Y, TotalRevenue=Z
...
Revenue Report - TotalRevenue: X, MonthlyData count: 12, DailyData count: Y
```

### **Bước 4: Kiểm tra dữ liệu thực tế**
Nếu không thấy dữ liệu, kiểm tra database:
```sql
-- Kiểm tra orders
SELECT COUNT(*) as OrderCount, SUM(total_price) as TotalRevenue 
FROM [Order] 
WHERE Status != 'cart' AND YEAR(created_at) = 2024;

-- Kiểm tra bookings
SELECT COUNT(*) as BookingCount, SUM(total_price) as TotalRevenue 
FROM Booking 
WHERE YEAR(created_at) = 2024;

-- Kiểm tra dữ liệu theo tháng
SELECT 
    MONTH(created_at) as Month,
    SUM(total_price) as Revenue
FROM [Order] 
WHERE Status != 'cart' AND YEAR(created_at) = 2024
GROUP BY MONTH(created_at)
ORDER BY Month;
```

## 🔍 **Các trường hợp có thể xảy ra:**

### **Trường hợp 1: Console hiển thị "No monthly data, using fallback"**
- **Nguyên nhân:** Dữ liệu từ database không được truyền đúng
- **Giải pháp:** Kiểm tra Server logs và database

### **Trường hợp 2: Server logs hiển thị "Total orders: 0, Total bookings: 0"**
- **Nguyên nhân:** Dữ liệu chưa được thêm vào database
- **Giải pháp:** Chạy lại script AddTestData.sql

### **Trường hợp 3: Dữ liệu có nhưng Revenue = 0**
- **Nguyên nhân:** Cột total_price = NULL hoặc 0
- **Giải pháp:** Kiểm tra và cập nhật dữ liệu

### **Trường hợp 4: Biểu đồ không hiển thị**
- **Nguyên nhân:** Lỗi JavaScript hoặc Chart.js
- **Giải pháp:** Kiểm tra Console errors

## 📊 **Dữ liệu mong đợi:**

### **Monthly Revenue (2024):**
- Tháng 1: ~2,330,000 VNĐ
- Tháng 2: ~2,050,000 VNĐ
- Tháng 3: ~2,220,000 VNĐ
- ...và tiếp tục

### **Daily Revenue (7 ngày gần nhất):**
- Mỗi ngày có dữ liệu sẽ hiển thị doanh thu tương ứng

## 🎯 **Kết quả mong đợi:**
- ✅ **Console logs hiển thị dữ liệu thực**
- ✅ **Biểu đồ monthly hiển thị 12 tháng với dữ liệu**
- ✅ **Biểu đồ daily hiển thị 7 ngày gần nhất**
- ✅ **Không có lỗi JavaScript**

---

## 🚨 **QUAN TRỌNG:**
**Hãy làm theo từng bước một cách cẩn thận và báo cáo kết quả từng bước!**

**Nếu vẫn không hiển thị, hãy gửi:**
1. **Console logs** từ Developer Tools
2. **Server logs** từ terminal
3. **Screenshot** của trang Revenue Report
