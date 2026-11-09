using System.Security.Cryptography;

namespace SistemaContable;

/// <summary>
/// Clave con la que se cifra la base de datos (contabilidad.db).
/// Se genera al azar la primera vez y se guarda en datos/clave.bin protegida
/// con DPAPI de Windows: solo la puede abrir el mismo usuario de Windows
/// en esta misma computadora.
/// </summary>
public static class ClaveBaseDatos
{
    private static string _clave;

    private static string RutaClave =>
        Path.Combine(AppContext.BaseDirectory, "datos", "clave.bin");

    public static string Obtener()
    {
        if (_clave != null)
            return _clave;

        Directory.CreateDirectory(Path.GetDirectoryName(RutaClave)!);

        byte[] bytes;
        if (File.Exists(RutaClave))
        {
            var protegida = File.ReadAllBytes(RutaClave);
            bytes = ProtectedData.Unprotect(protegida, null, DataProtectionScope.CurrentUser);
        }
        else
        {
            bytes = RandomNumberGenerator.GetBytes(32); // 256 bits
            var protegida = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(RutaClave, protegida);
        }

        _clave = Convert.ToHexString(bytes);
        return _clave;
    }
}
