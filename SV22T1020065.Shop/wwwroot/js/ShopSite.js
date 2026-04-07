// Hiển thị ảnh được chọn từ input file lên thẻ img
// (Thẻ input có thuộc tính data-img-preview trỏ đến id của thẻ img dung để hiển thị ảnh)
function previewImage(input) {
    if (!input.files || !input.files[0]) return;

    const previewId = input.dataset.imgPreview; // lấy data-img-preview
    if (!previewId) return;

    const img = document.getElementById(previewId);
    if (!img) return;

    const reader = new FileReader();
    reader.onload = function (e) {
        img.src = e.target.result;
    };
    reader.readAsDataURL(input.files[0]);
}

// Tìm kiếm phân trang bằng AJAX
function paginationSearch(event, form, page) {
    if (event) event.preventDefault();
    if (!form) return;

    const url = form.action;
    const method = (form.method || "GET").toUpperCase();
    const targetId = form.dataset.target;

    const formData = new FormData(form);
    formData.append("page", page);

    let fetchUrl = url;
    if (method === "GET") {
        const params = new URLSearchParams(formData).toString();
        fetchUrl = url + "?" + params;
    }

    let targetEl = null;
    if (targetId) {
        targetEl = document.getElementById(targetId);
        if (targetEl) {
            targetEl.innerHTML = `
                <div class="text-center py-4">
                    <span>Đang tải dữ liệu...</span>
                </div>`;
        }
    }

    fetch(fetchUrl, {
        method: method,
        body: method === "GET" ? null : formData
    })
    .then(res => res.text())
    .then(html => {
        if (targetEl) {
            targetEl.innerHTML = html;
        }
    })
    .catch(() => {
        if (targetEl) {
            targetEl.innerHTML = `
                <div class="text-danger">
                    Không tải được dữ liệu
                </div>`;
        }
    });
}

// Mở modal và load nội dung từ link vào modal s
(function () {
    //dialogModal là id của modal dùng chung đuơc định nghĩa trong _Layout.cshtml
    const modalEl = document.getElementById("dialogModal");
    if (!modalEl) return;

    const modalContent = modalEl.querySelector(".modal-content");

    // Clear nội dung khi modal đóng
    modalEl.addEventListener('hidden.bs.modal', function () {
        modalContent.innerHTML = '';
    });

    window.openModal = function (event, link) {
        if (!link) return;
        if (event) event.preventDefault();

        const url = link.getAttribute("href");

        // Hiển thị loading
        modalContent.innerHTML = `
            <div class="modal-body text-center py-5">
                <span>Đang tải dữ liệu...</span>
            </div>`;

        // Khởi tạo modal (chỉ tạo 1 lần)
        let modal = bootstrap.Modal.getInstance(modalEl);
        if (!modal) {
            modal = new bootstrap.Modal(modalEl, {
                backdrop: 'static',
                keyboard: false
            });
        }

        modal.show();

        // Load nội dung
        fetch(url)
            .then(res => res.text())
            .then(html => {
                modalContent.innerHTML = html;
            })
            .catch(() => {
                modalContent.innerHTML = `
                    <div class="modal-body text-danger">
                        Không tải được dữ liệu
                    </div>`;
            });
    };
})();

// Hiển thị chi tiết sản phẩm trong modal
function showProductDetails(productId) {
    fetch(`/ShopProduct/Details/${productId}`)
        .then(response => response.text())
        .then(html => {
            const modalContent = document.getElementById('productModalContent');
            if (!modalContent) {
                alert('Không tìm thấy modal chi tiết sản phẩm.');
                return;
            }
            modalContent.innerHTML = html;
            var modal = new bootstrap.Modal(document.getElementById('productModal'));
            modal.show();
        })
        .catch(error => {
            console.error('Error loading product details:', error);
            alert('Không thể tải chi tiết sản phẩm.');
        });
}

// Thêm sản phẩm vào giỏ hàng
function addToCart(productId) {
    fetch('/ShopOrder/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams({
            productId: productId,
            quantity: 1
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert(data.message);
            const cartCountEl = document.getElementById('cart-count');
            if (cartCountEl && typeof data.totalItems !== 'undefined') {
                cartCountEl.textContent = data.totalItems;
            }
        } else {
            alert('Lỗi: ' + data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi thêm vào giỏ hàng');
    });
}

// Cập nhật số lượng sản phẩm trong giỏ hàng
function updateQuantity(productId, quantity) {
    quantity = parseInt(quantity);
    if (quantity < 1) {
        if (confirm('Bạn có muốn xóa sản phẩm này khỏi giỏ hàng?')) {
            removeItem(productId);
        } else {
            const qtyEl = document.getElementById('qty_' + productId);
            if (qtyEl) qtyEl.value = 1;
        }
        return;
    }

    fetch('/ShopOrder/UpdateCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams({
            productId: productId,
            quantity: quantity
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert(data.message);
            const cartCountEl = document.getElementById('cart-count');
            if (cartCountEl && typeof data.totalItems !== 'undefined') {
                cartCountEl.textContent = data.totalItems;
            }
            // Reload để cập nhật UI giỏ hàng
            location.reload();
        } else {
            alert('Lỗi: ' + data.message);
            location.reload();
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Có lỗi xảy ra, vui lòng thử lại');
        location.reload();
    });
}

// Xóa sản phẩm khỏi giỏ hàng
function removeItem(productId) {
    if (confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) {
        fetch('/ShopOrder/RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: new URLSearchParams({
                productId: productId
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message);
                const cartCountEl = document.getElementById('cart-count');
                if (cartCountEl && typeof data.totalItems !== 'undefined') {
                    cartCountEl.textContent = data.totalItems;
                }
                // Reload để cập nhật UI giỏ hàng
                location.reload();
            } else {
                alert('Lỗi: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Có lỗi xảy ra, vui lòng thử lại');
        });
    }
}

// Xóa toàn bộ giỏ hàng
function clearCart() {
    if (confirm('Bạn có chắc muốn xóa toàn bộ giỏ hàng?')) {
        fetch('/ShopOrder/ClearCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: new URLSearchParams({})
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message);
                const cartCountEl = document.getElementById('cart-count');
                if (cartCountEl && typeof data.totalItems !== 'undefined') {
                    cartCountEl.textContent = data.totalItems;
                }
                // Reload để cập nhật UI giỏ hàng
                location.reload();
            } else {
                alert('Lỗi: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Có lỗi xảy ra, vui lòng thử lại');
        });
    }
}


