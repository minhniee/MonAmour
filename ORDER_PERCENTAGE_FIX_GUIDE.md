# 🔧 Hướng dẫn Sửa Lỗi OrderPercentage

## ❌ **Lỗi gặp phải:**
```
Microsoft.CSharp.RuntimeBinder.RuntimeBinderException: 'MonAmour.ViewModels.RevenueReportViewModel' does not contain a definition for 'OrderPercentage'
```

## 🔍 **Nguyên nhân:**
- View `RevenueReport.cshtml` đang truy cập `ViewBag.RevenueReport?.OrderPercentage`
- Nhưng `RevenueReportViewModel` không có property `OrderPercentage`
- Tương tự với `BookingPercentage`

## ✅ **Giải pháp đã áp dụng:**

### 1. **Thêm Properties vào ViewModel:**

#### **File: `ViewModels/ReportViewModel.cs`**
```csharp
public class RevenueReportViewModel
{
    public decimal TotalRevenue { get; set; }
    public decimal OrderRevenue { get; set; }
    public decimal BookingRevenue { get; set; }
    public decimal GrowthRate { get; set; }
    public int TotalOrders { get; set; }
    public int TotalBookings { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal AverageBookingValue { get; set; }
    // ✅ THÊM MỚI:
    public decimal OrderPercentage { get; set; }
    public decimal BookingPercentage { get; set; }
    public List<MonthlyRevenueViewModel> MonthlyData { get; set; } = new List<MonthlyRevenueViewModel>();
}
```

### 2. **Cập nhật Logic tính toán trong Service:**

#### **File: `Services/Implements/ReportService.cs`**
```csharp
// ✅ THÊM TÍNH TOÁN PERCENTAGE:
var orderPercentage = totalRevenue > 0 ? (orderRevenue / totalRevenue) * 100 : 0;
var bookingPercentage = totalRevenue > 0 ? (bookingRevenue / totalRevenue) * 100 : 0;

return new RevenueReportViewModel
{
    TotalRevenue = totalRevenue,
    OrderRevenue = orderRevenue,
    BookingRevenue = bookingRevenue,
    GrowthRate = growthRate,
    TotalOrders = totalOrders,
    TotalBookings = totalBookings,
    AverageOrderValue = averageOrderValue,
    AverageBookingValue = averageBookingValue,
    // ✅ THÊM MỚI:
    OrderPercentage = orderPercentage,
    BookingPercentage = bookingPercentage,
    MonthlyData = monthlyData
};
```

### 3. **View đã sử dụng đúng:**

#### **File: `Views/Report/RevenueReport.cshtml`**
```html
<!-- ✅ Đã có sẵn trong view: -->
<td>@(ViewBag.RevenueReport?.OrderPercentage.ToString("F1") ?? "0")%</td>
<td>@(ViewBag.RevenueReport?.BookingPercentage.ToString("F1") ?? "0")%</td>
```

## 📊 **Công thức tính toán:**

### **OrderPercentage:**
```csharp
OrderPercentage = (OrderRevenue / TotalRevenue) * 100
```

### **BookingPercentage:**
```csharp
BookingPercentage = (BookingRevenue / TotalRevenue) * 100
```

### **Ví dụ:**
- TotalRevenue = 1,000,000 VNĐ
- OrderRevenue = 600,000 VNĐ
- BookingRevenue = 400,000 VNĐ
- **OrderPercentage = (600,000 / 1,000,000) * 100 = 60%**
- **BookingPercentage = (400,000 / 1,000,000) * 100 = 40%**

## 🎯 **Kết quả:**
- ✅ **Build thành công** (Exit code: 0)
- ✅ **Không còn lỗi runtime**
- ✅ **Ứng dụng chạy bình thường**
- ✅ **Percentage hiển thị đúng**

## 🚀 **Cách test:**
1. **Truy cập:** `http://localhost:5012`
2. **Đăng nhập** với admin
3. **Vào menu "BÁO CÁO & THỐNG KÊ"**
4. **Chọn "Revenue Report"**
5. **Kiểm tra bảng thống kê** - sẽ thấy:
   - Order Percentage: XX.X%
   - Booking Percentage: XX.X%

## 📝 **Lưu ý quan trọng:**
- **Luôn kiểm tra division by zero:** `totalRevenue > 0 ? (orderRevenue / totalRevenue) * 100 : 0`
- **Sử dụng decimal** cho percentage để có độ chính xác cao
- **Format hiển thị:** `.ToString("F1")` để hiển thị 1 chữ số thập phân

---

## 🎉 **LỖI ORDERPERCENTAGE ĐÃ ĐƯỢC SỬA HOÀN TOÀN!**

**Hệ thống báo cáo doanh thu đã hoạt động hoàn hảo với đầy đủ thông tin percentage!** 🚀
