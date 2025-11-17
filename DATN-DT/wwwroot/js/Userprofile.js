class UserProfile {
    constructor() {
        this.dataTttk = {};
        this.cropper = null;
        this.croppedImageFile = null;
        this.pendingAvatarUpdate = null;
        this.originalUserName = ''; // Lưu tên gốc để khôi phục nếu cần
        this.init();
    }

    // Format số
    formatNumber(num) {
        return new Intl.NumberFormat('vi-VN').format(num);
    }

    // Lấy ngày tham gia
    getJoinDate() {
        return new Date().toLocaleDateString('vi-VN');
    }

    // Hiển thị trạng thái
    displayTrangThai(trangThai) {
        const statusElement = document.getElementById('trangThai');
        let statusClass = '';
        let statusText = '';

        switch (trangThai) {
            case 1:
                statusClass = 'status-active';
                statusText = 'Đang hoạt động';
                break;
            case 0:
                statusClass = 'status-inactive';
                statusText = 'Ngừng hoạt động';
                break;
            case 2:
                statusClass = 'status-warning';
                statusText = 'Khóa tạm thời';
                break;
            default:
                statusClass = 'status-warning';
                statusText = 'Không xác định';
        }

        statusElement.className = `float-end ${statusClass}`;
        statusElement.textContent = statusText;
    }

    // Cập nhật thông tin sidebar (avatar và tên)
    updateSidebarInfo() {
        const data = this.dataTttk;

        // Cập nhật avatar trong sidebar
        const avatarImage = document.getElementById('avatarImage');
        const previewImage = document.getElementById('previewImage');

        if (data.defaultImage) {
            avatarImage.src = data.defaultImage;
            previewImage.src = data.defaultImage;
        } else {
            avatarImage.src = '/images/default-avatar.jpg';
            previewImage.src = '/images/default-avatar.jpg';
        }

        // Cập nhật tên trong sidebar - chỉ dùng data từ server
        const userName = document.getElementById('userName');
        userName.textContent = data.hoTenKhachHang || 'Chưa có tên';

        // Lưu tên gốc để khôi phục nếu cần
        this.originalUserName = data.hoTenKhachHang || '';

        console.log('Đã cập nhật sidebar - Tên:', data.hoTenKhachHang, 'Avatar:', data.defaultImage);
    }

    // Khôi phục tên gốc nếu update thất bại
    restoreOriginalUserName() {
        const userName = document.getElementById('userName');
        userName.textContent = this.originalUserName;

        // Cũng khôi phục giá trị trong input form
        document.getElementById('hoTenKhachHang').value = this.originalUserName;
    }

    // Lấy thông tin khách hàng từ API
    async loadCustomerInfo() {
        try {
            console.log('Đang gọi API lấy thông tin profile...');
            const response = await fetch('/UserProfile/GetProfileData');

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Lỗi khi lấy thông tin: ${response.status} - ${errorText}`);
            }

            const contentType = response.headers.get('content-type');
            if (!contentType || !contentType.includes('application/json')) {
                const text = await response.text();
                throw new Error('Server trả về không phải JSON');
            }

            this.dataTttk = await response.json();
            console.log('Data nhận được:', this.dataTttk);
            this.displayCustomerInfo();

        } catch (error) {
            console.error("Lỗi khi lấy thông tin khách hàng:", error);
            alert("Lỗi! Không thể lấy thông tin tài khoản: " + error.message);
        }
    }

    // Hiển thị thông tin khách hàng lên form
    displayCustomerInfo() {
        const data = this.dataTttk;

        // Cập nhật sidebar trước
        this.updateSidebarInfo();

        // Điền form
        document.getElementById('hoTenKhachHang').value = data.hoTenKhachHang || '';
        document.getElementById('emailKhachHang').value = data.emailKhachHang || '';
        document.getElementById('sdtKhachHang').value = data.sdtKhachHang || '';
        document.getElementById('diaChiKhachHang').value = data.diaChiKhachHang || '';

        // Hiển thị thông tin bổ sung
        document.getElementById('diemTichLuy').textContent = this.formatNumber(data.diemTichLuy || 0);
        document.getElementById('ngayThamGia').textContent = this.getJoinDate();

        this.displayTrangThai(data.trangThaiKhachHang);
    }

    // Hiển thị modal xác nhận cập nhật thông tin
    showConfirmUpdateModal() {
        const confirmUpdateModal = new bootstrap.Modal(document.getElementById('confirmUpdateModal'));
        confirmUpdateModal.show();
    }

    // Xử lý cập nhật thông tin sau khi xác nhận
    async processProfileUpdate() {
        // Validation
        const hoTen = document.getElementById('hoTenKhachHang').value.trim();
        const sdt = document.getElementById('sdtKhachHang').value.trim();
        const email = document.getElementById('emailKhachHang').value.trim();

        if (!hoTen) {
            alert("Lỗi! Vui lòng nhập họ tên.");
            return;
        }

        if (!sdt) {
            alert("Lỗi! Vui lòng nhập số điện thoại.");
            return;
        }

        if (!email) {
            alert("Lỗi! Vui lòng nhập email.");
            return;
        }

        const dataToUpdate = {
            HoTenKhachHang: hoTen,
            SdtKhachHang: sdt,
            EmailKhachHang: email,
            DiaChiKhachHang: document.getElementById('diaChiKhachHang').value.trim() || null
        };

        try {
            // Hiển thị loading thông báo
            const updateButton = document.getElementById('btnConfirmUpdate');
            const originalText = updateButton.innerHTML;
            updateButton.innerHTML = '<i class="bi bi-hourglass-split me-2"></i>Đang cập nhật...';
            updateButton.disabled = true;

            console.log('Đang gửi dữ liệu cập nhật:', dataToUpdate);

            // Gọi API endpoint mới
            const response = await fetch('/UserProfile/UpdateProfileData', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify(dataToUpdate)
            });

            console.log('Response status:', response.status);
            console.log('Response ok:', response.ok);

            // Kiểm tra content type trước khi parse JSON
            const contentType = response.headers.get('content-type');
            let result = null;

            if (contentType && contentType.includes('application/json')) {
                try {
                    result = await response.json();
                    console.log('Kết quả JSON:', result);
                } catch (jsonError) {
                    console.error('Lỗi parse JSON:', jsonError);

                    // Đọc response text để debug
                    const textResponse = await response.text();
                    console.error('Response text:', textResponse);

                    throw new Error('Dữ liệu trả về không hợp lệ');
                }
            } else {
                // Nếu không phải JSON, đọc response dưới dạng text
                const textResponse = await response.text();
                console.log('Non-JSON response:', textResponse.substring(0, 200));

                if (response.ok) {
                    // Nếu response ok nhưng không phải JSON, coi như thành công
                    result = { message: "Cập nhật thành công!" };
                } else {
                    throw new Error(`Lỗi server: ${response.status} - ${textResponse.substring(0, 100)}`);
                }
            }

            // Khôi phục trạng thái nút
            updateButton.innerHTML = originalText;
            updateButton.disabled = false;

            if (!response.ok) {
                // Nếu có lỗi, khôi phục tên gốc
                this.restoreOriginalUserName();
                throw new Error(result?.message || `Lỗi khi cập nhật thông tin: ${response.status}`);
            }

            // CHỈ CẬP NHẬT KHI THÀNH CÔNG
            // Cập nhật thông tin sidebar với data từ server
            if (result.data) {
                this.dataTttk = { ...this.dataTttk, ...result.data };
            } else {
                this.dataTttk.hoTenKhachHang = hoTen;
            }
            this.updateSidebarInfo();

            alert("Thành công! " + (result.message || "Thông tin đã được cập nhật."));

            // Reload toàn bộ data để đảm bảo đồng bộ
            setTimeout(() => {
                this.loadCustomerInfo();
            }, 1000);

        } catch (error) {
            console.error("Lỗi khi cập nhật:", error);

            // Khôi phục trạng thái nút trong trường hợp lỗi
            const updateButton = document.getElementById('btnConfirmUpdate');
            updateButton.innerHTML = '<i class="bi bi-check-circle me-2"></i>Xác nhận cập nhật';
            updateButton.disabled = false;

            // Khôi phục tên gốc khi có lỗi
            this.restoreOriginalUserName();

            alert("Lỗi! " + (error.message || "Không thể cập nhật thông tin."));
        }
    }

    // Lấy AntiForgeryToken
    getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    }

    // Hiển thị modal xác nhận upload ảnh
    showConfirmUploadModal(croppedImageDataUrl) {
        const confirmPreviewImage = document.getElementById('confirmPreviewImage');
        confirmPreviewImage.src = croppedImageDataUrl;

        const confirmUploadModal = new bootstrap.Modal(document.getElementById('confirmUploadModal'));
        confirmUploadModal.show();

        // Lưu ảnh đã crop để sử dụng sau khi xác nhận
        this.pendingAvatarUpdate = croppedImageDataUrl;
    }

    // Xử lý upload avatar sau khi xác nhận
    async processAvatarUpload() {
        if (!this.pendingAvatarUpdate) {
            alert("Lỗi! Không có ảnh để upload.");
            return;
        }

        try {
            // Hiển thị loading thông báo
            const uploadButton = document.getElementById('btnConfirmUpload');
            const originalText = uploadButton.innerHTML;
            uploadButton.innerHTML = '<i class="bi bi-hourglass-split me-2"></i>Đang upload...';
            uploadButton.disabled = true;

            // Chuyển base64 thành file
            const blob = this.dataURLtoBlob(this.pendingAvatarUpdate);
            const fileName = this.generateRandomFileName() + '.png';
            const file = new File([blob], fileName, { type: 'image/png' });

            const formData = new FormData();
            formData.append("avatarFile", file);

            const response = await fetch('/UserProfile/UpdateAvatar', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: formData
            });

            // Khôi phục trạng thái nút
            uploadButton.innerHTML = originalText;
            uploadButton.disabled = false;

            if (!response.ok) {
                throw new Error("Lỗi khi upload ảnh");
            }

            const result = await response.json();

            // Cập nhật thông tin avatar ngay lập tức khi thành công
            const newAvatarUrl = result.avatarUrl || this.pendingAvatarUpdate;
            this.dataTttk.defaultImage = newAvatarUrl;
            this.updateSidebarInfo();

            alert("Thành công! " + (result.message || "Ảnh đại diện đã được cập nhật."));

            // Reload toàn bộ data để đảm bảo đồng bộ
            setTimeout(() => {
                this.loadCustomerInfo();
            }, 1000);

            // Reset pending update
            this.pendingAvatarUpdate = null;

        } catch (error) {
            console.error("Lỗi upload avatar:", error);
            alert("Lỗi! " + (error.message || "Không thể cập nhật ảnh đại diện."));
        }
    }

    // Khởi tạo cropper và xử lý ảnh
    initImageUpload() {
        const imageInput = document.getElementById("imageInput");
        const previewImage = document.getElementById("previewImage");
        const btnSelectImage = document.getElementById("btnSelectImage");
        const btnCropConfirm = document.getElementById("btnCropConfirm");
        const cropModal = new bootstrap.Modal(document.getElementById("cropModal"));
        const cropperImage = document.getElementById("cropperImage");

        btnSelectImage.addEventListener("click", () => {
            imageInput.click();
        });

        imageInput.addEventListener("change", (event) => {
            const file = event.target.files[0];
            if (file) {
                // Kiểm tra dung lượng và định dạng ảnh
                if (file.size > 10 * 1024 * 1024) {
                    alert("Lỗi! File quá lớn! Vui lòng chọn ảnh nhỏ hơn 10MB.");
                    return;
                }
                if (!['image/png', 'image/jpeg'].includes(file.type)) {
                    alert("Lỗi! Chỉ hỗ trợ định dạng PNG hoặc JPEG.");
                    return;
                }

                const reader = new FileReader();
                reader.onload = (e) => {
                    cropperImage.src = e.target.result;
                    cropModal.show();

                    setTimeout(() => {
                        if (this.cropper) {
                            this.cropper.destroy();
                        }
                        this.cropper = new Cropper(cropperImage, {
                            aspectRatio: 1,
                            viewMode: 1,
                            dragMode: 'move',
                            autoCropArea: 1,
                            movable: true,
                            zoomable: true,
                            rotatable: true,
                            scalable: true
                        });
                    }, 500);
                };
                reader.readAsDataURL(file);
            }
        });

        btnCropConfirm.addEventListener("click", () => {
            if (this.cropper) {
                const croppedCanvas = this.cropper.getCroppedCanvas({
                    width: 200,
                    height: 200
                });

                const croppedImageDataUrl = croppedCanvas.toDataURL("image/png");

                // Hiển thị preview tạm thời (chỉ preview, chưa update thật)
                previewImage.src = croppedImageDataUrl;

                // Đóng modal crop
                cropModal.hide();

                // Hiển thị modal xác nhận upload
                setTimeout(() => {
                    this.showConfirmUploadModal(croppedImageDataUrl);
                }, 300);
            }
        });
    }

    // Helper functions
    dataURLtoBlob(dataURL) {
        const arr = dataURL.split(',');
        const mime = arr[0].match(/:(.*?);/)[1];
        const bstr = atob(arr[1]);
        let n = bstr.length;
        const u8arr = new Uint8Array(n);
        while (n--) {
            u8arr[n] = bstr.charCodeAt(n);
        }
        return new Blob([u8arr], { type: mime });
    }

    generateRandomFileName() {
        const timestamp = new Date().getTime();
        const randomString = Math.random().toString(36).substring(2, 8);
        return `avatar_${timestamp}_${randomString}`;
    }

    // Khởi tạo
    init() {
        // Load thông tin khách hàng
        this.loadCustomerInfo();

        // Khởi tạo upload ảnh
        this.initImageUpload();

        // Gán sự kiện cho nút lưu
        document.getElementById('btnSave').addEventListener('click', () => {
            this.showConfirmUpdateModal();
        });

        // Gán sự kiện cho nút xác nhận update thông tin
        document.getElementById('btnConfirmUpdate').addEventListener('click', () => {
            const modal = bootstrap.Modal.getInstance(document.getElementById('confirmUpdateModal'));
            modal.hide();
            this.processProfileUpdate();
        });

        // Gán sự kiện cho nút xác nhận upload ảnh
        document.getElementById('btnConfirmUpload').addEventListener('click', () => {
            const modal = bootstrap.Modal.getInstance(document.getElementById('confirmUploadModal'));
            modal.hide();
            this.processAvatarUpload();
        });

        // XÓA real-time updates để tránh thay đổi tên trước khi update thành công
        // this.initRealTimeUpdates();
    }

    // ĐÃ XÓA: Cập nhật real-time khi người dùng nhập
    // initRealTimeUpdates() {
    //     const hoTenInput = document.getElementById('hoTenKhachHang');
    //
    //     hoTenInput.addEventListener('input', (e) => {
    //         // Cập nhật tên trong sidebar khi người dùng đang nhập
    //         const newName = e.target.value.trim();
    //         if (newName) {
    //             document.getElementById('userName').textContent = newName;
    //         }
    //     });
    // }
}

// Khởi tạo ứng dụng khi trang được load
document.addEventListener('DOMContentLoaded', function () {
    new UserProfile();
});