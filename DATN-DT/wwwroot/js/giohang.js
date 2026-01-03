// Biến toàn cục
let cartData = [];
let selectedItems = new Set(); // Lưu trữ IdGioHangChiTiet
let itemToDelete = null;

// Format tiền VND
function formatMoney(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Hiển thị thông báo
function showAlert(type, message) {
    if (typeof toastr !== 'undefined') {
        toastr[type](message);
    } else {
        alert(message);
    }
}

// Đồng bộ localStorage lên database
async function syncLocalStorageToDatabase() {
    try {
        const localCart = JSON.parse(localStorage.getItem('cart')) || [];
        if (localCart.length === 0) return;

        // Đồng bộ từng sản phẩm trong localStorage lên database
        for (const item of localCart) {
            try {
                const response = await fetch('/GioHang/AddToCart', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        IdModelSanPham: item.id,
                        Quantity: item.quantity
                    })
                });

                const result = await response.json();
                if (result.Success) {
                    console.log(`Đã đồng bộ sản phẩm ${item.id} lên database`);
                }
            } catch (error) {
                console.error(`Lỗi đồng bộ sản phẩm ${item.id}:`, error);
            }
        }

        // Xóa localStorage sau khi đồng bộ thành công
        localStorage.removeItem('cart');
    } catch (error) {
        console.error('Lỗi đồng bộ localStorage:', error);
    }
}

