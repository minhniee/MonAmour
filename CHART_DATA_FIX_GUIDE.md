# 🔧 Hướng dẫn Sửa Lỗi Biểu đồ Không Hiện Doanh thu

## ❌ **Vấn đề:**
- Biểu đồ doanh thu không hiển thị dữ liệu
- Có thể do dữ liệu trống hoặc lỗi JavaScript

## 🔍 **Nguyên nhân có thể:**
1. **Dữ liệu trống:** Database không có dữ liệu mẫu
2. **Lỗi serialization:** Dữ liệu không được truyền đúng từ controller
3. **Lỗi JavaScript:** Biểu đồ không được khởi tạo đúng
4. **Lỗi property mapping:** Tên properties không khớp

## ✅ **Giải pháp đã áp dụng:**

### 1. **Thêm Debug Logging:**

#### **File: `Views/Report/RevenueReport.cshtml`**
```javascript
// Debug logging
console.log('Monthly data:', monthlyData);
console.log('Daily data:', dailyData);

// Debug processed data
console.log('Monthly labels:', monthlyLabels);
console.log('Monthly values:', monthlyDataValues);
console.log('Daily labels:', dailyLabels);
console.log('Daily values:', dailyDataValues);
```

### 2. **Thêm Fallback Data:**

```javascript
// Fallback data if no data available
if (monthlyLabels.length === 0 || monthlyDataValues.every(v => v === 0)) {
    console.log('No monthly data, using fallback');
    monthlyLabels.push('Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6');
    monthlyDataValues.push(0, 0, 0, 0, 0, 0);
}

if (dailyLabels.length === 0 || dailyDataValues.every(v => v === 0)) {
    console.log('No daily data, using fallback');
    const today = new Date();
    for (let i = 6; i >= 0; i--) {
        const date = new Date(today);
        date.setDate(date.getDate() - i);
        dailyLabels.push(date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' }));
        dailyDataValues.push(0);
    }
}
```

## 🚀 **Cách kiểm tra và sửa:**

### **Bước 1: Kiểm tra Console**
1. Mở **Developer Tools** (F12)
2. Vào tab **Console**
3. Reload trang Revenue Report
4. Xem các log messages:
   - `Monthly data: [...]`
   - `Daily data: [...]`
   - `Monthly labels: [...]`
   - `Monthly values: [...]`

### **Bước 2: Kiểm tra dữ liệu**
- Nếu `Monthly data: []` → Database không có dữ liệu
- Nếu `Monthly data: [...]` nhưng `Monthly values: [0,0,0...]` → Dữ liệu có nhưng Revenue = 0
- Nếu có lỗi serialization → Kiểm tra ViewModel properties

### **Bước 3: Thêm dữ liệu mẫu**
```sql
-- Chạy script AddSampleData.sql để thêm dữ liệu mẫu
-- Hoặc tạo orders và bookings mới qua admin panel
```

### **Bước 4: Kiểm tra Properties**
Đảm bảo ViewModel có đúng properties:
```csharp
public class MonthlyRevenueViewModel
{
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    // ...
}
```

## 🔧 **Các bước debug chi tiết:**

### **1. Kiểm tra Controller:**
```csharp
// Trong ReportController.cs
var monthlyData = await _reportService.GetMonthlyRevenueAsync(DateTime.Now.Year);
var dailyData = await _reportService.GetDailyRevenueAsync(
    filter.FromDate ?? DateTime.Now.AddDays(-30), 
    filter.ToDate ?? DateTime.Now);

ViewBag.MonthlyData = monthlyData ?? new List<MonthlyRevenueViewModel>();
ViewBag.DailyData = dailyData ?? new List<DailyRevenueViewModel>();
```

### **2. Kiểm tra Service:**
```csharp
// Trong ReportService.cs
public async Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync(int year)
{
    // Kiểm tra xem có dữ liệu trong database không
    var orders = await _context.Orders.Where(...).ToListAsync();
    var bookings = await _context.Bookings.Where(...).ToListAsync();
    
    // Debug logging
    _logger.LogInformation($"Found {orders.Count} orders and {bookings.Count} bookings");
}
```

### **3. Kiểm tra Database:**
```sql
-- Kiểm tra có dữ liệu không
SELECT COUNT(*) FROM [Order] WHERE Status != 'cart';
SELECT COUNT(*) FROM Booking;
SELECT COUNT(*) FROM PaymentDetail;
```

## 🎯 **Kết quả mong đợi:**
- ✅ **Console hiển thị dữ liệu:** `Monthly data: [{MonthName: "Tháng 1", Revenue: 1000000}, ...]`
- ✅ **Biểu đồ hiển thị:** Có đường line chart với dữ liệu
- ✅ **Fallback data:** Nếu không có dữ liệu, hiển thị biểu đồ trống với labels

## 📝 **Lưu ý quan trọng:**
- **Luôn kiểm tra Console** để debug
- **Thêm dữ liệu mẫu** nếu database trống
- **Kiểm tra property names** phải khớp giữa ViewModel và JavaScript
- **Sử dụng fallback data** để đảm bảo biểu đồ luôn hiển thị

---

## 🎉 **BIỂU ĐỒ DOANH THU SẼ HIỂN THỊ SAU KHI ÁP DỤNG CÁC BƯỚC TRÊN!**

**Hãy kiểm tra Console và thêm dữ liệu mẫu nếu cần!** 🚀
