-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 26-08-2026 a las 02:34:43
-- Versión del servidor: 8.0.33
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `asteroid_destroy`
--
CREATE DATABASE IF NOT EXISTS `asteroid_destroy` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE `asteroid_destroy`;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `abilities`
--

DROP TABLE IF EXISTS `abilities`;
CREATE TABLE `abilities` (
  `id` int NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Cooldown` decimal(3,1) UNSIGNED NOT NULL,
  `Damage` int UNSIGNED NOT NULL DEFAULT '0',
  `Consume` int UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `asteroidtypes`
--

DROP TABLE IF EXISTS `asteroidtypes`;
CREATE TABLE `asteroidtypes` (
  `id` int NOT NULL,
  `tamano` decimal(2,1) DEFAULT NULL,
  `velocidad_base` decimal(4,2) UNSIGNED NOT NULL,
  `sprite` varchar(100) NOT NULL,
  `puntos_evasion` int UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `collisionlogs`
--

DROP TABLE IF EXISTS `collisionlogs`;
CREATE TABLE `collisionlogs` (
  `id` int NOT NULL,
  `session_id` int NOT NULL,
  `asteroid_id` int NOT NULL,
  `timestamp` datetime DEFAULT CURRENT_TIMESTAMP,
  `coordenada_impacto` varchar(30) NOT NULL,
  `daño_recibido` int DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `dodgerecords`
--

DROP TABLE IF EXISTS `dodgerecords`;
CREATE TABLE `dodgerecords` (
  `id` int NOT NULL,
  `session_id` int NOT NULL,
  `asteroid_id` int NOT NULL,
  `timestamp` datetime DEFAULT CURRENT_TIMESTAMP,
  `distancia_minima` decimal(5,2) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `players`
--

DROP TABLE IF EXISTS `players`;
CREATE TABLE `players` (
  `id` int NOT NULL,
  `UserName` varchar(30) NOT NULL,
  `HP` int UNSIGNED NOT NULL DEFAULT '100',
  `XP` int UNSIGNED NOT NULL DEFAULT '0',
  `level` int UNSIGNED NOT NULL DEFAULT '1',
  `id_ability` int DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `poweruplogs`
--

DROP TABLE IF EXISTS `poweruplogs`;
CREATE TABLE `poweruplogs` (
  `id` int NOT NULL,
  `session_id` int NOT NULL,
  `powerup_id` int NOT NULL,
  `timestamp_aplicacion` datetime DEFAULT CURRENT_TIMESTAMP,
  `timestamp_fin` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `powerups`
--

DROP TABLE IF EXISTS `powerups`;
CREATE TABLE `powerups` (
  `id` int NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `efecto` varchar(50) NOT NULL,
  `duracion_base` int UNSIGNED NOT NULL,
  `probabilidad_spawn` decimal(5,2) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `sessions`
--

DROP TABLE IF EXISTS `sessions`;
CREATE TABLE `sessions` (
  `id` int NOT NULL,
  `player_id` int DEFAULT NULL,
  `inicio` datetime NOT NULL,
  `fin` datetime DEFAULT NULL,
  `tiempo_supervivencia` int UNSIGNED DEFAULT NULL,
  `dificultad_inicial` decimal(2,1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `abilities`
--
ALTER TABLE `abilities`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `Name` (`Name`);

--
-- Indices de la tabla `asteroidtypes`
--
ALTER TABLE `asteroidtypes`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `collisionlogs`
--
ALTER TABLE `collisionlogs`
  ADD PRIMARY KEY (`id`),
  ADD KEY `session_id` (`session_id`),
  ADD KEY `asteroid_id` (`asteroid_id`);

--
-- Indices de la tabla `dodgerecords`
--
ALTER TABLE `dodgerecords`
  ADD PRIMARY KEY (`id`),
  ADD KEY `session_id` (`session_id`),
  ADD KEY `asteroid_id` (`asteroid_id`);

--
-- Indices de la tabla `players`
--
ALTER TABLE `players`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `UserName` (`UserName`),
  ADD KEY `id_ability` (`id_ability`);

--
-- Indices de la tabla `poweruplogs`
--
ALTER TABLE `poweruplogs`
  ADD PRIMARY KEY (`id`),
  ADD KEY `session_id` (`session_id`),
  ADD KEY `powerup_id` (`powerup_id`);

--
-- Indices de la tabla `powerups`
--
ALTER TABLE `powerups`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre` (`nombre`);

--
-- Indices de la tabla `sessions`
--
ALTER TABLE `sessions`
  ADD PRIMARY KEY (`id`),
  ADD KEY `player_id` (`player_id`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `abilities`
--
ALTER TABLE `abilities`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `asteroidtypes`
--
ALTER TABLE `asteroidtypes`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `collisionlogs`
--
ALTER TABLE `collisionlogs`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `dodgerecords`
--
ALTER TABLE `dodgerecords`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `players`
--
ALTER TABLE `players`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `poweruplogs`
--
ALTER TABLE `poweruplogs`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `powerups`
--
ALTER TABLE `powerups`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `sessions`
--
ALTER TABLE `sessions`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `collisionlogs`
--
ALTER TABLE `collisionlogs`
  ADD CONSTRAINT `collisionlogs_ibfk_1` FOREIGN KEY (`session_id`) REFERENCES `sessions` (`id`),
  ADD CONSTRAINT `collisionlogs_ibfk_2` FOREIGN KEY (`asteroid_id`) REFERENCES `asteroidtypes` (`id`);

--
-- Filtros para la tabla `dodgerecords`
--
ALTER TABLE `dodgerecords`
  ADD CONSTRAINT `dodgerecords_ibfk_1` FOREIGN KEY (`session_id`) REFERENCES `sessions` (`id`),
  ADD CONSTRAINT `dodgerecords_ibfk_2` FOREIGN KEY (`asteroid_id`) REFERENCES `asteroidtypes` (`id`);

--
-- Filtros para la tabla `players`
--
ALTER TABLE `players`
  ADD CONSTRAINT `players_ibfk_1` FOREIGN KEY (`id_ability`) REFERENCES `abilities` (`id`);

--
-- Filtros para la tabla `poweruplogs`
--
ALTER TABLE `poweruplogs`
  ADD CONSTRAINT `poweruplogs_ibfk_1` FOREIGN KEY (`session_id`) REFERENCES `sessions` (`id`),
  ADD CONSTRAINT `poweruplogs_ibfk_2` FOREIGN KEY (`powerup_id`) REFERENCES `powerups` (`id`);

--
-- Filtros para la tabla `sessions`
--
ALTER TABLE `sessions`
  ADD CONSTRAINT `sessions_ibfk_1` FOREIGN KEY (`player_id`) REFERENCES `players` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
