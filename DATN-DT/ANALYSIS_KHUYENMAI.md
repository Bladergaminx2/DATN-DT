# PHÂN TÍCH NGHIỆP VỤ KHUYẾN MÃI

## 1. PHÂN TÍCH NGHIỆP VỤ HIỆN TẠI

### 1.1. Các tính năng hiện có:
- ✅ Tạo/Sửa/Xóa khuyến mãi
- ✅ Quản lý thời gian khuyến mãi (Ngày bắt đầu, ngày kết thúc)
- ✅ Hai loại giảm giá: Phần trăm (%) và Số tiền (VNĐ)
- ✅ Gán sản phẩm cho khuyến mãi
- ✅ Tự động cập nhật trạng thái (Sắp diễn ra, Đang diễn ra, Đã kết thúc)
- ✅ Kiểm tra trùng thời gian khuyến mãi
- ✅ Kiểm tra khuyến mãi đã được sử dụng trước khi xóa

### 1.2. Các vấn đề cần cải thiện:

#### ❌ Vấn đề 1: Logic kiểm tra trùng thời gian quá nghiêm ngặt
- **Hiện tại**: Không cho phép bất kỳ khuyến mãi nào trùng thời gian
- **Vấn đề**: Trong thực tế, có thể có nhiều khuyến mãi khác nhau cho các sản phẩm khác nhau cùng thời gian
- **Giải pháp**: Chỉ kiểm tra trùng khi cùng sản phẩm, hoặc cho phép nhiều khuyến mãi nhưng ưu tiên khuyến mãi có giá trị cao hơn

#### ❌ Vấn đề 2: Validation giá trị giảm chưa đầy đủ
- **Phần trăm**: Chỉ kiểm tra <= 100%, chưa kiểm tra > 0
- **Số tiền**: Chưa kiểm tra giá trị có vượt quá giá sản phẩm không
- **Giải pháp**: Thêm validation đầy đủ hơn

#### ❌ Vấn đề 3: Tính giá sau giảm chưa tối ưu
- **Hiện tại**: Có thể tính ra giá âm nếu giảm số tiền lớn hơn giá sản phẩm
- **Vấn đề**: Giá sau giảm không được làm tròn hợp lý
- **Giải pháp**: Đảm bảo giá >= 0, làm tròn đến 1000 VNĐ

#### ❌ Vấn đề 4: Chưa kiểm tra sản phẩm có đang trong khuyến mãi khác
- **Vấn đề**: Một sản phẩm có thể được gán vào nhiều khuyến mãi cùng lúc
- **Giải pháp**: Kiểm tra và chỉ cho phép 1 khuyến mãi active cho 1 sản phẩm tại 1 thời điểm

#### ❌ Vấn đề 5: Logic cập nhật trạng thái có thể cải thiện
- **Hiện tại**: Chỉ cập nhật khi load Index
- **Vấn đề**: Nếu không vào trang Index, trạng thái không được cập nhật
- **Giải pháp**: Tạo background job hoặc trigger khi query

## 2. ĐỀ XUẤT CẢI THIỆN

### 2.1. Cải thiện logic kiểm tra trùng thời gian
- Cho phép nhiều khuyến mãi cùng thời gian nếu khác sản phẩm
- Chỉ kiểm tra trùng khi cùng sản phẩm

### 2.2. Cải thiện validation
- Phần trăm: 0 < giá trị <= 100
- Số tiền: 0 < giá trị <= giá sản phẩm (khi gán sản phẩm)

### 2.3. Cải thiện tính giá sau giảm
- Đảm bảo giá >= 0
- Làm tròn đến 1000 VNĐ
- Lưu giá gốc và giá sau giảm

### 2.4. Kiểm tra sản phẩm đã có khuyến mãi
- Khi gán sản phẩm, kiểm tra sản phẩm có đang trong khuyến mãi active khác không
- Nếu có, hỏi xác nhận hoặc tự động remove khuyến mãi cũ

### 2.5. Cải thiện cập nhật trạng thái
- Tạo helper method để cập nhật trạng thái
- Gọi method này khi cần thiết (query, create, update)

## 3. QUY TẮC NGHIỆP VỤ

### 3.1. Quy tắc tạo khuyến mãi:
1. Mã khuyến mãi phải unique
2. Giá trị giảm phải hợp lệ (phần trăm: 0-100%, số tiền: > 0)
3. Ngày kết thúc phải sau ngày bắt đầu
4. Có thể có nhiều khuyến mãi cùng thời gian nếu khác sản phẩm

### 3.2. Quy tắc gán sản phẩm:
1. Sản phẩm phải tồn tại và đang active
2. Khuyến mãi phải đang active hoặc sắp diễn ra
3. Một sản phẩm chỉ nên có 1 khuyến mãi active tại 1 thời điểm
4. Nếu sản phẩm đã có khuyến mãi, cần xác nhận hoặc tự động remove

### 3.3. Quy tắc tính giá:
1. Phần trăm: Giá sau = Giá gốc * (1 - %/100)
2. Số tiền: Giá sau = Max(0, Giá gốc - Số tiền)
3. Làm tròn đến 1000 VNĐ
4. Giá sau giảm không được < 0

### 3.4. Quy tắc xóa khuyến mãi:
1. Không thể xóa nếu đã được sử dụng trong đơn hàng/hóa đơn
2. Không thể xóa nếu đang active
3. Có thể xóa nếu đã kết thúc và chưa được sử dụng

