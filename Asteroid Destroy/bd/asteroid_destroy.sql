SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";

CREATE DATABASE IF NOT EXISTS `asteroid_destroy` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `asteroid_destroy`;

DROP TABLE IF EXISTS `gachalogs`;
DROP TABLE IF EXISTS `player_allies`;
DROP TABLE IF EXISTS `player_ships`;
DROP TABLE IF EXISTS `poweruplogs`;
DROP TABLE IF EXISTS `dodgerecords`;
DROP TABLE IF EXISTS `collisionlogs`;
DROP TABLE IF EXISTS `sessions`;
DROP TABLE IF EXISTS `players`;
DROP TABLE IF EXISTS `allies`;
DROP TABLE IF EXISTS `ships`;
DROP TABLE IF EXISTS `powerups`;
DROP TABLE IF EXISTS `asteroidtypes`;
DROP TABLE IF EXISTS `abilities`;

CREATE TABLE `abilities` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  `cooldown` decimal(4,1) UNSIGNED NOT NULL,
  `damage` int UNSIGNED NOT NULL DEFAULT '0',
  `consume` int UNSIGNED NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_abilities_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `asteroidtypes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `tamano` decimal(3,1) UNSIGNED NOT NULL,
  `velocidad_base` decimal(4,2) UNSIGNED NOT NULL,
  `sprite` varchar(100) NOT NULL,
  `puntos_evasion` int UNSIGNED NOT NULL,
  `vida_base` int UNSIGNED NOT NULL DEFAULT '3',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `powerups` (
  `id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) NOT NULL,
  `efecto` varchar(50) NOT NULL,
  `duracion_base` int UNSIGNED NOT NULL,
  `probabilidad_spawn` decimal(5,2) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_powerups_nombre` (`nombre`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `ships` (
  `id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) NOT NULL,
  `velocidad_base` decimal(4,2) UNSIGNED NOT NULL,
  `vidas_maximas` int UNSIGNED NOT NULL DEFAULT '5',
  `cadencia_disparo` decimal(4,2) UNSIGNED NOT NULL,
  `precio_monedas` int UNSIGNED NOT NULL DEFAULT '0',
  `sprite` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_ships_nombre` (`nombre`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `allies` (
  `id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) NOT NULL,
  `tipo_bonificacion` enum('DANO','ESCUDO','MONEDAS','CADENCIA') NOT NULL,
  `valor_bonificacion` decimal(5,2) UNSIGNED NOT NULL,
  `precio_monedas` int UNSIGNED NOT NULL,
  `sprite` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_allies_nombre` (`nombre`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `players` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_name` varchar(30) NOT NULL,
  `hp` int UNSIGNED NOT NULL DEFAULT '5',
  `xp` int UNSIGNED NOT NULL DEFAULT '0',
  `level` int UNSIGNED NOT NULL DEFAULT '1',
  `monedas` int UNSIGNED NOT NULL DEFAULT '500',
  `llaves_gacha` int UNSIGNED NOT NULL DEFAULT '3',
  `id_ability` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_players_username` (`user_name`),
  KEY `idx_players_ability` (`id_ability`),
  CONSTRAINT `fk_players_ability` FOREIGN KEY (`id_ability`) REFERENCES `abilities` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `sessions` (
  `id` int NOT NULL AUTO_INCREMENT,
  `player_id` int NOT NULL,
  `ship_id` int NOT NULL,
  `inicio` datetime NOT NULL,
  `fin` datetime DEFAULT NULL,
  `tiempo_supervivencia` int UNSIGNED DEFAULT '0',
  `dificultad_inicial` decimal(3,1) UNSIGNED NOT NULL DEFAULT '1.0',
  `puntaje_total` int UNSIGNED NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `idx_sessions_player` (`player_id`),
  KEY `idx_sessions_ship` (`ship_id`),
  CONSTRAINT `fk_sessions_player` FOREIGN KEY (`player_id`) REFERENCES `players` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_sessions_ship` FOREIGN KEY (`ship_id`) REFERENCES `ships` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `collisionlogs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `session_id` int NOT NULL,
  `asteroid_id` int NOT NULL,
  `timestamp` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `coordenada_impacto` varchar(30) NOT NULL,
  `dano_recibido` int UNSIGNED NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `idx_collision_session` (`session_id`),
  KEY `idx_collision_asteroid` (`asteroid_id`),
  CONSTRAINT `fk_collision_session` FOREIGN KEY (`session_id`) REFERENCES `sessions` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_collision_asteroid` FOREIGN KEY (`asteroid_id`) REFERENCES `asteroidtypes` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `dodgerecords` (
  `id` int NOT NULL AUTO_INCREMENT,
  `session_id` int NOT NULL,
  `asteroid_id` int NOT NULL,
  `timestamp` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `distancia_minima` decimal(5,2) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_dodge_session` (`session_id`),
  KEY `idx_dodge_asteroid` (`asteroid_id`),
  CONSTRAINT `fk_dodge_session` FOREIGN KEY (`session_id`) REFERENCES `sessions` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_dodge_asteroid` FOREIGN KEY (`asteroid_id`) REFERENCES `asteroidtypes` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `poweruplogs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `session_id` int NOT NULL,
  `powerup_id` int NOT NULL,
  `timestamp_aplicacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `timestamp_fin` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_poweruplogs_session` (`session_id`),
  KEY `idx_poweruplogs_powerup` (`powerup_id`),
  CONSTRAINT `fk_poweruplogs_session` FOREIGN KEY (`session_id`) REFERENCES `sessions` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_poweruplogs_powerup` FOREIGN KEY (`powerup_id`) REFERENCES `powerups` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `player_ships` (
  `id` int NOT NULL AUTO_INCREMENT,
  `player_id` int NOT NULL,
  `ship_id` int NOT NULL,
  `equipada` tinyint(1) NOT NULL DEFAULT '0',
  `fecha_adquisicion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_player_ship` (`player_id`,`ship_id`),
  KEY `idx_pships_ship` (`ship_id`),
  CONSTRAINT `fk_pships_player` FOREIGN KEY (`player_id`) REFERENCES `players` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_pships_ship` FOREIGN KEY (`ship_id`) REFERENCES `ships` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `player_allies` (
  `id` int NOT NULL AUTO_INCREMENT,
  `player_id` int NOT NULL,
  `ally_id` int NOT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT '1',
  `fecha_adquisicion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_player_ally` (`player_id`,`ally_id`),
  KEY `idx_pallies_ally` (`ally_id`),
  CONSTRAINT `fk_pallies_player` FOREIGN KEY (`player_id`) REFERENCES `players` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_pallies_ally` FOREIGN KEY (`ally_id`) REFERENCES `allies` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `gachalogs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `player_id` int NOT NULL,
  `rareza` enum('COMUN','RARO','EPICO','LEGENDARIO') NOT NULL,
  `recompensa_obtenida` varchar(80) NOT NULL,
  `fecha_tirada` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_gacha_player` (`player_id`),
  CONSTRAINT `fk_gacha_player` FOREIGN KEY (`player_id`) REFERENCES `players` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `abilities` (`name`, `cooldown`, `damage`, `consume`) VALUES
('Pulso Electromagnetico', 12.0, 150, 30),
('Sobrecarga de Propulsores', 8.5, 0, 20),
('Rayo Desintegrador', 15.0, 300, 50);

INSERT INTO `asteroidtypes` (`tamano`, `velocidad_base`, `sprite`, `puntos_evasion`, `vida_base`) VALUES
(1.0, 0.80, 'asteroide_pequeno.png', 50, 1),
(2.5, 0.50, 'asteroide_mediano.png', 100, 3),
(4.0, 0.30, 'asteroide_gigante.png', 250, 6);

INSERT INTO `powerups` (`nombre`, `efecto`, `duracion_base`, `probabilidad_spawn`) VALUES
('Escudo Cuantico', 'INMUNIDAD_TOTAL', 10, 15.50),
('Rafaga Doble', 'CADENCIA_X2', 12, 22.00),
('Reparacion Nanobot', 'RESTAURAR_1_HP', 1, 10.00),
('Iman Estelar', 'ATRAER_MONEDAS', 15, 25.00);

INSERT INTO `ships` (`nombre`, `velocidad_base`, `vidas_maximas`, `cadencia_disparo`, `precio_monedas`, `sprite`) VALUES
('Interceptor Base', 1.50, 5, 0.15, 0, 'nave.png'),
('Cazador Nova', 1.85, 6, 0.12, 800, 'nave_nova.png'),
('Acorazado Titan', 1.25, 8, 0.18, 1500, 'nave_titan.png'),
('Espectro Vacio', 2.20, 4, 0.09, 2500, 'nave_espectro.png');

INSERT INTO `allies` (`nombre`, `tipo_bonificacion`, `valor_bonificacion`, `precio_monedas`, `sprite`) VALUES
('Dron Centinela', 'DANO', 25.00, 500, 'aliado_centinela.png'),
('Modulo Escudo', 'ESCUDO', 1.00, 750, 'aliado_escudo.png'),
('Recolector Orbital', 'MONEDAS', 1.30, 600, 'aliado_recolector.png');

INSERT INTO `players` (`user_name`, `hp`, `xp`, `level`, `monedas`, `llaves_gacha`, `id_ability`) VALUES
('AstroPilot', 5, 1450, 4, 1200, 5, 1),
('NovaHunter', 5, 620, 2, 350, 2, 2),
('VoidWalker', 5, 3890, 9, 2900, 8, 3),
('StarDust', 5, 150, 1, 100, 1, NULL);

INSERT INTO `player_ships` (`player_id`, `ship_id`, `equipada`) VALUES
(1, 1, 0),
(1, 2, 1),
(2, 1, 1),
(3, 1, 0),
(3, 3, 0),
(3, 4, 1),
(4, 1, 1);

INSERT INTO `player_allies` (`player_id`, `ally_id`, `activo`) VALUES
(1, 1, 1),
(1, 3, 1),
(3, 2, 1);

INSERT INTO `sessions` (`player_id`, `ship_id`, `inicio`, `fin`, `tiempo_supervivencia`, `dificultad_inicial`, `puntaje_total`) VALUES
(1, 2, '2026-08-26 10:00:00', '2026-08-26 10:03:20', 200, 1.0, 4800),
(1, 2, '2026-08-26 10:15:00', '2026-08-26 10:19:10', 250, 1.5, 6400),
(2, 1, '2026-08-26 11:00:00', '2026-08-26 11:01:45', 105, 1.0, 1950),
(3, 4, '2026-08-26 12:30:00', '2026-08-26 12:35:00', 300, 2.0, 9500),
(4, 1, '2026-08-26 14:00:00', '2026-08-26 14:01:10', 70, 1.0, 1100);

INSERT INTO `collisionlogs` (`session_id`, `asteroid_id`, `timestamp`, `coordenada_impacto`, `dano_recibido`) VALUES
(1, 2, '2026-08-26 10:01:12', '45.2, 60.8', 1),
(2, 3, '2026-08-26 10:17:05', '72.0, 34.5', 2),
(3, 1, '2026-08-26 11:00:50', '20.5, 80.1', 1),
(3, 2, '2026-08-26 11:01:30', '50.0, 85.0', 1),
(5, 3, '2026-08-26 14:00:55', '33.4, 70.2', 2);

INSERT INTO `dodgerecords` (`session_id`, `asteroid_id`, `timestamp`, `distancia_minima`) VALUES
(1, 2, '2026-08-26 10:00:45', 4.15),
(1, 3, '2026-08-26 10:02:10', 4.80),
(2, 1, '2026-08-26 10:16:20', 3.90),
(4, 3, '2026-08-26 12:32:15', 3.65),
(4, 2, '2026-08-26 12:34:00', 4.05);

INSERT INTO `poweruplogs` (`session_id`, `powerup_id`, `timestamp_aplicacion`, `timestamp_fin`) VALUES
(1, 2, '2026-08-26 10:01:30', '2026-08-26 10:01:42'),
(2, 1, '2026-08-26 10:17:20', '2026-08-26 10:17:30'),
(4, 4, '2026-08-26 12:31:00', '2026-08-26 12:31:15'),
(4, 2, '2026-08-26 12:33:10', '2026-08-26 12:33:22');

INSERT INTO `gachalogs` (`player_id`, `rareza`, `recompensa_obtenida`, `fecha_tirada`) VALUES
(1, 'RARO', '500 Monedas Estelares', '2026-08-26 09:50:00'),
(1, 'EPICO', 'Aliado Recolector Orbital', '2026-08-26 09:52:00'),
(3, 'LEGENDARIO', 'Nave Espectro Vacio', '2026-08-26 12:20:00'),
(2, 'COMUN', '100 Monedas Estelares', '2026-08-26 10:55:00');

COMMIT;
