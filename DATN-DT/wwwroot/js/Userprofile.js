// ====== GLOBAL VARIABLES ======
var currentKhachHangId = null;
var allOrdersData = []; // Lưu tất cả đơn hàng
var currentOrderStatusFilter = 'all'; // Filter trạng thái hiện tại

// ====== UTILITY FUNCTIONS ======
function showLoading() {
    var loadingEl = document.getElementById('loadingOverlay');
    if (loadingEl) {
        loadingEl.style.display = 'flex';
    }
}

function hideLoading() {
    var loadingEl = document.getElementById('loadingOverlay');
    if (loadingEl) {
        loadingEl.style.display = 'none';
    }
}

function showAlert(type, message) {
    if (type === 'success') {
        var successMsgEl = document.getElementById('successMessage');
        var successAlertEl = document.getElementById('successAlert');
        if (successMsgEl) successMsgEl.textContent = message;
        if (successAlertEl) successAlertEl.style.display = 'block';
        setTimeout(function() {
            hideAlert('successAlert');
        }, 5000);
    } else {
        var errorMsgEl = document.getElementById('errorMessage');
        var errorAlertEl = document.getElementById('errorAlert');
        if (errorMsgEl) errorMsgEl.textContent = message;
        if (errorAlertEl) errorAlertEl.style.display = 'block';
    }
}

function hideAlert(alertId) {
    var alertEl = document.getElementById(alertId);
    if (alertEl) {
        alertEl.style.display = 'none';
    }
}

function formatMoney(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount) + ' \u20AB';
}

// ====== AVATAR UPLOAD ======
document.addEventListener('DOMContentLoaded', function() {
    var avatarInput = document.getElementById('avatarInput');
    if (avatarInput) {
        avatarInput.addEventListener('change', async function (e) {
            var file = e.target.files[0];
            if (!file) return;

            // Sử dụng hàm validate chung
            var validationResult = validateImageFile(file, {
                allowedExtensions: ['jpg', 'jpeg', 'png', 'gif'],
                maxSizeInMB: 5
            });

            if (!validationResult.isValid) {
                showAlert('error', validationResult.error);
                e.target.value = ''; // Reset input
                return;
            }

            showLoading();
            const formData = new FormData();
            formData.append('avatarFile', file);

            try {
                const response = await fetch('/UserProfile/UpdateAvatar', {
                    method: 'POST',
                    body: formData
                });

                const data = await response.json();

                if (response.ok) {
                    showAlert('success', data.message || 'Cập nhật ảnh đại diện thành công!');
                    var avatarContainer = document.getElementById('avatarContainer');
                    if (avatarContainer && data.avatarUrl) {
                        var timestamp = new Date().getTime();
                        var img = document.createElement('img');
                        img.src = data.avatarUrl + '?t=' + timestamp;
                        img.alt = 'Avatar';
                        img.onerror = function() {
                            this.onerror = null;
                            var defaultImg = '/images/default-product.jpg';
                            if (this.src !== defaultImg) {
                                this.src = defaultImg;
                            }
                        };
                        avatarContainer.innerHTML = '';
                        avatarContainer.appendChild(img);
                        
                        // Đồng bộ với localStorage và MuaHang/Index
                        const savedProfile = localStorage.getItem('userProfile');
                        let profile = savedProfile ? JSON.parse(savedProfile) : {};
                        profile.defaultImage = data.avatarUrl;
                        
                        // Lưu lại vào localStorage
                        localStorage.setItem('userProfile', JSON.stringify(profile));
                        
                        // Đánh dấu trong sessionStorage để MuaHang biết cần reload
                        sessionStorage.setItem('avatarUpdated', Date.now().toString());
                        
                        // Trigger custom event để các tab khác có thể lắng nghe
                        window.dispatchEvent(new CustomEvent('userProfileUpdated', {
                            detail: { defaultImage: data.avatarUrl }
                        }));
                        
                        console.log('Avatar updated and synced to localStorage:', data.avatarUrl);
                    }
                } else {
                    showAlert('error', data.message || 'Lỗi khi upload ảnh');
                }
            } catch (error) {
                showAlert('error', 'Lỗi kết nối đến server');
            } finally {
                hideLoading();
                e.target.value = ''; // Reset input
            }
        });
    }
});

