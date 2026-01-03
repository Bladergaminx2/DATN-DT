// ====== UTILITY FUNCTIONS ======
function showLoading() {
    // Simple loading indicator
    console.log('Loading...');
}

function hideLoading() {
    console.log('Loaded');
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

function showPasswordAlert(type, message) {
    if (type === 'success') {
        var successMsgEl = document.getElementById('passwordSuccessMessage');
        var successAlertEl = document.getElementById('passwordSuccessAlert');
        if (successMsgEl) successMsgEl.textContent = message;
        if (successAlertEl) successAlertEl.style.display = 'block';
        setTimeout(function() {
            hideAlert('passwordSuccessAlert');
        }, 5000);
    } else {
        var errorMsgEl = document.getElementById('passwordErrorMessage');
        var errorAlertEl = document.getElementById('passwordErrorAlert');
        if (errorMsgEl) errorMsgEl.textContent = message;
        if (errorAlertEl) errorAlertEl.style.display = 'block';
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

// ====== PROFILE FUNCTIONS ======
async function loadProfileData() {
    try {
        const response = await fetch('/AdminProfile/GetProfileData', {
            credentials: 'include'
        });
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
            var hoTenEl = document.getElementById('HoTenNhanVien');
            if (hoTenEl) hoTenEl.value = data.hoTenNhanVien || '';
            
            var sdtEl = document.getElementById('SdtNhanVien');
            if (sdtEl) sdtEl.value = data.sdtNhanVien || '';
            
            var emailEl = document.getElementById('EmailNhanVien');
            if (emailEl) emailEl.value = data.emailNhanVien || '';
            
            var diaChiEl = document.getElementById('DiaChiNV');
            if (diaChiEl) diaChiEl.value = data.diaChiNV || '';

            var tenTaiKhoanEl = document.getElementById('TenTaiKhoanNV');
            if (tenTaiKhoanEl) tenTaiKhoanEl.value = data.tenTaiKhoanNV || '';

            var ngayVaoLamEl = document.getElementById('NgayVaoLam');
            if (ngayVaoLamEl && data.ngayVaoLam) {
                var date = new Date(data.ngayVaoLam);
                ngayVaoLamEl.value = date.toLocaleDateString('vi-VN');
            }
            
            var profileNameEl = document.getElementById('profileName');
            if (profileNameEl) profileNameEl.textContent = data.hoTenNhanVien || 'Admin';
            
            var profileRoleEl = document.getElementById('profileRole');
            if (profileRoleEl) profileRoleEl.textContent = data.tenChucVu || 'Admin';
        }
    } catch (error) {
        console.error('Error loading profile:', error);
        showAlert('error', 'Lỗi! Không thể lấy thông tin tài khoản: ' + (error.message || 'Unknown error'));
    }
}

async function saveProfile() {
    hideError('errorHoTenNhanVien');
    hideError('errorSdtNhanVien');
    hideError('errorEmailNhanVien');

    var hoTenEl = document.getElementById('HoTenNhanVien');
    var sdtEl = document.getElementById('SdtNhanVien');
    var emailEl = document.getElementById('EmailNhanVien');
    var diaChiEl = document.getElementById('DiaChiNV');

    if (!hoTenEl || !sdtEl) {
        showAlert('error', 'Không tìm thấy các trường nhập liệu');
        return;
    }

    const profileData = {
        HoTenNhanVien: hoTenEl.value.trim(),
        SdtNhanVien: sdtEl.value.trim(),
        EmailNhanVien: emailEl ? emailEl.value.trim() || null : null,
        DiaChiNV: diaChiEl ? diaChiEl.value.trim() || null : null
    };

    if (!profileData.HoTenNhanVien) {
        showError('errorHoTenNhanVien', 'Họ và tên là bắt buộc');
        return;
    }

    if (!profileData.SdtNhanVien) {
        showError('errorSdtNhanVien', 'Số điện thoại là bắt buộc');
        return;
    }

    showLoading();
    try {
        const response = await fetch('/AdminProfile/UpdateProfileData', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(profileData),
            credentials: 'include'
        });

        const data = await response.json();

        if (response.ok) {
            showAlert('success', data.message || 'Cập nhật thông tin thành công!');
            var profileNameEl = document.getElementById('profileName');
            if (profileNameEl) {
                profileNameEl.textContent = profileData.HoTenNhanVien;
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
        showPasswordAlert('error', 'Không tìm thấy các trường nhập liệu');
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
        const response = await fetch('/AdminProfile/ChangePassword', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(passwordData),
            credentials: 'include'
        });

        const data = await response.json();

        if (response.ok) {
            showPasswordAlert('success', data.message || 'Đổi mật khẩu thành công!');
            if (currentPasswordEl) currentPasswordEl.value = '';
            if (newPasswordEl) newPasswordEl.value = '';
            if (confirmPasswordEl) confirmPasswordEl.value = '';
        } else {
            showPasswordAlert('error', data.message || 'Đổi mật khẩu thất bại!');
        }
    } catch (error) {
        showPasswordAlert('error', 'Lỗi kết nối đến server');
    } finally {
        hideLoading();
    }
}

function cancelChanges() {
    if (confirm('Bạn có chắc chắn muốn hủy các thay đổi?')) {
        loadProfileData();
    }
}

// ====== INITIALIZATION ======
document.addEventListener('DOMContentLoaded', function () {
    loadProfileData();
});

