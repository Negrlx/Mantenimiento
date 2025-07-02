using MySql.Data.MySqlClient;
using System;

namespace Mantenimiento
{
    public static class DBConnection
    {
        // Cadena de conexion a la base de datos MySQL
        private static readonly string connectionString = "server=localhost;port=3306;database=db;user=root;password=;";

        // Metodo para obtener y abrir una conexion MySQL
        public static MySqlConnection GetConnection()
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            try
            {
                conn.Open();
                Console.WriteLine("Conexion exitosa a MySQL.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al conectar con MySQL: " + ex.Message);
            }
            return conn;
        }
    }

    public static class DBHandler
    {
        // Metodo para registrar un usuario y asignarle un rol dentro de una transaccion
        public static void RegistrarDatosUsuarioEnSQL(string nombre, string correo, string telefono, string documento_id, string nombre_rol)
        {
            using (MySqlConnection conn = DBConnection.GetConnection())
            {
                // Iniciar transaccion para asegurar atomicidad
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Insertar nuevo usuario y obtener su id
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

                    // Buscar el id del rol por su nombre
                    string buscarRol = "SELECT id FROM roles WHERE nombre_rol = @nombre_rol LIMIT 1";
                    int rolId;

                    using (MySqlCommand cmd = new MySqlCommand(buscarRol, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@nombre_rol", nombre_rol);
                        var result = cmd.ExecuteScalar();

                        if (result == null)
                            throw new Exception("❌ El rol ingresado no existe.");

                        rolId = Convert.ToInt32(result);
                    }

                    // Insertar relacion usuario-rol
                    string insertarRol = @"INSERT INTO usuario_rol (usuario_id, rol_id)
                                           VALUES (@usuario_id, @rol_id)";

                    using (MySqlCommand cmd = new MySqlCommand(insertarRol, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
                        cmd.Parameters.AddWithValue("@rol_id", rolId);
                        cmd.ExecuteNonQuery();
                    }

                    // Confirmar transaccion si todo salio bien
                    transaction.Commit();
                    Console.WriteLine("✅ Usuario y rol registrados correctamente.");
                }
                catch (Exception ex)
                {
                    // Revertir cambios en caso de error
                    transaction.Rollback();
                    Console.WriteLine("❌ Error al registrar: " + ex.Message);
                }
            }
        }
    }
}