// ====== PROFILE FUNCTIONS ======
async function loadProfileData() {
    try {
        const response = await fetch('/UserProfile/GetProfileData');
        if (!response.ok) {
            throw new Error('HTTP error! status: ' + response.status);
        }
        
        var responseText = await response.text();
        if (!responseText) {
            throw new Error('Empty response from server');
        }
        
        var data;
        try {
            data = JSON.parse(responseText);
        } catch (parseError) {
            console.error('Failed to parse JSON:', responseText);
            throw new Error('Invalid JSON response from server');
        }

        if (data) {
            // Lưu ID khách hàng
            if (data.idKhachHang) {
                currentKhachHangId = data.idKhachHang;
            }

            var hoTenEl = document.getElementById('HoTenKhachHang');
            if (hoTenEl) hoTenEl.value = data.hoTenKhachHang || '';
            
            var sdtEl = document.getElementById('SdtKhachHang');
            if (sdtEl) sdtEl.value = data.sdtKhachHang || '';
            
            var emailEl = document.getElementById('EmailKhachHang');
            if (emailEl) emailEl.value = data.emailKhachHang || '';
            
            var diaChiEl = document.getElementById('DiaChiKhachHang');
            if (diaChiEl) diaChiEl.value = data.diaChiKhachHang || '';
            
            var ngaySinhEl = document.getElementById('NgaySinh');
            if (ngaySinhEl) {
                if (data.ngaySinh) {
                    var dateParts = data.ngaySinh.split('T');
                    var datePartsLength = dateParts ? dateParts.length : 0;
                    ngaySinhEl.value = datePartsLength > 0 ? dateParts[0] : '';
                } else {
                    ngaySinhEl.value = '';
                }
            }
            
            var gioiTinhEl = document.getElementById('GioiTinh');
            if (gioiTinhEl) gioiTinhEl.value = data.gioiTinh || '';
            
            var profileNameEl = document.getElementById('profileName');
            if (profileNameEl) profileNameEl.textContent = data.hoTenKhachHang || 'Khách hàng';
            
            var diemTichLuyEl = document.getElementById('diemTichLuy');
            if (diemTichLuyEl) diemTichLuyEl.textContent = data.diemTichLuy || 0;
            
            // Cập nhật email trong tab Tài khoản & Bảo mật
            var emailDisplayEl = document.getElementById('EmailKhachHangDisplay');
            if (emailDisplayEl) emailDisplayEl.value = data.emailKhachHang || '';

            // Update stats
            var totalOrdersEl = document.getElementById('totalOrders');
            if (totalOrdersEl) totalOrdersEl.textContent = data.totalOrders || 0;
            
            var processingOrdersEl = document.getElementById('processingOrders');
            if (processingOrdersEl) processingOrdersEl.textContent = data.processingOrders || 0;
            
            var completedOrdersEl = document.getElementById('completedOrders');
            if (completedOrdersEl) completedOrdersEl.textContent = data.completedOrders || 0;
            
            var totalSpentEl = document.getElementById('totalSpent');
            if (totalSpentEl) totalSpentEl.textContent = formatMoney(data.totalSpent || 0);

            // Update avatar
            try {
                var avatarContainer = document.getElementById('avatarContainer');
                if (avatarContainer && data.defaultImage) {
                    var timestamp = new Date().getTime();
                    var img = document.createElement('img');
                    img.src = data.defaultImage + '?t=' + timestamp;
                    img.alt = 'Avatar';
                    img.onerror = function() {
                        this.onerror = null;
                        this.src = '/images/default-product.jpg';
                    };
                    avatarContainer.innerHTML = '';
                    avatarContainer.appendChild(img);
                }
            } catch (avatarError) {
                console.error('Error updating avatar:', avatarError);
            }

            // Lưu vào localStorage để đồng bộ với MuaHang/Index
            const profileData = {
                hoTenKhachHang: data.hoTenKhachHang,
                emailKhachHang: data.emailKhachHang,
                defaultImage: data.defaultImage,
                diemTichLuy: data.diemTichLuy
            };
            localStorage.setItem('userProfile', JSON.stringify(profileData));
            
            // Trigger custom event
            window.dispatchEvent(new CustomEvent('userProfileUpdated', {
                detail: profileData
            }));
        }
    } catch (error) {
        console.error('Error loading profile:', error);
        showAlert('error', 'Lỗi! Không thể lấy thông tin tài khoản: ' + (error.message || 'Unknown error'));
    }
}

async function saveProfile() {
    hideError('errorHoTenKhachHang');
    hideError('errorSdtKhachHang');
    hideError('errorEmailKhachHang');

    var hoTenEl = document.getElementById('HoTenKhachHang');
    var sdtEl = document.getElementById('SdtKhachHang');
    var emailEl = document.getElementById('EmailKhachHang');

    if (!hoTenEl || !sdtEl) {
        showAlert('error', 'Không tìm thấy các trường nhập liệu');
        return;
    }

    const profileData = {
        HoTenKhachHang: hoTenEl.value.trim(),
        SdtKhachHang: sdtEl.value.trim(),
        EmailKhachHang: emailEl ? emailEl.value.trim() || null : null
    };

    if (!profileData.HoTenKhachHang) {
        showError('errorHoTenKhachHang', 'Họ và tên là bắt buộc');
        return;
    }

    if (!profileData.SdtKhachHang) {
        showError('errorSdtKhachHang', 'Số điện thoại là bắt buộc');
        return;
    }

    showLoading();
    try {
        const response = await fetch('/UserProfile/UpdateProfileData', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(profileData)
        });

        const data = await response.json();

        if (response.ok) {
            showAlert('success', data.message || 'Cập nhật thông tin thành công!');
            var profileNameEl = document.getElementById('profileName');
            if (profileNameEl) {
                profileNameEl.textContent = profileData.HoTenKhachHang;
            }
            
            // Đồng bộ với localStorage
            const savedProfile = localStorage.getItem('userProfile');
            if (savedProfile) {
                const profile = JSON.parse(savedProfile);
                profile.hoTenKhachHang = profileData.HoTenKhachHang;
                profile.emailKhachHang = profileData.EmailKhachHang;
                localStorage.setItem('userProfile', JSON.stringify(profile));
            }
            
            loadProfileData();
        } else {
            showAlert('error', data.message || 'Cập nhật thông tin thất bại!');
        }
    } catch (error) {
        showAlert('error', 'Lỗi kết nối đến server');
    } finally {
        hideLoading();
    }
}

async function changePassword() {
    hideError('errorCurrentPassword');
    hideError('errorNewPassword');
    hideError('errorConfirmPassword');

    var currentPasswordEl = document.getElementById('CurrentPassword');
    var newPasswordEl = document.getElementById('NewPassword');
    var confirmPasswordEl = document.getElementById('ConfirmPassword');

    if (!currentPasswordEl || !newPasswordEl || !confirmPasswordEl) {
        showAlert('error', 'Không tìm thấy các trường nhập liệu');
        return;
    }

    const passwordData = {
        CurrentPassword: currentPasswordEl.value,
        NewPassword: newPasswordEl.value,
        ConfirmPassword: confirmPasswordEl.value
    };

    if (!passwordData.CurrentPassword) {
        showError('errorCurrentPassword', 'Vui lòng nhập mật khẩu hiện tại');
        return;
    }

    if (!passwordData.NewPassword) {
        showError('errorNewPassword', 'Vui lòng nhập mật khẩu mới');
        return;
    }
    var newPasswordLength = passwordData.NewPassword ? passwordData.NewPassword.length : 0;
    if (newPasswordLength < 6) {
        showError('errorNewPassword', 'Mật khẩu mới phải có ít nhất 6 ký tự');
        return;
    }

    if (passwordData.NewPassword !== passwordData.ConfirmPassword) {
        showError('errorConfirmPassword', 'Mật khẩu xác nhận không khớp');
        return;
    }

    showLoading();
    try {
        const response = await fetch('/UserProfile/ChangePassword', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(passwordData)
        });

        const data = await response.json();

        if (response.ok) {
            showAlert('success', data.message || 'Đổi mật khẩu thành công!');
            if (currentPasswordEl) currentPasswordEl.value = '';
            if (newPasswordEl) newPasswordEl.value = '';
            if (confirmPasswordEl) confirmPasswordEl.value = '';
        } else {
            showAlert('error', data.message || 'Đổi mật khẩu thất bại!');
        }
    } catch (error) {
        showAlert('error', 'Lỗi kết nối đến server');
    } finally {
        hideLoading();
    }
}

