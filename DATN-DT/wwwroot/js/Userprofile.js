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

// ====== AVATAR UPLOAD & CROP ======
// Cấu hình nghiệp vụ
const AVATAR_CONFIG = {
    aspectType: 'circle', // circle / square / free
    minSize: 256, // Kích thước tối thiểu (px)
    maxSize: 1024, // Kích thước tối đa (px)
    quality: 0.95, // Chất lượng JPEG (0-1)
    allowedFormats: ['jpg', 'jpeg', 'png', 'webp'],
    maxFileSize: 5 * 1024 * 1024, // 5MB
    zoomMin: 0.5,
    zoomMax: 3.0
};

// Helper function: Update avatar display
function updateAvatarDisplay(avatarUrl) {
    var avatarContainer = document.getElementById('avatarContainer');
    if (!avatarContainer) return;
    
    if (avatarUrl) {
        var timestamp = new Date().getTime();
        var img = document.createElement('img');
        img.src = avatarUrl + '?t=' + timestamp;
        img.alt = 'Avatar';
        img.style.width = '100%';
        img.style.height = '100%';
        img.style.objectFit = 'cover';
        img.onerror = function() {
            this.onerror = null;
            // Fallback to default avatar
            var defaultImg = '/images/default-avatar.png';
            if (this.src !== defaultImg) {
                this.src = defaultImg;
            }
        };
        avatarContainer.innerHTML = '';
        avatarContainer.appendChild(img);
        
        // Hide default icon
        const avatarIcon = document.getElementById('avatarIcon');
        if (avatarIcon) avatarIcon.style.display = 'none';
    } else {
        // Show default icon if no avatar
        avatarContainer.innerHTML = '<i class="bi bi-person-circle" id="avatarIcon"></i>';
        const avatarIcon = document.getElementById('avatarIcon');
        if (avatarIcon) avatarIcon.style.display = 'block';
    }
}

let avatarCropData = {
    image: null,
    originalFile: null, // Lưu file gốc để reset
    zoom: 1,
    rotation: 0,
    offsetX: 0,
    offsetY: 0,
    isDragging: false,
    startX: 0,
    startY: 0,
    startOffsetX: 0,
    startOffsetY: 0,
    originalImage: null,
    originalState: null // Lưu state ban đầu để reset
};

document.addEventListener('DOMContentLoaded', function() {
    // Xử lý tự động mở tab "Lịch sử đơn hàng" khi có query parameter payment
    const urlParams = new URLSearchParams(window.location.search);
    const paymentStatus = urlParams.get('payment');
    const maDon = urlParams.get('maDon');
    
    if (paymentStatus) {
        // Tự động mở tab "Lịch sử đơn hàng" (tab thứ 4)
        const ordersTab = document.getElementById('orders-tab');
        if (ordersTab) {
            // Đợi một chút để đảm bảo DOM đã sẵn sàng
            setTimeout(() => {
                // Sử dụng Bootstrap tab API để switch tab
                const tab = new bootstrap.Tab(ordersTab);
                tab.show();
                
                // Đảm bảo loadOrders được gọi sau khi tab được hiển thị
                ordersTab.addEventListener('shown.bs.tab', function onTabShown() {
                    // Gỡ listener để tránh gọi nhiều lần
                    ordersTab.removeEventListener('shown.bs.tab', onTabShown);
                    
                    // Load đơn hàng
                    if (typeof loadOrders === 'function') {
                        loadOrders();
                    } else {
                        // Nếu hàm loadOrders chưa tồn tại, gọi trực tiếp API
                        loadOrdersFromAPI();
                    }
                }, { once: true });
                
                // Trigger event manually nếu tab đã active
                if (ordersTab.classList.contains('active')) {
                    ordersTab.dispatchEvent(new Event('shown.bs.tab'));
                }
            }, 100);
            
            // Hiển thị thông báo nếu có
            if (paymentStatus === 'success') {
                const message = maDon ? `Thanh toán thành công cho đơn hàng ${maDon}!` : 'Thanh toán thành công!';
                setTimeout(() => {
                    showAlert('success', message);
                }, 800);
            } else if (paymentStatus === 'cancelled') {
                const message = maDon ? `Bạn đã hủy thanh toán cho đơn hàng ${maDon}.` : 'Bạn đã hủy thanh toán.';
                setTimeout(() => {
                    showAlert('error', message);
                }, 800);
            }
            
            // Xóa query parameter để tránh reload lại tab
            setTimeout(() => {
                const newUrl = window.location.pathname;
                window.history.replaceState({}, '', newUrl);
            }, 1000);
        }
    }
    
    try {
        var avatarInput = document.getElementById('avatarInput');
        if (avatarInput) {
            // Flag để tránh xử lý nhiều lần
            var isUploadingAvatar = false;
            
            avatarInput.addEventListener('change', function (e) {
                // Tránh xử lý nếu đang upload hoặc không có file
                if (isUploadingAvatar || !e.target.files || e.target.files.length === 0) {
                    e.target.value = '';
                    return;
                }

                try {
                    var file = e.target.files[0];
                    if (!file) {
                        e.target.value = '';
                        return;
                    }

                    // Validate ảnh
                    const validationResult = validateAvatarFile(file);
                    if (!validationResult.isValid) {
                        showAlert('error', validationResult.error);
                        e.target.value = '';
                        return;
                    }

                    // Set flag để tránh xử lý lại
                    isUploadingAvatar = true;
                    
                    // Clear input ngay để tránh trigger lại
                    e.target.value = '';
                    
                    // Upload trực tiếp không qua crop (async, không await để không block)
                    uploadAvatarDirectly(file).finally(function() {
                        // Reset flag sau khi upload xong (thành công hoặc thất bại)
                        setTimeout(function() {
                            isUploadingAvatar = false;
                        }, 1000);
                    });
                } catch (error) {
                    console.error('Error in avatar input change:', error);
                    showAlert('error', 'Lỗi khi xử lý file ảnh. Vui lòng thử lại.');
                    if (e && e.target) e.target.value = '';
                    isUploadingAvatar = false;
                }
            });
        }
    } catch (error) {
        console.error('Error in DOMContentLoaded for avatar:', error);
        // 🔹 QUAN TRỌNG: Đảm bảo code vẫn tiếp tục hoạt động
        // Không có exception nào làm dừng toàn bộ ứng dụng
    }
});

