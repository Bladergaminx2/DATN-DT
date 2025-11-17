// Biến toàn cục
let cartData = [];
let selectedItems = new Set();
let itemToDelete = null;

// Format tiền VND
function formatMoney(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Load giỏ hàng
async function loadCart() {
    try {
        showLoading();

        const response = await fetch('/GioHang/GetGioHangByKhachHang');
        const result = await response.json();

        if (result.success) {
            cartData = result.data.gioHangChiTiets || [];
            renderCartItems();
            updateTotal();
        } else {
            showAlert('error', result.message);
        }
    } catch (error) {
        console.error('Lỗi load giỏ hàng:', error);
        showAlert('error', 'Lỗi tải giỏ hàng');
    }
}

// Hiển thị loading
function showLoading() {
    const tbody = document.getElementById('cartItems');
    tbody.innerHTML = `
        <tr>
            <td colspan="7" class="text-center py-4">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-2 text-muted">Đang tải giỏ hàng...</p>
            </td>
        </tr>
    `;
}

// Render danh sách sản phẩm
function renderCartItems() {
    const tbody = document.getElementById('cartItems');

    if (cartData.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-4">
                    <i class="fas fa-shopping-cart fa-2x text-muted mb-3"></i>
                    <p class="text-muted">Giỏ hàng của bạn đang trống</p>
                    <a href="/SanPham" class="btn btn-primary">Mua sắm ngay</a>
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = cartData.map(item => {
        const anhDaiDien = item.modelSanPham.anhSanPhams && item.modelSanPham.anhSanPhams.length > 0
            ? item.modelSanPham.anhSanPhams[0].duongDan
            : '/images/default-product.jpg';

        return `
            <tr data-item-id="${item.idGioHangChiTiet}">
                <td class="text-center">
                    <input type="checkbox" class="item-checkbox" 
                           value="${item.idGioHangChiTiet}"
                           ${selectedItems.has(item.idGioHangChiTiet) ? 'checked' : ''}>
                </td>
                <td>
                    <img src="${anhDaiDien}" 
                         alt="${item.modelSanPham.sanPham.tenSanPham}" 
                         class="img-thumbnail" style="width: 60px; height: 60px; object-fit: cover;">
                </td>
                <td>
                    <h6 class="mb-1">${item.modelSanPham.sanPham.tenSanPham}</h6>
                    <small class="text-muted">
                        ${item.modelSanPham.tenModel} - ${item.modelSanPham.mau}<br>
                        ${item.modelSanPham.ram.dungLuongRAM} - ${item.modelSanPham.rom.dungLuongROM}<br>
                        Thương hiệu: ${item.modelSanPham.sanPham.thuongHieu.tenThuongHieu}
                    </small>
                </td>
                <td class="text-center">
                    <strong>${formatMoney(item.modelSanPham.giaBanModel)}</strong>
                </td>
                <td class="text-center">
                    <div class="input-group input-group-sm" style="width: 120px;">
                        <button class="btn btn-outline-secondary btn-minus" type="button">
                            <i class="fas fa-minus"></i>
                        </button>
                        <input type="number" class="form-control text-center quantity-input" 
                               value="${item.soLuong}" min="1" max="99"
                               data-price="${item.modelSanPham.giaBanModel}"
                               data-item-id="${item.idGioHangChiTiet}">
                        <button class="btn btn-outline-secondary btn-plus" type="button">
                            <i class="fas fa-plus"></i>
                        </button>
                    </div>
                </td>
                <td class="text-center">
                    <strong class="item-total">${formatMoney(item.thanhTien)}</strong>
                </td>
                <td class="text-center">
                    <button class="btn btn-outline-danger btn-sm btn-delete" 
                            data-item-id="${item.idGioHangChiTiet}">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    }).join('');

    attachEventListeners();
}

// Các hàm xử lý sự kiện khác (giữ nguyên từ code trước)
// handleCheckboxChange, handleSelectAll, handleQuantityChange, etc.

// Khởi tạo
document.addEventListener('DOMContentLoaded', function () {
    loadCart();
});