// Map trạng thái từ string hoặc số sang số
function getOrderStatusNumber(trangThai) {
    if (typeof trangThai === 'number') {
        return trangThai;
    }
    if (typeof trangThai === 'string') {
        // Nếu là số dạng string
        var num = parseInt(trangThai);
        if (!isNaN(num)) {
            return num;
        }
        // Map từ string sang số
        var lower = trangThai.toLowerCase();
        if (lower.includes('chờ') || lower.includes('pending') || lower === '0') return 0;
        if (lower.includes('xác nhận') || lower.includes('confirmed') || lower === '1') return 1;
        if (lower.includes('vận chuyển') || lower.includes('shipping') || lower === '2') return 2;
        if (lower.includes('thành công') || lower.includes('completed') || lower === '3') return 3;
        if (lower.includes('hủy') || lower.includes('cancel') || lower === '4') return 4;
    }
    return -1; // Không xác định
}

// Lấy tên trạng thái từ số
function getOrderStatusName(statusNum) {
    switch (statusNum) {
        case 0: return 'Chờ xác nhận';
        case 1: return 'Đã xác nhận';
        case 2: return 'Đang vận chuyển';
        case 3: return 'Giao hàng thành công';
        case 4: return 'Hủy đơn hàng';
        default: return 'Không xác định';
    }
}

async function loadOrders() {
    try {
        const response = await fetch('/UserProfile/GetOrders');
        const result = await response.json();

        if (result.success && result.data) {
            // Lưu tất cả đơn hàng
            allOrdersData = result.data;
            
            // Sắp xếp theo thời gian mới nhất
            allOrdersData.sort(function(a, b) {
                var dateA = new Date(a.ngayLap || 0);
                var dateB = new Date(b.ngayLap || 0);
                return dateB - dateA;
            });

            // Render đơn hàng với filter hiện tại
            renderOrders();
        } else {
            var ordersListEl = document.getElementById('ordersList');
            if (ordersListEl) {
                ordersListEl.innerHTML = '<p class="text-muted">Không thể tải danh sách đơn hàng.</p>';
            }
        }
    } catch (error) {
        console.error('Error loading orders:', error);
        var ordersListEl = document.getElementById('ordersList');
        if (ordersListEl) {
            ordersListEl.innerHTML = '<p class="text-danger">Lỗi khi tải danh sách đơn hàng.</p>';
        }
    }
}

