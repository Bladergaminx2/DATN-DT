// ====== GLOBAL VARIABLES ======
let currentKhachHangId = null;
let currentAddressId = null;

// ====== INITIALIZATION ======
document.addEventListener('DOMContentLoaded', function() {
    const btnDoiDiaChi = document.getElementById('btnDoiDiaChi');
    if (btnDoiDiaChi) {
        btnDoiDiaChi.addEventListener('click', function() {
            openAddressModal();
        });
    }

    // Load initial profile data
    loadProfileDataCheckout();
});

// ====== LOAD PROFILE DATA ======
async function loadProfileDataCheckout() {
    try {
        const response = await fetch('/UserProfile/GetProfileData');
        if (!response.ok) {
            throw new Error('HTTP error! status: ' + response.status);
        }
        
        const responseText = await response.text();
        if (!responseText) {
            throw new Error('Empty response from server');
        }
        
        const data = JSON.parse(responseText);

        if (data) {
            if (data.idKhachHang) {
                currentKhachHangId = data.idKhachHang;
            }

            // Update email field if exists
            const emailEl = document.getElementById('email');
            if (emailEl && data.emailKhachHang) {
                emailEl.value = data.emailKhachHang;
            }

            // Set current address ID if exists
            const diaChiIdEl = document.getElementById('diaChiId');
            if (diaChiIdEl && diaChiIdEl.value) {
                currentAddressId = parseInt(diaChiIdEl.value);
            }
        }
    } catch (error) {
        console.error('Error loading profile data:', error);
    }
}

// ====== OPEN ADDRESS MODAL ======
async function openAddressModal() {
    if (!currentKhachHangId) {
        await loadProfileDataCheckout();
        if (!currentKhachHangId) {
            alert('Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.');
            return;
        }
    }

    const modalEl = document.getElementById('diaChiModal');
    if (!modalEl) return;

    // Load user info
    await loadUserInfo();

    // Load addresses
    await loadAddressListCheckout();

    const modal = new bootstrap.Modal(modalEl);
    modal.show();
}

// ====== LOAD USER INFO ======
async function loadUserInfo() {
    try {
        const response = await fetch('/UserProfile/GetProfileData');
        if (response.ok) {
            const data = await response.json();
            
            const fullNameEl = document.getElementById('userFullName');
            const emailEl = document.getElementById('userEmail');
            const phoneEl = document.getElementById('userPhone');

            if (fullNameEl) fullNameEl.textContent = data.hoTenKhachHang || 'Chưa cập nhật';
            if (emailEl) emailEl.textContent = data.emailKhachHang || 'Chưa cập nhật';
            if (phoneEl) phoneEl.textContent = data.sdtKhachHang || 'Chưa cập nhật';
        }
    } catch (error) {
        console.error('Error loading user info:', error);
    }
}

// ====== LOAD ADDRESS LIST ======
async function loadAddressListCheckout() {
    if (!currentKhachHangId) {
        const container = document.getElementById('diaChiList');
        if (container) {
            container.innerHTML = '<div class="col-12"><p class="text-danger">Không tìm thấy thông tin khách hàng</p></div>';
        }
        return;
    }

    try {
        const response = await fetch('/DiaChi/GetByKhachHang?idKhachHang=' + currentKhachHangId);
        if (response.ok) {
            const addresses = await response.json();
            renderAddressListCheckout(addresses);
        } else {
            const container = document.getElementById('diaChiList');
            if (container) {
                container.innerHTML = '<div class="col-12"><p class="text-danger">Lỗi khi tải danh sách địa chỉ</p></div>';
            }
        }
    } catch (error) {
        console.error('Error loading addresses:', error);
        const container = document.getElementById('diaChiList');
        if (container) {
            container.innerHTML = '<div class="col-12"><p class="text-danger">Lỗi kết nối</p></div>';
        }
    }
}

