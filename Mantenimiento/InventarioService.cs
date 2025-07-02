// Espacios de nombres necesarios
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mantenimiento
{
    public class InventarioService
    {
        // Consulta todo el inventario desde la base de datos
        public static void ConsultarInventarioDesdeSQL()
        {
            using (var conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("No se pudo abrir la conexion a la base de datos.");
                    return;
                }

                string query = "SELECT codigo_pieza, nombre_pieza, descripcion, cantidad, proveedor, fecha_ultima_actualizacion FROM Inventario";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("--- Inventario de Piezas ---");
                        while (reader.Read())
                        {
                            Console.WriteLine($"Codigo: {reader["codigo_pieza"]}");
                            Console.WriteLine($"Nombre: {reader["nombre_pieza"]}");
                            Console.WriteLine($"Descripcion: {reader["descripcion"]}");
                            Console.WriteLine($"Cantidad Disponible: {reader["cantidad"]}");
                            Console.WriteLine($"Proveedor: {reader["proveedor"]}");
                            Console.WriteLine($"Ultima actualizacion: {reader["fecha_ultima_actualizacion"]}");
                            Console.WriteLine("-----------------------------");
                        }
                    }
                }

                Console.WriteLine("\nPresiona cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }

        // Registra una entrada de piezas en el inventario
        public static void RegistrarEntradaPiezaEnSQL(string codigoPieza, string nombrePieza, int cantidad, string descripcionMovimiento, int usuarioId)
        {
            using (var conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("No se pudo abrir la conexion a la base de datos.");
                    return;
                }

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Verifica si la pieza ya existe
                        string selectPieza = "SELECT id, cantidad FROM Inventario WHERE codigo_pieza = @codigoPieza";
                        int piezaId = 0;
                        int cantidadActual = 0;

                        using (var cmd = new MySqlCommand(selectPieza, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@codigoPieza", codigoPieza);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    piezaId = Convert.ToInt32(reader["id"]);
                                    cantidadActual = Convert.ToInt32(reader["cantidad"]);
                                }
                                reader.Close();
                            }
                        }

                        if (piezaId == 0)
                        {
                            // Inserta la nueva pieza si no existe
                            string insertPieza = @"INSERT INTO Inventario 
                                (codigo_pieza, nombre_pieza, cantidad, fecha_ultima_actualizacion) 
                                VALUES (@codigoPieza, @nombrePieza, @cantidad, NOW())";

                            using (var cmdInsert = new MySqlCommand(insertPieza, conn, transaction))
                            {
                                cmdInsert.Parameters.AddWithValue("@codigoPieza", codigoPieza);
                                cmdInsert.Parameters.AddWithValue("@nombrePieza", nombrePieza);
                                cmdInsert.Parameters.AddWithValue("@cantidad", cantidad);
                                cmdInsert.ExecuteNonQuery();

                                piezaId = (int)cmdInsert.LastInsertedId;
                            }
                        }
                        else
                        {
                            // Actualiza la cantidad si la pieza ya existe
                            string updateCantidad = "UPDATE Inventario SET cantidad = cantidad + @cantidad, fecha_ultima_actualizacion = NOW() WHERE id = @piezaId";
                            using (var cmd = new MySqlCommand(updateCantidad, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@cantidad", cantidad);
                                cmd.Parameters.AddWithValue("@piezaId", piezaId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Registra el movimiento de entrada
                        string insertMovimiento = @"INSERT INTO movimientos 
                            (pieza_id, tipo_movimiento, cantidad, descripcion, usuario_id) 
                            VALUES (@piezaId, 'Entrada', @cantidad, @descripcion, @usuarioId)";
                        using (var cmd = new MySqlCommand(insertMovimiento, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@piezaId", piezaId);
                            cmd.Parameters.AddWithValue("@cantidad", cantidad);
                            cmd.Parameters.AddWithValue("@descripcion", descripcionMovimiento);
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        Console.WriteLine("Entrada registrada y cantidad actualizada correctamente.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error al registrar entrada: " + ex.Message);
                    }
                }
            }
        }

        // Retira piezas del inventario y registra el movimiento
        public static void RetirarPiezaYActualizarSQL(string codigoPieza, int cantidad, string descripcionMovimiento, int usuarioId)
        {
            using (var conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("No se pudo abrir la conexion a la base de datos.");
                    return;
                }

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Verifica si la pieza existe y si hay suficiente cantidad
                        string selectPieza = "SELECT id, cantidad FROM Inventario WHERE codigo_pieza = @codigoPieza";
                        int piezaId = 0;
                        int cantidadActual = 0;

                        using (var cmd = new MySqlCommand(selectPieza, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@codigoPieza", codigoPieza);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    piezaId = Convert.ToInt32(reader["id"]);
                                    cantidadActual = Convert.ToInt32(reader["cantidad"]);
                                }
                                else
                                {
                                    throw new Exception("La pieza no existe en el inventario.");
                                }
                            }
                        }

                        if (cantidadActual < cantidad)
                            throw new Exception("Cantidad insuficiente en inventario para retirar.");

                        // Actualiza la cantidad restando las unidades
                        string updateCantidad = "UPDATE Inventario SET cantidad = cantidad - @cantidad WHERE id = @piezaId";
                        using (var cmd = new MySqlCommand(updateCantidad, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@cantidad", cantidad);
                            cmd.Parameters.AddWithValue("@piezaId", piezaId);
                            cmd.ExecuteNonQuery();
                        }

                        // Registra el movimiento de salida
                        string insertMovimiento = @"INSERT INTO movimientos 
                            (pieza_id, tipo_movimiento, cantidad, descripcion, usuario_id) 
                            VALUES (@piezaId, 'Salida', @cantidad, @descripcion, @usuarioId)";
                        using (var cmd = new MySqlCommand(insertMovimiento, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@piezaId", piezaId);
                            cmd.Parameters.AddWithValue("@cantidad", cantidad);
                            cmd.Parameters.AddWithValue("@descripcion", descripcionMovimiento);
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        Console.WriteLine("Retiro registrado y cantidad actualizada correctamente.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error al retirar pieza: " + ex.Message);
                    }
                }
            }
        }

        // Metodo auxiliar para leer un campo desde consola
        private static string LeerDato(string campo)
        {
            Console.Write($"{campo}: ");
            return Console.ReadLine()?.Trim();
        }

        // Busca una pieza por nombre o codigo en la base de datos
        public static void BuscarPiezaPorNombreOCodigoEnSQL()
        {
            Console.Clear();
            Console.WriteLine("--- BUSCAR PIEZA POR NOMBRE O CODIGO ---");
            string filtro = LeerDato("Ingresa el nombre o codigo");

            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                string query = @"SELECT codigo_pieza, nombre_pieza, descripcion, proveedor, cantidad, fecha_ultima_actualizacion
                                 FROM Inventario
                                 WHERE nombre_pieza LIKE @filtro OR codigo_pieza LIKE @filtro";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool encontrado = false;
                        while (reader.Read())
                        {
                            encontrado = true;
                            Console.WriteLine("\n--- PIEZA ENCONTRADA ---");
                            Console.WriteLine($"Codigo: {reader["codigo_pieza"]}");
                            Console.WriteLine($"Nombre: {reader["nombre_pieza"]}");
                            Console.WriteLine($"Descripcion: {reader["descripcion"]}");
                            Console.WriteLine($"Proveedor: {reader["proveedor"]}");
                            Console.WriteLine($"Cantidad: {reader["cantidad"]}");
                            Console.WriteLine($"Mantenimiento: {Convert.ToDateTime(reader["fecha_ultima_actualizacion"]).ToShortDateString()}");
                        }

                        if (!encontrado)
                            Console.WriteLine("No se encontraron piezas con ese criterio.");
                    }
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Notifica a los administradores sobre piezas con stock bajo
        public static void NotificarReposicion()
        {
            Console.Clear();
            Console.WriteLine("--- ALERTA DE REPOSICION DE PIEZAS ---");

            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("No se pudo abrir la conexion a la base de datos.");
                    return;
                }

                List<Tuple<string, string, int>> piezasBajas = new List<Tuple<string, string, int>>();

                // Consulta las piezas con cantidad menor a 5
                string queryPiezas = @"SELECT codigo_pieza, nombre_pieza, cantidad FROM Inventario WHERE cantidad < 5";

                using (MySqlCommand cmd = new MySqlCommand(queryPiezas, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string codigo = reader["codigo_pieza"].ToString();
                            string nombre = reader["nombre_pieza"].ToString();
                            int cantidad = Convert.ToInt32(reader["cantidad"]);

                            Console.WriteLine("Pieza con stock bajo:");
                            Console.WriteLine($"Codigo: {codigo}");
                            Console.WriteLine($"Nombre: {nombre}");
                            Console.WriteLine($"Cantidad disponible: {cantidad}");
                            Console.WriteLine(new string('-', 30));

                            piezasBajas.Add(Tuple.Create(codigo, nombre, cantidad));
                        }
                    }
                }

                if (piezasBajas.Count == 0)
                {
                    Console.WriteLine("No hay piezas que requieran reposicion.");
                    Console.WriteLine("\nPresiona una tecla para continuar...");
                    Console.ReadKey();
                    return;
                }

                // Obtiene los documento_id de los administradores
                List<string> administradores = new List<string>();

                string queryAdmins = @"
                    SELECT u.documento_id
                    FROM usuarios u
                    INNER JOIN usuario_rol ur ON u.id = ur.usuario_id
                    INNER JOIN roles r ON ur.rol_id = r.id
                    WHERE r.nombre_rol = 'admin'";

                using (MySqlCommand cmd = new MySqlCommand(queryAdmins, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string doc = reader["documento_id"].ToString();
                            administradores.Add(doc);
                        }
                    }
                }

                // Envia notificaciones a los administradores
                foreach (string adminDoc in administradores)
                {
                    foreach (var pieza in piezasBajas)
                    {
                        string mensaje = $"La pieza '{pieza.Item2}' (codigo: {pieza.Item1}) tiene solo {pieza.Item3} unidades disponibles. Se requiere reposicion.";
                        NtfService.EnviarNotificacion("Sistema", adminDoc, "StockBajo", mensaje, conn);
                    }
                }

                Console.WriteLine("Notificaciones de stock bajo enviadas a administradores.");
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }
    }
}