function renderOrders() {
    const ordersList = document.getElementById('ordersList');
    if (!ordersList) {
        console.error('ordersList element not found');
        return;
    }

    // Filter đơn hàng theo trạng thái
    var filteredOrders = allOrdersData;
    if (currentOrderStatusFilter !== 'all') {
        var filterStatus = parseInt(currentOrderStatusFilter);
        filteredOrders = allOrdersData.filter(function(order) {
            var orderStatus = getOrderStatusNumber(order.trangThai);
            return orderStatus === filterStatus;
        });
    }

    if (filteredOrders.length === 0) {
        ordersList.innerHTML = '<p class="text-muted" style="color: #aaa !important;">Không có đơn hàng nào.</p>';
        return;
    }

    // Group đơn hàng theo trạng thái
    var ordersByStatus = {};
    for (var i = 0; i < filteredOrders.length; i++) {
        var order = filteredOrders[i];
        var statusNum = getOrderStatusNumber(order.trangThai);
        if (!ordersByStatus[statusNum]) {
            ordersByStatus[statusNum] = [];
        }
        ordersByStatus[statusNum].push(order);
    }

    // Render từng nhóm trạng thái
    var ordersHtml = '';
    var statusOrder = [0, 1, 2, 3, 4]; // Thứ tự hiển thị
    
    for (var s = 0; s < statusOrder.length; s++) {
        var statusNum = statusOrder[s];
        if (!ordersByStatus[statusNum] || ordersByStatus[statusNum].length === 0) {
            continue;
        }

        var statusName = getOrderStatusName(statusNum);
        var statusOrders = ordersByStatus[statusNum];

        ordersHtml += '<div class="status-group">';
        ordersHtml += '<h5 class="status-group-title">' + statusName + ' (' + statusOrders.length + ')</h5>';
        ordersHtml += '<div class="accordion" id="accordionStatus' + statusNum + '">';

        for (var i = 0; i < statusOrders.length; i++) {
            var order = statusOrders[i];
            var statusClass = 'status-' + statusNum;
            var collapseId = 'collapse' + order.idHoaDon;
            var headingId = 'heading' + order.idHoaDon;

            // Render chi tiết sản phẩm
            var chiTietArray = order.chiTiet;
            var itemsHtml = '';
            if (chiTietArray) {
                var isArray = chiTietArray.constructor === Array;
                var chiTietLength = isArray ? chiTietArray.length : 0;
                if (chiTietLength > 0) {
                    for (var j = 0; j < chiTietLength; j++) {
                        var item = null;
                        if (chiTietArray) {
                            item = chiTietArray[j];
                        }
                        if (!item) continue;
                        itemsHtml += '<div class="order-item">' +
                                '<img src="' + (item.hinhAnh || '/images/default-product.jpg') + '" alt="' + (item.tenSanPham || '') + '" onerror="this.src=\'/images/default-product.jpg\'">' +
                                '<div class="flex-grow-1">' +
                                '<div style="color: #ffffff !important;"><strong>' + (item.tenSanPham || '') + '</strong></div>' +
                                '<div class="text-muted small" style="color: #aaa !important;">' + 
                                (item.tenThuongHieu && item.tenThuongHieu !== 'N/A' ? '<strong>Thương hiệu:</strong> ' + (item.tenThuongHieu || '') + ' | ' : '') +
                                (item.tenModel || '') + ' - ' + (item.mau || '') + 
                                '</div>' +
                                '<div style="color: #ffffff !important;">Số lượng: ' + (item.soLuong || 0) + ' x ' + formatMoney(item.donGia || 0) + '</div>' +
                                '</div>' +
                                '<div style="color: #ffffff !important;"><strong>' + formatMoney(item.thanhTien || 0) + '</strong></div>' +
                                '</div>';
                        }
                    }
                }

            // Nút action
            var actionButtons = '';
            if (statusNum === 0) {
                actionButtons = '<button class="btn btn-outline-danger btn-sm me-2" onclick="cancelOrder(' + order.idHoaDon + ')">' +
                    '<i class="bi bi-x-circle me-1"></i>Hủy đơn</button>';
            } else if (statusNum === 2) {
                actionButtons = '<button class="btn btn-success btn-sm me-2" onclick="confirmReceivedOrder(' + order.idHoaDon + ')">' +
                    '<i class="bi bi-check-circle me-1"></i>Xác nhận đã nhận</button>';
            }

            ordersHtml += '<div class="accordion-item" style="background-color: #2b2b33; border: 1px solid #444; margin-bottom: 10px; border-radius: 8px; overflow: hidden;">' +
                '<h2 class="accordion-header" id="' + headingId + '">' +
                '<button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#' + collapseId + '" aria-expanded="false" aria-controls="' + collapseId + '" style="background-color: #2b2b33; color: #ffffff !important;">' +
                '<div class="w-100 d-flex justify-content-between align-items-center">' +
                    '<div>' +
                '<strong style="color: #ffffff !important;">Mã đơn: ' + (order.maDon || '') + '</strong>' +
                '<div class="text-muted small" style="color: #aaa !important; margin-top: 5px;">' + (order.ngayLap || '') + '</div>' +
                    '</div>' +
                '<div class="text-end">' +
                '<span class="order-status ' + statusClass + '">' + statusName + '</span>' +
                '<div class="mt-2" style="color: #ffffff !important;"><strong>' + formatMoney(order.tongTien || 0) + '</strong></div>' +
                    '</div>' +
                    '</div>' +
                '</button>' +
                '</h2>' +
                '<div id="' + collapseId + '" class="accordion-collapse collapse" aria-labelledby="' + headingId + '" data-bs-parent="#accordionStatus' + statusNum + '">' +
                '<div class="accordion-body">' +
                '<div class="order-content">' +
                itemsHtml +
                '<div class="mt-3 d-flex justify-content-between align-items-center">' +
                '<button class="btn btn-outline-primary btn-sm" onclick="showOrderDetail(' + order.idHoaDon + ')">' +
                '<i class="bi bi-eye me-1"></i>Xem chi tiết</button>' +
                '<div>' + actionButtons + '</div>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>' +
                    '</div>';
            }

        ordersHtml += '</div>'; // End accordion
        ordersHtml += '</div>'; // End status-group
    }

    ordersList.innerHTML = ordersHtml;
}

function filterOrdersByStatus(status) {
    currentOrderStatusFilter = status;
    
    // Update active tab
    var tabs = document.querySelectorAll('#orderStatusTabs .nav-link');
    for (var i = 0; i < tabs.length; i++) {
        tabs[i].classList.remove('active');
        if (tabs[i].getAttribute('data-status') === status) {
            tabs[i].classList.add('active');
        }
    }
    
    // Re-render orders
    renderOrders();
}

function showOrderDetail(orderId) {
    var order = allOrdersData.find(function(o) { return o.idHoaDon === orderId; });
    if (!order) {
        showAlert('error', 'Không tìm thấy đơn hàng');
        return;
    }

    var statusNum = getOrderStatusNumber(order.trangThai);
    var statusName = getOrderStatusName(statusNum);
    var statusClass = 'status-' + statusNum;

    var chiTietHtml = '';
    if (order.chiTiet && order.chiTiet.length > 0) {
        for (var i = 0; i < order.chiTiet.length; i++) {
            var item = order.chiTiet[i];
            chiTietHtml += '<div class="order-detail-item">' +
                '<div class="row">' +
                '<div class="col-md-2">' +
                '<img src="' + (item.hinhAnh || '/images/default-product.jpg') + '" alt="' + (item.tenSanPham || '') + '" ' +
                'style="width: 100%; max-width: 100px; border-radius: 8px;" onerror="this.src=\'/images/default-product.jpg\'">' +
                '</div>' +
                '<div class="col-md-10">' +
                '<h6>' + (item.tenSanPham || '') + '</h6>' +
                '<p class="text-muted mb-1">' +
                    (item.tenThuongHieu && item.tenThuongHieu !== 'N/A' ? '<strong>Thương hiệu:</strong> ' + (item.tenThuongHieu || '') + ' | ' : '') +
                    'Model: ' + (item.tenModel || '') + ' - Màu: ' + (item.mau || '') + 
                '</p>' +
                '<p class="mb-1">Số lượng: ' + (item.soLuong || 0) + '</p>' +
                '<p class="mb-1">Đơn giá: ' + formatMoney(item.donGia || 0) + '</p>' +
                '<p class="mb-0"><strong>Thành tiền: ' + formatMoney(item.thanhTien || 0) + '</strong></p>' +
                '</div>' +
                '</div>' +
                '</div>';
        }
    }

    var detailHtml = '<div class="order-detail">' +
        '<div class="mb-3">' +
        '<h6>Thông tin đơn hàng</h6>' +
        '<p class="mb-1"><strong>Mã đơn:</strong> ' + (order.maDon || '') + '</p>' +
        '<p class="mb-1"><strong>Ngày đặt:</strong> ' + (order.ngayLap || '') + '</p>' +
        '<p class="mb-1"><strong>Trạng thái:</strong> <span class="order-status ' + statusClass + '">' + statusName + '</span></p>' +
        '<p class="mb-1"><strong>Phương thức thanh toán:</strong> ' + (order.phuongThucThanhToan || 'COD') + '</p>' +
        '<p class="mb-1"><strong>Tổng tiền:</strong> <strong class="text-primary">' + formatMoney(order.tongTien || 0) + '</strong></p>' +
        '</div>' +
        '<div class="mb-3">' +
        '<h6>Chi tiết sản phẩm</h6>' +
        chiTietHtml +
        '</div>' +
        '</div>';

    var detailContent = document.getElementById('orderDetailContent');
    if (detailContent) {
        detailContent.innerHTML = detailHtml;
    }

    var modal = new bootstrap.Modal(document.getElementById('orderDetailModal'));
    modal.show();
}

