# Documentación Técnica: Normalización de Base de Datos y Análisis Algorítmico
**Proyecto:** Asteroid Destroy  
**Tecnologías:** C# (.NET / MonoGame) + MySQL 8.0  

---

## 1. Justificación de Normalización (1FN → 3FN)

El diseño de la base de datos `asteroid_destroy` está compuesto por **13 tablas** normalizadas hasta la **Tercera Forma Normal (3FN)**. A continuación, se detalla el proceso formal de normalización aplicado sobre las entidades del juego para eliminar redundancias y anomalías de inserción, actualización y borrado.

### 1.1. Primera Forma Normal (1FN)
**Regla:** Cada celda de una tabla debe contener un único valor atómico (indivisible) y no deben existir grupos repetitivos ni arreglos dentro de un mismo atributo.

* **Aplicación en el modelo:**
  * **Separación de inventarios (Relaciones N:M):** En lugar de almacenar una lista de naves o aliados separados por comas dentro de la tabla `players` (ej. `naves_desbloqueadas = "1,2,4"`), se crearon las tablas asociativas **`player_ships`** y **`player_allies`**. Cada fila representa una única relación atómica entre un jugador y un ítem.
  * **Atomicidad en telemetría (`sessions`, `collisionlogs`, `dodgerecords`):** Cada evento ocurrido durante una partida (una colisión, una evasión cercana o la activación de un power-up) se registra como una fila independiente en su respectiva tabla de log en lugar de acumular múltiples eventos en un campo de texto dentro de `sessions`.
  * **Descomposición de atributos:** En `asteroidtypes`, las propiedades físicas (`tamano`, `velocidad_base`, `puntos_evasion`, `vida_base`) están separadas en columnas individuales con tipos de datos estrictos (`DECIMAL`, `INT`).

### 1.2. Segunda Forma Normal (2FN)
**Regla:** La tabla debe estar en 1FN y todos los atributos no clave deben depender funcionalmente de la totalidad de la clave primaria (eliminación de dependencias parciales).

* **Aplicación en el modelo:**
  * **Claves primarias subrogadas y restricciones únicas:** Todas las tablas utilizan una clave primaria simple autoincremental (`id`), mientras que en las tablas puente (`player_ships` y `player_allies`) se impusieron restricciones `UNIQUE(player_id, ship_id)` y `UNIQUE(player_id, ally_id)`.
  * **Dependencia completa:** Atributos como `equipada` o `fecha_adquisicion` en `player_ships` dependen exclusivamente de la combinación completa del jugador y la nave específica, y no únicamente de `player_id` o de `ship_id` por separado.
  * **Separación de catálogos:** Los datos propios de una nave (`nombre`, `velocidad_base`, `vidas_maximas`, `precio_monedas`) residen exclusivamente en `ships` y no en `player_ships` ni en `sessions`, evitando que se repitan por cada partida jugada.

### 1.3. Tercera Forma Normal (3FN)
**Regla:** La tabla debe estar en 2FN y ningún atributo no clave debe depender transitivamente de la clave primaria a través de otro atributo no clave.

* **Aplicación en el modelo:**
  * **Eliminación de dependencias transitivas en `players`:** El jugador posee una habilidad equipada referenciada mediante `id_ability` (Clave Foránea). Los atributos descriptivos de dicha habilidad (`name`, `cooldown`, `damage`, `consume`) se almacenan únicamente en `abilities`. Si `damage` estuviera en `players`, dependería transitivamente de `id` a través de `id_ability`.
  * **Eliminación de dependencias transitivas en logs (`collisionlogs`, `dodgerecords`, `poweruplogs`):** En `collisionlogs` se almacena `asteroid_id` (FK), pero no `tamano` ni `velocidad_base` del asteroide, ya que estos dependen exclusivamente de `asteroidtypes.id`. De igual modo, `poweruplogs` referencia `powerup_id` sin duplicar `efecto` ni `duracion_base`.
  * **Integridad en `sessions`:** Una sesión registra `player_id` y `ship_id`. Las estadísticas base de la nave (`cadencia_disparo`, `velocidad_base`) permanecen en `ships`, garantizando que una actualización de balanceo en el catálogo no genere inconsistencias en los registros históricos.

---

## 2. Estructuras de Datos Utilizadas en C# (MonoGame)

