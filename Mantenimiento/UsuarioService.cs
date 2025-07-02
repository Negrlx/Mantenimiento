using System;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Mantenimiento
{
    public static class UsuarioService
    {
        // Metodo auxiliar para leer datos desde la consola
        private static string LeerDato(string campo)
        {
            Console.Write($"{campo}: ");
            return Console.ReadLine()?.Trim();
        }

        // Metodo para verificar si un correo es valido
        public static bool VerificarCorreoElectronico(string correo)
        {
            return !string.IsNullOrWhiteSpace(correo)
                && correo.Contains("@")
                && correo.EndsWith(".com", StringComparison.OrdinalIgnoreCase);
        }

        // Metodo para verificar si un telefono es valido
        public static bool VerificarTLF(string telefono)
        {
            return !string.IsNullOrWhiteSpace(telefono)
                && telefono.StartsWith("04")
                && telefono.Length == 11
                && telefono.All(char.IsDigit);
        }

        // Metodo principal para registrar un usuario y asignarle un rol
        public static void RegistrarDatosUsuarioEnSQL(string nombre, string correo, string telefono, string documento_id, string nombre_rol)
        {
            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("❌ No se pudo abrir la conexión a la base de datos.");
                    return;
                }

                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Insertar usuario
                        string insertUsuario = @"INSERT INTO usuarios (nombre, correo, telefono, documento_id)
                                                 VALUES (@nombre, @correo, @telefono, @documento_id);
                                                 SELECT LAST_INSERT_ID();";

                        int usuarioId;
                        using (MySqlCommand cmd = new MySqlCommand(insertUsuario, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@correo", correo);
                            cmd.Parameters.AddWithValue("@telefono", telefono);
                            cmd.Parameters.AddWithValue("@documento_id", documento_id);
                            usuarioId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Buscar ID del rol por su nombre
                        string buscarRol = "SELECT id FROM roles WHERE nombre_rol = @nombre_rol LIMIT 1";
                        int rolId;

                        using (MySqlCommand cmd = new MySqlCommand(buscarRol, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@nombre_rol", nombre_rol);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                                throw new Exception("El rol no existe.");
                            rolId = Convert.ToInt32(result);
                        }

                        // Asociar usuario con rol
                        string insertarRol = @"INSERT INTO usuario_rol (usuario_id, rol_id)
                                               VALUES (@usuario_id, @rol_id)";

                        using (MySqlCommand cmd = new MySqlCommand(insertarRol, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
                            cmd.Parameters.AddWithValue("@rol_id", rolId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        Console.WriteLine("✅ Usuario y rol registrados correctamente.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("❌ Error al registrar: " + ex.Message);
                    }
                }
            }
        }

        // Metodo para actualizar los datos personales de un usuario existente
        public static void ActualizarDatosPersonales()
        {
            Console.Clear();
            Console.WriteLine("--- ACTUALIZAR DATOS PERSONALES ---");
            string documento = LeerDato("Ingrese el Documento ID del usuario a actualizar");

            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("❌ No se pudo abrir la conexión.");
                    return;
                }

                // Verificar existencia del usuario
                string verificar = "SELECT COUNT(*) FROM usuarios WHERE documento_id = @doc";
                using (MySqlCommand cmd = new MySqlCommand(verificar, conn))
                {
                    cmd.Parameters.AddWithValue("@doc", documento);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count == 0)
                    {
                        Console.WriteLine("⚠️ Usuario no encontrado.");
                        return;
                    }
                }

                // Solicitar nuevos datos
                string nuevoNombre = LeerDato("Nuevo Nombre");
                string nuevoCorreo = LeerDato("Nuevo Correo");
                string nuevoTelefono = LeerDato("Nuevo Teléfono");

                // Validaciones
                if (!VerificarCorreoElectronico(nuevoCorreo))
                {
                    Console.WriteLine("❌ Correo inválido.");
                    return;
                }

                if (!VerificarTLF(nuevoTelefono))
                {
                    Console.WriteLine("❌ Teléfono inválido.");
                    return;
                }

                // Actualizar datos en la base de datos
                string update = @"UPDATE usuarios 
                                  SET nombre = @nombre, correo = @correo, telefono = @telefono 
                                  WHERE documento_id = @documento";

                using (MySqlCommand cmd = new MySqlCommand(update, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nuevoNombre);
                    cmd.Parameters.AddWithValue("@correo", nuevoCorreo);
                    cmd.Parameters.AddWithValue("@telefono", nuevoTelefono);
                    cmd.Parameters.AddWithValue("@documento", documento);

                    int filas = cmd.ExecuteNonQuery();
                    Console.WriteLine(filas > 0 ? "✅ Datos actualizados correctamente." : "⚠️ No se pudo actualizar.");
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Metodo para consultar todos los usuarios que tienen un rol especifico
        public static void ConsultarUsuariosPorRol()
        {
            Console.Clear();
            Console.WriteLine("--- CONSULTAR USUARIOS POR ROL ---");
            string rol = LeerDato("Ingresa el nombre del rol a consultar (admin, tecnico, supervisor)");

            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("❌ No se pudo abrir la conexión.");
                    return;
                }

                // Consulta con JOIN entre usuarios, roles y tabla intermedia
                string consulta = @"
                SELECT u.nombre, u.telefono, u.correo, r.nombre_rol
                FROM usuarios u
                INNER JOIN usuario_rol ur ON u.id = ur.usuario_id
                INNER JOIN roles r ON ur.rol_id = r.id
                WHERE r.nombre_rol = @rol";

                using (MySqlCommand cmd = new MySqlCommand(consulta, conn))
                {
                    cmd.Parameters.AddWithValue("@rol", rol);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool encontrado = false;
                        while (reader.Read())
                        {
                            encontrado = true;
                            Console.WriteLine("-----");
                            Console.WriteLine($"Nombre: {reader["nombre"]}");
                            Console.WriteLine($"Rol: {reader["nombre_rol"]}");
                            Console.WriteLine($"Teléfono: {reader["telefono"]}");
                            Console.WriteLine($"Correo: {reader["correo"]}");
                        }

                        if (!encontrado)
                            Console.WriteLine("⚠️ No se encontraron usuarios con ese rol.");
                    }
                }
            }

            Console.WriteLine("\nPresiona una tecla para continuar...");
            Console.ReadKey();
        }

        // Menu interactivo para registrar usuarios uno por uno
        public static void MenuRegistroUsuarios()
        {
            bool continuar = true;

            while (continuar)
            {
                Console.Clear();
                Console.WriteLine("--- Registro de Usuario ---\n");

                string nombre = LeerDato("Nombre");

                string correo;
                do
                {
                    correo = LeerDato("Correo");
                    if (!VerificarCorreoElectronico(correo))
                        Console.WriteLine("❌ Correo inválido. Debe contener '@' y terminar en '.com'");
                } while (!VerificarCorreoElectronico(correo));

                string telefono;
                do
                {
                    telefono = LeerDato("Teléfono");
                    if (!VerificarTLF(telefono))
                        Console.WriteLine("❌ Teléfono inválido. Debe comenzar con '04' y tener 11 dígitos.");
                } while (!VerificarTLF(telefono));

                string documento = LeerDato("Documento ID");
                string rol = LeerDato("Rol (admin, tecnico, supervisor)");

                RegistrarDatosUsuarioEnSQL(nombre, correo, telefono, documento, rol);

                Console.Write("\n¿Deseas registrar otro usuario? (s/n): ");
                string respuesta = Console.ReadLine()?.Trim().ToLower();
                continuar = (respuesta == "s");
            }

            Console.WriteLine("\n🟢 Proceso terminado. Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }

        // Metodo para buscar usuarios por nombre y mostrar su informacion
        public static void MostrarInfoUsuario(string nombreBusqueda)
        {
            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("❌ No se pudo abrir la conexión a la base de datos.");
                    return;
                }

                // Consulta con LIKE para buscar por nombre parcial
                string consulta = @"
                SELECT u.nombre, r.nombre_rol, u.telefono, u.correo
                FROM usuarios u
                INNER JOIN usuario_rol ur ON u.id = ur.usuario_id
                INNER JOIN roles r ON ur.rol_id = r.id
                WHERE u.nombre LIKE @nombreBusqueda";

                using (MySqlCommand cmd = new MySqlCommand(consulta, conn))
                {
                    cmd.Parameters.AddWithValue("@nombreBusqueda", "%" + nombreBusqueda + "%");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool encontrado = false;
                        while (reader.Read())
                        {
                            encontrado = true;
                            Console.WriteLine("-----");
                            Console.WriteLine($"Nombre: {reader["nombre"]}");
                            Console.WriteLine($"Rol: {reader["nombre_rol"]}");
                            Console.WriteLine($"Teléfono: {reader["telefono"]}");
                            Console.WriteLine($"Correo: {reader["correo"]}");
                        }

                        if (!encontrado)
                            Console.WriteLine("⚠️ No se encontraron usuarios con ese nombre.");
                    }
                }
            }
        }
    }
}