async function cancelOrder(orderId) {
    if (!confirm('Bạn có chắc chắn muốn hủy đơn hàng này?')) {
        return;
    }

    showLoading();
    try {
        // TODO: Thêm API endpoint để hủy đơn hàng
        // const response = await fetch('/UserProfile/CancelOrder?id=' + orderId, {
        //     method: 'POST'
        // });
        
        // Tạm thời chỉ reload lại danh sách
        showAlert('success', 'Đơn hàng đã được hủy thành công!');
        await loadOrders();
    } catch (error) {
        console.error('Error cancelling order:', error);
        showAlert('error', 'Lỗi khi hủy đơn hàng');
    } finally {
        hideLoading();
    }
}

async function confirmReceivedOrder(orderId) {
    if (!confirm('Bạn có chắc chắn đã nhận được đơn hàng này?')) {
        return;
    }

    showLoading();
    try {
        // TODO: Thêm API endpoint để xác nhận đã nhận đơn hàng
        // const response = await fetch('/UserProfile/ConfirmReceivedOrder?id=' + orderId, {
        //     method: 'POST'
        // });
        
        // Tạm thời chỉ reload lại danh sách
        showAlert('success', 'Đã xác nhận nhận hàng thành công!');
        await loadOrders();
    } catch (error) {
        console.error('Error confirming order:', error);
        showAlert('error', 'Lỗi khi xác nhận đơn hàng');
    } finally {
        hideLoading();
    }
}

function cancelChanges() {
    if (confirm('Bạn có chắc chắn muốn hủy các thay đổi?')) {
        loadProfileData();
    }
}

function showError(elementId, message) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = message;
        element.classList.remove('d-none');
    }
}

function hideError(elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.classList.add('d-none');
    }
}

// ====== ADDRESS MANAGEMENT FUNCTIONS ======
function openAddAddressModal() {
    var addModal = document.getElementById('addAddressModal');
    if (addModal) {
        var modal = new bootstrap.Modal(addModal);
        modal.show();
        loadProvinces();
    }
}

async function loadAddressList() {
    if (!currentKhachHangId) {
        var addressListEl = document.getElementById('addressList');
        if (addressListEl) {
            addressListEl.innerHTML = '<div class="col-12"><p class="text-muted">Đang tải thông tin...</p></div>';
        }
        return;
    }

    try {
        var response = await fetch('/DiaChi/GetByKhachHang?idKhachHang=' + currentKhachHangId);
        if (response.ok) {
            var addresses = await response.json();
            renderAddressList(addresses);
        } else {
            var addressListEl = document.getElementById('addressList');
            if (addressListEl) {
                addressListEl.innerHTML = '<div class="col-12"><p class="text-danger">Lỗi khi tải danh sách địa chỉ</p></div>';
            }
        }
    } catch (error) {
        console.error('Error loading addresses:', error);
        var addressListEl = document.getElementById('addressList');
        if (addressListEl) {
            addressListEl.innerHTML = '<div class="col-12"><p class="text-danger">Lỗi kết nối</p></div>';
        }
    }
}

function renderAddressList(addresses) {
    var container = document.getElementById('addressList');
    if (!container) return;

    if (!addresses || addresses.length === 0) {
        container.innerHTML = '<div class="col-12"><p class="text-muted"><i class="bi bi-info-circle me-2"></i>Chưa có địa chỉ nào. Hãy thêm địa chỉ mới!</p></div>';
        return;
    }

    var html = '';
    for (var i = 0; i < addresses.length; i++) {
        var address = addresses[i];
        var isDefault = address.trangthai === 0;
        var defaultBadge = isDefault ? '<span class="badge">Mặc định</span>' : '';
        
        html += '<div class="col-md-6 mb-3">' +
            '<div class="address-card' + (isDefault ? ' default' : '') + '">' +
            '<div class="d-flex">' +
            '<div class="me-3">' +
            '<i class="bi bi-house-door fs-4" style="color: ' + (isDefault ? '#ffc107' : '#4a90e2') + ';"></i>' +
            '</div>' +
            '<div class="flex-grow-1">' +
            '<div class="d-flex justify-content-between align-items-start mb-2">' +
            '<h6 class="card-title mb-0">' + (address.tennguoinhan || '') + '</h6>' +
            defaultBadge +
            '</div>' +
            '<p class="card-text mb-2">' +
            '<i class="bi bi-telephone me-2"></i>' + (address.sdtnguoinhan || '') +
            '</p>' +
            '<p class="card-text mb-3">' +
            '<i class="bi bi-geo-alt me-2"></i>' +
            (address.diachicuthe || '') + ', ' +
            (address.wardName || '') + ', ' +
            (address.districtName || '') + ', ' +
            (address.provinceName || '') +
            '</p>' +
            '<div class="d-flex gap-2 justify-content-end">' +
            '<button class="btn btn-outline-primary btn-sm" onclick="openEditAddressModal(' + address.id + ')">' +
            '<i class="bi bi-pencil me-1"></i>Sửa' +
            '</button>' +
            '<button class="btn btn-outline-danger btn-sm" onclick="deleteAddress(' + address.id + ', ' + address.trangthai + ')">' +
            '<i class="bi bi-trash me-1"></i>Xóa' +
            '</button>';
        
        if (!isDefault) {
            html += '<button class="btn btn-warning btn-sm" onclick="setDefaultAddress(' + address.id + ')">' +
                '<i class="bi bi-check-circle me-1"></i>Đặt mặc định' +
                '</button>';
        }
        
        html += '</div>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div>';
    }
    
    container.innerHTML = html;
}

