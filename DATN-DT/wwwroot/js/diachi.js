// Biến toàn cục
let currentKhachHangId = 1; // ID khách hàng từ controller

// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', function () {
    loadAddressList();
    loadProvinces();
    setupEventListeners();
});

// Setup event listeners
function setupEventListeners() {
    // Thêm địa chỉ
    document.getElementById('btnSaveAddress').addEventListener('click', addNewAddress);

    // Cập nhật địa chỉ
    document.getElementById('btnUpdateAddress').addEventListener('click', updateAddress);

    // Change events cho select
    document.getElementById('selectTinh').addEventListener('change', function () {
        loadDistricts(this.value, 'selectQuan');
    });

    document.getElementById('selectQuan').addEventListener('change', function () {
        loadWards(this.value, 'selectPhuong');
    });

    document.getElementById('editTinh').addEventListener('change', function () {
        loadDistricts(this.value, 'editQuan');
    });

    document.getElementById('editQuan').addEventListener('change', function () {
        loadWards(this.value, 'editPhuong');
    });
}

// Load danh sách địa chỉ
async function loadAddressList() {
    try {
        showLoading();
        const response = await fetch(`/DiaChi/GetByKhachHang?idKhachHang=${currentKhachHangId}`);
        if (response.ok) {
            const addresses = await response.json();
            renderAddressList(addresses);
        } else {
            showError('Lỗi khi tải danh sách địa chỉ');
        }
    } catch (error) {
        console.error('Error:', error);
        showError('Lỗi kết nối');
    } finally {
        hideLoading();
    }
}

// Render danh sách địa chỉ
function renderAddressList(addresses) {
    const container = document.querySelector('.diachi-list');

    if (!addresses || addresses.length === 0) {
        container.innerHTML = `
            <div class="col-12">
                <div class="alert alert-info text-center">
                    <i class="bi bi-info-circle"></i> Chưa có địa chỉ nào. Hãy thêm địa chỉ mới!
                </div>
            </div>`;
        return;
    }

    container.innerHTML = addresses.map(address => `
        <div class="col-md-6 mb-3">
            <div class="card h-100 ${address.trangthai === 0 ? 'border-warning' : ''}">
                <div class="card-body">
                    <div class="d-flex">
                        <div class="me-3">
                            <i class="bi bi-house-door fs-4 ${address.trangthai === 0 ? 'text-warning' : 'text-muted'}"></i>
                        </div>
                        <div class="flex-grow-1">
                            <div class="d-flex justify-content-between align-items-start">
                                <h5 class="card-title">${address.tennguoinhan}</h5>
                                ${address.trangthai === 0 ? '<span class="badge bg-warning">Mặc định</span>' : ''}
                            </div>
                            <p class="card-text text-muted">
                                <i class="bi bi-telephone me-2"></i>${address.sdtnguoinhan}
                            </p>
                            <p class="card-text">
                                <i class="bi bi-geo-alt me-2"></i>
                                ${address.diachicuthe}, ${address.wardName}, ${address.districtName}, ${address.provinceName}
                            </p>
                        </div>
                    </div>
                </div>
                <div class="card-footer bg-transparent">
                    <div class="d-flex gap-2 justify-content-end">
                        <button class="btn btn-outline-primary btn-sm" onclick="openEditModal(${address.id})">
                            <i class="bi bi-pencil"></i> Sửa
                        </button>
                        <button class="btn btn-outline-danger btn-sm" onclick="deleteAddress(${address.id}, ${address.trangthai})">
                            <i class="bi bi-trash"></i> Xóa
                        </button>
                        ${address.trangthai !== 0 ? `
                        <button class="btn btn-warning btn-sm" onclick="setDefaultAddress(${address.id})">
                            <i class="bi bi-check-circle"></i> Đặt mặc định
                        </button>` : ''}
                    </div>
                </div>
            </div>
        </div>
    `).join('');
}

