function updateNavbar() {
    const email = localStorage.getItem('email');
    const navbarUl = document.querySelector('#navbarSupportedContent ul');
    const loginNavItem = document.querySelector('.nav-item a[href="loginPage.html"]');
    const registerNavItem = document.querySelector('.nav-item a[href="RegisterPage.html"]');

    if (email) {
        loginNavItem?.parentElement.remove();
        registerNavItem?.parentElement.remove();

        navbarUl.innerHTML += `<li class="nav-item"><a class="nav-link" href="ProfileSetting.html">Hesabım</a></li>`;
        navbarUl.innerHTML += `<li class="nav-item"><a class="nav-link" href="AddDocument.html">Makale Ekle</a></li>`;
        navbarUl.innerHTML += `<li class="nav-item"><a class="nav-link" href="#" onclick="logout()">Çıkış Yap</a></li>`;
    }
}

function logout() {
    localStorage.removeItem('userToken');
    localStorage.removeItem('email');
    window.location.href = 'loginPage.html';
}

// Sayfa yüklendiğinde updateNavbar fonksiyonunu çağır
document.addEventListener('DOMContentLoaded', function() {
    updateNavbar();
});

// logout fonksiyonunu global alana taşı
window.logout = logout;