// 🔹 Bước 1: Validate ảnh
function validateAvatarFile(file) {
    // Kiểm tra file tồn tại
    if (!file) {
        return { isValid: false, error: 'Vui lòng chọn file ảnh' };
    }

    // Kiểm tra định dạng
    const fileExtension = file.name.split('.').pop().toLowerCase();
    if (!AVATAR_CONFIG.allowedFormats.includes(fileExtension)) {
        return { 
            isValid: false, 
            error: `Chỉ chấp nhận file ảnh: ${AVATAR_CONFIG.allowedFormats.join(', ')}` 
        };
    }

    // Kiểm tra kích thước file
    if (file.size > AVATAR_CONFIG.maxFileSize) {
        return { 
            isValid: false, 
            error: `Kích thước file không được vượt quá ${(AVATAR_CONFIG.maxFileSize / 1024 / 1024).toFixed(0)}MB` 
        };
    }

    return { isValid: true };
}

// Upload ảnh trực tiếp (không qua crop)
async function uploadAvatarDirectly(file) {
    // Validate file trước khi upload
    if (!file) {
        showAlert('error', 'Không có file để upload');
        return;
    }
    
    // Kiểm tra file size trước khi xử lý
    if (file.size > 5 * 1024 * 1024) {
        showAlert('error', 'Kích thước file không được vượt quá 5MB');
        return;
    }
    
    showLoading();
    
    try {
        const formData = new FormData();
        formData.append('avatarFile', file);

        // Tạo AbortController để có thể cancel request nếu timeout
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 60000); // 60 giây timeout

        let response;
        try {
            response = await fetch('/UserProfile/UpdateAvatar', {
                method: 'POST',
                body: formData,
                credentials: 'include', // Đảm bảo gửi cookie
                signal: controller.signal // Thêm signal để có thể abort
            });
            clearTimeout(timeoutId);
        } catch (fetchError) {
            clearTimeout(timeoutId);
            if (fetchError.name === 'AbortError') {
                throw new Error('Upload ảnh quá lâu. Vui lòng thử lại với file nhỏ hơn.');
            }
            throw new Error('Lỗi kết nối đến server. Vui lòng kiểm tra kết nối mạng.');
        }

        if (!response.ok) {
            let errorMessage = 'Lỗi khi upload ảnh';
            try {
                const errorText = await response.text();
                if (errorText) {
                    try {
                        const errorData = JSON.parse(errorText);
                        errorMessage = errorData.message || errorMessage;
                    } catch {
                        // Nếu không parse được JSON, sử dụng text gốc
                        errorMessage = errorText.length > 200 ? 'Lỗi không xác định từ server' : errorText;
                    }
                }
            } catch (parseError) {
                console.error('Error parsing error response:', parseError);
                // Sử dụng message mặc định
            }
            throw new Error(errorMessage);
        }

        let data;
        try {
            const responseText = await response.text();
            if (!responseText) {
                throw new Error('Empty response from server');
            }
            data = JSON.parse(responseText);
        } catch (parseError) {
            console.error('Error parsing response:', parseError);
            throw new Error('Lỗi khi xử lý phản hồi từ server');
        }

        if (data.avatarUrl) {
            showAlert('success', data.message || 'Cập nhật ảnh đại diện thành công!');
            
            // Update avatar display
            updateAvatarDisplay(data.avatarUrl);
            
            // Đồng bộ với localStorage
            const savedProfile = localStorage.getItem('userProfile');
            let profile = savedProfile ? JSON.parse(savedProfile) : {};
            profile.defaultImage = data.avatarUrl;
            profile.avatarUrl = data.avatarUrl; // Lưu cả avatarUrl để tương thích
            localStorage.setItem('userProfile', JSON.stringify(profile));
            
            // Trigger event
            sessionStorage.setItem('avatarUpdated', Date.now().toString());
            window.dispatchEvent(new CustomEvent('userProfileUpdated', {
                detail: { defaultImage: data.avatarUrl, avatarUrl: data.avatarUrl }
            }));
            
            console.log('Avatar updated and synced to localStorage:', data.avatarUrl);
        } else {
            // Nếu không có avatarUrl trong response, vẫn hiển thị thông báo thành công
            showAlert('success', data.message || 'Cập nhật ảnh đại diện thành công!');
            // Reload profile data để lấy avatar mới
            setTimeout(() => {
                loadProfileData();
            }, 500);
        }
    } catch (error) {
        console.error('Error uploading avatar:', error);
        
        // Hiển thị thông báo lỗi chi tiết hơn
        let errorMessage = 'Lỗi khi upload ảnh';
        if (error && error.message) {
            errorMessage = error.message;
        } else if (error && typeof error === 'string') {
            errorMessage = error;
        }
        
        showAlert('error', errorMessage);
    } finally {
        // 🔹 QUAN TRỌNG: Luôn ẩn loading, ngay cả khi có lỗi
        try {
            hideLoading();
        } catch (loadingError) {
            console.error('Error hiding loading:', loadingError);
            // Không throw, chỉ log
        }
    }
}

