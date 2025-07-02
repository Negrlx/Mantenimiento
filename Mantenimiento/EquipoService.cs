using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Mantenimiento
{
    internal class EquipoService
    {
        // Metodo auxiliar para leer datos desde consola
        private static string LeerDato(string campo)
        {
            Console.Write($"{campo}: ");
            return Console.ReadLine()?.Trim();
        }

        // Metodo para registrar un nuevo equipo en la base de datos
        public static void RegistrarEquipo()
        {
            Console.Clear();
            Console.WriteLine("--- REGISTRO DE EQUIPO ---");

            // Solicitar datos del equipo al usuario
            string codigo = LeerDato("Código del equipo");
            string nombre = LeerDato("Nombre del equipo");
            string descripcion = LeerDato("Descripción");
            string proveedor = LeerDato("Proveedor");

            // Validar y leer fecha de mantenimiento
            DateTime fechaMantenimiento;
            do
            {
                Console.Write("Fecha de mantenimiento programada (YYYY-MM-DD): ");
                string fechaInput = Console.ReadLine()?.Trim();

                if (DateTime.TryParse(fechaInput, out fechaMantenimiento))
                    break;

                Console.WriteLine("❌ Fecha invalida. Intenta de nuevo.");
            } while (true);

            // Abrir conexion a la base de datos
            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("❌ No se pudo abrir la conexion a la base de datos.");
                    return;
                }

                // Preparar consulta de insercion
                string insertar = @"INSERT INTO equipos (codigo, nombre, descripcion, proveedor, fecha_mantenimiento)
                                    VALUES (@codigo, @nombre, @descripcion, @proveedor, @fecha_mantenimiento);";

                using (MySqlCommand cmd = new MySqlCommand(insertar, conn))
                {
                    // Asignar parametros
                    cmd.Parameters.AddWithValue("@codigo", codigo);
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@proveedor", proveedor);
                    cmd.Parameters.AddWithValue("@fecha_mantenimiento", fechaMantenimiento);

                    try
                    {
                        // Ejecutar insercion
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("✅ Equipo registrado correctamente.");
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("❌ Error al registrar el equipo: " + ex.Message);
                    }
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Metodo para editar datos de un equipo existente
        public static void EditarEquipo()
        {
            Console.Clear();
            Console.WriteLine("--- EDITAR EQUIPO ---");

            // Solicitar codigo del equipo a modificar
            string codigo = LeerDato("Código del equipo a editar");

            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("❌ No se pudo abrir la conexion.");
                    return;
                }

                // Verificar si el equipo existe
                string verificar = "SELECT COUNT(*) FROM equipos WHERE codigo = @codigo";
                using (MySqlCommand cmd = new MySqlCommand(verificar, conn))
                {
                    cmd.Parameters.AddWithValue("@codigo", codigo);
                    int existe = Convert.ToInt32(cmd.ExecuteScalar());

                    if (existe == 0)
                    {
                        Console.WriteLine("⚠️ No se encontro ningun equipo con ese codigo.");
                        return;
                    }
                }

                // Solicitar nuevos datos del equipo
                string nuevoNombre = LeerDato("Nuevo nombre");
                string nuevaDescripcion = LeerDato("Nueva descripcion");
                string nuevoProveedor = LeerDato("Nuevo proveedor");

                Console.Write("Nueva fecha de mantenimiento (YYYY-MM-DD): ");
                DateTime nuevaFecha;
                while (!DateTime.TryParse(Console.ReadLine(), out nuevaFecha))
                {
                    Console.Write("❌ Fecha invalida. Intenta de nuevo: ");
                }

                // Preparar consulta de actualizacion
                string actualizar = @"UPDATE equipos 
                              SET nombre = @nombre, descripcion = @desc, proveedor = @prov, fecha_mantenimiento = @fecha 
                              WHERE codigo = @codigo";

                using (MySqlCommand cmd = new MySqlCommand(actualizar, conn))
                {
                    // Asignar parametros
                    cmd.Parameters.AddWithValue("@nombre", nuevoNombre);
                    cmd.Parameters.AddWithValue("@desc", nuevaDescripcion);
                    cmd.Parameters.AddWithValue("@prov", nuevoProveedor);
                    cmd.Parameters.AddWithValue("@fecha", nuevaFecha);
                    cmd.Parameters.AddWithValue("@codigo", codigo);

                    // Ejecutar actualizacion
                    int filas = cmd.ExecuteNonQuery();

                    Console.WriteLine(filas > 0 ? "✅ Equipo actualizado." : "⚠️ No se pudo actualizar.");
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Metodo para eliminar un equipo de la base de datos
        public static void EliminarEquipo()
        {
            Console.Clear();
            Console.WriteLine("--- ELIMINAR EQUIPO ---");

            // Solicitar codigo del equipo a eliminar
            string codigo = LeerDato("Codigo del equipo a eliminar");

            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("❌ No se pudo abrir la conexion.");
                    return;
                }

                // Preparar consulta de eliminacion
                string eliminar = "DELETE FROM equipos WHERE codigo = @codigo";

                using (MySqlCommand cmd = new MySqlCommand(eliminar, conn))
                {
                    // Asignar parametro codigo
                    cmd.Parameters.AddWithValue("@codigo", codigo);

                    // Ejecutar eliminacion
                    int filas = cmd.ExecuteNonQuery();
                    Console.WriteLine(filas > 0 ? "🗑️ Equipo eliminado." : "⚠️ No se encontro el equipo.");
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Metodo para buscar equipos por nombre o codigo
        public static void BuscarEquipoPorNombreOCodigo()
        {
            Console.Clear();
            Console.WriteLine("--- BUSCAR EQUIPO ---");

            // Solicitar termino de busqueda
            string termino = LeerDato("Nombre o codigo a buscar");

            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("❌ No se pudo abrir la conexion.");
                    return;
                }

                // Consulta para buscar por nombre o codigo con LIKE
                string consulta = @"SELECT * FROM equipos 
                            WHERE nombre LIKE @term OR codigo LIKE @term";

                using (MySqlCommand cmd = new MySqlCommand(consulta, conn))
                {
                    cmd.Parameters.AddWithValue("@term", "%" + termino + "%");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool encontrado = false;

                        // Mostrar resultados encontrados
                        while (reader.Read())
                        {
                            encontrado = true;
                            Console.WriteLine("\n🔎 Equipo encontrado:");
                            Console.WriteLine($"Codigo: {reader["codigo"]}");
                            Console.WriteLine($"Nombre: {reader["nombre"]}");
                            Console.WriteLine($"Descripcion: {reader["descripcion"]}");
                            Console.WriteLine($"Proveedor: {reader["proveedor"]}");
                            Console.WriteLine($"Fecha de mantenimiento: {Convert.ToDateTime(reader["fecha_mantenimiento"]).ToShortDateString()}");
                        }

                        if (!encontrado)
                            Console.WriteLine("⚠️ No se encontraron coincidencias.");
                    }
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Metodo para listar todos los equipos registrados
        public static void ListarEquipos()
        {
            Console.Clear();
            Console.WriteLine("--- LISTADO DE EQUIPOS ---");

            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("❌ No se pudo abrir la conexion.");
                    return;
                }

                // Consulta para obtener todos los equipos
                string consulta = "SELECT * FROM equipos";

                using (MySqlCommand cmd = new MySqlCommand(consulta, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    int count = 0;

                    // Mostrar cada equipo encontrado
                    while (reader.Read())
                    {
                        count++;
                        Console.WriteLine($"\n[{count}] -------------------");
                        Console.WriteLine($"Codigo: {reader["codigo"]}");
                        Console.WriteLine($"Nombre: {reader["nombre"]}");
                        Console.WriteLine($"Descripcion: {reader["descripcion"]}");
                        Console.WriteLine($"Proveedor: {reader["proveedor"]}");
                        Console.WriteLine($"Fecha mantenimiento: {Convert.ToDateTime(reader["fecha_mantenimiento"]).ToShortDateString()}");
                    }

                    if (count == 0)
                        Console.WriteLine("⚠️ No hay equipos registrados.");
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }
    }
}
