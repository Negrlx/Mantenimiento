// Referencias necesarias para MySQL y funciones del sistema
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mantenimiento
{
    // Clase encargada de gestionar las notificaciones
    internal class NtfService
    {
        // Metodo para insertar una nueva notificacion en la base de datos
        public static void EnviarNotificacion(string emisor, string receptor, string tipo, string mensaje, MySqlConnection conn)
        {
            string insert = @"
                INSERT INTO notificaciones (emisor, receptor, tipo, mensaje)
                VALUES (@emisor, @receptor, @tipo, @mensaje)";

            using (var cmd = new MySqlCommand(insert, conn))
            {
                cmd.Parameters.AddWithValue("@emisor", emisor);
                cmd.Parameters.AddWithValue("@receptor", receptor);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.Parameters.AddWithValue("@mensaje", mensaje);
                cmd.ExecuteNonQuery();
            }
        }

        // Metodo que muestra todas las notificaciones registradas
        public static void VerTodasNotificaciones()
        {
            Console.Clear();
            Console.WriteLine("--- TODAS LAS NOTIFICACIONES ---");

            using (var conn = DBConnection.GetConnection())
            {
                // Validar conexion
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("No se pudo conectar a la base de datos.");
                    return;
                }

                // Consulta para obtener todas las notificaciones ordenadas por fecha
                string query = @"
                    SELECT id, emisor, receptor, tipo, mensaje, fecha, leido
                    FROM notificaciones
                    ORDER BY fecha DESC";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    // Si no hay notificaciones, se informa al usuario
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("No hay notificaciones registradas.");
                        return;
                    }

                    // Mostrar cada notificacion
                    while (reader.Read())
                    {
                        Console.WriteLine("ID          : " + reader["id"]);
                        Console.WriteLine("Emisor      : " + reader["emisor"]);
                        Console.WriteLine("Receptor    : " + reader["receptor"]);
                        Console.WriteLine("Tipo        : " + reader["tipo"]);
                        Console.WriteLine("Mensaje     : " + reader["mensaje"]);
                        Console.WriteLine("Fecha       : " + ((DateTime)reader["fecha"]).ToString("yyyy-MM-dd"));
                        Console.WriteLine("Leido       : " + (((bool)reader["leido"]) ? "Si" : "No"));
                        Console.WriteLine(new string('-', 60));
                    }
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Metodo que muestra las notificaciones no leidas de un usuario y permite marcarlas como leidas
        public static void VerNotificacionesUsuario(string documentoUsuario)
        {
            Console.Clear();
            Console.WriteLine("--- TUS NOTIFICACIONES PENDIENTES ---");

            using (var conn = DBConnection.GetConnection())
            {
                // Validar conexion
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("No se pudo conectar a la base de datos.");
                    return;
                }

                // Consulta para obtener las notificaciones no leidas del usuario
                string query = @"
                    SELECT id, tipo, mensaje, fecha
                    FROM notificaciones
                    WHERE receptor = @doc AND leido = FALSE
                    ORDER BY fecha DESC";

                List<int> idsPendientes = new List<int>(); // Para validar seleccion
                Dictionary<int, string> tiposPorId = new Dictionary<int, string>(); // Para obtener el tipo original

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@doc", documentoUsuario);
                    using (var reader = cmd.ExecuteReader())
                    {
                        // Si no hay notificaciones pendientes, se informa
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("No tienes notificaciones pendientes.");
                            return;
                        }

                        // Mostrar notificaciones y guardar ids y tipos
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");
                            string tipo = reader["tipo"].ToString();

                            idsPendientes.Add(id);
                            tiposPorId[id] = tipo;

                            Console.WriteLine("ID      : " + id);
                            Console.WriteLine("Tipo    : " + tipo);
                            Console.WriteLine("Mensaje : " + reader["mensaje"]);
                            Console.WriteLine("Fecha   : " + ((DateTime)reader["fecha"]).ToString("yyyy-MM-dd"));
                            Console.WriteLine(new string('-', 60));
                        }
                    }
                }

                // Solicitar al usuario el ID a marcar como leido
                Console.Write("\nIngresa el ID de la notificacion que deseas marcar como leida: ");
                if (!int.TryParse(Console.ReadLine(), out int idSeleccionado) || !idsPendientes.Contains(idSeleccionado))
                {
                    Console.WriteLine("ID invalido.");
                    return;
                }

                string tipoOriginal = tiposPorId[idSeleccionado];

                // Obtener el emisor original de la notificacion
                string queryEmisor = "SELECT emisor FROM notificaciones WHERE id = @id";
                string emisor = "";

                using (var cmd = new MySqlCommand(queryEmisor, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idSeleccionado);
                    emisor = cmd.ExecuteScalar()?.ToString();
                }

                // Marcar la notificacion como leida
                string update = "UPDATE notificaciones SET leido = TRUE WHERE id = @id";
                using (var cmd = new MySqlCommand(update, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idSeleccionado);
                    cmd.ExecuteNonQuery();
                }

                // Enviar confirmacion al emisor si no es ya una notificacion de confirmacion
                if (tipoOriginal != "Confirmacion")
                {
                    string mensajeConfirmacion = $"El usuario {documentoUsuario} ha leido tu mensaje (ID {idSeleccionado}).";
                    EnviarNotificacion(documentoUsuario, emisor, "Confirmacion", mensajeConfirmacion, conn);
                    Console.WriteLine("Notificacion marcada como leida y confirmacion enviada al emisor.");
                }
                else
                {
                    Console.WriteLine("Notificacion marcada como leida. (No se envio confirmacion porque ya era de tipo 'Confirmacion')");
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }
    }
}
