// Fetch profile data and update header avatar
async function syncHeaderAvatar() {
    try {
        const res = await fetch('/UserProfile/GetProfileData');
        if (!res.ok) return;
        const data = await res.json();
        if (!data) return;

        const avatarUrl = data.avatarUrl || '/images/default-avatar.png';
        const container = document.getElementById('headerAvatar');
        const icon = document.getElementById('headerAvatarIcon');
        if (!container) return;

        const img = document.createElement('img');
        img.src = avatarUrl + '?t=' + Date.now();
        img.alt = 'Avatar';
        img.style.width = '40px';
        img.style.height = '40px';
        img.style.objectFit = 'cover';
        img.style.borderRadius = '50%';

        // Replace icon with image
        if (icon && icon.parentElement) icon.parentElement.replaceChild(img, icon);
    } catch (e) {
        // ignore
        console.error('syncHeaderAvatar error', e);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    // Try to sync avatar on page load
    syncHeaderAvatar();
});