// 🔹 Bước 1: Load ảnh và mở modal (DEPRECATED - không dùng nữa)
function openAvatarCropModal(file) {
    showLoading();
    
    const reader = new FileReader();
    reader.onerror = function() {
        hideLoading();
        showAlert('error', 'Lỗi khi đọc file ảnh');
    };
    
    reader.onload = function(e) {
        const img = new Image();
        img.onerror = function() {
            hideLoading();
            showAlert('error', 'Không thể load ảnh. Vui lòng chọn file ảnh hợp lệ.');
        };
        
        img.onload = function() {
            // Kiểm tra kích thước ảnh
            if (img.width < AVATAR_CONFIG.minSize || img.height < AVATAR_CONFIG.minSize) {
                hideLoading();
                showAlert('error', `Kích thước ảnh tối thiểu: ${AVATAR_CONFIG.minSize}x${AVATAR_CONFIG.minSize}px`);
                return;
            }

            // Lưu state ban đầu
            avatarCropData.originalFile = file;
            avatarCropData.originalImage = img;
            avatarCropData.image = img;
            avatarCropData.zoom = 1;
            avatarCropData.rotation = 0;
            avatarCropData.offsetX = 0;
            avatarCropData.offsetY = 0;
            
            // Lưu original state để reset
            avatarCropData.originalState = {
                zoom: 1,
                rotation: 0,
                offsetX: 0,
                offsetY: 0
            };

            hideLoading();

            // Mở modal
            const modalEl = document.getElementById('avatarCropModal');
            if (!modalEl) {
                showAlert('error', 'Không tìm thấy modal edit ảnh');
                return;
            }

            const modal = new bootstrap.Modal(modalEl);
            
            // 🔹 QUAN TRỌNG: Setup cancel handler - KHÔNG upload khi đóng modal
            // Event này được trigger khi modal đóng (Cancel, X, hoặc backdrop click)
            modalEl.addEventListener('hidden.bs.modal', function onHidden() {
                // Cleanup khi đóng modal - HỦY mọi thay đổi, KHÔNG upload
                cleanupAvatarCrop();
                modalEl.removeEventListener('hidden.bs.modal', onHidden);
            }, { once: true });
            
            // Đảm bảo Cancel button cũng cleanup (backup)
            const cancelBtn = modalEl.querySelector('[data-bs-dismiss="modal"]');
            if (cancelBtn) {
                cancelBtn.addEventListener('click', function() {
                    // Đảm bảo cleanup khi click Cancel
                    cleanupAvatarCrop();
                }, { once: true });
            }

            modal.show();

            // Render image sau khi modal hiển thị hoàn toàn
            modalEl.addEventListener('shown.bs.modal', function onShown() {
                setTimeout(() => {
                    renderAvatarCrop();
                }, 100);
                modalEl.removeEventListener('shown.bs.modal', onShown);
            }, { once: true });
        };
        
        img.src = e.target.result;
    };
    
    reader.readAsDataURL(file);
}

// 🔹 Cleanup khi đóng modal (Cancel hoặc X) - KHÔNG upload
// Hàm này được gọi khi:
// - User click Cancel
// - User click X (close button)
// - User click backdrop
// - Modal đóng bằng bất kỳ cách nào trừ Apply
function cleanupAvatarCrop() {
    // Reset tất cả data - HỦY mọi thay đổi
    avatarCropData.image = null;
    avatarCropData.originalFile = null;
    avatarCropData.originalImage = null;
    avatarCropData.zoom = 1;
    avatarCropData.rotation = 0;
    avatarCropData.offsetX = 0;
    avatarCropData.offsetY = 0;
    avatarCropData.originalState = null;
    
    // Clear canvas
    const canvas = document.getElementById('avatarCropCanvas');
    if (canvas) {
        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        canvas.style.display = 'none';
    }
    
    // Reset zoom slider nếu có
    const zoomSlider = document.getElementById('avatarZoomSlider');
    if (zoomSlider) {
        zoomSlider.value = 1;
    }
    
    // 🔹 QUAN TRỌNG: Không có request upload nào ở đây
    // Tất cả thay đổi đã bị hủy, không upload lên server
}

// Setup controls cho crop
// 🔹 QUAN TRỌNG: Hàm này cần được gọi lại sau khi modal đóng để đảm bảo controls hoạt động
// Sử dụng named functions để có thể remove listeners nếu cần
let zoomHandler = null;
let rotateHandler = null;
let resetHandler = null;
let applyHandler = null;

