// Referencias necesarias
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mantenimiento
{
    // Servicio para gestionar ordenes de mantenimiento
    public static class OrdenMantenimientoService
    {
        // Metodo auxiliar para leer datos desde consola
        private static string LeerDato(string campo)
        {
            Console.Write($"{campo}: ");
            return Console.ReadLine()?.Trim();
        }

        // Metodo para crear una nueva orden de mantenimiento
        public static void CrearOrdenMantenimiento()
        {
            Console.Clear();
            Console.WriteLine("--- CREAR ORDEN DE MANTENIMIENTO ---");

            using (var conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("No se pudo abrir la conexion a la base de datos.");
                    return;
                }

                try
                {
                    // Obtener documento y nombre del supervisor
                    Console.Write("Documento del supervisor: ");
                    string docSupervisor = Console.ReadLine()?.Trim();

                    string querySupervisor = "SELECT nombre FROM usuarios WHERE documento_id = @doc";
                    string supervisorNombre;

                    using (var cmd = new MySqlCommand(querySupervisor, conn))
                    {
                        cmd.Parameters.AddWithValue("@doc", docSupervisor);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            Console.WriteLine("Supervisor no encontrado.");
                            return;
                        }
                        supervisorNombre = result.ToString();
                    }

                    Console.WriteLine($"Hola, {supervisorNombre}!");

                    // Obtener ID del equipo
                    Console.Write("Codigo del equipo para mantenimiento: ");
                    string codigoEquipo = Console.ReadLine()?.Trim();

                    string queryEquipo = "SELECT id FROM equipos WHERE codigo = @codigo";
                    int equipoId;

                    using (var cmd = new MySqlCommand(queryEquipo, conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigoEquipo);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            Console.WriteLine("Equipo no encontrado.");
                            return;
                        }
                        equipoId = Convert.ToInt32(result);
                    }

                    // Obtener documento y nombre del tecnico
                    Console.Write("Documento del tecnico responsable: ");
                    string docTecnico = Console.ReadLine()?.Trim();

                    string queryTecnico = "SELECT nombre FROM usuarios WHERE documento_id = @doc";
                    string tecnicoNombre;

                    using (var cmd = new MySqlCommand(queryTecnico, conn))
                    {
                        cmd.Parameters.AddWithValue("@doc", docTecnico);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            Console.WriteLine("Tecnico no encontrado.");
                            return;
                        }
                        tecnicoNombre = result.ToString();
                    }

                    // Obtener descripcion del mantenimiento
                    Console.Write("Descripcion del mantenimiento: ");
                    string descripcion = Console.ReadLine()?.Trim();

                    // Insertar la orden en la base de datos
                    string insertOrden = @"
                        INSERT INTO ordenesmantenimiento
                        (equipo_id, tecnico_id, supervisor_id, tipo_mantenimiento, descripcion, fecha_creacion, estado)
                        VALUES
                        (@equipo_id, @tecnico_id, @supervisor_id, 'Por Revisar', @descripcion, NOW(), 'Pendiente')";

                    using (var cmd = new MySqlCommand(insertOrden, conn))
                    {
                        cmd.Parameters.AddWithValue("@equipo_id", equipoId);
                        cmd.Parameters.AddWithValue("@tecnico_id", docTecnico);
                        cmd.Parameters.AddWithValue("@supervisor_id", docSupervisor);
                        cmd.Parameters.AddWithValue("@descripcion", descripcion);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            Console.WriteLine("Orden de mantenimiento creada correctamente.");

                            // Enviar notificacion al tecnico
                            string mensaje = $"Se te ha asignado una nueva orden de mantenimiento para el equipo {codigoEquipo}.\nDescripcion: {descripcion}";
                            NtfService.EnviarNotificacion(docSupervisor, docTecnico, "Aviso", mensaje, conn);
                            Console.WriteLine("Notificacion enviada al tecnico.");
                        }
                        else
                        {
                            Console.WriteLine("No se pudo crear la orden.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Metodo para listar todas las ordenes de mantenimiento
        public static void ListarOrdenesMantenimiento()
        {
            Console.Clear();
            Console.WriteLine("--- LISTADO DE ORDENES DE MANTENIMIENTO ---");

            using (var conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("No se pudo abrir la conexion a la base de datos.");
                    return;
                }

                string query = @"
                    SELECT om.id,
                           e.codigo AS codigo_equipo,
                           om.tecnico_id,
                           om.supervisor_id,
                           om.tipo_mantenimiento,
                           om.descripcion,
                           om.fecha_creacion,
                           om.estado,
                           t.nombre AS tecnico_nombre,
                           s.nombre AS supervisor_nombre
                    FROM ordenesmantenimiento om
                    INNER JOIN equipos e ON om.equipo_id = e.id
                    LEFT JOIN usuarios t ON om.tecnico_id = t.documento_id
                    LEFT JOIN usuarios s ON om.supervisor_id = s.documento_id
                    ORDER BY om.fecha_creacion DESC";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("No hay ordenes de mantenimiento registradas.");
                        return;
                    }

                    // Encabezados de la tabla
                    Console.WriteLine("{0,-5} {1,-10} {2,-20} {3,-20} {4,-15} {5,-10} {6}",
                        "ID", "Equipo", "Tecnico", "Supervisor", "Tipo", "Estado", "Fecha");

                    Console.WriteLine(new string('-', 90));

                    // Mostrar ordenes
                    while (reader.Read())
                    {
                        Console.WriteLine("{0,-5} {1,-10} {2,-20} {3,-20} {4,-15} {5,-10} {6:yyyy-MM-dd}",
                            reader["id"],
                            reader["codigo_equipo"],
                            reader["tecnico_nombre"] ?? reader["tecnico_id"],
                            reader["supervisor_nombre"] ?? reader["supervisor_id"],
                            reader["tipo_mantenimiento"],
                            reader["estado"],
                            reader.GetDateTime("fecha_creacion"));
                    }
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Metodo para que el tecnico registre el tipo de mantenimiento
        public static void RegistrarMantenimientoTecnico()
        {
            Console.Clear();
            Console.WriteLine("--- REGISTRAR MANTENIMIENTO COMO TECNICO ---");

            using (var conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("No se pudo abrir la conexion a la base de datos.");
                    return;
                }

                Console.Write("Documento del tecnico: ");
                string docTecnico = Console.ReadLine()?.Trim();

                // Validar que el tecnico existe
                string queryTecnico = "SELECT nombre FROM usuarios WHERE documento_id = @doc";
                string nombreTecnico;

                using (var cmd = new MySqlCommand(queryTecnico, conn))
                {
                    cmd.Parameters.AddWithValue("@doc", docTecnico);
                    var result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        Console.WriteLine("Tecnico no encontrado.");
                        return;
                    }
                    nombreTecnico = result.ToString();
                }

                Console.WriteLine($"Bienvenido, {nombreTecnico}!");

                // Buscar ordenes pendientes asignadas al tecnico
                string queryOrdenes = @"
                    SELECT om.id, e.codigo AS codigo_equipo, om.descripcion, om.fecha_creacion
                    FROM ordenesmantenimiento om
                    INNER JOIN equipos e ON om.equipo_id = e.id
                    WHERE om.tecnico_id = @doc AND om.estado = 'Pendiente'
                    ORDER BY om.fecha_creacion";

                List<int> ordenesDisponibles = new List<int>();

                using (var cmd = new MySqlCommand(queryOrdenes, conn))
                {
                    cmd.Parameters.AddWithValue("@doc", docTecnico);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("No tienes ordenes pendientes asignadas.");
                            return;
                        }

                        Console.WriteLine("\nOrdenes pendientes asignadas:\n");
                        Console.WriteLine("{0,-5} {1,-10} {2,-40} {3}",
                            "ID", "Equipo", "Descripcion", "Fecha");

                        Console.WriteLine(new string('-', 80));

                        while (reader.Read())
                        {
                            int ordenId = reader.GetInt32("id");
                            ordenesDisponibles.Add(ordenId);

                            Console.WriteLine("{0,-5} {1,-10} {2,-40} {3:yyyy-MM-dd}",
                                ordenId,
                                reader["codigo_equipo"],
                                reader["descripcion"],
                                reader.GetDateTime("fecha_creacion"));
                        }
                    }
                }

                // Seleccionar orden a trabajar
                Console.Write("\nIngresa el ID de la orden que vas a trabajar: ");
                if (!int.TryParse(Console.ReadLine(), out int ordenSeleccionada) || !ordenesDisponibles.Contains(ordenSeleccionada))
                {
                    Console.WriteLine("ID invalido o no esta en tu lista de ordenes.");
                    return;
                }

                // Elegir tipo de mantenimiento realizado
                string tipoMantenimiento = "";
                while (true)
                {
                    Console.Write("Tipo de mantenimiento realizado ('Rutinario' o 'Cambio de Pieza'): ");
                    string input = Console.ReadLine()?.Trim();
                    if (input == "Rutinario" || input == "Cambio de Pieza")
                    {
                        tipoMantenimiento = input;
                        break;
                    }
                    Console.WriteLine("Entrada invalida. Debe ser 'Rutinario' o 'Cambio de Pieza'.");
                }

                // Actualizar estado de la orden
                string updateOrden = @"
                    UPDATE ordenesmantenimiento
                    SET tipo_mantenimiento = @tipo, estado = 'En Proceso'
                    WHERE id = @id AND tecnico_id = @doc";

                using (var cmd = new MySqlCommand(updateOrden, conn))
                {
                    cmd.Parameters.AddWithValue("@tipo", tipoMantenimiento);
                    cmd.Parameters.AddWithValue("@id", ordenSeleccionada);
                    cmd.Parameters.AddWithValue("@doc", docTecnico);

                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        Console.WriteLine("Orden actualizada correctamente. Ahora esta en proceso.");

                        if (tipoMantenimiento == "Cambio de Pieza")
                        {
                            // Si es cambio de pieza, se solicita notificacion al supervisor
                            SolicitarPieza(docTecnico, ordenSeleccionada, conn);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No se pudo actualizar la orden.");
                    }
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Metodo para enviar una notificacion de solicitud de pieza al supervisor
        public static void SolicitarPieza(string documentoTecnico, int ordenId, MySqlConnection conn)
        {
            string query = @"
                SELECT e.codigo AS codigo_equipo, u.documento_id AS supervisor_id, u.nombre AS supervisor_nombre
                FROM ordenesmantenimiento om
                INNER JOIN equipos e ON om.equipo_id = e.id
                INNER JOIN usuarios u ON om.supervisor_id = u.documento_id
                WHERE om.id = @ordenId";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ordenId", ordenId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return;

                    string codigoEquipo = reader["codigo_equipo"].ToString();
                    string supervisorDoc = reader["supervisor_id"].ToString();

                    reader.Close(); // Cerrar antes de hacer nueva operacion

                    string mensaje = $"El tecnico {documentoTecnico} necesita una pieza para el mantenimiento del equipo {codigoEquipo} (orden #{ordenId}).";
                    NtfService.EnviarNotificacion(documentoTecnico, supervisorDoc, "SolicitarPieza", mensaje, conn);
                }
            }
        }

        // Metodo para finalizar una orden que esta en proceso
        public static void FinalizarOrden()
        {
            Console.Clear();
            Console.WriteLine("--- FINALIZAR ORDEN DE MANTENIMIENTO ---");

            using (var conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("No se pudo abrir la conexion a la base de datos.");
                    return;
                }

                Console.Write("Documento del tecnico: ");
                string docTecnico = Console.ReadLine()?.Trim();

                // Validar que el tecnico existe
                string queryTecnico = "SELECT nombre FROM usuarios WHERE documento_id = @doc";
                string nombreTecnico;

                using (var cmd = new MySqlCommand(queryTecnico, conn))
                {
                    cmd.Parameters.AddWithValue("@doc", docTecnico);
                    var result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        Console.WriteLine("Tecnico no encontrado.");
                        return;
                    }
                    nombreTecnico = result.ToString();
                }

                Console.WriteLine($"Bienvenido, {nombreTecnico}!");

                // Buscar ordenes en proceso
                string queryOrdenes = @"
                    SELECT om.id, e.codigo AS codigo_equipo, om.descripcion, om.fecha_creacion
                    FROM ordenesmantenimiento om
                    INNER JOIN equipos e ON om.equipo_id = e.id
                    WHERE om.tecnico_id = @doc AND om.estado = 'En Proceso'
                    ORDER BY om.fecha_creacion";

                List<int> ordenesEnProceso = new List<int>();

                using (var cmd = new MySqlCommand(queryOrdenes, conn))
                {
                    cmd.Parameters.AddWithValue("@doc", docTecnico);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("No tienes ordenes en proceso actualmente.");
                            return;
                        }

                        Console.WriteLine("\nOrdenes en proceso asignadas:\n");
                        Console.WriteLine("{0,-5} {1,-10} {2,-40} {3}",
                            "ID", "Equipo", "Descripcion", "Fecha");

                        Console.WriteLine(new string('-', 80));

                        while (reader.Read())
                        {
                            int ordenId = reader.GetInt32("id");
                            ordenesEnProceso.Add(ordenId);

                            Console.WriteLine("{0,-5} {1,-10} {2,-40} {3:yyyy-MM-dd}",
                                ordenId,
                                reader["codigo_equipo"],
                                reader["descripcion"],
                                reader.GetDateTime("fecha_creacion"));
                        }
                    }
                }

                // Seleccionar orden a finalizar
                Console.Write("\nIngresa el ID de la orden que deseas finalizar: ");
                if (!int.TryParse(Console.ReadLine(), out int ordenSeleccionada) || !ordenesEnProceso.Contains(ordenSeleccionada))
                {
                    Console.WriteLine("ID invalido o no corresponde a tus ordenes en proceso.");
                    return;
                }

                // Actualizar estado a Finalizada
                string updateOrden = @"
                    UPDATE ordenesmantenimiento
                    SET estado = 'Finalizada'
                    WHERE id = @id AND tecnico_id = @doc AND estado = 'En Proceso'";

                using (var cmd = new MySqlCommand(updateOrden, conn))
                {
                    cmd.Parameters.AddWithValue("@id", ordenSeleccionada);
                    cmd.Parameters.AddWithValue("@doc", docTecnico);

                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                        Console.WriteLine("La orden fue finalizada exitosamente.");
                    else
                        Console.WriteLine("No se pudo finalizar la orden.");
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }
    }
}
