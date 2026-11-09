using SistemaContable.Interfaz;

namespace SistemaContable;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
            MessageBox.Show($"{e.Exception.GetType().Name}: {e.Exception.Message}", "Error inesperado",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

        ApplicationConfiguration.Initialize();

        // Activa el motor SQLite con cifrado (SQLCipher). Debe ir antes de abrir la base.
        SQLitePCL.Batteries_V2.Init();

        try
        {
            Almacenamiento.Cargar();
        }
        catch (Exception ex) when (ex is Microsoft.Data.Sqlite.SqliteException
                                   || ex is System.Security.Cryptography.CryptographicException)
        {
            MessageBox.Show("No se pudo abrir la base de datos cifrada.\n\n" +
                "Posibles causas: falta el archivo 'clave.bin' en la carpeta 'datos', " +
                "la base se copió desde otro usuario o computadora de Windows, o el archivo está dañado.\n\n" +
                "No borre contabilidad.db: restaure 'clave.bin' o un respaldo.\n\nDetalle: " + ex.Message,
                "Base de datos protegida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is UnauthorizedAccessException)
        {
            MessageBox.Show($"No se pudieron cargar los datos: {ex.Message}\n\nCorrija o elimine el archivo indicado en la carpeta 'datos' y vuelva a abrir el programa.",
                "Error al iniciar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        
        if (!ServicioAutenticacion.HayUsuarios())
            ServicioAutenticacion.CrearUsuario("Lopez", "12345678", "admin");

        
        using (var login = new FormLogin())
        {
            if (login.ShowDialog() != DialogResult.OK) return;
        }

        Application.Run(new VentanaPrincipal());
    }
}