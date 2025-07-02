// Espacios de nombres necesarios
using Mantenimiento;
using System;
using System.Threading;

namespace Mantenimiento
{
    class Program
    {
        // Metodo principal que inicia el programa
        static void Main(string[] args)
        {
            Console.Title = "Sistema de Mantenimiento";
            MenuPrincipal();
        }

        // Menu principal del sistema
        public static void MenuPrincipal()
        {
            bool salir = false;

            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("=== MENU PRINCIPAL ===");
                Console.WriteLine("1. Gestion de Usuarios");
                Console.WriteLine("2. Gestion de Equipos");
                Console.WriteLine("3. Gestion de Inventario");
                Console.WriteLine("4. Gestion de Ordenes de Mantenimiento");
                Console.WriteLine("5. Notificaciones");
                Console.WriteLine("6. Salir");
                Console.Write("\nSelecciona una opcion: ");

                string opcion = Console.ReadLine()?.Trim();

                switch (opcion)
                {
                    case "1":
                        MenuUsuarios();
                        break;
                    case "2":
                        MenuEquipos();
                        break;
                    case "3":
                        MenuInventario();
                        break;
                    case "4":
                        MenuMantenimiento();
                        break;
                    case "5":
                        MenuNtf();
                        break;
                    case "6":
                        salir = true;
                        break;
                    default:
                        Console.WriteLine("Opcion invalida. Intenta de nuevo.");
                        Thread.Sleep(1500);
                        break;
                }
            }
        }

