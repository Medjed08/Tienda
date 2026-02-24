// Obtener parámetro de categoría de la URL
function obtenerCategoria() {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('cat');
}

// Cargar productos de la categoría
function cargarCategoria() {
    const categoriaParam = obtenerCategoria();

    if (!categoriaParam || !productosDB[categoriaParam]) {
        window.location.href = 'index.html';
        return;
    }

    const categoria = productosDB[categoriaParam];

    // Actualizar título y descripción
    document.getElementById('categoria-titulo').textContent = categoria.nombre;
    document.getElementById('categoria-descripcion').textContent = categoria.descripcion;
    document.getElementById('categoria-icono').textContent = categoria.icono;
    document.getElementById('categoria-nombre').textContent = categoria.nombre;
    document.title = `${categoria.nombre} - MediaStyle`;

    // Cargar productos
    const productosLista = document.getElementById('productos-lista');
    productosLista.innerHTML = '';

    categoria.productos.forEach(producto => {
        const productoCard = crearProductoCard(producto);
        productosLista.appendChild(productoCard);
    });
}

// Crear tarjeta de producto
function crearProductoCard(producto) {
    const card = document.createElement('div');
    card.className = 'producto-detalle-card';

    // Crear HTML de colores
    const coloresHTML = producto.colores.map(color =>
        `<span class="color-badge">${color}</span>`
    ).join('');

    // Crear HTML de tallas
    const tallasHTML = producto.tallas.map(talla =>
        `<span class="talla-badge">${talla}</span>`
    ).join('');

    card.innerHTML = `
        <div class="producto-detalle-img">${producto.imagen}</div>
        <div class="producto-detalle-info">
            <h3>${producto.nombre}</h3>
            <p class="producto-descripcion">${producto.descripcion}</p>
            
            <div class="producto-opciones">
                <div class="opcion-grupo">
                    <label>Colores disponibles:</label>
                    <div class="opciones-lista">
                        ${coloresHTML}
                    </div>
                </div>
                
                <div class="opcion-grupo">
                    <label>Tallas disponibles:</label>
                    <div class="opciones-lista">
                        ${tallasHTML}
                    </div>
                </div>
            </div>
            
            <div class="producto-footer">
                <p class="precio-grande">$${producto.precio.toFixed(2)}</p>
                <button class="btn-agregar-carrito" 
                        data-id="${producto.id}"
                        data-nombre="${producto.nombre}" 
                        data-precio="${producto.precio}">
                    Agregar al Carrito
                </button>
            </div>
        </div>
    `;

    return card;
}

// Event listener para botones de agregar al carrito
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('btn-agregar-carrito')) {
        const nombre = e.target.getAttribute('data-nombre');
        const precio = e.target.getAttribute('data-precio');
        const id = e.target.getAttribute('data-id');

        agregarAlCarrito(nombre, precio, id);
    }
});

// Cargar categoría cuando la página esté lista
document.addEventListener('DOMContentLoaded', cargarCategoria);