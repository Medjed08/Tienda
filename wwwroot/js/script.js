// Carrito de compras
let carrito = JSON.parse(localStorage.getItem('carrito')) || [];

// Elementos del DOM
const carritoBtn = document.getElementById('carrito-btn');
const carritoModal = document.getElementById('carrito-modal');
const closeModal = document.querySelector('.close');
const carritoCount = document.getElementById('carrito-count');
const carritoItems = document.getElementById('carrito-items');
const totalElement = document.getElementById('total');
const btnFinalizar = document.getElementById('btn-finalizar');

// Función para guardar carrito en localStorage
function guardarCarrito() {
    localStorage.setItem('carrito', JSON.stringify(carrito));
    console.log('Carrito guardado:', carrito); // Para debugging
}

// Función para agregar producto al carrito
function agregarAlCarrito(nombre, precio, id, color, talla, cantidad) {
    const producto = {
        id: id || Date.now(),
        nombre: nombre,
        precio: parseFloat(precio),
        color: color || null,
        talla: talla || 'Única',
        cantidad: cantidad || 1
    };

    carrito.push(producto);
    guardarCarrito();
    actualizarCarrito();

    let mensaje = `${nombre} agregado al carrito`;
    if (color) {
        mensaje += ` - Color: ${color}`;
    }
    mostrarNotificacion(mensaje);

    console.log('Producto agregado:', producto); // Para debugging
}

// Función para eliminar producto del carrito
function eliminarDelCarrito(id) {
    carrito = carrito.filter(item => item.id != id);
    guardarCarrito();
    actualizarCarrito();
}

// Función para actualizar la visualización del carrito
function actualizarCarrito() {
    carritoCount.textContent = carrito.length;

    if (carrito.length === 0) {
        carritoItems.innerHTML = '<p style="text-align: center; color: #999;">El carrito está vacío</p>';
        totalElement.textContent = '0.00';
        return;
    }

    carritoItems.innerHTML = '';
    let total = 0;

    carrito.forEach(item => {
        total += item.precio;

        const itemDiv = document.createElement('div');
        itemDiv.className = 'carrito-item';
        itemDiv.innerHTML = `
            <div class="carrito-item-info">
                <div class="carrito-item-nombre">
                    ${item.nombre}
                    ${item.color ? `<br><small style="color: #718096;">Color: ${item.color}</small>` : ''}
                    ${item.talla && item.talla !== 'Única' ? `<br><small style="color: #718096;">Talla: ${item.talla}</small>` : ''}
                </div>
                <div class="carrito-item-precio">$${item.precio.toFixed(2)}</div>
            </div>
            <button class="btn-eliminar" onclick="eliminarDelCarrito('${item.id}')">
                Eliminar
            </button>
        `;
        carritoItems.appendChild(itemDiv);
    });

    totalElement.textContent = total.toFixed(2);
}

// Función para mostrar notificación
function mostrarNotificacion(mensaje) {
    const notificacion = document.createElement('div');
    notificacion.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #28a745;
        color: white;
        padding: 15px 25px;
        border-radius: 8px;
        box-shadow: 0 4px 15px rgba(0,0,0,0.2);
        z-index: 2000;
        animation: slideIn 0.3s;
    `;
    notificacion.textContent = mensaje;
    document.body.appendChild(notificacion);

    setTimeout(() => {
        notificacion.style.animation = 'slideOut 0.3s';
        setTimeout(() => notificacion.remove(), 300);
    }, 2000);
}

// Abrir modal del carrito
if (carritoBtn) {
    carritoBtn.addEventListener('click', () => {
        carritoModal.style.display = 'block';
    });
}

// Cerrar modal
if (closeModal) {
    closeModal.addEventListener('click', () => {
        carritoModal.style.display = 'none';
    });
}

// Cerrar modal al hacer click fuera
window.addEventListener('click', (e) => {
    if (e.target === carritoModal) {
        carritoModal.style.display = 'none';
    }
});

// Finalizar compra - Redirigir a checkout
if (btnFinalizar) {
    btnFinalizar.addEventListener('click', () => {
        if (carrito.length === 0) {
            alert('El carrito está vacío');
            return;
        }

        // Redirigir a la página de checkout
        window.location.href = '/Checkout';
    });
}

// Event listener para botones de agregar al carrito
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('btn-agregar-carrito')) {
        const nombre = e.target.getAttribute('data-nombre');
        const precio = e.target.getAttribute('data-precio');
        const id = e.target.getAttribute('data-id');
        const color = e.target.getAttribute('data-color');
        const talla = e.target.getAttribute('data-talla') || 'Única';

        console.log('Datos del botón:', { nombre, precio, id, color, talla }); // Para debugging

        // Validar que se haya seleccionado un color si el producto tiene colores
        const productoCard = e.target.closest('.producto-detalle-card');
        if (productoCard) {
            const colorContainer = productoCard.querySelector('[id^="colores-"]');
            if (colorContainer && (!color || color === '')) {
                mostrarNotificacion('⚠️ Por favor selecciona un color');
                return;
            }
        }

        agregarAlCarrito(nombre, precio, id, color, talla);
    }
});

// Animaciones CSS
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

// Inicializar carrito al cargar la página
document.addEventListener('DOMContentLoaded', () => {
    actualizarCarrito();
});
