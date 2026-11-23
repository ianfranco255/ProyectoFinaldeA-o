# Changelog

Todos los cambios notables en el proyecto **Asteroid Destroy** se documentan en este archivo siguiendo el formato de versionado semántico.

---

## [0.5.0] - 2026-11-23
### Agregado
* **Base de Datos Ampliada (`db/asteroid_destroy.sql`):** Esquema relacional de 13 tablas normalizadas en 3FN integrando telemetría de combate (`sessions`, `collisionlogs`, `dodgerecords`, `poweruplogs`) y economía de tienda (`ships`, `allies`, `player_ships`, `player_allies`, `gachalogs`).
* **Consultas Analíticas (`db/consultas_analiticas.sql`):** 7 consultas SQL avanzadas con `INNER JOIN`, `LEFT JOIN`, `GROUP BY`, `HAVING`, subconsultas y funciones agregadas.
* **Diagrama Entidad-Relación (`docs/DER_Asteroid_Destroy.drawio`):** Modelo actualizado sincronizado con las restricciones de integridad referencial.
* **Documentación Técnica (`docs/documentacion_tecnica_db_algoritmos.md`):** Justificación formal de normalización (1FN → 3FN), justificación de estructuras de datos en C# y trazas algorítmicas con análisis de complejidad asintótica $O$.
* **Panel Central de Escalas (`Game1.cs`):** Variables globales para escalar de forma independiente los sprites del menú, tienda, HUD y entidades del nivel.

---

## [0.4.0] - 2026-11-18
### Agregado
* **Sistema de Animación Universal (`SpriteAnimado`):** Clase reutilizable para recortar y animar hojas de sprites (*SpriteSheets*) horizontales con soporte para fotograma de reposo y bucle infinito.
* **Interfaz de Tienda por Capas:** Renderizado ordenado en 5 capas de profundidad (fondo espacial, agujero negro animado, marco HUD, pestañas laterales interactivas y panel de categoría).
* **Menú Principal Interactivo:** Navegación híbrida mediante teclado y puntero del mouse con estados de juego (`EstadoJuego`).

### Corregido
* Prevención de selección accidental al iniciar la aplicación mediante detección de desplazamiento relativo del puntero (`_ultimaPosicionMouse`).

---

## [0.3.0] - 2026-11-12
### Agregado
* **HUD de Combate:** Indicador horizontal de 5 vidas (corazones dinámicos) y barra vertical de espacios reservados para Power-Ups en el margen derecho.
* **Sistema de Vidas e Invulnerabilidad:** La nave soporta 5 impactos contra asteroides con parpadeo rojo y 1.5 segundos de inmunidad tras cada colisión.
* **Pantalla Completa y Fondo Dinámico:** Ajuste automático a la resolución nativa del monitor y desplazamiento vertical infinito del fondo espacial.

### Cambiado
* Reasignación de la tecla de disparo a `LeftShift` con modo ráfaga continuo controlado por cadencia (`_cadenciaDisparo`).

---

## [0.2.0] - 2026-11-05
### Agregado
* **Generación y Física de Asteroides:** Aparición aleatoria superior, movimiento diagonal con rebote en bordes laterales y estados visuales de daño por impacto.
* **Sistema de Disparos Múltiples:** Gestión dinámica de proyectiles mediante `List<Bala>` y detección de colisiones euclidianas.
* **Control de Nave:** Rotación angular con flechas (`Left`/`Right`) y propulsión en todas las direcciones (`WASD`) sincronizada con la animación de motores.

---

## [0.1.0] - 2026-10-28
### Agregado
* Migración del prototipo lógico de consola a entorno gráfico **MonoGame (DesktopGL)**.
* Configuración del pipeline de contenido (`Content.mgcb`) y renderizado inicial de sprites PNG exportados desde Pixilart.
* Creación del modelo inicial de base de datos relacional (`asteroid_destroy`) con 8 tablas base y primer Diagrama Entidad-Relación.