// ====== RENDER ADDRESS LIST ======
function renderAddressListCheckout(addresses) {
    const container = document.getElementById('diaChiList');
    if (!container) return;

    if (!addresses || addresses.length === 0) {
        container.innerHTML = '<div class="col-12"><p class="text-muted text-center"><i class="bi bi-info-circle me-2"></i>Chưa có địa chỉ nào. Hãy thêm địa chỉ mới!</p></div>';
        return;
    }

    let html = '';
    for (let i = 0; i < addresses.length; i++) {
        const address = addresses[i];
        const isDefault = address.trangthai === 0;
        const isSelected = currentAddressId === address.id;
        const defaultBadge = isDefault ? '<span class="badge bg-warning text-dark">Mặc định</span>' : '';
        const selectedClass = isSelected ? 'border-primary border-2' : '';
        
        html += '<div class="col-md-6 mb-3">' +
            '<div class="card h-100 ' + selectedClass + '">' +
            '<div class="card-body">' +
            '<div class="d-flex justify-content-between align-items-start mb-2">' +
            '<h6 class="card-title mb-0"><i class="bi bi-house-door me-2"></i>' + (address.tennguoinhan || '') + '</h6>' +
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
            '<div class="d-flex gap-2 flex-wrap">' +
            '<button class="btn btn-primary btn-sm" onclick="selectAddressCheckout(' + address.id + ', ' + 
                JSON.stringify(address.tennguoinhan || '') + ', ' + 
                JSON.stringify(address.sdtnguoinhan || '') + ', ' + 
                JSON.stringify(address.diachicuthe || '') + ', ' + 
                JSON.stringify(address.wardName || '') + ', ' + 
                JSON.stringify(address.districtName || '') + ', ' + 
                JSON.stringify(address.provinceName || '') + ')">' +
            '<i class="bi bi-check-circle me-1"></i>Chọn địa chỉ này' +
            '</button>' +
            '<button class="btn btn-outline-secondary btn-sm" onclick="openEditAddressModalCheckout(' + address.id + ')">' +
            '<i class="bi bi-pencil me-1"></i>Sửa' +
            '</button>' +
            (isDefault ? '' : '<button class="btn btn-outline-danger btn-sm" onclick="deleteAddressCheckout(' + address.id + ')">' +
            '<i class="bi bi-trash me-1"></i>Xóa' +
            '</button>') +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div>';
    }
    
    container.innerHTML = html;
}

// ====== SELECT ADDRESS ======
function selectAddressCheckout(id, tennguoinhan, sdt, diachicuthe, wardName, districtName, provinceName) {
    currentAddressId = id;
    
    // Update hidden field
    const diaChiIdEl = document.getElementById('diaChiId');
    if (diaChiIdEl) {
        diaChiIdEl.value = id;
    }

    // Update display
    const addressDisplayEl = document.getElementById('addressDisplay');
    if (addressDisplayEl) {
        addressDisplayEl.innerHTML = '<div class="p-3 border rounded" style="background-color: #2b2b33; border-color: #444 !important;">' +
            '<div class="fw-bold text-white">' + tennguoinhan + ' - ' + sdt + '</div>' +
            '<div class="text-white">' + diachicuthe + ', ' + wardName + ', ' + districtName + ', ' + provinceName + '</div>' +
            '<small class="text-muted">Địa chỉ giao hàng</small>' +
            '</div>' +
            '<input type="hidden" id="diaChiId" value="' + id + '">';
    }

    // Enable checkout button
    const btnXacNhan = document.getElementById('btnXacNhanThanhToan');
    if (btnXacNhan) {
        btnXacNhan.disabled = false;
        btnXacNhan.classList.remove('disabled');
    }

    // Close modal
    const modalEl = document.getElementById('diaChiModal');
    if (modalEl) {
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    }

    // Show success message
    showToast('Đã chọn địa chỉ giao hàng', 'success');
}

// ====== OPEN ADD ADDRESS MODAL ======
function openAddAddressModalCheckout() {
    const modalEl = document.getElementById('addAddressModalCheckout');
    if (modalEl) {
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
        loadProvincesCheckout('addTinh');
    }
}