// Thêm địa chỉ mới
async function addNewAddress() {
    const formData = getAddFormData();

    if (!validateFormData(formData)) {
        return;
    }

    try {
        showLoading();
        const response = await fetch('/DiaChi/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                ...formData,
                IdKhachHang: currentKhachHangId,
                Id: 0
            })
        });

        if (response.ok) {
            const result = await response.json();
            showSuccess('Thêm địa chỉ thành công!');
            $('#addAddressModal').modal('hide');
            resetAddForm();
            await loadAddressList();
        } else {
            const error = await response.text();
            showError(error);
        }
    } catch (error) {
        console.error('Error:', error);
        showError('Lỗi kết nối');
    } finally {
        hideLoading();
    }
}

// Mở modal sửa
async function openEditModal(id) {
    try {
        showLoading();
        const response = await fetch(`/DiaChi/GetByKhachHang?idKhachHang=${currentKhachHangId}`);
        if (response.ok) {
            const addresses = await response.json();
            const address = addresses.find(a => a.id === id);

            if (address) {
                document.getElementById('editId').value = address.id;
                document.getElementById('editTrangThai').value = address.trangthai;
                document.getElementById('editTenNguoiNhan').value = address.tennguoinhan;
                document.getElementById('editSdtNguoiNhan').value = address.sdtnguoinhan;
                document.getElementById('editDiaChiCuThe').value = address.diachicuthe;

                // Load provinces và set selected
                await loadProvincesForEdit(address.thanhpho);
                await loadDistrictsForEdit(address.thanhpho, address.quanhuyen);
                await loadWardsForEdit(address.quanhuyen, address.phuongxa);

                $('#editAddressModal').modal('show');
            }
        }
    } catch (error) {
        console.error('Error:', error);
        showError('Lỗi khi tải thông tin địa chỉ');
    } finally {
        hideLoading();
    }
}

// Cập nhật địa chỉ
async function updateAddress() {
    const formData = getEditFormData();

    if (!validateFormData(formData)) {
        return;
    }

    try {
        showLoading();
        const response = await fetch(`/DiaChi/Update?id=${formData.id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });

        if (response.ok) {
            showSuccess('Cập nhật địa chỉ thành công!');
            $('#editAddressModal').modal('hide');
            await loadAddressList();
        } else {
            const error = await response.text();
            showError(error);
        }
    } catch (error) {
        console.error('Error:', error);
        showError('Lỗi kết nối');
    } finally {
        hideLoading();
    }
}

// Xóa địa chỉ
async function deleteAddress(id, trangthai) {
    if (trangthai === 0) {
        showWarning('Không thể xóa địa chỉ mặc định');
        return;
    }

    if (!confirm('Bạn có chắc muốn xóa địa chỉ này?')) {
        return;
    }

    try {
        showLoading();
        const response = await fetch(`/DiaChi/Delete?id=${id}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            showSuccess('Xóa địa chỉ thành công!');
            await loadAddressList();
        } else {
            const error = await response.text();
            showError(error);
        }
    } catch (error) {
        console.error('Error:', error);
        showError('Lỗi kết nối');
    } finally {
        hideLoading();
    }
}

// Đặt địa chỉ mặc định
async function setDefaultAddress(idDiaChi) {
    try {
        showLoading();
        const response = await fetch(`/DiaChi/SetDefaultAddress?idDiaChi=${idDiaChi}&idKhachHang=${currentKhachHangId}`, {
            method: 'POST'
        });

        if (response.ok) {
            showSuccess('Đã cập nhật địa chỉ mặc định!');
            await loadAddressList();
        } else {
            const error = await response.text();
            showError(error);
        }
    } catch (error) {
        console.error('Error:', error);
        showError('Lỗi kết nối');
    } finally {
        hideLoading();
    }
}