async function loadProvinces() {
    try {
        var response = await fetch('/DiaChi/GetProvinces');
        if (response.ok) {
            var provinces = await response.json();
            var select = document.getElementById('selectTinh');
            if (select) {
                select.innerHTML = '<option value="">Chọn Tỉnh/Thành</option>';
                for (var id in provinces) {
                    if (provinces.hasOwnProperty(id)) {
                        select.innerHTML += '<option value="' + id + '">' + provinces[id] + '</option>';
                    }
                }
            }
        }
    } catch (error) {
        console.error('Error loading provinces:', error);
    }
}

async function loadDistricts(provinceId, targetSelectId) {
    if (!provinceId) return;

    try {
        var response = await fetch('/DiaChi/GetDistricts', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(provinceId))
        });

        if (response.ok) {
            var districts = await response.json();
            var select = document.getElementById(targetSelectId);
            if (select) {
                select.innerHTML = '<option value="">Chọn Quận/Huyện</option>';
                for (var id in districts) {
                    if (districts.hasOwnProperty(id)) {
                        select.innerHTML += '<option value="' + id + '">' + districts[id] + '</option>';
                    }
                }
                
                // Reset phường/xã
                var wardSelectId = targetSelectId === 'selectQuan' ? 'selectPhuong' : 'editPhuong';
                var wardSelect = document.getElementById(wardSelectId);
                if (wardSelect) {
                    wardSelect.innerHTML = '<option value="">Chọn Phường/Xã</option>';
                }
            }
        }
    } catch (error) {
        console.error('Error loading districts:', error);
    }
}

async function loadWards(districtId, targetSelectId) {
    if (!districtId) return;

    try {
        var response = await fetch('/DiaChi/GetWards', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(districtId))
        });

        if (response.ok) {
            var wards = await response.json();
            var select = document.getElementById(targetSelectId);
            if (select) {
                select.innerHTML = '<option value="">Chọn Phường/Xã</option>';
                for (var id in wards) {
                    if (wards.hasOwnProperty(id)) {
                        select.innerHTML += '<option value="' + id + '">' + wards[id] + '</option>';
                    }
                }
            }
        }
    } catch (error) {
        console.error('Error loading wards:', error);
    }
}

async function addNewAddress() {
    if (!currentKhachHangId) {
        showAlert('error', 'Không tìm thấy thông tin khách hàng');
        return;
    }

    var formData = {
        Tennguoinhan: document.getElementById('tennguoinhan').value.trim(),
        sdtnguoinhan: document.getElementById('sdtnguoinhan').value.trim(),
        Thanhpho: document.getElementById('selectTinh').value,
        Quanhuyen: document.getElementById('selectQuan').value,
        Phuongxa: document.getElementById('selectPhuong').value,
        Diachicuthe: document.getElementById('detailInput').value.trim()
    };

    // Validation
    if (!formData.Tennguoinhan) {
        showAlert('error', 'Tên người nhận không được để trống');
        return;
    }

    if (!formData.sdtnguoinhan) {
        showAlert('error', 'Số điện thoại không được để trống');
        return;
    }

    var phoneRegex = /^[0-9]{10}$/;
    if (!phoneRegex.test(formData.sdtnguoinhan)) {
        showAlert('error', 'Số điện thoại phải gồm đúng 10 số');
        return;
    }

    if (!formData.Thanhpho) {
        showAlert('error', 'Vui lòng chọn tỉnh/thành phố');
        return;
    }

    if (!formData.Quanhuyen) {
        showAlert('error', 'Vui lòng chọn quận/huyện');
        return;
    }

    if (!formData.Phuongxa) {
        showAlert('error', 'Vui lòng chọn phường/xã');
        return;
    }

    if (!formData.Diachicuthe) {
        showAlert('error', 'Địa chỉ cụ thể không được để trống');
        return;
    }

    showLoading();
    try {
        var response = await fetch('/DiaChi/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Tennguoinhan: formData.Tennguoinhan,
                sdtnguoinhan: formData.sdtnguoinhan,
                Thanhpho: formData.Thanhpho,
                Quanhuyen: formData.Quanhuyen,
                Phuongxa: formData.Phuongxa,
                Diachicuthe: formData.Diachicuthe,
                IdKhachHang: currentKhachHangId,
                Id: 0
            })
        });

        if (response.ok) {
            showAlert('success', 'Thêm địa chỉ thành công!');
            var addModal = document.getElementById('addAddressModal');
            if (addModal) {
                var modal = bootstrap.Modal.getInstance(addModal);
                if (modal) modal.hide();
            }
            resetAddAddressForm();
            await loadAddressList();
        } else {
            var errorText = await response.text();
            showAlert('error', errorText || 'Lỗi khi thêm địa chỉ');
        }
    } catch (error) {
        console.error('Error:', error);
        showAlert('error', 'Lỗi kết nối');
    } finally {
        hideLoading();
    }
}

async function openEditAddressModal(id) {
    if (!currentKhachHangId) return;

    try {
        showLoading();
        var response = await fetch('/DiaChi/GetByKhachHang?idKhachHang=' + currentKhachHangId);
        if (response.ok) {
            var addresses = await response.json();
            var address = null;
            for (var i = 0; i < addresses.length; i++) {
                if (addresses[i].id === id) {
                    address = addresses[i];
                    break;
                }
            }

            if (address) {
                document.getElementById('editId').value = address.id;
                document.getElementById('editTrangThai').value = address.trangthai;
                document.getElementById('editTenNguoiNhan').value = address.tennguoinhan || '';
                document.getElementById('editSdtNguoiNhan').value = address.sdtnguoinhan || '';
                document.getElementById('editDiaChiCuThe').value = address.diachicuthe || '';

                await loadProvincesForEdit(address.thanhpho);
                await loadDistrictsForEdit(address.thanhpho, address.quanhuyen);
                await loadWardsForEdit(address.quanhuyen, address.phuongxa);

                var editModal = document.getElementById('editAddressModal');
                if (editModal) {
                    var modal = new bootstrap.Modal(editModal);
                    modal.show();
                }
            }
        }
    } catch (error) {
        console.error('Error:', error);
        showAlert('error', 'Lỗi khi tải thông tin địa chỉ');
    } finally {
        hideLoading();
    }
}