Para cumplir con los requerimientos algorítmicos y de rendimiento en tiempo real (60 FPS), el motor del juego implementa estructuras estáticas y dinámicas seleccionadas según su patrón de acceso:

| Estructura | Implementación en `Game1.cs` | Justificación Técnica |
| :--- | :--- | :--- |
| **Arreglo Estático (`[]`)** | `SpriteAnimado[] _animBotonesMenu` (tamaño 4)<br>`Rectangle[] _rectangulosBotonesTienda` (tamaño 5) | La cantidad de opciones en los menús es fija y conocida en tiempo de compilación. Permite acceso directo por índice en tiempo constante $O(1)$ sin sobrecarga de memoria dinámica. |
| **Registro (`struct`)** | `Vector2Virtual`, `Nave`, `Bala`, `Asteroide`, `PowerUpActivo` | Almacenados en memoria contigua (Stack / Arrays internos de listas), evitando la recolección de basura (*Garbage Collection*) frecuente durante el bucle de actualización (`Update`). |
| **Lista Dinámica (`List<T>`)** | `List<Bala> _listaBalas`<br>`List<Asteroide> _listaAsteroides`<br>`List<PowerUpActivo> _powerUpsActivos` | El número de proyectiles y enemigos varía constantemente durante la partida (creación por ráfaga con `LeftShift` y destrucción por colisión). Permite inserción amortizada $O(1)$ y recorrido secuencial rápido. |

---

## 3. Trazas Algorítmicas y Análisis de Complejidad ($O$)

### 3.1. Algoritmo 1: Detección de Colisiones Bala–Asteroide y Nave–Asteroide
**Ubicación:** `ActualizarNivel1(float deltaTiempo, KeyboardState estadoTecladoActual)`  
**Propósito:** Evaluar en cada fotograma si algún proyectil activo impacta contra un asteroide o si un asteroide colisiona con la nave del jugador mediante distancia euclidiana entre radios de colisión.

#### Pseudocódigo / Lógica Crítica
```csharp
for (int i = 0; i < _listaAsteroides.Count; i++) // N asteroides
{
    Asteroide ast = _listaAsteroides[i];
    // 1. Verificar colisión contra cada bala activa (M balas)
    for (int j = 0; j < _listaBalas.Count; j++)
    {
        Bala b = _listaBalas[j];
        float distancia = Math.Sqrt(Math.Pow(b.Posicion.X - ast.Posicion.X, 2) + Math.Pow(b.Posicion.Y - ast.Posicion.Y, 2));
        if (distancia < ast.RadioColision)
        {
            ast.Vida--;
            b.Activa = false;
            if (ast.Vida <= 0) { ast.Activo = false; break; }
        }
    }
    // 2. Verificar colisión contra la nave (1 jugador)
    if (ast.Activo) { /* Cálculo de distancia Nave-Asteroide */ }
}
_listaBalas.RemoveAll(b => !b.Activa);
_listaAsteroides.RemoveAll(a => !a.Activo);
```

#### Traza Algorítmica de Ejecución (Caso de Prueba)
Supongamos un instante $t$ donde existen **2 asteroides** ($A_0, A_1$) y **2 balas** ($B_0, B_1$):
* **Estado inicial:**
  * $A_0$: `Pos(50, 40)`, `Vida = 1`, `Radio = 4.0`, `Activo = true`
  * $A_1$: `Pos(20, 15)`, `Vida = 3`, `Radio = 4.0`, `Activo = true`
  * $B_0$: `Pos(51, 41)`, `Activa = true`
  * $B_1$: `Pos(80, 70)`, `Activa = true`
  * `Jugador`: `Pos(50, 85)`, `Radio = 3.5`, `Vidas = 5`