function setupAvatarCropControls() {
    try {
        // Setup controls - sử dụng named functions để có thể remove nếu cần
    
        // Zoom slider
        const zoomSlider = document.getElementById('avatarZoomSlider');
        if (zoomSlider) {
            // Remove old listener nếu có
            if (zoomHandler) {
                zoomSlider.removeEventListener('input', zoomHandler);
            }
            
            zoomSlider.min = AVATAR_CONFIG.zoomMin;
            zoomSlider.max = AVATAR_CONFIG.zoomMax;
            zoomSlider.value = 1;
            
            // Create named handler
            zoomHandler = function() {
                try {
                    avatarCropData.zoom = parseFloat(this.value);
                    // 🔹 Bước 3: Preview tạm thời sau mỗi thao tác
                    renderAvatarCrop();
                } catch (error) {
                    console.error('Error in zoom:', error);
                }
            };
            
            zoomSlider.addEventListener('input', zoomHandler);
        }

        // Rotate button - xoay 90°
        const rotateBtn = document.getElementById('avatarRotateBtn');
        if (rotateBtn) {
            // Remove old listener nếu có
            if (rotateHandler) {
                rotateBtn.removeEventListener('click', rotateHandler);
            }
            
            rotateHandler = function() {
                try {
                    avatarCropData.rotation = (avatarCropData.rotation + 90) % 360;
                    // 🔹 Bước 3: Preview tạm thời
                    renderAvatarCrop();
                } catch (error) {
                    console.error('Error rotating:', error);
                }
            };
            
            rotateBtn.addEventListener('click', rotateHandler);
        }

        // Reset button - quay về ảnh ban đầu
        const resetBtn = document.getElementById('avatarResetBtn');
        if (resetBtn) {
            // Remove old listener nếu có
            if (resetHandler) {
                resetBtn.removeEventListener('click', resetHandler);
            }
            
            resetHandler = function() {
                try {
                    if (avatarCropData.originalState) {
                        avatarCropData.zoom = avatarCropData.originalState.zoom;
                        avatarCropData.rotation = avatarCropData.originalState.rotation;
                        avatarCropData.offsetX = avatarCropData.originalState.offsetX;
                        avatarCropData.offsetY = avatarCropData.originalState.offsetY;
                    } else {
                        avatarCropData.zoom = 1;
                        avatarCropData.rotation = 0;
                        avatarCropData.offsetX = 0;
                        avatarCropData.offsetY = 0;
                    }
                    const currentSlider = document.getElementById('avatarZoomSlider');
                    if (currentSlider) currentSlider.value = avatarCropData.zoom;
                    // 🔹 Bước 3: Preview tạm thời
                    renderAvatarCrop();
                } catch (error) {
                    console.error('Error resetting:', error);
                }
            };
            
            resetBtn.addEventListener('click', resetHandler);
        }

        // Apply button
        const applyBtn = document.getElementById('avatarApplyBtn');
        if (applyBtn) {
            // Remove old listener nếu có
            if (applyHandler) {
                applyBtn.removeEventListener('click', applyHandler);
            }
            
            applyHandler = async function() {
                try {
                    await applyAvatarCrop();
                } catch (error) {
                    console.error('Error applying crop:', error);
                    showAlert('error', 'Lỗi khi xử lý ảnh. Vui lòng thử lại.');
                    hideLoading();
                }
            };
            
            applyBtn.addEventListener('click', applyHandler);
        }

        // Drag to move image
        const preview = document.getElementById('avatarCropPreview');
        if (preview) {
            preview.addEventListener('mousedown', startDrag);
            preview.addEventListener('touchstart', startDrag);
            document.addEventListener('mousemove', drag);
            document.addEventListener('touchmove', drag);
            document.addEventListener('mouseup', stopDrag);
            document.addEventListener('touchend', stopDrag);
        }
    } catch (error) {
        console.error('Error setting up avatar crop controls:', error);
        // Đảm bảo code vẫn tiếp tục hoạt động
    }
}

// Render avatar crop
function renderAvatarCrop() {
    if (!avatarCropData.image) {
        console.error('No image to render');
        return;
    }

    const canvas = document.getElementById('avatarCropCanvas');
    const preview = document.getElementById('avatarCropPreview');
    if (!canvas || !preview) {
        console.error('Canvas or preview element not found');
        return;
    }

    // Wait for preview to have dimensions
    if (preview.offsetWidth === 0 || preview.offsetHeight === 0) {
        setTimeout(renderAvatarCrop, 100);
        return;
    }

    const containerWidth = preview.offsetWidth;
    const containerHeight = preview.offsetHeight;
    const cropSize = Math.min(containerWidth, containerHeight) * 0.8;
    const cropX = (containerWidth - cropSize) / 2;
    const cropY = (containerHeight - cropSize) / 2;

    // Set canvas size
    canvas.width = containerWidth;
    canvas.height = containerHeight;
    const ctx = canvas.getContext('2d');

    // Clear canvas with background color
    ctx.fillStyle = '#36363f';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Calculate base scale to fit image in container (zoom = 1)
    const imgAspect = avatarCropData.image.width / avatarCropData.image.height;
    const containerAspect = containerWidth / containerHeight;
    
    // Base scale: scale để ảnh vừa khít container khi zoom = 1
    let baseScale;
    if (imgAspect > containerAspect) {
        // Image is wider - fit to width
        baseScale = containerWidth / avatarCropData.image.width;
    } else {
        // Image is taller - fit to height
        baseScale = containerHeight / avatarCropData.image.height;
    }
    
    // Apply zoom to base scale
    const currentScale = baseScale * avatarCropData.zoom;
    const drawWidth = avatarCropData.image.width * currentScale;
    const drawHeight = avatarCropData.image.height * currentScale;

    // Draw faded background image (full image, faded) - như ảnh mẫu
    ctx.globalAlpha = 0.3;
    // Scale to fit container (luôn fit to cover để hiển thị toàn bộ ảnh)
    const bgScale = Math.max(containerWidth / avatarCropData.image.width, containerHeight / avatarCropData.image.height);
    const bgWidth = avatarCropData.image.width * bgScale;
    const bgHeight = avatarCropData.image.height * bgScale;
    ctx.drawImage(
        avatarCropData.image,
        (containerWidth - bgWidth) / 2,
        (containerHeight - bgHeight) / 2,
        bgWidth,
        bgHeight
    );
    ctx.globalAlpha = 1;

    // Draw vibrant crop circle area (only inside the circle)
    ctx.save();
    ctx.beginPath();
    ctx.arc(cropX + cropSize / 2, cropY + cropSize / 2, cropSize / 2, 0, 2 * Math.PI);
    ctx.clip();
    
    // Draw image with transformations inside crop circle
    ctx.translate(containerWidth / 2 + avatarCropData.offsetX, containerHeight / 2 + avatarCropData.offsetY);
    ctx.rotate((avatarCropData.rotation * Math.PI) / 180);
    ctx.translate(-drawWidth / 2, -drawHeight / 2);
    ctx.globalAlpha = 1;
    ctx.drawImage(avatarCropData.image, 0, 0, drawWidth, drawHeight);
    ctx.restore();

    canvas.style.display = 'block';
}