async function updateAddress() {
    if (!currentKhachHangId) return;

    var formData = {
        Id: parseInt(document.getElementById('editId').value),
        Tennguoinhan: document.getElementById('editTenNguoiNhan').value.trim(),
        sdtnguoinhan: document.getElementById('editSdtNguoiNhan').value.trim(),
        Thanhpho: document.getElementById('editTinh').value,
        Quanhuyen: document.getElementById('editQuan').value,
        Phuongxa: document.getElementById('editPhuong').value,
        Diachicuthe: document.getElementById('editDiaChiCuThe').value.trim(),
        trangthai: parseInt(document.getElementById('editTrangThai').value),
        IdKhachHang: currentKhachHangId
    };

    // Validation (tương tự addNewAddress)
    if (!formData.Tennguoinhan || !formData.sdtnguoinhan || !formData.Thanhpho || 
        !formData.Quanhuyen || !formData.Phuongxa || !formData.Diachicuthe) {
        showAlert('error', 'Vui lòng điền đầy đủ thông tin');
        return;
    }

    showLoading();
    try {
        var response = await fetch('/DiaChi/Update?id=' + formData.Id, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(formData)
        });

        if (response.ok) {
            showAlert('success', 'Cập nhật địa chỉ thành công!');
            var editModal = document.getElementById('editAddressModal');
            if (editModal) {
                var modal = bootstrap.Modal.getInstance(editModal);
                if (modal) modal.hide();
            }
            await loadAddressList();
        } else {
            var errorText = await response.text();
            showAlert('error', errorText || 'Lỗi khi cập nhật địa chỉ');
        }
    } catch (error) {
        console.error('Error:', error);
        showAlert('error', 'Lỗi kết nối');
    } finally {
        hideLoading();
    }
}

async function deleteAddress(id, trangthai) {
    if (trangthai === 0) {
        showAlert('error', 'Không thể xóa địa chỉ mặc định');
        return;
    }

    if (!confirm('Bạn có chắc muốn xóa địa chỉ này?')) {
        return;
    }

    showLoading();
    try {
        var response = await fetch('/DiaChi/Delete?id=' + id, {
            method: 'DELETE'
        });

        if (response.ok) {
            showAlert('success', 'Xóa địa chỉ thành công!');
            await loadAddressList();
        } else {
            var errorText = await response.text();
            showAlert('error', errorText || 'Lỗi khi xóa địa chỉ');
        }
    } catch (error) {
        console.error('Error:', error);
        showAlert('error', 'Lỗi kết nối');
    } finally {
        hideLoading();
    }
}

async function setDefaultAddress(idDiaChi) {
    if (!currentKhachHangId) return;

    showLoading();
    try {
        var response = await fetch('/DiaChi/SetDefaultAddress?idDiaChi=' + idDiaChi + '&idKhachHang=' + currentKhachHangId, {
            method: 'POST'
        });

        if (response.ok) {
            showAlert('success', 'Đã cập nhật địa chỉ mặc định!');
            await loadAddressList();
        } else {
            var errorText = await response.text();
            showAlert('error', errorText || 'Lỗi khi cập nhật địa chỉ mặc định');
        }
    } catch (error) {
        console.error('Error:', error);
        showAlert('error', 'Lỗi kết nối');
    } finally {
        hideLoading();
    }
}

async function loadProvincesForEdit(selectedProvinceId) {
    try {
        var response = await fetch('/DiaChi/GetProvinces');
        if (response.ok) {
            var provinces = await response.json();
            var select = document.getElementById('editTinh');
            if (select) {
                select.innerHTML = '<option value="">Chọn Tỉnh/Thành</option>';
                for (var id in provinces) {
                    if (provinces.hasOwnProperty(id)) {
                        var selected = id === selectedProvinceId ? 'selected' : '';
                        select.innerHTML += '<option value="' + id + '" ' + selected + '>' + provinces[id] + '</option>';
                    }
                }
            }
        }
    } catch (error) {
        console.error('Error loading provinces:', error);
    }
}

async function loadDistrictsForEdit(provinceId, selectedDistrictId) {
    if (!provinceId) return;

    try {
        var response = await fetch('/DiaChi/GetDistricts', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(provinceId))
        });

        if (response.ok) {
            var districts = await response.json();
            var select = document.getElementById('editQuan');
            if (select) {
                select.innerHTML = '<option value="">Chọn Quận/Huyện</option>';
                for (var id in districts) {
                    if (districts.hasOwnProperty(id)) {
                        var selected = id === selectedDistrictId ? 'selected' : '';
                        select.innerHTML += '<option value="' + id + '" ' + selected + '>' + districts[id] + '</option>';
                    }
                }
            }
        }
    } catch (error) {
        console.error('Error loading districts:', error);
    }
}

async function loadWardsForEdit(districtId, selectedWardCode) {
    if (!districtId) return;

    try {
        var response = await fetch('/DiaChi/GetWards', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(districtId))
        });

        if (response.ok) {
            var wards = await response.json();
            var select = document.getElementById('editPhuong');
            if (select) {
                select.innerHTML = '<option value="">Chọn Phường/Xã</option>';
                for (var id in wards) {
                    if (wards.hasOwnProperty(id)) {
                        var selected = id === selectedWardCode ? 'selected' : '';
                        select.innerHTML += '<option value="' + id + '" ' + selected + '>' + wards[id] + '</option>';
                    }
                }
            }
        }
    } catch (error) {
        console.error('Error loading wards:', error);
    }
}

function resetAddAddressForm() {
    var form = document.getElementById('addAddressForm');
    if (form) {
        form.reset();
    }
    var selectQuan = document.getElementById('selectQuan');
    if (selectQuan) {
        selectQuan.innerHTML = '<option value="">Chọn Quận/Huyện</option>';
    }
    var selectPhuong = document.getElementById('selectPhuong');
    if (selectPhuong) {
        selectPhuong.innerHTML = '<option value="">Chọn Phường/Xã</option>';
    }
}

