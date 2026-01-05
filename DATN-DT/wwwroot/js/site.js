// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ====== IMAGE VALIDATION UTILITIES ======

/**
 * Validate image file extension
 * @param {File} file - The file to validate
 * @param {Array<string>} allowedExtensions - Array of allowed extensions (e.g., ['jpg', 'jpeg', 'png', 'gif', 'webp'])
 * @returns {Object} - { isValid: boolean, error: string|null, extension: string }
 */
function validateImageExtension(file, allowedExtensions) {
    if (!file) {
        return { isValid: false, error: 'Vui lòng chọn file ảnh', extension: null };
    }

    // Default allowed extensions if not provided
    if (!allowedExtensions || allowedExtensions.length === 0) {
        allowedExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp'];
    }

    // Get file extension
    var fileName = file.name || '';
    var fileNameParts = fileName.split('.');
    var fileNamePartsLength = fileNameParts ? fileNameParts.length : 0;
    var lastIndex = fileNamePartsLength > 0 ? fileNamePartsLength - 1 : -1;
    var fileExtension = lastIndex >= 0 ? fileNameParts[lastIndex].toLowerCase() : '';

    if (!fileExtension) {
        return { isValid: false, error: 'File không có phần mở rộng', extension: null };
    }

    // Check if extension is allowed
    var isValidExtension = false;
    var allowedExtLength = allowedExtensions.length;
    for (var i = 0; i < allowedExtLength; i++) {
        if (allowedExtensions[i].toLowerCase() === fileExtension) {
            isValidExtension = true;
            break;
        }
    }

    if (!isValidExtension) {
        var allowedExtsString = allowedExtensions.join(', ');
        return {
            isValid: false,
            error: 'Chỉ chấp nhận file ảnh (' + allowedExtsString + ')',
            extension: fileExtension
        };
    }

    return { isValid: true, error: null, extension: fileExtension };
}

/**
 * Validate image file size
 * @param {File} file - The file to validate
 * @param {number} maxSizeInMB - Maximum file size in MB (default: 5)
 * @returns {Object} - { isValid: boolean, error: string|null }
 */
function validateImageSize(file, maxSizeInMB) {
    if (!file) {
        return { isValid: false, error: 'Vui lòng chọn file ảnh' };
    }

    // Default max size is 5MB
    if (!maxSizeInMB || maxSizeInMB <= 0) {
        maxSizeInMB = 5;
    }

    var maxSizeInBytes = maxSizeInMB * 1024 * 1024;

    if (file.size > maxSizeInBytes) {
        return {
            isValid: false,
            error: 'Kích thước file không được vượt quá ' + maxSizeInMB + 'MB'
        };
    }

    return { isValid: true, error: null };
}

/**
 * Validate image file (both extension and size)
 * @param {File} file - The file to validate
 * @param {Object} options - Validation options
 * @param {Array<string>} options.allowedExtensions - Array of allowed extensions
 * @param {number} options.maxSizeInMB - Maximum file size in MB
 * @returns {Object} - { isValid: boolean, error: string|null, extension: string }
 */
function validateImageFile(file, options) {
    options = options || {};
    var allowedExtensions = options.allowedExtensions || ['jpg', 'jpeg', 'png', 'gif', 'webp'];
    var maxSizeInMB = options.maxSizeInMB || 5;

    // Validate extension
    var extensionResult = validateImageExtension(file, allowedExtensions);
    if (!extensionResult.isValid) {
        return extensionResult;
    }

    // Validate size
    var sizeResult = validateImageSize(file, maxSizeInMB);
    if (!sizeResult.isValid) {
        return { isValid: false, error: sizeResult.error, extension: extensionResult.extension };
    }

    return { isValid: true, error: null, extension: extensionResult.extension };
}