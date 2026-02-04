// Base de datos de productos organizada por categorías
const productosDB = {
    deportivas: {
        nombre: "Medias Deportivas",
        descripcion: "Diseñadas especialmente para actividades físicas intensas",
        icono: "🧦",
        productos: [
            {
                id: 1,
                nombre: "Medias Deportivas Básicas",
                descripcion: "Perfectas para entrenar con máximo confort. Material transpirable.",
                precio: 15.99,
                imagen: "🧦",
                colores: ["Negro", "Blanco", "Gris"],
                tallas: ["S", "M", "L", "XL"]
            },
            {
                id: 2,
                nombre: "Medias Deportivas Pro",
                descripcion: "Con compresión graduada para mejor rendimiento deportivo.",
                precio: 22.99,
                imagen: "🧦",
                colores: ["Negro", "Azul", "Rojo"],
                tallas: ["S", "M", "L", "XL"]
            },
            {
                id: 3,
                nombre: "Medias Deportivas Running",
                descripcion: "Acolchado extra en talón y puntera. Antideslizantes.",
                precio: 19.99,
                imagen: "🧦",
                colores: ["Negro", "Blanco", "Naranja"],
                tallas: ["S", "M", "L", "XL"]
            },
            {
                id: 4,
                nombre: "Medias Deportivas Gym",
                descripcion: "Ideales para gimnasio. Máxima ventilación.",
                precio: 17.99,
                imagen: "🧦",
                colores: ["Negro", "Gris", "Verde"],
                tallas: ["M", "L", "XL"]
            }
        ]
    },
    ejecutivas: {
        nombre: "Medias Ejecutivas",
        descripcion: "Elegancia y profesionalismo para tu día a día",
        icono: "👔",
        productos: [
            {
                id: 5,
                nombre: "Medias Ejecutivas Clásicas",
                descripcion: "Elegancia para el día a día profesional. Diseño atemporal.",
                precio: 12.99,
                imagen: "👔",
                colores: ["Negro", "Azul Marino", "Gris"],
                tallas: ["S", "M", "L", "XL"]
            },
            {
                id: 6,
                nombre: "Medias Ejecutivas Premium",
                descripcion: "Tejido de alta calidad. Perfectas para ocasiones formales.",
                precio: 18.99,
                imagen: "👔",
                colores: ["Negro", "Café"],
                tallas: ["M", "L", "XL"]
            },
            {
                id: 7,
                nombre: "Medias Ejecutivas Rayas",
                descripcion: "Diseño con rayas sutiles. Estilo distinguido.",
                precio: 14.99,
                imagen: "👔",
                colores: ["Negro/Gris", "Azul/Celeste"],
                tallas: ["S", "M", "L"]
            },
            {
                id: 8,
                nombre: "Medias Ejecutivas Lisas",
                descripcion: "100% algodón peinado. Comodidad superior.",
                precio: 13.99,
                imagen: "👔",
                colores: ["Negro", "Azul", "Gris", "Café"],
                tallas: ["S", "M", "L", "XL"]
            }
        ]
    },
    casuales: {
        nombre: "Medias Casuales",
        descripcion: "Diversión y color para tu estilo personal",
        icono: "🌈",
        productos: [
            {
                id: 9,
                nombre: "Medias Casuales Coloridas",
                descripcion: "Colores y diseños divertidos para uso diario.",
                precio: 9.99,
                imagen: "🌈",
                colores: ["Multicolor", "Arcoíris", "Estampadas"],
                tallas: ["S", "M", "L", "XL"]
            },
            {
                id: 10,
                nombre: "Medias Casuales Rayas",
                descripcion: "Diseño a rayas horizontales. Estilo retro.",
                precio: 10.99,
                imagen: "🌈",
                colores: ["Rojo/Blanco", "Azul/Blanco", "Verde/Amarillo"],
                tallas: ["M", "L", "XL"]
            },
            {
                id: 11,
                nombre: "Medias Casuales Dibujos",
                descripcion: "Con divertidos diseños y patrones únicos.",
                precio: 11.99,
                imagen: "🌈",
                colores: ["Varios diseños"],
                tallas: ["S", "M", "L"]
            },
            {
                id: 12,
                nombre: "Medias Casuales Lisas Colores",
                descripcion: "Colores vibrantes para combinar con tu outfit.",
                precio: 8.99,
                imagen: "🌈",
                colores: ["Rojo", "Azul", "Verde", "Amarillo", "Rosa"],
                tallas: ["S", "M", "L", "XL"]
            }
        ]
    },
    termicas: {
        nombre: "Medias Térmicas",
        descripcion: "Máximo calor para los días más fríos",
        icono: "❄️",
        productos: [
            {
                id: 13,
                nombre: "Medias Térmicas Básicas",
                descripcion: "Calor extremo para días fríos. Interior afelpado.",
                precio: 18.99,
                imagen: "❄️",
                colores: ["Negro", "Gris", "Café"],
                tallas: ["M", "L", "XL"]
            },
            {
                id: 14,
                nombre: "Medias Térmicas Extreme",
                descripcion: "Doble capa térmica. Para frío intenso.",
                precio: 25.99,
                imagen: "❄️",
                colores: ["Negro", "Gris Oscuro"],
                tallas: ["L", "XL"]
            },
            {
                id: 15,
                nombre: "Medias Térmicas Lana",
                descripcion: "Mezcla de lana merino. Calidez natural.",
                precio: 29.99,
                imagen: "❄️",
                colores: ["Café", "Gris", "Negro"],
                tallas: ["S", "M", "L", "XL"]
            },
            {
                id: 16,
                nombre: "Medias Térmicas Ski",
                descripcion: "Especiales para deportes de invierno.",
                precio: 32.99,
                imagen: "❄️",
                colores: ["Negro", "Rojo", "Azul"],
                tallas: ["M", "L", "XL"]
            }
        ]
    },
    premium: {
        nombre: "Medias Premium",
        descripcion: "La máxima calidad en medias de lujo",
        icono: "✨",
        productos: [
            {
                id: 17,
                nombre: "Medias Premium Bambú",
                descripcion: "Fibra de bambú ultra suave. Antibacteriales.",
                precio: 24.99,
                imagen: "✨",
                colores: ["Negro", "Beige", "Gris"],
                tallas: ["S", "M", "L", "XL"]
            },
            {
                id: 18,
                nombre: "Medias Premium Seda",
                descripcion: "Mezcla con seda natural. Suavidad excepcional.",
                precio: 34.99,
                imagen: "✨",
                colores: ["Negro", "Blanco Perla"],
                tallas: ["M", "L"]
            },
            {
                id: 19,
                nombre: "Medias Premium Cashmere",
                descripcion: "Toque de cashmere. Lujo absoluto.",
                precio: 39.99,
                imagen: "✨",
                colores: ["Gris", "Café", "Negro"],
                tallas: ["M", "L", "XL"]
            },
            {
                id: 20,
                nombre: "Medias Premium Algodón Egipcio",
                descripcion: "100% algodón egipcio de fibra larga.",
                precio: 27.99,
                imagen: "✨",
                colores: ["Negro", "Blanco", "Azul Marino"],
                tallas: ["S", "M", "L", "XL"]
            }
        ]
    },
    running: {
        nombre: "Medias Running",
        descripcion: "Tecnología avanzada para corredores exigentes",
        icono: "🏃",
        productos: [
            {
                id: 21,
                nombre: "Medias Running Pro",
                descripcion: "Tecnología anti-ampollas. Soporte de arco.",
                precio: 21.99,
                imagen: "🏃",
                colores: ["Negro", "Blanco", "Neón"],
                tallas: ["S", "M", "L", "XL"]
            },
            {
                id: 22,
                nombre: "Medias Running Compresión",
                descripcion: "Compresión media. Mejora circulación.",
                precio: 26.99,
                imagen: "🏃",
                colores: ["Negro", "Azul"],
                tallas: ["M", "L", "XL"]
            },
            {
                id: 23,
                nombre: "Medias Running Trail",
                descripcion: "Para terrenos irregulares. Refuerzo extra.",
                precio: 24.99,
                imagen: "🏃",
                colores: ["Negro/Naranja", "Gris/Verde"],
                tallas: ["M", "L", "XL"]
            },
            {
                id: 24,
                nombre: "Medias Running Maratón",
                descripcion: "Diseñadas para largas distancias.",
                precio: 28.99,
                imagen: "🏃",
                colores: ["Negro", "Blanco"],
                tallas: ["S", "M", "L", "XL"]
            }
        ]
    }
};