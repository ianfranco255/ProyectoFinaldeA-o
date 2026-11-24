USE `asteroid_destroy`;

-- 1. INNER JOIN + GROUP BY + HAVING + FUNCIONES AGREGADAS (COUNT, SUM, AVG, MAX)
-- Objetivo: Obtener el ranking de jugadores experimentados que hayan acumulado mas de 200 segundos totales de supervivencia.
SELECT 
    p.id AS player_id,
    p.user_name,
    p.level,
    COUNT(s.id) AS total_partidas,
    SUM(s.tiempo_supervivencia) AS segundos_totales_supervivencia,
    ROUND(AVG(s.puntaje_total), 2) AS promedio_puntaje,
    MAX(s.puntaje_total) AS record_puntaje
FROM players p
INNER JOIN sessions s ON p.id = s.player_id
GROUP BY p.id, p.user_name, p.level
HAVING SUM(s.tiempo_supervivencia) > 200
ORDER BY record_puntaje DESC;

-- 2. LEFT JOIN + GROUP BY + COUNT + AVG
-- Objetivo: Auditar todas las naves del catalogo de la tienda, mostrando cuantas veces fue usada cada una y su rendimiento promedio (incluyendo naves nunca usadas).
SELECT 
    sh.id AS ship_id,
    sh.nombre AS nombre_nave,
    sh.precio_monedas,
    COUNT(s.id) AS total_sesiones_jugadas,
    COALESCE(ROUND(AVG(s.puntaje_total), 2), 0) AS puntaje_promedio,
    COALESCE(MAX(s.tiempo_supervivencia), 0) AS max_supervivencia_segundos
FROM ships sh
LEFT JOIN sessions s ON sh.id = s.ship_id
GROUP BY sh.id, sh.nombre, sh.precio_monedas
ORDER BY total_sesiones_jugadas DESC;

-- 3. MULTIPLE INNER JOIN + GROUP BY + HAVING + SUM + MIN
-- Objetivo: Identificar que tipos de asteroides causaron mas de 1 punto de dano acumulado en las partidas y cual fue el impacto minimo registrado.
SELECT 
    a.id AS asteroid_type_id,
    a.sprite,
    a.tamano,
    COUNT(c.id) AS cantidad_colisiones,
    SUM(c.dano_recibido) AS dano_total_infligido,
    MIN(c.dano_recibido) AS dano_minimo_por_choque,
    MAX(c.dano_recibido) AS dano_maximo_por_choque
FROM asteroidtypes a
INNER JOIN collisionlogs c ON a.id = c.asteroid_id
INNER JOIN sessions s ON c.session_id = s.id
GROUP BY a.id, a.sprite, a.tamano
HAVING SUM(c.dano_recibido) > 1
ORDER BY dano_total_infligido DESC;

-- 4. SUBQUERY EN CLAUSULA WHERE + INNER JOIN
-- Objetivo: Listar los jugadores y sus sesiones donde lograron un puntaje superior al promedio global de todas las partidas registradas.
SELECT 
    p.user_name,
    sh.nombre AS nave_utilizada,
    s.id AS session_id,
    s.puntaje_total,
    s.tiempo_supervivencia
FROM players p
INNER JOIN sessions s ON p.id = s.player_id
INNER JOIN ships sh ON s.ship_id = sh.id
WHERE s.puntaje_total > (
    SELECT AVG(puntaje_total)
    FROM sessions
)
ORDER BY s.puntaje_total DESC;

-- 5. SUBQUERY EN CLAUSULA FROM (TABLA DERIVADA) + LEFT JOIN
-- Objetivo: Calcular el balance de evasiones exitosas frente a colisiones sufridas por cada jugador en sus sesiones.
SELECT 
    p.user_name,
    p.monedas,
    COALESCE(metricas_evasion.total_evasiones, 0) AS asteroides_esquivados,
    COALESCE(metricas_evasion.distancia_mas_cercana, 0) AS evasion_mas_arriesgada,
    COALESCE(metricas_choque.total_choques, 0) AS choques_sufridos
FROM players p
LEFT JOIN (
    SELECT 
        s.player_id,
        COUNT(d.id) AS total_evasiones,
        MIN(d.distancia_minima) AS distancia_mas_cercana
    FROM sessions s
    INNER JOIN dodgerecords d ON s.id = d.session_id
    GROUP BY s.player_id
) AS metricas_evasion ON p.id = metricas_evasion.player_id
LEFT JOIN (
    SELECT 
        s.player_id,
        COUNT(c.id) AS total_choques
    FROM sessions s
    INNER JOIN collisionlogs c ON s.id = c.session_id
    GROUP BY s.player_id
) AS metricas_choque ON p.id = metricas_choque.player_id
ORDER BY asteroides_esquivados DESC;

-- 6. INNER JOIN + GROUP BY + HAVING + COUNT
-- Objetivo: Determinar los Power-Ups mas efectivos del juego (aquellos activados al menos 2 veces) y su duracion base configurada.
SELECT 
    pu.nombre AS powerup,
    pu.efecto,
    pu.duracion_base,
    COUNT(pl.id) AS veces_activado
FROM powerups pu
INNER JOIN poweruplogs pl ON pu.id = pl.powerup_id
GROUP BY pu.id, pu.nombre, pu.efecto, pu.duracion_base
HAVING COUNT(pl.id) >= 2
ORDER BY veces_activado DESC;

-- 7. SUBQUERY CORRELACIONADA EN WHERE + LEFT JOIN
-- Objetivo: Consultar el inventario economico de cada jugador (cantidad de naves, aliados y tiradas gacha) solo para jugadores que poseen mas de una nave.
SELECT 
    p.user_name,
    p.monedas,
    p.llaves_gacha,
    (SELECT COUNT(*) FROM player_ships ps WHERE ps.player_id = p.id) AS naves_desbloqueadas,
    (SELECT COUNT(*) FROM player_allies pa WHERE pa.player_id = p.id) AS aliados_adquiridos,
    (SELECT COUNT(*) FROM gachalogs g WHERE g.player_id = p.id) AS tiradas_gacha_realizadas
FROM players p
WHERE (
    SELECT COUNT(*) 
    FROM player_ships ps 
    WHERE ps.player_id = p.id
) > 1
ORDER BY naves_desbloqueadas DESC;