// Load giỏ hàng
async function loadCart() {
    try {
        showLoading();

        // Đồng bộ localStorage lên database trước khi load
        await syncLocalStorageToDatabase();

        const response = await fetch('/GioHang/GetGioHangByKhachHang');
        const result = await response.json();

        if (result.Success || result.success) {
            cartData = (result.data || result.Data || {}).gioHangChiTiets || [];
            renderCartItems();
            updateTotal();
        } else {
            showAlert('error', result.Message || result.message || 'Không thể tải giỏ hàng');
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
                    <i class="bi bi-cart-x" style="font-size: 3rem; color: #6c757d;"></i>
                    <p class="text-muted mt-2">Giỏ hàng của bạn đang trống</p>
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

        // Xử lý dữ liệu có thể null
        const ramText = item.modelSanPham.ram?.dungLuongRAM || 'N/A';
        const romText = item.modelSanPham.rom?.dungLuongROM || 'N/A';
        const thuongHieuText = item.modelSanPham.sanPham?.thuongHieu?.tenThuongHieu || 'N/A';
        const tenSanPham = item.modelSanPham.sanPham?.tenSanPham || 'N/A';

        // Lấy số lượng tồn kho và trạng thái
        const soLuongTon = item.soLuongTon || 0;
        const trangThai = item.modelSanPham.trangThai;
        const isActive = trangThai === 1; // Đang kinh doanh
        const isOutOfStock = trangThai === 2; // Tạm hết hàng
        const isInactive = trangThai === 0; // Ngừng kinh doanh

        // Xác định số lượng tối đa và trạng thái
        let maxQuantity = 0;
        let currentQuantity = 0;
        let isDisabled = true;
        let statusText = '';
        let statusClass = '';
        let rowClass = '';

        if (isActive) {
            // Đang kinh doanh - kiểm tra tồn kho
            maxQuantity = Math.min(soLuongTon, 99);
            // Sửa: Cập nhật số lượng trong cartData nếu vượt quá tồn kho
            if (item.soLuong > maxQuantity) {
                item.soLuong = maxQuantity;
                item.thanhTien = maxQuantity * item.modelSanPham.giaBanModel;
                // Đánh dấu cần cập nhật database
                item._needsUpdate = true;
                item._newQuantity = maxQuantity;
            }
            currentQuantity = item.soLuong;
            isDisabled = soLuongTon === 0;

            if (soLuongTon > 0) {
                statusText = `Còn ${soLuongTon} sản phẩm`;
            } else {
                statusText = `Hết hàng`;
                rowClass = 'table-warning';
            }
        } else if (isOutOfStock) {
            // Tạm hết hàng
            currentQuantity = 0;
            isDisabled = true;
            statusText = `Tạm hết hàng`;
            rowClass = 'table-warning';
        } else if (isInactive) {
            // Ngừng kinh doanh
            currentQuantity = 0;
            isDisabled = true;
            statusText = `Ngừng kinh doanh`;
            rowClass = 'table-secondary';
        }

        return `
            <tr data-item-id="${item.idGioHangChiTiet}" class="${rowClass}">
                <td class="text-center">
                    <input type="checkbox" class="item-checkbox" 
                           value="${item.idGioHangChiTiet}"
                           ${selectedItems.has(item.idGioHangChiTiet) ? 'checked' : ''}
                           ${isDisabled ? 'disabled' : ''}>
                </td>
                <td>
                    <img src="${anhDaiDien}" 
                         alt="${tenSanPham}" 
                         class="img-thumbnail"
                         style="width: 180px !important; height: 180px !important; object-fit: cover !important; ${!isActive ? 'opacity: 0.5;' : ''}"
                         onerror="this.src='/images/default-product.jpg'">
                </td>
                <td>
                    <div class="product-info">
                        <span class="product-name ${!isActive ? 'text-muted' : ''}">${tenSanPham}</span>
                        <small class="text-muted">
                            <strong>Model:</strong> ${item.modelSanPham.tenModel} - ${item.modelSanPham.mau}<br>
                            <strong>RAM/ROM:</strong> ${ramText} - ${romText}<br>
                            <span class="product-brand">Thương hiệu: ${thuongHieuText}</span>
                        </small>
                        ${statusText ? `<div class="product-status ${isActive && soLuongTon > 0 ? 'in-stock' : 'out-of-stock'}">${statusText}</div>` : ''}
                    </div>
                </td>
                <td class="text-center">
                    <strong class="price-display unit-price ${!isActive ? 'text-muted' : ''}">${formatMoney(item.modelSanPham.giaBanModel)}</strong>
                </td>
                <td class="text-center">
                    ${isActive ? `
                        <div class="quantity-control d-flex align-items-center justify-content-center">
                            <button class="btn btn-secondary btn-sm btn-minus" type="button" 
                                    style="width: 35px; height: 35px; font-weight: bold;"
                                    ${currentQuantity <= 1 ? 'disabled' : ''}>
                                −
                            </button>
                            <input type="number" class="form-control text-center quantity-input mx-2" 
                                   value="${currentQuantity}" 
                                   min="1" 
                                   max="${maxQuantity}" 
                                   style="width: 60px;"
                                   data-price="${item.modelSanPham.giaBanModel}"
                                   data-item-id="${item.idGioHangChiTiet}"
                                   data-max-quantity="${maxQuantity}"
                                   ${isDisabled ? 'disabled' : ''}>
                            <button class="btn btn-secondary btn-sm btn-plus" type="button" 
                                    style="width: 35px; height: 35px; font-weight: bold;"
                                    ${currentQuantity >= maxQuantity ? 'disabled' : ''}>
                                +
                            </button>
                        </div>
                        ${soLuongTon > 0 && currentQuantity >= maxQuantity ?
                            `<small class="text-warning d-block mt-1">Đã đạt số lượng tối đa</small>` : ''
                        }
                    ` : `
                        <span class="text-muted">Không khả dụng</span>
                    `}
                </td>
                <td class="text-center">
                    <strong class="price-display total-price item-total ${!isActive ? 'text-muted' : ''}">
                        ${formatMoney(currentQuantity * item.modelSanPham.giaBanModel)}
                    </strong>
                </td>
                <td class="text-center">
                    <button class="btn btn-danger btn-sm btn-delete btn-action" 
                            data-item-id="${item.idGioHangChiTiet}"
                            title="Xóa sản phẩm">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    }).join('');

    attachEventListeners();
    
    // Tự động cập nhật số lượng trong database nếu có item vượt quá tồn kho
    cartData.forEach(async (item) => {
        if (item._needsUpdate && item._newQuantity !== undefined) {
            try {
                await fetch('/GioHang/UpdateQuantity', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        cartItemId: item.idGioHangChiTiet,
                        quantity: item._newQuantity
                    })
                });
                // Xóa flag sau khi cập nhật
                delete item._needsUpdate;
                delete item._newQuantity;
            } catch (error) {
                console.error(`Lỗi cập nhật số lượng cho item ${item.idGioHangChiTiet}:`, error);
            }
        }
    });
}

// Gắn sự kiện
function attachEventListeners() {
    // Checkbox chọn sản phẩm - sử dụng IdGioHangChiTiet
    document.querySelectorAll('.item-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', handleCheckboxChange);
    });

    // Chọn tất cả - CHỈ chọn sản phẩm có trạng thái = 1 (Đang kinh doanh)
    const selectAll = document.getElementById('selectAll');
    if (selectAll) {
        selectAll.addEventListener('change', handleSelectAll);
    }

    // Tăng/giảm số lượng
    document.querySelectorAll('.btn-plus').forEach(btn => {
        btn.addEventListener('click', handleQuantityChange);
    });
    document.querySelectorAll('.btn-minus').forEach(btn => {
        btn.addEventListener('click', handleQuantityChange);
    });
    document.querySelectorAll('.quantity-input').forEach(input => {
        input.addEventListener('change', handleQuantityInputChange);
        input.addEventListener('blur', handleQuantityInputChange);
    });

    // Xóa sản phẩm
    document.querySelectorAll('.btn-delete').forEach(btn => {
        btn.addEventListener('click', handleDeleteClick);
    });

    // Xử lý thanh toán
    const btnCheckout = document.getElementById('btnCheckout');
    if (btnCheckout) {
        btnCheckout.addEventListener('click', handleCheckout);
    }
}

// Xử lý checkbox - sử dụng IdGioHangChiTiet
function handleCheckboxChange(e) {
    const gioHangChiTietId = parseInt(e.target.value);

    if (e.target.checked) {
        selectedItems.add(gioHangChiTietId);
    } else {
        selectedItems.delete(gioHangChiTietId);
    }

    updateSelectAllState();
    updateTotal();
}

// Chọn tất cả - CHỈ chọn sản phẩm có trạng thái = 1 (Đang kinh doanh)
function handleSelectAll(e) {
    const isChecked = e.target.checked;

    document.querySelectorAll('.item-checkbox').forEach(checkbox => {
        // CHỈ chọn những checkbox không bị disabled VÀ sản phẩm có trạng thái = 1
        if (!checkbox.disabled) {
            const itemId = parseInt(checkbox.value);
            const item = cartData.find(x => x.idGioHangChiTiet === itemId);

            // Chỉ chọn sản phẩm đang kinh doanh (trạng thái = 1)
            if (item && item.modelSanPham.trangThai === 1) {
                checkbox.checked = isChecked;

                if (isChecked) {
                    selectedItems.add(itemId);
                } else {
                    selectedItems.delete(itemId);
                }
            }
        }
    });

    updateTotal();
}

// Cập nhật trạng thái chọn tất cả
function updateSelectAllState() {
    // CHỈ đếm những checkbox không bị disabled VÀ sản phẩm có trạng thái = 1
    const activeCheckboxes = Array.from(document.querySelectorAll('.item-checkbox:not(:disabled)'))
        .filter(checkbox => {
            const itemId = parseInt(checkbox.value);
            const item = cartData.find(x => x.idGioHangChiTiet === itemId);
            return item && item.modelSanPham.trangThai === 1;
        });

    const checkedCount = activeCheckboxes.filter(checkbox => checkbox.checked).length;

    const allChecked = activeCheckboxes.length > 0 && checkedCount === activeCheckboxes.length;

    const selectAll = document.getElementById('selectAll');
    if (selectAll) {
        selectAll.checked = allChecked;
        // Enable/disable nút chọn tất cả nếu không có sản phẩm active nào
        selectAll.disabled = activeCheckboxes.length === 0;
    }
}

// Xử lý thay đổi số lượng
function handleQuantityChange(e) {
    const input = e.target.closest('.quantity-control').querySelector('.quantity-input');
    const currentValue = parseInt(input.value);
    const maxQuantity = parseInt(input.dataset.maxQuantity);

    if (e.target.classList.contains('btn-plus') && currentValue < maxQuantity) {
        input.value = currentValue + 1;
        updateQuantityState(input);
        updateItemTotal(input);
        updateQuantityInDatabase(input);
        updateTotal();
    } else if (e.target.classList.contains('btn-minus') && currentValue > 1) {
        input.value = currentValue - 1;
        updateQuantityState(input);
        updateItemTotal(input);
        updateQuantityInDatabase(input);
        updateTotal();
    }
}

// Xử lý nhập số lượng trực tiếp
function handleQuantityInputChange(e) {
    let value = parseInt(e.target.value);
    const maxQuantity = parseInt(e.target.dataset.maxQuantity);

    if (isNaN(value) || value < 1) value = 1;
    if (value > maxQuantity) value = maxQuantity;

    e.target.value = value;
    updateQuantityState(e.target);
    updateItemTotal(e.target);
    updateQuantityInDatabase(e.target);
    updateTotal();
}

// Cập nhật trạng thái nút tăng/giảm
function updateQuantityState(input) {
    const quantity = parseInt(input.value);
    const maxQuantity = parseInt(input.dataset.maxQuantity);
    const control = input.closest('.quantity-control');
    const btnMinus = control.querySelector('.btn-minus');
    const btnPlus = control.querySelector('.btn-plus');

    // Cập nhật trạng thái nút
    btnMinus.disabled = quantity <= 1;
    btnPlus.disabled = quantity >= maxQuantity;

    // Cập nhật trong cartData
    const itemId = input.dataset.itemId;
    const item = cartData.find(x => x.idGioHangChiTiet == itemId);
    if (item) {
        item.soLuong = quantity;
        item.thanhTien = quantity * item.modelSanPham.giaBanModel;
    }
}

// Cập nhật thành tiền cho từng sản phẩm
function updateItemTotal(input) {
    const quantity = parseInt(input.value);
    const price = parseFloat(input.dataset.price);
    const itemId = input.dataset.itemId;
    const total = quantity * price;

    const row = input.closest('tr');
    const totalElement = row.querySelector('.item-total');
    if (totalElement) {
        totalElement.textContent = formatMoney(total);
    }

    // Cập nhật trong cartData
    const item = cartData.find(x => x.idGioHangChiTiet == itemId);
    if (item) {
        item.soLuong = quantity;
        item.thanhTien = total;
    }
}

// Cập nhật tổng thanh toán - sử dụng IdGioHangChiTiet
function updateTotal() {
    let totalProductPrice = 0;

    cartData.forEach(item => {
        if (selectedItems.has(item.idGioHangChiTiet)) {
            totalProductPrice += item.thanhTien;
        }
    });

    const totalPaymentElement = document.getElementById('totalPayment');
    const selectedCountElement = document.getElementById('selectedCount');
    const selectedCountBtnElement = document.getElementById('selectedCountBtn');
    const btnCheckout = document.getElementById('btnCheckout');

    if (totalPaymentElement) totalPaymentElement.textContent = formatMoney(totalProductPrice);
    if (selectedCountElement) selectedCountElement.textContent = selectedItems.size;
    if (selectedCountBtnElement) selectedCountBtnElement.textContent = selectedItems.size;

    // Enable/disable nút thanh toán
    if (btnCheckout) {
        btnCheckout.disabled = selectedItems.size === 0;
        const btnText = btnCheckout.innerHTML;
        // Cập nhật số lượng trong nút mà không làm mất icon
        if (btnText.includes('<i')) {
            btnCheckout.innerHTML = `<i class="bi bi-credit-card me-2"></i>Thanh Toán (${selectedItems.size} sản phẩm)`;
        } else {
            btnCheckout.textContent = `Thanh Toán (${selectedItems.size} sản phẩm)`;
        }
    }
}

// Xử lý thanh toán - SỬA LẠI
async function handleCheckout() {
    try {
        if (selectedItems.size === 0) {
            showAlert('warning', 'Vui lòng chọn ít nhất một sản phẩm để thanh toán');
            return;
        }

        // Lấy danh sách sản phẩm đã chọn
        const selectedProducts = cartData.filter(item => selectedItems.has(item.idGioHangChiTiet));

        // Kiểm tra lại trạng thái sản phẩm - lấy số lượng thực tế từ input
        const invalidProducts = [];
        for (const item of selectedProducts) {
            // Lấy số lượng thực tế từ input
            const quantityInput = document.querySelector(`input[data-item-id="${item.idGioHangChiTiet}"]`);
            const actualQuantity = quantityInput ? parseInt(quantityInput.value) : item.soLuong;
            
            // Kiểm tra trạng thái và tồn kho
            if (item.modelSanPham.trangThai !== 1) {
                invalidProducts.push(item);
            } else {
                // Sửa: So sánh với số lượng thực tế từ input, không phải từ cartData
                const soLuongTon = item.soLuongTon || 0;
                if (soLuongTon < actualQuantity) {
                    invalidProducts.push(item);
                }
            }
        }

        if (invalidProducts.length > 0) {
            showAlert('error', 'Một số sản phẩm đã thay đổi trạng thái hoặc hết hàng. Vui lòng kiểm tra lại giỏ hàng.');
            loadCart();
            return;
        }

        // Chuyển hướng đến trang thanh toán với danh sách IdGioHangChiTiet
        const selectedItemsString = Array.from(selectedItems).join(',');
        window.location.href = `/GioHang/ThanhToan?selectedItems=${selectedItemsString}`;

    } catch (error) {
        console.error('Lỗi thanh toán:', error);
        showAlert('error', 'Lỗi thanh toán: ' + error.message);
    }
}

// API xử lý thanh toán (tạm thời)
async function processCheckout(selectedProductIds, productDetails, totalAmount) {
    try {
        // TODO: Thay thế bằng API thanh toán thực tế
        const response = await fetch('/GioHang/Checkout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                selectedProductIds: selectedProductIds,
                productDetails: productDetails,
                totalAmount: totalAmount,
                paymentMethod: 'COD' // Tạm thời thanh toán khi nhận hàng
            })
        });

        if (!response.ok) {
            throw new Error('Lỗi kết nối thanh toán');
        }

        const result = await response.json();

        if (result.success) {
            showAlert('success', 'Thanh toán thành công! Đơn hàng đang được xử lý.');
            // Xóa sản phẩm đã thanh toán khỏi giỏ hàng
            await removeCheckedOutItems();
        } else {
            showAlert('error', result.message || 'Thanh toán thất bại');
        }
    } catch (error) {
        console.error('Lỗi thanh toán:', error);
        throw error;
    }
}

// Xóa sản phẩm đã thanh toán khỏi giỏ hàng
async function removeCheckedOutItems() {
    try {
        // Xóa từng sản phẩm đã chọn khỏi giỏ hàng
        for (const itemId of selectedItems) {
            await fetch('/GioHang/DeleteCartItem?id=' + itemId, {
                method: 'DELETE'
            });
        }

        // Clear selected items và reload giỏ hàng
        selectedItems.clear();
        await loadCart();

    } catch (error) {
        console.error('Lỗi xóa sản phẩm đã thanh toán:', error);
        showAlert('error', 'Lỗi cập nhật giỏ hàng sau thanh toán');
    }
}

// Xử lý xóa sản phẩm
function handleDeleteClick(e) {
    itemToDelete = e.target.closest('.btn-delete').dataset.itemId;
    $('#deleteModal').modal('show');
}

// Xác nhận xóa
document.addEventListener('DOMContentLoaded', function () {
    const confirmDeleteBtn = document.getElementById('confirmDelete');
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', async function () {
            if (!itemToDelete) return;

            try {
                const response = await fetch('/GioHang/DeleteCartItem?id=' + itemToDelete, {
                    method: 'DELETE'
                });

                const result = await response.json();

                if (result.success) {
                    // Xóa khỏi danh sách
                    const deletedItem = cartData.find(item => item.idGioHangChiTiet == itemToDelete);
                    if (deletedItem) {
                        selectedItems.delete(deletedItem.idGioHangChiTiet);
                    }
                    cartData = cartData.filter(item => item.idGioHangChiTiet != itemToDelete);

                    renderCartItems();
                    updateTotal();
                    showAlert('success', 'Đã xóa sản phẩm khỏi giỏ hàng');
                } else {
                    showAlert('error', result.message);
                }
            } catch (error) {
                console.error('Lỗi xóa sản phẩm:', error);
                showAlert('error', 'Lỗi xóa sản phẩm');
            }

            $('#deleteModal').modal('hide');
            itemToDelete = null;
        });
    }
});

// Cập nhật số lượng trong database
async function updateQuantityInDatabase(input) {
    const itemId = input.dataset.itemId;
    const quantity = parseInt(input.value);

    try {
        const response = await fetch('/GioHang/UpdateQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                cartItemId: parseInt(itemId),
                quantity: quantity
            })
        });

        if (!response.ok) {
            throw new Error('Lỗi cập nhật số lượng');
        }

        // Cập nhật thành công
        const result = await response.json();
        if (result.success) {
            console.log('Đã cập nhật số lượng trong database');
        } else {
            showAlert('error', result.message);
            // Rollback UI nếu cần
            loadCart(); // Reload lại giỏ hàng để đồng bộ
        }
    } catch (error) {
        console.error('Lỗi cập nhật số lượng:', error);
        showAlert('error', 'Lỗi cập nhật số lượng');
        // Rollback UI
        loadCart(); // Reload lại giỏ hàng để đồng bộ
    }
}

// Khởi tạo
document.addEventListener('DOMContentLoaded', function () {
    loadCart();
});