        // Submenu para gestion de usuarios
        public static void MenuUsuarios()
        {
            bool volver = false;

            while (!volver)
            {
                Console.Clear();
                Console.WriteLine("--- GESTION DE USUARIOS ---");
                Console.WriteLine("1. Crear usuario");
                Console.WriteLine("2. Buscar usuario");
                Console.WriteLine("3. Actualizar usuario");
                Console.WriteLine("4. Consultar por rol");
                Console.WriteLine("5. Volver al menu principal");

                Console.Write("Opcion: ");
                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        UsuarioService.MenuRegistroUsuarios();
                        break;
                    case "2":
                        Console.Write("Nombre a buscar: ");
                        string nombre = Console.ReadLine();
                        UsuarioService.MostrarInfoUsuario(nombre);
                        Console.ReadKey();
                        break;
                    case "3":
                        UsuarioService.ActualizarDatosPersonales();
                        break;
                    case "4":
                        UsuarioService.ConsultarUsuariosPorRol();
                        break;
                    case "5":
                        volver = true;
                        break;
                    default:
                        Console.WriteLine("Opcion invalida.");
                        Thread.Sleep(1500);
                        break;
                }
            }
        }

        // Submenu para gestion de equipos
        public static void MenuEquipos()
        {
            bool volver = false;

            while (!volver)
            {
                Console.Clear();
                Console.WriteLine("--- GESTION DE EQUIPOS ---");
                Console.WriteLine("1. Registrar equipo");
                Console.WriteLine("2. Editar equipo");
                Console.WriteLine("3. Eliminar equipo");
                Console.WriteLine("4. Buscar equipo");
                Console.WriteLine("5. Listar todos los equipos");
                Console.WriteLine("6. Volver al menu principal");

                Console.Write("Opcion: ");
                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        EquipoService.RegistrarEquipo();
                        break;
                    case "2":
                        EquipoService.EditarEquipo();
                        break;
                    case "3":
                        EquipoService.EliminarEquipo();
                        break;
                    case "4":
                        EquipoService.BuscarEquipoPorNombreOCodigo();
                        break;
                    case "5":
                        EquipoService.ListarEquipos();
                        break;
                    case "6":
                        volver = true;
                        break;
                    default:
                        Console.WriteLine("Opcion invalida.");
                        Thread.Sleep(1500);
                        break;
                }
            }
        }

        // Submenu para gestion de ordenes de mantenimiento
        public static void MenuMantenimiento()
        {
            bool salir = false;

            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("=== MENU DE ORDENES DE MANTENIMIENTO ===");
                Console.WriteLine("1. Crear Orden de Mantenimiento");
                Console.WriteLine("2. Registrar Mantenimiento Tecnico");
                Console.WriteLine("3. Finalizar Orden de Mantenimiento");
                Console.WriteLine("4. Listar Ordenes de Mantenimiento");
                Console.WriteLine("0. Salir");
                Console.Write("Selecciona una opcion: ");

                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        OrdenMantenimientoService.CrearOrdenMantenimiento();
                        break;
                    case "2":
                        OrdenMantenimientoService.RegistrarMantenimientoTecnico();
                        break;
                    case "3":
                        OrdenMantenimientoService.FinalizarOrden();
                        break;
                    case "4":
                        OrdenMantenimientoService.ListarOrdenesMantenimiento();
                        break;
                    case "0":
                        salir = true;
                        break;
                    default:
                        Console.WriteLine("Opcion invalida. Intenta de nuevo.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // Submenu para notificaciones
        public static void MenuNtf()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== MENU DE NOTIFICACIONES ===");
                Console.WriteLine("1. Ver todas las notificaciones");
                Console.WriteLine("2. Ver mis notificaciones pendientes y marcar como leidas");
                Console.WriteLine("3. Eliminar Notificaciones");
                Console.WriteLine("4. Salir");
                Console.Write("Selecciona una opcion: ");

                string opcion = Console.ReadLine()?.Trim();

                switch (opcion)
                {
                    case "1":
                        NtfService.VerTodasNotificaciones();
                        break;

                    case "2":
                        Console.Write("Ingresa tu documento de usuario: ");
                        string documentoUsuario = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(documentoUsuario))
                        {
                            Console.WriteLine("Documento invalido.");
                            Console.WriteLine("Presiona una tecla para continuar...");
                            Console.ReadKey();
                        }
                        else
                        {
                            NtfService.VerNotificacionesUsuario(documentoUsuario);
                        }
                        break;

                    case "3":
                        NtfService.EliminarTodasLasNotificaciones();
                        Console.WriteLine("Todas las notificaciones han sido eliminadas.");
                        Console.WriteLine("Presiona una tecla para continuar...");
                        Console.ReadKey();
                        break;
                    case "4":
                        Console.WriteLine("Saliendo del menu de notificaciones...");
                        return;

                    default:
                        Console.WriteLine("Opcion invalida. Intenta de nuevo.");
                        Console.WriteLine("Presiona una tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // Submenu para gestion del inventario
        public static void MenuInventario()
        {
            bool volver = false;

            while (!volver)
            {
                Console.Clear();
                Console.WriteLine("--- GESTION DE INVENTARIO ---");
                Console.WriteLine("1. Consultar todo el inventario");
                Console.WriteLine("2. Registrar entrada de pieza");
                Console.WriteLine("3. Retirar pieza del inventario");
                Console.WriteLine("4. Buscar pieza por nombre o codigo");
                Console.WriteLine("6. Notificar piezas para reposicion");
                Console.WriteLine("7. Volver al menu principal");

                Console.Write("Opcion: ");
                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        InventarioService.ConsultarInventarioDesdeSQL();
                        break;

                    case "2":
                        Console.Write("Codigo de la pieza: ");
                        string codigoEntrada = Console.ReadLine();
                        Console.Write("Nombre de la pieza: ");
                        string nombreEntrada = Console.ReadLine();
                        Console.Write("Cantidad a registrar: ");
                        int cantidadEntrada = Convert.ToInt32(Console.ReadLine());
                        Console.Write("Descripcion del movimiento: ");
                        string descEntrada = Console.ReadLine();
                        Console.Write("ID del usuario que registra: ");
                        int userIdEntrada = Convert.ToInt32(Console.ReadLine());
                        Console.Write("Proveedor de la pieza: ");
                        string proveedor = Console.ReadLine(); ;

                        InventarioService.RegistrarEntradaPiezaEnSQL(codigoEntrada, nombreEntrada, cantidadEntrada, proveedor, descEntrada, userIdEntrada);
                        break;

                    case "3":
                        Console.Write("Codigo de la pieza: ");
                        string codigoSalida = Console.ReadLine();
                        Console.Write("Cantidad a retirar: ");
                        int cantidadSalida = Convert.ToInt32(Console.ReadLine());
                        Console.Write("Descripcion del movimiento: ");
                        string descSalida = Console.ReadLine();
                        Console.Write("ID del usuario que retira: ");
                        int userIdSalida = Convert.ToInt32(Console.ReadLine());

                        InventarioService.RetirarPiezaYActualizarSQL(codigoSalida, cantidadSalida, descSalida, userIdSalida);
                        break;

                    case "4":
                        InventarioService.BuscarPiezaPorNombreOCodigoEnSQL();
                        break;
                    case "6":
                        InventarioService.NotificarReposicion();
                        break;

                    case "7":
                        volver = true;
                        break;

                    default:
                        Console.WriteLine("Opcion invalida.");
                        Thread.Sleep(1500);
                        break;
                }

                if (!volver)
                {
                    Console.WriteLine("\nPresiona cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }
    }
}