| Iteración `i` (Asteroide) | Iteración `j` (Bala) | Distancia calculada ($d$) | Condición ($d < 4.0$) | Cambio de Estado |
| :---: | :---: | :---: | :---: | :--- |
| `i = 0` ($A_0$) | `j = 0` ($B_0$) | $\sqrt{(51-50)^2 + (41-40)^2} = 1.41$ | **Verdadero** | $A_0.	ext{Vida} \leftarrow 0$; $B_0.	ext{Activa} \leftarrow 	ext{false}$; $A_0.	ext{Activo} \leftarrow 	ext{false}$; `break` |
| `i = 0` ($A_0$) | *(Check Nave)* | No se ejecuta (`ast.Activo == false`) | — | Se omite chequeo con jugador |
| `i = 1` ($A_1$) | `j = 0` ($B_0$) | No se ejecuta (`b.Activa == false`) | — | Se omite bala desactivada |
| `i = 1` ($A_1$) | `j = 1` ($B_1$) | $\sqrt{(80-20)^2 + (70-15)^2} = 81.39$ | **Falso** | Sin cambios |
| `i = 1` ($A_1$) | *(Check Nave)* | $\sqrt{(50-20)^2 + (85-15)^2} = 76.15$ | **Falso** ($76.15 
less 7.5$) | Jugador conserva sus `5` vidas |
| **Limpieza** | `RemoveAll` | — | — | Se eliminan $A_0$ y $B_0$ en una sola pasada lineal |

#### Análisis de Complejidad Asintótica
* **Complejidad Temporal:**
  * **Peor caso:** $O(N \cdot M)$, donde $N$ es el número de asteroides activos y $M$ es el número de balas activas. Dado que la cadencia de disparo (`0.15s`) y el tiempo de aparición de asteroides (`2.0s`) acotan ambos valores a conjuntos pequeños ($N \le 15, M \le 25$), el costo real por fotograma es despreciable ($\le 375$ iteraciones simples).
  * **Limpieza (`RemoveAll`):** $O(N + M)$, compactando las listas en memoria en una única pasada lineal sin reallocaciones.
* **Complejidad Espacial:** $O(1)$ auxiliar, ya que las estructuras `struct` se modifican por valor sobre las listas preexistentes sin instanciar nuevos objetos en el *Heap*.

---

### 3.2. Algoritmo 2: Recorte Dinámico de Fotogramas (`SpriteAnimado`)
**Ubicación:** `SpriteAnimado.Actualizar(float deltaTiempo)` y `ObtenerCuadroRecorte()`  
**Propósito:** Calcular en tiempo real la región rectangular (`Rectangle`) correspondiente al fotograma actual dentro de una hoja de sprites horizontal (*SpriteSheet*).

#### Traza Algorítmica (Nave con 9 fotogramas, ancho total `288px`, alto `32px`, `TiempoPorFotograma = 0.08s`)
| Paso / Llamada | `deltaTiempo` | `Cronometro` | `FotogramaActual` | `ObtenerCuadroRecorte()` `(X, Y, W, H)` |
| :--- | :---: | :---: | :---: | :---: |
| Inicio (`Reiniciar()`) | `0.00s` | `0.00s` | `0` (Quieto) | `(0, 0, 32, 32)` |
| Frame 1 (Tecla `W` pulsada) | `0.05s` | `0.05s` | `0` | `(0, 0, 32, 32)` |
| Frame 2 (Tecla `W` pulsada) | `0.04s` | `0.09s` $ightarrow$ `0.00s` | `1` (Fuego 1) | `(32, 0, 32, 32)` |
| ... *(Avance continuo)* | — | — | `8` (Último) | `(256, 0, 32, 32)` |
| Frame N (Supera límite `9`) | `0.08s` | `0.00s` | `1` (`FotogramaInicioBucle`) | `(32, 0, 32, 32)` |

#### Análisis de Complejidad Asintótica
* **Complejidad Temporal:** $O(1)$ constante. Tanto la actualización del temporizador como el cálculo aritmético del rectángulo de recorte (`FotogramaActual * anchoFrame`) requieren un número fijo de operaciones básicas independientemente de la resolución de la textura o la cantidad de fotogramas.
* **Complejidad Espacial:** $O(1)$ constante por instancia de sprite.

---

### 3.3. Algoritmo 3: Navegación Interactiva Híbrida (Teclado + Puntero)
**Ubicación:** `ActualizarMenuPrincipal` y `ActualizarTienda`  
**Propósito:** Sincronizar la selección visual de opciones mediante detección de movimiento relativo del mouse (`posMouseActual != _ultimaPosicionMouse`) y eventos de teclado sin colisiones de estado.

#### Análisis de Complejidad Asintótica
* **Complejidad Temporal:** $O(K)$, donde $K$ es el número de botones en pantalla ($K = 4$ en el Menú Principal, $K = 5$ en la Tienda). Al ser $K$ una constante pequeña, el tiempo de ejecución es efectivamente $O(1)$.
* **Complejidad Espacial:** $O(1)$ constante.