// GHN API functions
async function loadProvinces() {
    try {
        const response = await fetch('/DiaChi/GetProvinces');
        if (response.ok) {
            const provinces = await response.json();
            const select = document.getElementById('selectTinh');
            select.innerHTML = '<option value="">Chọn Tỉnh/Thành</option>';

            for (const [id, name] of Object.entries(provinces)) {
                select.innerHTML += `<option value="${id}">${name}</option>`;
            }
        }
    } catch (error) {
        console.error('Error loading provinces:', error);
    }
}

async function loadProvincesForEdit(selectedProvinceId) {
    try {
        const response = await fetch('/DiaChi/GetProvinces');
        if (response.ok) {
            const provinces = await response.json();
            const select = document.getElementById('editTinh');
            select.innerHTML = '<option value="">Chọn Tỉnh/Thành</option>';

            for (const [id, name] of Object.entries(provinces)) {
                const selected = id === selectedProvinceId ? 'selected' : '';
                select.innerHTML += `<option value="${id}" ${selected}>${name}</option>`;
            }
        }
    } catch (error) {
        console.error('Error loading provinces:', error);
    }
}

async function loadDistricts(provinceId, targetSelectId) {
    if (!provinceId) return;

    try {
        const response = await fetch('/DiaChi/GetDistricts', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(parseInt(provinceId))
        });

        if (response.ok) {
            const districts = await response.json();
            const select = document.getElementById(targetSelectId);
            select.innerHTML = '<option value="">Chọn Quận/Huyện</option>';

            for (const [id, name] of Object.entries(districts)) {
                select.innerHTML += `<option value="${id}">${name}</option>`;
            }

            // Reset phường/xã
            const wardSelect = targetSelectId === 'selectQuan' ?
                document.getElementById('selectPhuong') : document.getElementById('editPhuong');
            if (wardSelect) {
                wardSelect.innerHTML = '<option value="">Chọn Phường/Xã</option>';
            }
        }
    } catch (error) {
        console.error('Error loading districts:', error);
    }
}

async function loadDistrictsForEdit(provinceId, selectedDistrictId) {
    if (!provinceId) return;

    try {
        const response = await fetch('/DiaChi/GetDistricts', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(parseInt(provinceId))
        });

        if (response.ok) {
            const districts = await response.json();
            const select = document.getElementById('editQuan');
            select.innerHTML = '<option value="">Chọn Quận/Huyện</option>';

            for (const [id, name] of Object.entries(districts)) {
                const selected = id === selectedDistrictId ? 'selected' : '';
                select.innerHTML += `<option value="${id}" ${selected}>${name}</option>`;
            }
        }
    } catch (error) {
        console.error('Error loading districts:', error);
    }
}

async function loadWards(districtId, targetSelectId) {
    if (!districtId) return;

    try {
        const response = await fetch('/DiaChi/GetWards', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(parseInt(districtId))
        });

        if (response.ok) {
            const wards = await response.json();
            const select = document.getElementById(targetSelectId);
            select.innerHTML = '<option value="">Chọn Phường/Xã</option>';

            for (const [id, name] of Object.entries(wards)) {
                select.innerHTML += `<option value="${id}">${name}</option>`;
            }
        }
    } catch (error) {
        console.error('Error loading wards:', error);
    }
}

async function loadWardsForEdit(districtId, selectedWardCode) {
    if (!districtId) return;

    try {
        const response = await fetch('/DiaChi/GetWards', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(parseInt(districtId))
        });

        if (response.ok) {
            const wards = await response.json();
            const select = document.getElementById('editPhuong');
            select.innerHTML = '<option value="">Chọn Phường/Xã</option>';

            for (const [id, name] of Object.entries(wards)) {
                const selected = id === selectedWardCode ? 'selected' : '';
                select.innerHTML += `<option value="${id}" ${selected}>${name}</option>`;
            }
        }
    } catch (error) {
        console.error('Error loading wards:', error);
    }
}