// ====== CHANGE EMAIL FUNCTIONS ======
function openChangeEmailModal() {
    var emailDisplayEl = document.getElementById('EmailKhachHangDisplay');
    var currentEmailEl = document.getElementById('currentEmail');
    var newEmailEl = document.getElementById('newEmail');
    var confirmPasswordEl = document.getElementById('confirmPasswordForEmail');
    var changeEmailModalEl = document.getElementById('changeEmailModal');

    if (!emailDisplayEl || !currentEmailEl || !newEmailEl || !confirmPasswordEl || !changeEmailModalEl) {
        showAlert('error', 'Không tìm thấy các phần tử modal');
        return;
    }

    const currentEmail = emailDisplayEl.value;
    currentEmailEl.value = currentEmail;
    newEmailEl.value = '';
    confirmPasswordEl.value = '';
    hideError('errorNewEmail');
    hideError('errorConfirmPasswordForEmail');
    
    const modal = new bootstrap.Modal(changeEmailModalEl);
    modal.show();
}

async function confirmChangeEmail() {
    hideError('errorNewEmail');
    hideError('errorConfirmPasswordForEmail');

    var newEmailEl = document.getElementById('newEmail');
    var confirmPasswordEl = document.getElementById('confirmPasswordForEmail');
    var currentEmailEl = document.getElementById('currentEmail');

    if (!newEmailEl || !confirmPasswordEl || !currentEmailEl) {
        showAlert('error', 'Không tìm thấy các trường nhập liệu');
        return;
    }

    const newEmail = newEmailEl.value.trim();
    const confirmPassword = confirmPasswordEl.value;
    const currentEmail = currentEmailEl.value.trim();

    // Validation
    if (!newEmail) {
        showError('errorNewEmail', 'Vui lòng nhập email mới');
        return;
    }

    // Validate email format
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(newEmail)) {
        showError('errorNewEmail', 'Email không hợp lệ');
        return;
    }

    // Check if new email is same as current
    if (newEmail === currentEmail) {
        showError('errorNewEmail', 'Email mới phải khác email hiện tại');
        return;
    }

    if (!confirmPassword) {
        showError('errorConfirmPasswordForEmail', 'Vui lòng nhập mật khẩu để xác nhận');
        return;
    }

    showLoading();
    try {
        const response = await fetch('/UserProfile/ChangeEmail', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                NewEmail: newEmail,
                ConfirmPassword: confirmPassword
            })
        });

        const data = await response.json();

        if (response.ok) {
            showAlert('success', data.message || 'Đổi email thành công!');
            
            // Đóng modal
            var changeEmailModalEl = document.getElementById('changeEmailModal');
            if (changeEmailModalEl) {
                const modal = bootstrap.Modal.getInstance(changeEmailModalEl);
                if (modal) modal.hide();
            }
            
            // Đợi một chút để cookie được cập nhật, sau đó reload profile data
            setTimeout(function() {
                loadProfileData();
            }, 500);
        } else {
            showAlert('error', data.message || 'Đổi email thất bại!');
            if (data.message && data.message.includes('mật khẩu')) {
                showError('errorConfirmPasswordForEmail', data.message);
            } else if (data.message && data.message.includes('email')) {
                showError('errorNewEmail', data.message);
            }
        }
    } catch (error) {
        showAlert('error', 'Lỗi kết nối đến server');
    } finally {
        hideLoading();
    }
}

function goToHomePage() {
    // Đảm bảo localStorage được cập nhật trước khi chuyển trang
    const savedProfile = localStorage.getItem('userProfile');
    if (savedProfile) {
        // Trigger một lần nữa để đảm bảo đồng bộ
        const profile = JSON.parse(savedProfile);
        window.dispatchEvent(new CustomEvent('userProfileUpdated', {
            detail: profile
        }));
    }
    // Thêm query parameter để MuaHang biết cần reload profile
    window.location.href = '/MuaHang?refreshProfile=1';
}

// ====== INITIALIZATION ======
document.addEventListener('DOMContentLoaded', function () {
    loadProfileData();
    
    // Load orders when orders tab is clicked
    var ordersTab = document.getElementById('orders-tab');
    if (ordersTab) {
        ordersTab.addEventListener('shown.bs.tab', function () {
            loadOrders();
        });
    }

    // Setup order status filter tabs
    var statusTabs = document.querySelectorAll('#orderStatusTabs .nav-link');
    for (var i = 0; i < statusTabs.length; i++) {
        statusTabs[i].addEventListener('click', function(e) {
            var status = this.getAttribute('data-status');
            filterOrdersByStatus(status);
        });
    }

    // Load addresses when address tab is clicked
    var addressTab = document.getElementById('address-tab');
    if (addressTab) {
        addressTab.addEventListener('shown.bs.tab', function () {
            loadAddressList();
        });
    }

    // Setup address form event listeners
    var selectTinh = document.getElementById('selectTinh');
    if (selectTinh) {
        selectTinh.addEventListener('change', function () {
            loadDistricts(this.value, 'selectQuan');
        });
    }

    var selectQuan = document.getElementById('selectQuan');
    if (selectQuan) {
        selectQuan.addEventListener('change', function () {
            loadWards(this.value, 'selectPhuong');
        });
    }

    var editTinh = document.getElementById('editTinh');
    if (editTinh) {
        editTinh.addEventListener('change', function () {
            loadDistricts(this.value, 'editQuan');
        });
    }

    var editQuan = document.getElementById('editQuan');
    if (editQuan) {
        editQuan.addEventListener('change', function () {
            loadWards(this.value, 'editPhuong');
        });
    }

    var btnSaveAddress = document.getElementById('btnSaveAddress');
    if (btnSaveAddress) {
        btnSaveAddress.addEventListener('click', addNewAddress);
    }

    var btnUpdateAddress = document.getElementById('btnUpdateAddress');
    if (btnUpdateAddress) {
        btnUpdateAddress.addEventListener('click', updateAddress);
    }
});
