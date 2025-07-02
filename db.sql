-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 02-07-2025 a las 02:45:39
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `db`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `equipos`
--

CREATE TABLE `equipos` (
  `id` int(11) NOT NULL,
  `codigo` varchar(50) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `descripcion` text DEFAULT NULL,
  `proveedor` varchar(100) DEFAULT NULL,
  `fecha_mantenimiento` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish2_ci;

--
-- Volcado de datos para la tabla `equipos`
--

INSERT INTO `equipos` (`id`, `codigo`, `nombre`, `descripcion`, `proveedor`, `fecha_mantenimiento`) VALUES
(1, '1', 'Condones', 'Son condones', 'Jony', '2025-07-19');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inventario`
--

CREATE TABLE `inventario` (
  `id` int(11) NOT NULL,
  `codigo_pieza` varchar(50) NOT NULL,
  `nombre_pieza` varchar(100) NOT NULL,
  `descripcion` text DEFAULT NULL,
  `cantidad` int(11) NOT NULL DEFAULT 0,
  `proveedor` varchar(100) DEFAULT NULL,
  `fecha_ultima_actualizacion` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish2_ci;

--
-- Volcado de datos para la tabla `inventario`
--

INSERT INTO `inventario` (`id`, `codigo_pieza`, `nombre_pieza`, `descripcion`, `cantidad`, `proveedor`, `fecha_ultima_actualizacion`) VALUES
(3, '1', 'Nombre por definir', NULL, 2, NULL, '2025-06-28 18:37:00'),
(4, '2', 'Nombre por definir', NULL, 600, NULL, '2025-06-28 18:37:22'),
(5, '342', 'Nombre por definir', NULL, 138, NULL, '2025-07-01 15:56:22'),
(6, '69', 'Nombre por definir', NULL, 4, NULL, '2025-07-01 16:28:43');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `mantenimientos`
--

CREATE TABLE `mantenimientos` (
  `id` int(11) NOT NULL,
  `orden_id` int(11) NOT NULL,
  `tecnico_id` int(11) NOT NULL,
  `tipo_mantenimiento` enum('rutinario','cambio_pieza') NOT NULL,
  `observaciones` text DEFAULT NULL,
  `fecha` date NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish2_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `movimientos`
--

CREATE TABLE `movimientos` (
  `id` int(11) NOT NULL,
  `pieza_id` int(11) NOT NULL,
  `tipo_movimiento` enum('Entrada','Salida') NOT NULL,
  `cantidad` int(11) NOT NULL,
  `fecha_movimiento` date NOT NULL,
  `descripcion` text NOT NULL,
  `usuario_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish2_ci;

--
-- Volcado de datos para la tabla `movimientos`
--

INSERT INTO `movimientos` (`id`, `pieza_id`, `tipo_movimiento`, `cantidad`, `fecha_movimiento`, `descripcion`, `usuario_id`) VALUES
(1, 3, 'Entrada', 1, '0000-00-00', 'Entrada', 2),
(2, 4, 'Entrada', 300, '0000-00-00', 'Entrada', 4),
(3, 5, 'Entrada', 69, '0000-00-00', 'Lubricante de menores', 5),
(4, 6, 'Entrada', 4, '0000-00-00', 'Aceite', 2),
(5, 6, 'Salida', 4, '0000-00-00', 'Retirar', 4);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `notificaciones`
--

CREATE TABLE `notificaciones` (
  `id` int(11) NOT NULL,
  `receptor` varchar(20) DEFAULT NULL,
  `emisor` varchar(20) DEFAULT NULL,
  `tipo` enum('Confirmacion','SolicitarPieza','StockBajo','Aviso') DEFAULT NULL,
  `mensaje` text DEFAULT NULL,
  `leido` tinyint(1) DEFAULT 0,
  `fecha` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish2_ci;

--
-- Volcado de datos para la tabla `notificaciones`
--

INSERT INTO `notificaciones` (`id`, `receptor`, `emisor`, `tipo`, `mensaje`, `leido`, `fecha`) VALUES
(4, '111', '30933082', 'Aviso', 'Se te ha asignado una nueva orden de mantenimiento para el equipo 1.\nDescripción: Hazloooo', 1, '2025-07-02 00:15:45'),
(5, '30933082', '111', 'SolicitarPieza', 'El técnico 111 necesita una pieza para el mantenimiento del equipo 1 (orden #9).', 1, '2025-07-02 00:16:44'),
(6, '30933082', '111', 'Confirmacion', 'El usuario 111 ha leído tu mensaje (ID 4).', 1, '2025-07-02 00:17:21'),
(7, '111', '30933082', 'Confirmacion', 'El usuario 30933082 ha leído tu mensaje (ID 5).', 1, '2025-07-02 00:18:08'),
(8, '30933083', 'Sistema', 'StockBajo', 'La pieza \'Nombre por definir\' (código: 1) tiene solo 2 unidades disponibles. Se requiere reposición.', 0, '2025-07-02 00:27:05'),
(9, '30933083', 'Sistema', 'StockBajo', 'La pieza \'Nombre por definir\' (código: 69) tiene solo 4 unidades disponibles. Se requiere reposición.', 0, '2025-07-02 00:27:05');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `ordenesmantenimiento`
--

CREATE TABLE `ordenesmantenimiento` (
  `id` int(11) NOT NULL,
  `equipo_id` int(50) NOT NULL,
  `tecnico_id` int(50) NOT NULL,
  `supervisor_id` int(50) NOT NULL,
  `tipo_mantenimiento` enum('Rutinario','Cambio de Pieza','Por Revisar') NOT NULL,
  `descripcion` text NOT NULL,
  `fecha_creacion` datetime NOT NULL,
  `estado` enum('Pendiente','En Proceso','Finalizada') NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish2_ci;

--
-- Volcado de datos para la tabla `ordenesmantenimiento`
--

INSERT INTO `ordenesmantenimiento` (`id`, `equipo_id`, `tecnico_id`, `supervisor_id`, `tipo_mantenimiento`, `descripcion`, `fecha_creacion`, `estado`) VALUES
(6, 1, 111, 30933082, 'Rutinario', 'lol', '2025-07-01 19:00:36', 'Finalizada'),
(7, 1, 111, 30933082, 'Cambio de Pieza', 'prueba 1', '2025-07-01 19:13:42', 'En Proceso'),
(8, 1, 111, 30933082, 'Cambio de Pieza', 'hazlo cabron', '2025-07-01 19:57:11', 'En Proceso'),
(9, 1, 111, 30933082, 'Cambio de Pieza', 'Hazloooo', '2025-07-01 20:15:45', 'En Proceso');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `roles`
--

CREATE TABLE `roles` (
  `id` int(11) UNSIGNED NOT NULL,
  `nombre_rol` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish2_ci;

--
-- Volcado de datos para la tabla `roles`
--

INSERT INTO `roles` (`id`, `nombre_rol`) VALUES
(1, 'admin'),
(2, 'tecnico'),
(3, 'supervisor');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `correo` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `telefono` varchar(30) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `documento_id` int(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish2_ci;

--
-- Volcado de datos para la tabla `usuarios`
--

INSERT INTO `usuarios` (`id`, `nombre`, `correo`, `telefono`, `documento_id`) VALUES
(2, 'Santiago Aparcedo', 'santiagoaparcedo01@gmail.com', '04148921159', 30933083),
(3, 'Adan D\'lima', 'adancito@gmail.com', '04141231122', 31933092),
(4, 'Santiago Malvado', 'santiagomalvado@gmail.com', '04148921150', 30933082),
(5, 'Sebastian Garcia', 'sebastian@gmail.com', '04143836720', 111),
(6, 'Maria Tovar de Aparcedo', 'maria@gmail.com', '04148836720', 10940587);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuario_rol`
--

CREATE TABLE `usuario_rol` (
  `id` int(20) NOT NULL,
  `usuario_id` int(20) NOT NULL,
  `rol_id` int(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_spanish2_ci;

--
-- Volcado de datos para la tabla `usuario_rol`
--

INSERT INTO `usuario_rol` (`id`, `usuario_id`, `rol_id`) VALUES
(1, 2, 1),
(2, 3, 2),
(3, 4, 3),
(4, 5, 2),
(5, 6, 3);

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `equipos`
--
ALTER TABLE `equipos`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `codigo` (`codigo`);

--
-- Indices de la tabla `inventario`
--
ALTER TABLE `inventario`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `codigo_pieza` (`codigo_pieza`);

--
-- Indices de la tabla `mantenimientos`
--
ALTER TABLE `mantenimientos`
  ADD PRIMARY KEY (`id`),
  ADD KEY `orden_id` (`orden_id`),
  ADD KEY `tecnico_id` (`tecnico_id`);

--
-- Indices de la tabla `movimientos`
--
ALTER TABLE `movimientos`
  ADD PRIMARY KEY (`id`),
  ADD KEY `piezas_id` (`pieza_id`),
  ADD KEY `usuario_id` (`usuario_id`);

--
-- Indices de la tabla `notificaciones`
--
ALTER TABLE `notificaciones`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `ordenesmantenimiento`
--
ALTER TABLE `ordenesmantenimiento`
  ADD PRIMARY KEY (`id`),
  ADD KEY `equipo_id` (`equipo_id`),
  ADD KEY `tecnico_id` (`tecnico_id`),
  ADD KEY `fk_supervisor_id` (`supervisor_id`);

--
-- Indices de la tabla `roles`
--
ALTER TABLE `roles`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `correo` (`correo`),
  ADD UNIQUE KEY `documento_id` (`documento_id`),
  ADD UNIQUE KEY `telefono` (`telefono`);

--
-- Indices de la tabla `usuario_rol`
--
ALTER TABLE `usuario_rol`
  ADD PRIMARY KEY (`id`),
  ADD KEY `usuario_id` (`usuario_id`),
  ADD KEY `rol_id` (`rol_id`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `equipos`
--
ALTER TABLE `equipos`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT de la tabla `inventario`
--
ALTER TABLE `inventario`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT de la tabla `mantenimientos`
--
ALTER TABLE `mantenimientos`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `movimientos`
--
ALTER TABLE `movimientos`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT de la tabla `notificaciones`
--
ALTER TABLE `notificaciones`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT de la tabla `ordenesmantenimiento`
--
ALTER TABLE `ordenesmantenimiento`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT de la tabla `roles`
--
ALTER TABLE `roles`
  MODIFY `id` int(11) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT de la tabla `usuario_rol`
--
ALTER TABLE `usuario_rol`
  MODIFY `id` int(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `mantenimientos`
--
ALTER TABLE `mantenimientos`
  ADD CONSTRAINT `mantenimientos_ibfk_1` FOREIGN KEY (`orden_id`) REFERENCES `ordenesmantenimiento` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `mantenimientos_ibfk_2` FOREIGN KEY (`tecnico_id`) REFERENCES `usuarios` (`id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `ordenesmantenimiento`
--
ALTER TABLE `ordenesmantenimiento`
  ADD CONSTRAINT `fk_supervisor_id` FOREIGN KEY (`supervisor_id`) REFERENCES `usuarios` (`documento_id`),
  ADD CONSTRAINT `ordenesmantenimiento_ibfk_1` FOREIGN KEY (`equipo_id`) REFERENCES `equipos` (`id`),
  ADD CONSTRAINT `ordenesmantenimiento_ibfk_3` FOREIGN KEY (`tecnico_id`) REFERENCES `usuarios` (`documento_id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