// Drag functions
function startDrag(e) {
    avatarCropData.isDragging = true;
    const preview = document.getElementById('avatarCropPreview');
    if (!preview) return;
    
    const rect = preview.getBoundingClientRect();
    const touch = e.touches ? e.touches[0] : e;
    
    // Lưu vị trí chuột ban đầu và offset hiện tại
    avatarCropData.startX = touch.clientX - rect.left;
    avatarCropData.startY = touch.clientY - rect.top;
    avatarCropData.startOffsetX = avatarCropData.offsetX;
    avatarCropData.startOffsetY = avatarCropData.offsetY;
    
    e.preventDefault();
}

function drag(e) {
    if (!avatarCropData.isDragging) return;
    
    const preview = document.getElementById('avatarCropPreview');
    if (!preview) return;
    
    const rect = preview.getBoundingClientRect();
    const touch = e.touches ? e.touches[0] : e;
    
    // Tính toán vị trí chuột trong preview
    const currentX = touch.clientX - rect.left;
    const currentY = touch.clientY - rect.top;
    
    // Tính toán offset mới dựa trên sự thay đổi vị trí chuột
    const deltaX = currentX - avatarCropData.startX;
    const deltaY = currentY - avatarCropData.startY;
    
    avatarCropData.offsetX = avatarCropData.startOffsetX + deltaX;
    avatarCropData.offsetY = avatarCropData.startOffsetY + deltaY;
    
    // 🔹 Bước 3: Preview tạm thời sau mỗi thao tác drag
    renderAvatarCrop();
    e.preventDefault();
}

function stopDrag() {
    avatarCropData.isDragging = false;
}

