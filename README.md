# Asteroid Destroy

Videojuego arcade espacial 2D desarrollado en **C#** utilizando el framework **MonoGame**, con persistencia de telemetría, inventario y economía virtual gestionada mediante **MySQL**.

---

## Descripción del Proyecto

**Asteroid Destroy** combina mecánicas clásicas de combate y evasión espacial con una interfaz gráfica estructurada por estados y renderizado por capas (*Layered Rendering*). El sistema registra métricas en tiempo real (sesiones de juego, colisiones, evasiones cercanas, uso de power-ups y transacciones en la tienda) sobre una base de datos relacional normalizada hasta Tercera Forma Normal (3FN).

### Características Principales
* **Sistema de Navegación por Estados:** Menú principal interactivo, selector de Tienda por capas (Naves, Gacha, Aliados, Llaves), pantalla de Opciones y Nivel 1 jugable.
* **Motor de Animación Modular (`SpriteAnimado`):** Soporte nativo para hojas de sprites (*SpriteSheets*) creadas en Pixilart con control independiente de fotogramas y escala global.
* **Combate y Física:** Rotación independiente, desplazamiento omnidireccional, disparo en ráfaga continuo, rebote de asteroides, sistema de 5 vidas con invulnerabilidad temporal y HUD dinámico.
* **Persistencia Relacional (MySQL):** Esquema de 13 tablas con integridad referencial para auditoría de partidas y análisis SQL.

---

## Stack Tecnológico

* **Lenguaje:** C# (.NET 8.0)
* **Motor / Framework Gráfico:** MonoGame (`MonoGame.Framework.DesktopGL`)
* **Base de Datos:** MySQL 8.0 (InnoDB, `utf8mb4_unicode_ci`)
* **Conector de Datos:** `MySqlConnector`
* **Diseño de Assets y Diagramas:** Pixilart (Spritesheets PNG), Draw.io (DER)
* **Control de Versiones:** Git / GitHub (Flujo basado en `main`, `develop` y ramas `feature/*`)

---

## Estructura del Repositorio

```text
Asteroid-Destroy/
├── src/                        # Código fuente del videojuego en C# (MonoGame)
│   ├── Content/                # Pipeline de contenido (.mgcb) y recursos compilados
│   ├── Game1.cs                # Bucle principal, estados de pantalla, físicas y renderizado
│   ├── Program.cs              # Punto de entrada de la aplicación
│   └── Asteroid Destroy.csproj # Configuración y dependencias del proyecto .NET
├── db/                         # Scripts de Base de Datos MySQL
│   ├── asteroid_destroy_ampliado.sql  # Creación de esquema (13 tablas) y datos de prueba
│   └── consultas_analiticas.sql       # Consultas con JOIN, GROUP BY, HAVING y SUBQUERIES
├── docs/                       # Documentación técnica y académica
│   ├── DER_Asteroid_Destroy.drawio            # Diagrama Entidad-Relación editable
│   └── documentacion_tecnica_db_algoritmos.md # Justificación 1FN-3FN y trazas algorítmicas
├── assets/                     # Recursos gráficos originales (SpriteSheets PNG de Pixilart)
├── README.md                   # Documentación general del proyecto
└── CHANGELOG.md                # Historial cronológico de versiones y cambios
```

---

## Instrucciones de Instalación y Ejecución

### 1. Configurar la Base de Datos (MySQL)
1. Asegúrate de tener activo un servidor **MySQL 8.0** (vía XAMPP, Laragon, Docker o MySQL Workbench).
2. Importa y ejecuta el archivo `db/asteroid_destroy_ampliado.sql` para crear la base de datos `asteroid_destroy`, sus 13 tablas y los registros iniciales de prueba.
3. *(Opcional)* Ejecuta `db/consultas_analiticas.sql` para verificar las métricas y reportes analíticos.

### 2. Compilar y Ejecutar el Videojuego (MonoGame)
1. Clona este repositorio en tu equipo:
   ```bash
   git clone <URL_DEL_REPOSITORIO>
   cd Asteroid-Destroy/src
   ```
2. Restaura las dependencias de NuGet y compila el proyecto:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Ejecuta el juego desde la terminal o abriendo la solución en **Visual Studio 2022**:
   ```bash
   dotnet run
   ```

---

## Controles del Juego

| Contexto | Tecla / Acción | Función |
| :--- | :--- | :--- |
| **Menú y Tienda** | `Flechas Arriba / Abajo` o `Mouse` | Navegar entre opciones |
| **Menú y Tienda** | `Enter`, `Espacio` o `Clic Izquierdo` | Confirmar selección |
| **Nivel 1 (Combate)** | `W`, `A`, `S`, `D` | Desplazamiento omnidireccional de la nave |
| **Nivel 1 (Combate)** | `Flechas Izquierda / Derecha` | Rotar ángulo de la nave |
| **Nivel 1 (Combate)** | `Shift Izquierdo` (Mantener) | Disparo continuo en ráfaga |
| **Global** | `ESC` / `Retroceso` | Volver al menú anterior o salir |