// Helper functions
function getAddFormData() {
    return {
        Tennguoinhan: document.getElementById('tennguoinhan').value.trim(),
        sdtnguoinhan: document.getElementById('sdtnguoinhan').value.trim(),
        Thanhpho: document.getElementById('selectTinh').value,
        Quanhuyen: document.getElementById('selectQuan').value,
        Phuongxa: document.getElementById('selectPhuong').value,
        Diachicuthe: document.getElementById('detailInput').value.trim()
    };
}

function getEditFormData() {
    return {
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
}

function validateFormData(data) {
    if (!data.Tennguoinhan) {
        showError('Tên người nhận không được để trống');
        return false;
    }

    if (!data.sdtnguoinhan) {
        showError('Số điện thoại không được để trống');
        return false;
    }

    const phoneRegex = /^[0-9]{10}$/;
    if (!phoneRegex.test(data.sdtnguoinhan)) {
        showError('Số điện thoại phải gồm đúng 10 số');
        return false;
    }

    if (!data.Thanhpho) {
        showError('Vui lòng chọn tỉnh/thành phố');
        return false;
    }

    if (!data.Quanhuyen) {
        showError('Vui lòng chọn quận/huyện');
        return false;
    }

    if (!data.Phuongxa) {
        showError('Vui lòng chọn phường/xã');
        return false;
    }

    if (!data.Diachicuthe) {
        showError('Địa chỉ cụ thể không được để trống');
        return false;
    }

    return true;
}

function resetAddForm() {
    document.getElementById('addAddressForm').reset();
    document.getElementById('selectQuan').innerHTML = '<option value="">Chọn Quận/Huyện</option>';
    document.getElementById('selectPhuong').innerHTML = '<option value="">Chọn Phường/Xã</option>';
}

// Notification functions - ĐÃ SỬA: Thay thế Swal.fire bằng các phương thức đơn giản
function showSuccess(message) {
    // Tạo và hiển thị toast notification
    showToast('success', 'Thành công!', message);
}

function showError(message) {
    // Tạo và hiển thị toast notification
    showToast('error', 'Lỗi!', message);
}

function showWarning(message) {
    // Tạo và hiển thị toast notification
    showToast('warning', 'Cảnh báo!', message);
}

// Toast notification function
function showToast(type, title, message) {
    // Tạo toast container nếu chưa có
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }

    // Tạo toast element
    const toastId = 'toast-' + Date.now();
    const bgColor = type === 'success' ? 'bg-success' :
        type === 'error' ? 'bg-danger' :
            'bg-warning';

    const icon = type === 'success' ? 'bi-check-circle' :
        type === 'error' ? 'bi-exclamation-circle' :
            'bi-exclamation-triangle';

    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white ${bgColor} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi ${icon} me-2"></i>
                    <strong>${title}</strong> ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);

    // Hiển thị toast
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, {
        autohide: true,
        delay: 3000
    });
    toast.show();

    // Xóa toast khỏi DOM sau khi ẩn
    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
}

// Hoặc sử dụng alert đơn giản (nếu không muốn dùng toast)
function showSuccess(message) {
    alert('✅ ' + message);
}

function showError(message) {
    alert('❌ ' + message);
}

function showWarning(message) {
    alert('⚠️ ' + message);
}

// Loading functions
function showLoading() {
    // Thêm loading indicator nếu cần
    // Ví dụ: hiển thị spinner
    const loadingElement = document.createElement('div');
    loadingElement.id = 'loading-overlay';
    loadingElement.className = 'loading-overlay';
    loadingElement.innerHTML = `
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
    `;
    document.body.appendChild(loadingElement);
}

function hideLoading() {
    // Ẩn loading indicator nếu có
    const loadingElement = document.getElementById('loading-overlay');
    if (loadingElement) {
        loadingElement.remove();
    }
}

// Thêm CSS cho loading overlay (có thể thêm vào file CSS)
const loadingStyles = `
.loading-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999;
}
`;

// Thêm styles vào head
const styleSheet = document.createElement('style');
styleSheet.textContent = loadingStyles;
document.head.appendChild(styleSheet);