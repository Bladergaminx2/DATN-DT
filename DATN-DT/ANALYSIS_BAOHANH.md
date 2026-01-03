# PHÂN TÍCH NGHIỆP VỤ BẢO HÀNH

## 📋 TỔNG QUAN

Hệ thống quản lý bảo hành cho phép:
- Tạo phiếu bảo hành mới
- Cập nhật thông tin phiếu bảo hành
- Theo dõi trạng thái bảo hành

## ❌ CÁC VẤN ĐỀ PHÁT HIỆN

### 1. **Validation IMEI**
- ❌ **Vấn đề:** Không kiểm tra IMEI có đã được bán chưa
- ❌ **Vấn đề:** Không kiểm tra IMEI có tồn tại trong hệ thống
- ❌ **Vấn đề:** Cho phép chọn bất kỳ IMEI nào, kể cả IMEI chưa bán (đang trong kho)
- 🔧 **Giải pháp:** 
  - Chỉ lấy IMEI có trạng thái "Đã bán"
  - Kiểm tra IMEI tồn tại trước khi tạo phiếu bảo hành

### 2. **Validation Khách Hàng**
- ❌ **Vấn đề:** Không kiểm tra IMEI có thuộc về khách hàng đó không
- 🔧 **Giải pháp:** 
  - Kiểm tra IMEI có trong đơn hàng/hóa đơn của khách hàng đó
  - Hoặc cho phép chọn khách hàng bất kỳ (nếu IMEI đã được bán)

### 3. **Validation Ngày**
- ❌ **Vấn đề:** Không kiểm tra NgayTra >= NgayNhan
- ❌ **Vấn đề:** Không validate ngày nhận không được là tương lai
- 🔧 **Giải pháp:**
  - Ngày nhận không được > ngày hiện tại
  - Ngày trả (nếu có) phải >= Ngày nhận

### 4. **Validation Trạng Thái**
- ❌ **Vấn đề:** Không bắt buộc NgayTra khi trạng thái "Đã hoàn thành"
- ❌ **Vấn đề:** Không kiểm tra logic chuyển trạng thái hợp lệ
- 🔧 **Giải pháp:**
  - Khi trạng thái = "Đã hoàn thành" → BẮT BUỘC phải có NgayTra
  - Validate logic chuyển trạng thái (VD: không thể chuyển từ "Đã hoàn thành" về "Đang tiếp nhận")

### 5. **Validation Chi Phí**
- ❌ **Vấn đề:** Chi phí phát sinh có thể < 0 (chỉ validate min="0" ở client)
- 🔧 **Giải pháp:** Validate server-side: ChiPhiPhatSinh >= 0

### 6. **Duplicate Bảo Hành**
- ❌ **Vấn đề:** Không kiểm tra IMEI đã có phiếu bảo hành đang xử lý chưa
- 🔧 **Giải pháp:**
  - Kiểm tra IMEI đã có phiếu bảo hành với trạng thái "Đang tiếp nhận" hoặc "Đang xử lý" chưa
  - Cho phép tạo phiếu mới nếu phiếu cũ đã "Đã hoàn thành" hoặc "Từ chối bảo hành"

### 7. **API GetImeis**
- ❌ **Vấn đề:** Trả về TẤT CẢ IMEI, kể cả IMEI chưa bán
- 🔧 **Giải pháp:**
  - Chỉ lấy IMEI có TrangThai = "Đã bán"
  - Hoặc lấy IMEI có trong HoaDonChiTiet/DonHangChiTiet

### 8. **Mô Tả Lỗi**
- ❌ **Vấn đề:** Chỉ validate không rỗng, không giới hạn độ dài
- 🔧 **Giải pháp:** 
  - Giới hạn độ dài (VD: 10-500 ký tự)
  - Validate cả client và server

### 9. **Xử Lý (Nội Bộ)**
- ⚠️ **Lưu ý:** Field XuLy chỉ có trong form edit, không có trong form create (hợp lý)
- ✅ **OK:** Field này chỉ nhân viên mới điền khi xử lý

### 10. **Nhân Viên**
- ❌ **Vấn đề:** Không kiểm tra nhân viên có tồn tại và đang hoạt động
- 🔧 **Giải pháp:** Validate nhân viên tồn tại trong hệ thống

## ✅ CÁC ĐIỂM TỐT

1. ✅ Có validation cơ bản cho các field bắt buộc
2. ✅ Có chuẩn hóa dữ liệu (Trim)
3. ✅ Có xử lý lỗi cơ bản
4. ✅ Có eager loading để hiển thị thông tin liên quan
5. ✅ UI có dark theme và rõ ràng

## 🔧 ĐỀ XUẤT CẢI THIỆN

### Mức độ ưu tiên CAO:
1. ✅ Kiểm tra IMEI đã được bán chưa
2. ✅ Validate NgayTra >= NgayNhan
3. ✅ Bắt buộc NgayTra khi trạng thái "Đã hoàn thành"
4. ✅ Validate ChiPhiPhatSinh >= 0 (server-side)
5. ✅ Chỉ lấy IMEI đã bán trong GetImeis API

### Mức độ ưu tiên TRUNG BÌNH:
1. ⚠️ Kiểm tra IMEI thuộc về khách hàng (nếu có yêu cầu)
2. ⚠️ Kiểm tra duplicate bảo hành đang xử lý
3. ⚠️ Validate logic chuyển trạng thái
4. ⚠️ Giới hạn độ dài Mô Tả Lỗi

### Mức độ ưu tiên THẤP:
1. 📝 Kiểm tra nhân viên có tồn tại
2. 📝 Validate ngày nhận không được là tương lai

## 📝 GỢI Ý NGHIỆP VỤ

1. **Quy trình bảo hành đề xuất:**
   - Đang tiếp nhận → Đang xử lý → Đã hoàn thành / Từ chối bảo hành
   - Không nên cho phép quay lại trạng thái cũ

2. **Thời gian bảo hành:**
   - Có thể thêm tính năng kiểm tra thời hạn bảo hành (dựa trên ngày mua)
   - Hiển thị cảnh báo nếu quá hạn bảo hành

3. **Báo cáo:**
   - Thống kê số lượng phiếu bảo hành theo trạng thái
   - Thống kê chi phí phát sinh theo tháng
   - Danh sách sản phẩm bảo hành nhiều nhất