// ====== LOAD PROVINCES ======
async function loadProvincesCheckout(selectId) {
    try {
        const response = await fetch('/DiaChi/GetProvinces');
        if (response.ok) {
            const provinces = await response.json();
            const select = document.getElementById(selectId);
            if (select) {
                select.innerHTML = '<option value="">Chọn Tỉnh/Thành</option>';
                for (const id in provinces) {
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

// ====== LOAD DISTRICTS ======
async function loadDistrictsCheckout(provinceId, selectId) {
    if (!provinceId) return;

    try {
        const response = await fetch('/DiaChi/GetDistricts', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(provinceId))
        });

        if (response.ok) {
            const districts = await response.json();
            const select = document.getElementById(selectId);
            if (select) {
                select.innerHTML = '<option value="">Chọn Quận/Huyện</option>';
                for (const id in districts) {
                    if (districts.hasOwnProperty(id)) {
                        select.innerHTML += '<option value="' + id + '">' + districts[id] + '</option>';
                    }
                }
                
                // Reset ward select
                const wardSelectId = selectId === 'addQuan' ? 'addPhuong' : 'editPhuongCheckout';
                const wardSelect = document.getElementById(wardSelectId);
                if (wardSelect) {
                    wardSelect.innerHTML = '<option value="">Chọn Phường/Xã</option>';
                }
            }
        }
    } catch (error) {
        console.error('Error loading districts:', error);
    }
}

// ====== LOAD WARDS ======
async function loadWardsCheckout(districtId, selectId) {
    if (!districtId) return;

    try {
        const response = await fetch('/DiaChi/GetWards', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(districtId))
        });

        if (response.ok) {
            const wards = await response.json();
            const select = document.getElementById(selectId);
            if (select) {
                select.innerHTML = '<option value="">Chọn Phường/Xã</option>';
                for (const id in wards) {
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

// ====== EVENT LISTENERS FOR SELECTS ======
document.addEventListener('DOMContentLoaded', function() {
    // Add address form
    const addTinh = document.getElementById('addTinh');
    if (addTinh) {
        addTinh.addEventListener('change', function() {
            loadDistrictsCheckout(this.value, 'addQuan');
        });
    }

    const addQuan = document.getElementById('addQuan');
    if (addQuan) {
        addQuan.addEventListener('change', function() {
            loadWardsCheckout(this.value, 'addPhuong');
        });
    }

    // Edit address form
    const editTinhCheckout = document.getElementById('editTinhCheckout');
    if (editTinhCheckout) {
        editTinhCheckout.addEventListener('change', function() {
            loadDistrictsCheckout(this.value, 'editQuanCheckout');
        });
    }

    const editQuanCheckout = document.getElementById('editQuanCheckout');
    if (editQuanCheckout) {
        editQuanCheckout.addEventListener('change', function() {
            loadWardsCheckout(this.value, 'editPhuongCheckout');
        });
    }
});

// ====== ADD NEW ADDRESS ======
async function addNewAddressCheckout() {
    if (!currentKhachHangId) {
        alert('Không tìm thấy thông tin khách hàng');
        return;
    }

    const formData = {
        Tennguoinhan: document.getElementById('addTenNguoiNhan').value.trim(),
        sdtnguoinhan: document.getElementById('addSdtNguoiNhan').value.trim(),
        Thanhpho: document.getElementById('addTinh').value,
        Quanhuyen: document.getElementById('addQuan').value,
        Phuongxa: document.getElementById('addPhuong').value,
        Diachicuthe: document.getElementById('addDiaChiCuThe').value.trim()
    };

    // Validation
    if (!formData.Tennguoinhan) {
        alert('Tên người nhận không được để trống');
        return;
    }

    if (!formData.sdtnguoinhan) {
        alert('Số điện thoại không được để trống');
        return;
    }

    const phoneRegex = /^[0-9]{10}$/;
    if (!phoneRegex.test(formData.sdtnguoinhan)) {
        alert('Số điện thoại phải gồm đúng 10 số');
        return;
    }

    if (!formData.Thanhpho || !formData.Quanhuyen || !formData.Phuongxa) {
        alert('Vui lòng chọn đầy đủ tỉnh/thành, quận/huyện, phường/xã');
        return;
    }

    if (!formData.Diachicuthe) {
        alert('Địa chỉ cụ thể không được để trống');
        return;
    }

    try {
        const response = await fetch('/DiaChi/Create', {
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
            showToast('Thêm địa chỉ thành công!', 'success');
            const modalEl = document.getElementById('addAddressModalCheckout');
            if (modalEl) {
                const modal = bootstrap.Modal.getInstance(modalEl);
                if (modal) modal.hide();
            }
            
            // Reset form
            document.getElementById('addAddressFormCheckout').reset();
            document.getElementById('addQuan').innerHTML = '<option value="">Chọn Quận/Huyện</option>';
            document.getElementById('addPhuong').innerHTML = '<option value="">Chọn Phường/Xã</option>';
            
            // Reload address list
            await loadAddressListCheckout();
        } else {
            const errorText = await response.text();
            alert(errorText || 'Lỗi khi thêm địa chỉ');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Lỗi kết nối');
    }
}

// ====== OPEN EDIT ADDRESS MODAL ======
async function openEditAddressModalCheckout(id) {
    if (!currentKhachHangId) return;

    try {
        const response = await fetch('/DiaChi/GetByKhachHang?idKhachHang=' + currentKhachHangId);
        if (response.ok) {
            const addresses = await response.json();
            const address = addresses.find(a => a.id === id);

            if (address) {
                document.getElementById('editIdCheckout').value = address.id;
                document.getElementById('editTrangThaiCheckout').value = address.trangthai;
                document.getElementById('editTenNguoiNhanCheckout').value = address.tennguoinhan || '';
                document.getElementById('editSdtNguoiNhanCheckout').value = address.sdtnguoinhan || '';
                document.getElementById('editDiaChiCuTheCheckout').value = address.diachicuthe || '';

                await loadProvincesForEditCheckout(address.thanhpho);
                await loadDistrictsForEditCheckout(address.thanhpho, address.quanhuyen);
                await loadWardsForEditCheckout(address.quanhuyen, address.phuongxa);

                const modalEl = document.getElementById('editAddressModalCheckout');
                if (modalEl) {
                    const modal = new bootstrap.Modal(modalEl);
                    modal.show();
                }
            }
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Lỗi khi tải thông tin địa chỉ');
    }
}

// ====== UPDATE ADDRESS ======
async function updateAddressCheckout() {
    if (!currentKhachHangId) return;

    const formData = {
        Id: parseInt(document.getElementById('editIdCheckout').value),
        Tennguoinhan: document.getElementById('editTenNguoiNhanCheckout').value.trim(),
        sdtnguoinhan: document.getElementById('editSdtNguoiNhanCheckout').value.trim(),
        Thanhpho: document.getElementById('editTinhCheckout').value,
        Quanhuyen: document.getElementById('editQuanCheckout').value,
        Phuongxa: document.getElementById('editPhuongCheckout').value,
        Diachicuthe: document.getElementById('editDiaChiCuTheCheckout').value.trim(),
        trangthai: parseInt(document.getElementById('editTrangThaiCheckout').value),
        IdKhachHang: currentKhachHangId
    };

    // Validation
    if (!formData.Tennguoinhan || !formData.sdtnguoinhan || !formData.Thanhpho || 
        !formData.Quanhuyen || !formData.Phuongxa || !formData.Diachicuthe) {
        alert('Vui lòng điền đầy đủ thông tin');
        return;
    }

    try {
        const response = await fetch('/DiaChi/Update?id=' + formData.Id, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(formData)
        });

        if (response.ok) {
            showToast('Cập nhật địa chỉ thành công!', 'success');
            const modalEl = document.getElementById('editAddressModalCheckout');
            if (modalEl) {
                const modal = bootstrap.Modal.getInstance(modalEl);
                if (modal) modal.hide();
            }
            await loadAddressListCheckout();
        } else {
            const errorText = await response.text();
            alert(errorText || 'Lỗi khi cập nhật địa chỉ');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Lỗi kết nối');
    }
}

// ====== DELETE ADDRESS ======
async function deleteAddressCheckout(id) {
    if (!confirm('Bạn có chắc muốn xóa địa chỉ này?')) {
        return;
    }

    try {
        const response = await fetch('/DiaChi/Delete?id=' + id, {
            method: 'DELETE'
        });

        if (response.ok) {
            showToast('Xóa địa chỉ thành công!', 'success');
            await loadAddressListCheckout();
        } else {
            const errorText = await response.text();
            alert(errorText || 'Lỗi khi xóa địa chỉ');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Lỗi kết nối');
    }
}

// ====== LOAD PROVINCES FOR EDIT ======
async function loadProvincesForEditCheckout(selectedProvinceId) {
    try {
        const response = await fetch('/DiaChi/GetProvinces');
        if (response.ok) {
            const provinces = await response.json();
            const select = document.getElementById('editTinhCheckout');
            if (select) {
                select.innerHTML = '<option value="">Chọn Tỉnh/Thành</option>';
                for (const id in provinces) {
                    if (provinces.hasOwnProperty(id)) {
                        const selected = id === selectedProvinceId ? 'selected' : '';
                        select.innerHTML += '<option value="' + id + '" ' + selected + '>' + provinces[id] + '</option>';
                    }
                }
            }
        }
    } catch (error) {
        console.error('Error loading provinces:', error);
    }
}

// ====== LOAD DISTRICTS FOR EDIT ======
async function loadDistrictsForEditCheckout(provinceId, selectedDistrictId) {
    if (!provinceId) return;

    try {
        const response = await fetch('/DiaChi/GetDistricts', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(provinceId))
        });

        if (response.ok) {
            const districts = await response.json();
            const select = document.getElementById('editQuanCheckout');
            if (select) {
                select.innerHTML = '<option value="">Chọn Quận/Huyện</option>';
                for (const id in districts) {
                    if (districts.hasOwnProperty(id)) {
                        const selected = id === selectedDistrictId ? 'selected' : '';
                        select.innerHTML += '<option value="' + id + '" ' + selected + '>' + districts[id] + '</option>';
                    }
                }
            }
        }
    } catch (error) {
        console.error('Error loading districts:', error);
    }
}

// ====== LOAD WARDS FOR EDIT ======
async function loadWardsForEditCheckout(districtId, selectedWardCode) {
    if (!districtId) return;

    try {
        const response = await fetch('/DiaChi/GetWards', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(districtId))
        });

        if (response.ok) {
            const wards = await response.json();
            const select = document.getElementById('editPhuongCheckout');
            if (select) {
                select.innerHTML = '<option value="">Chọn Phường/Xã</option>';
                for (const id in wards) {
                    if (wards.hasOwnProperty(id)) {
                        const selected = id === selectedWardCode ? 'selected' : '';
                        select.innerHTML += '<option value="' + id + '" ' + selected + '>' + wards[id] + '</option>';
                    }
                }
            }
        }
    } catch (error) {
        console.error('Error loading wards:', error);
    }
}

// ====== UTILITY FUNCTIONS ======
function escapeHtml(text) {
    if (!text) return '';
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}

function showToast(message, type) {
    // Try to use Bootstrap toast if available, otherwise use alert
    const toastContainer = document.querySelector('.toast-container');
    if (toastContainer) {
        const toastEl = document.createElement('div');
        toastEl.className = 'toast align-items-center text-white bg-' + (type === 'success' ? 'success' : 'danger') + ' border-0';
        toastEl.setAttribute('role', 'alert');
        toastEl.innerHTML = '<div class="d-flex"><div class="toast-body">' + message + '</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>';
        toastContainer.appendChild(toastEl);
        const toast = new bootstrap.Toast(toastEl);
        toast.show();
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
    } else {
        alert(message);
    }
}