// 🔹 Bước 4: Apply - Crop và upload (CHỈ KHI USER CLICK APPLY)
// Đây là hàm DUY NHẤT thực hiện upload lên server
// Chỉ được gọi khi user click nút "Apply"
async function applyAvatarCrop() {
    if (!avatarCropData.image) {
        showAlert('error', 'Không có ảnh để xử lý');
        return;
    }

    // 🔹 QUAN TRỌNG: Chỉ upload khi user click Apply
    // Tất cả xử lý trước đó đều client-side, không upload
    
    showLoading();

    try {
        // 🔹 Crop hình tròn 1:1 (square canvas, circular crop)
        // Đảm bảo chất lượng: sử dụng kích thước tối thiểu hoặc tối đa
        const size = Math.max(
            AVATAR_CONFIG.minSize,
            Math.min(AVATAR_CONFIG.maxSize, 512) // Mặc định 512px cho chất lượng tốt
        );
        
        // Tạo canvas vuông 1:1 để crop hình tròn
        const canvas = document.createElement('canvas');
        canvas.width = size;  // 1:1 aspect ratio
        canvas.height = size; // 1:1 aspect ratio

        const ctx = canvas.getContext('2d');
        const preview = document.getElementById('avatarCropPreview');
        const containerWidth = preview.offsetWidth || 500;
        const containerHeight = preview.offsetHeight || 500;
        const cropSize = Math.min(containerWidth, containerHeight) * 0.8;

        // Calculate how image is displayed in preview (same as renderAvatarCrop)
        const imgAspect = avatarCropData.image.width / avatarCropData.image.height;
        const containerAspect = containerWidth / containerHeight;
        
        // Base scale: scale để ảnh vừa khít container khi zoom = 1
        let baseScale;
        if (imgAspect > containerAspect) {
            baseScale = containerWidth / avatarCropData.image.width;
        } else {
            baseScale = containerHeight / avatarCropData.image.height;
        }
        
        // Apply zoom to base scale
        const currentScale = baseScale * avatarCropData.zoom;
        const drawWidth = avatarCropData.image.width * currentScale;
        const drawHeight = avatarCropData.image.height * currentScale;

        // Calculate crop circle center in container coordinates
        const cropCenterX = containerWidth / 2;
        const cropCenterY = containerHeight / 2;
        const cropRadius = cropSize / 2;

        // Calculate the crop area in the original image coordinates
        // First, find where the crop center is in the transformed image space
        const transformedCropCenterX = cropCenterX + avatarCropData.offsetX;
        const transformedCropCenterY = cropCenterY + avatarCropData.offsetY;
        
        // Convert to image coordinates (accounting for zoom and position)
        const imageCenterX = avatarCropData.image.width / 2;
        const imageCenterY = avatarCropData.image.height / 2;
        
        // Calculate offset in image space
        const offsetInImageX = ((transformedCropCenterX - containerWidth / 2) / drawWidth) * avatarCropData.image.width;
        const offsetInImageY = ((transformedCropCenterY - containerHeight / 2) / drawHeight) * avatarCropData.image.height;
        
        const sourceCenterX = imageCenterX + offsetInImageX;
        const sourceCenterY = imageCenterY + offsetInImageY;
        
        // Calculate crop size in original image
        const cropRatioInImage = (cropRadius * 2) / drawWidth;
        const sourceCropSize = avatarCropData.image.width * cropRatioInImage;

        // Tạo canvas lớn hơn để đảm bảo chất lượng không bị vỡ
        // Sử dụng 2x hoặc 3x để có chất lượng tốt hơn
        const tempCanvas = document.createElement('canvas');
        const tempSize = size * 3; // 3x để đảm bảo chất lượng cao
        tempCanvas.width = tempSize;
        tempCanvas.height = tempSize;
        const tempCtx = tempCanvas.getContext('2d');
        
        // Enable image smoothing để tránh bị vỡ
        tempCtx.imageSmoothingEnabled = true;
        tempCtx.imageSmoothingQuality = 'high';

        // Draw cropped portion with rotation
        tempCtx.save();
        tempCtx.translate(tempSize / 2, tempSize / 2);
        tempCtx.rotate((avatarCropData.rotation * Math.PI) / 180);
        
        // Calculate source rectangle bounds
        const sourceX = Math.max(0, Math.min(avatarCropData.image.width - sourceCropSize, sourceCenterX - sourceCropSize / 2));
        const sourceY = Math.max(0, Math.min(avatarCropData.image.height - sourceCropSize, sourceCenterY - sourceCropSize / 2));
        const actualSourceSize = Math.min(sourceCropSize, avatarCropData.image.width - sourceX, avatarCropData.image.height - sourceY);
        
        tempCtx.drawImage(
            avatarCropData.image,
            sourceX,
            sourceY,
            actualSourceSize,
            actualSourceSize,
            -tempSize / 2,
            -tempSize / 2,
            tempSize,
            tempSize
        );
        tempCtx.restore();

        // Draw circular crop to final canvas với chất lượng cao
        ctx.save();
        ctx.imageSmoothingEnabled = true;
        ctx.imageSmoothingQuality = 'high';
        ctx.beginPath();
        ctx.arc(size / 2, size / 2, size / 2, 0, 2 * Math.PI);
        ctx.clip();
        ctx.drawImage(tempCanvas, 0, 0, size, size);
        ctx.restore();

        // Kiểm tra hỗ trợ toBlob
        if (typeof canvas.toBlob !== 'function') {
            hideLoading();
            showAlert('error', 'Trình duyệt không hỗ trợ xử lý ảnh. Vui lòng sử dụng trình duyệt khác.');
            return;
        }

        // Convert to blob với chất lượng cao (đảm bảo không bị vỡ)
        // Sử dụng quality cao để đảm bảo chất lượng ảnh tốt
        canvas.toBlob(async function(blob) {
            try {
                if (!blob) {
                    hideLoading();
                    showAlert('error', 'Lỗi khi xử lý ảnh. Vui lòng thử lại.');
                    return;
                }

                // Kiểm tra kích thước blob (đảm bảo không quá lớn)
                if (blob.size > AVATAR_CONFIG.maxFileSize) {
                    hideLoading();
                    showAlert('error', 'Ảnh sau khi xử lý quá lớn. Vui lòng thử lại với zoom nhỏ hơn.');
                    return;
                }

                // Kiểm tra kích thước tối thiểu
                if (blob.size < 100) {
                    hideLoading();
                    showAlert('error', 'Ảnh sau khi xử lý không hợp lệ. Vui lòng thử lại.');
                    return;
                }

                // 🔹 Bước 4: Upload lên server (CHỈ KHI APPLY)
                // Đảm bảo không lưu ảnh rác nếu user hủy (đã xử lý ở cleanupAvatarCrop)
                const formData = new FormData();
                formData.append('avatarFile', blob, 'avatar.jpg');

                try {
                // Tạo AbortController để có thể cancel request nếu timeout
                const controller = new AbortController();
                const timeoutId = setTimeout(() => controller.abort(), 60000); // 60 giây timeout

                let response;
                try {
                    response = await fetch('/UserProfile/UpdateAvatar', {
                        method: 'POST',
                        body: formData,
                        credentials: 'include', // Đảm bảo gửi cookie
                        signal: controller.signal // Thêm signal để có thể abort
                    });
                    clearTimeout(timeoutId);
                } catch (fetchError) {
                    clearTimeout(timeoutId);
                    if (fetchError.name === 'AbortError') {
                        throw new Error('Upload ảnh quá lâu. Vui lòng thử lại với file nhỏ hơn.');
                    }
                    throw new Error('Lỗi kết nối đến server. Vui lòng kiểm tra kết nối mạng.');
                }

                if (!response.ok) {
                    let errorMessage = 'Lỗi khi upload ảnh';
                    try {
                        const errorText = await response.text();
                        if (errorText) {
                            try {
                                const errorData = JSON.parse(errorText);
                                errorMessage = errorData.message || errorMessage;
                            } catch {
                                // Nếu không parse được JSON, sử dụng text gốc
                                errorMessage = errorText.length > 200 ? 'Lỗi không xác định từ server' : errorText;
                            }
                        }
                    } catch (parseError) {
                        console.error('Error parsing error response:', parseError);
                        // Sử dụng message mặc định
                    }
                    throw new Error(errorMessage);
                }

                let data;
                try {
                    const responseText = await response.text();
                    if (!responseText) {
                        throw new Error('Empty response from server');
                    }
                    data = JSON.parse(responseText);
                } catch (parseError) {
                    console.error('Error parsing response:', parseError);
                    throw new Error('Lỗi khi xử lý phản hồi từ server');
                }

                if (data.avatarUrl) {
                    showAlert('success', data.message || 'Cập nhật ảnh đại diện thành công!');
                    
                    // Update avatar display
                    updateAvatarDisplay(data.avatarUrl);
                    
                    // Đồng bộ với localStorage
                    const savedProfile = localStorage.getItem('userProfile');
                    let profile = savedProfile ? JSON.parse(savedProfile) : {};
                    profile.defaultImage = data.avatarUrl;
                    profile.avatarUrl = data.avatarUrl;
                    localStorage.setItem('userProfile', JSON.stringify(profile));
                    
                    // Trigger event
                    sessionStorage.setItem('avatarUpdated', Date.now().toString());
                    window.dispatchEvent(new CustomEvent('userProfileUpdated', {
                        detail: { defaultImage: data.avatarUrl, avatarUrl: data.avatarUrl }
                    }));
                    
                    console.log('Avatar updated and synced to localStorage:', data.avatarUrl);
                } else {
                    showAlert('success', data.message || 'Cập nhật ảnh đại diện thành công!');
                    // Reload profile data để lấy avatar mới
                    setTimeout(() => {
                        loadProfileData();
                    }, 500);
                }

                // Đóng modal sau khi thành công
                try {
                    const modalEl = document.getElementById('avatarCropModal');
                    if (modalEl) {
                        const modal = bootstrap.Modal.getInstance(modalEl);
                        if (modal) {
                            modal.hide();
                            // Cleanup sau khi apply thành công
                            setTimeout(() => {
                                cleanupAvatarCrop();
                                // 🔹 QUAN TRỌNG: Đảm bảo code tiếp tục hoạt động sau khi upload
                                // Reset input để có thể chọn file mới
                                const avatarInput = document.getElementById('avatarInput');
                                if (avatarInput) {
                                    avatarInput.value = '';
                                }
                            }, 300);
                        }
                    }
                } catch (modalError) {
                    console.error('Error closing modal:', modalError);
                    // Đảm bảo không có lỗi nào làm dừng execution
                    hideLoading();
                }

            } catch (error) {
                console.error('Error uploading avatar:', error);
                
                // Hiển thị thông báo lỗi chi tiết hơn
                let errorMessage = 'Lỗi khi upload ảnh';
                if (error && error.message) {
                    errorMessage = error.message;
                } else if (error && typeof error === 'string') {
                    errorMessage = error;
                }
                
                showAlert('error', errorMessage);
                
                // Đảm bảo modal vẫn có thể đóng được
                try {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('avatarCropModal'));
                    if (modal) {
                        // Không đóng modal nếu có lỗi, để user có thể thử lại
                    }
                } catch (modalError) {
                    console.error('Error accessing modal:', modalError);
                    // Không throw, chỉ log
                }
            } finally {
                // 🔹 QUAN TRỌNG: Luôn ẩn loading, ngay cả khi có lỗi
                try {
                    hideLoading();
                } catch (loadingError) {
                    console.error('Error hiding loading:', loadingError);
                    // Không throw, chỉ log
                }
                
                // 🔹 QUAN TRỌNG: Đảm bảo code tiếp tục hoạt động
                // Không có exception nào làm dừng execution
                }
            } catch (blobError) {
                console.error('Error in blob callback:', blobError);
                hideLoading();
                showAlert('error', 'Lỗi khi xử lý ảnh. Vui lòng thử lại.');
            }
        }, 'image/jpeg', 0.9);
    } catch (error) {
        console.error('Error processing avatar:', error);
        showAlert('error', error.message || 'Lỗi khi xử lý ảnh');
        hideLoading();
        // 🔹 QUAN TRỌNG: Đảm bảo không có exception nào làm dừng execution
        // Code vẫn tiếp tục hoạt động sau khi có lỗi
    }
}

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
                // Sử dụng helper function để update avatar
                updateAvatarDisplay(data.avatarUrl);
            } catch (avatarError) {
                console.error('Error updating avatar:', avatarError);
            }

            // Lưu vào localStorage để đồng bộ với MuaHang/Index
            const profileData = {
                hoTenKhachHang: data.hoTenKhachHang,
                emailKhachHang: data.emailKhachHang,
                defaultImage: data.avatarUrl, // Lưu avatarUrl vào defaultImage để tương thích với code khác
                avatarUrl: data.avatarUrl,
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
        // Kiểm tra "chờ xác nhận" trước (vì nó chứa cả "chờ" và "xác nhận")
        if (lower.includes('chờ xác nhận') || (lower.includes('chờ') && lower.includes('xác nhận')) || lower.includes('pending') || lower === '0') return 0;
        // "Đã thanh toán" tương đương "Đã xác nhận" (status = 1)
        if (lower.includes('đã thanh toán') || lower.includes('paid')) return 1;
        if ((lower.includes('xác nhận') && !lower.includes('chờ')) || lower.includes('confirmed') || lower === '1') return 1;
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
        case 1: return 'Đã xác nhận'; // Bao gồm cả "Đã thanh toán"
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
    
    // Thêm status -1 (không xác định) vào cuối nếu có
    if (ordersByStatus[-1] && ordersByStatus[-1].length > 0) {
        statusOrder.push(-1);
    }
    
    for (var s = 0; s < statusOrder.length; s++) {
        var statusNum = statusOrder[s];
        if (!ordersByStatus[statusNum] || ordersByStatus[statusNum].length === 0) {
            continue;
        }

        var statusName = getOrderStatusName(statusNum);
        // Nếu status -1, hiển thị tên trạng thái gốc từ order
        if (statusNum === -1 && ordersByStatus[statusNum].length > 0) {
            statusName = ordersByStatus[statusNum][0].trangThai || 'Không xác định';
        }
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
                        var specInfo = [];
                        if (item.ram && item.ram !== 'N/A') specInfo.push('RAM: ' + item.ram);
                        if (item.rom && item.rom !== 'N/A') specInfo.push('ROM: ' + item.rom);
                        if (item.manHinh && item.manHinh !== 'N/A') specInfo.push('Màn hình: ' + item.manHinh);
                        if (item.cameraTruoc && item.cameraTruoc !== 'N/A') specInfo.push('Camera trước: ' + item.cameraTruoc);
                        if (item.cameraSau && item.cameraSau !== 'N/A') specInfo.push('Camera sau: ' + item.cameraSau);
                        var specHtml = specInfo.length > 0 ? '<div class="text-muted small mt-1" style="color: #aaa !important;">' + specInfo.join(' | ') + '</div>' : '';
                        
                        itemsHtml += '<div class="order-item">' +
                                '<img src="' + (item.hinhAnh || '/images/default-product.jpg') + '" alt="' + (item.tenSanPham || '') + '" onerror="this.src=\'/images/default-product.jpg\'">' +
                                '<div class="flex-grow-1">' +
                                '<div style="color: #ffffff !important;"><strong>' + (item.tenSanPham || '') + '</strong></div>' +
                                '<div class="text-muted small" style="color: #aaa !important;">' + 
                                (item.tenThuongHieu && item.tenThuongHieu !== 'N/A' ? '<strong>Thương hiệu:</strong> ' + (item.tenThuongHieu || '') + ' | ' : '') +
                                (item.tenModel && item.tenModel !== 'N/A' ? '<strong>Model:</strong> ' + item.tenModel + ' - ' : '') +
                                '<strong>Màu:</strong> ' + (item.mau || '') + 
                                '</div>' +
                                specHtml +
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
            var giaKhuyenMaiHtml = '';
            if (item.giaKhuyenMai && item.giaKhuyenMai > 0 && item.giaKhuyenMai !== item.donGia) {
                giaKhuyenMaiHtml = '<p class="mb-1 text-muted"><small>Giá gốc: <del>' + formatMoney(item.donGia || 0) + '</del> | Giá khuyến mãi: <strong class="text-danger">' + formatMoney(item.giaKhuyenMai) + '</strong></small></p>';
            } else {
                giaKhuyenMaiHtml = '<p class="mb-1">Đơn giá: ' + formatMoney(item.donGia || 0) + '</p>';
            }
            
            var imeiInfoHtml = '';
            if (item.maImei && item.maImei !== 'N/A') {
                imeiInfoHtml = '<p class="mb-1 text-muted small"><strong>IMEI:</strong> ' + (item.maImei || '') + ' | <strong>Trạng thái:</strong> ' + (item.trangThaiImei || 'N/A') + '</p>';
            }
            
            var specDetailHtml = '';
            var specDetails = [];
            if (item.ram && item.ram !== 'N/A') specDetails.push('<strong>RAM:</strong> ' + item.ram);
            if (item.rom && item.rom !== 'N/A') specDetails.push('<strong>ROM:</strong> ' + item.rom);
            if (item.manHinh && item.manHinh !== 'N/A') specDetails.push('<strong>Màn hình:</strong> ' + item.manHinh);
            if (item.cameraTruoc && item.cameraTruoc !== 'N/A') specDetails.push('<strong>Camera trước:</strong> ' + item.cameraTruoc);
            if (item.cameraSau && item.cameraSau !== 'N/A') specDetails.push('<strong>Camera sau:</strong> ' + item.cameraSau);
            if (specDetails.length > 0) {
                specDetailHtml = '<p class="text-muted mb-1 small" style="color: #aaa !important;">' + specDetails.join(' | ') + '</p>';
            }
            
            chiTietHtml += '<div class="order-detail-item" style="padding: 15px; margin-bottom: 15px; background-color: #2b2b33; border-radius: 8px; border: 1px solid #444;">' +
                '<div class="row">' +
                '<div class="col-md-2">' +
                '<img src="' + (item.hinhAnh || '/images/default-product.jpg') + '" alt="' + (item.tenSanPham || '') + '" ' +
                'style="width: 100%; max-width: 100px; border-radius: 8px;" onerror="this.src=\'/images/default-product.jpg\'">' +
                '</div>' +
                '<div class="col-md-10">' +
                '<h6 style="color: #ffffff;">' + (item.tenSanPham || 'N/A') + '</h6>' +
                '<p class="text-muted mb-1" style="color: #aaa !important;">' +
                    (item.tenThuongHieu && item.tenThuongHieu !== 'N/A' ? '<strong>Thương hiệu:</strong> ' + (item.tenThuongHieu || '') + ' | ' : '') +
                    (item.tenModel && item.tenModel !== 'N/A' ? '<strong>Model:</strong> ' + item.tenModel + ' - ' : '') +
                    '<strong>Màu:</strong> ' + (item.mau || 'N/A') + 
                '</p>' +
                specDetailHtml +
                imeiInfoHtml +
                '<p class="mb-1" style="color: #ffffff;">Số lượng: <strong>' + (item.soLuong || 0) + '</strong></p>' +
                giaKhuyenMaiHtml +
                '<p class="mb-0" style="color: #ffffff;"><strong>Thành tiền: ' + formatMoney(item.thanhTien || 0) + '</strong></p>' +
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
    if (!confirm('Bạn có chắc chắn muốn hủy đơn hàng này? Số lượng sản phẩm sẽ được trả lại vào kho.')) {
        return;
    }

    showLoading();
    try {
        const response = await fetch('/UserProfile/CancelOrder?id=' + orderId, {
            method: 'POST',
            credentials: 'include'
        });
        
        const result = await response.json();
        
        if (result.success) {
            showAlert('success', result.message || 'Hủy đơn hàng thành công! Số lượng sản phẩm đã được trả lại vào kho.');
            await loadOrders();
        } else {
            showAlert('error', result.message || 'Lỗi khi hủy đơn hàng');
        }
    } catch (error) {
        console.error('Error cancelling order:', error);
        showAlert('error', 'Lỗi kết nối đến server');
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

// ====== ORDER FUNCTIONS ======
async function loadOrdersFromAPI() {
    try {
        showLoading();
        const response = await fetch('/UserProfile/GetOrders', {
            credentials: 'include'
        });
        if (!response.ok) {
            throw new Error('HTTP error! status: ' + response.status);
        }
        
        const result = await response.json();
        if (result && result.success && result.data) {
            allOrdersData = result.data;
            renderOrders();
        } else {
            allOrdersData = [];
            renderOrders();
        }
    } catch (error) {
        console.error('Error loading orders:', error);
        showAlert('error', 'Lỗi khi tải danh sách đơn hàng');
        allOrdersData = [];
        renderOrders();
    } finally {
        hideLoading();
    }
}

// Alias function để tương thích với code hiện tại
async function loadOrders() {
    await loadOrdersFromAPI();
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
