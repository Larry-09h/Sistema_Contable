using System.Security.Cryptography;

public static class ServicioAutenticacion
{
    static readonly string Ruta = Path.Combine(AppContext.BaseDirectory, "datos", "usuarios.txt");

    static string Hashear(string clave, byte[] salt) =>
        Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(clave, salt, 100_000, HashAlgorithmName.SHA256, 32));

    public static bool HayUsuarios() => File.Exists(Ruta) && File.ReadAllLines(Ruta).Length > 0;

    public static void CrearUsuario(string nombre, string clave, string rol)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Ruta)!);
        var salt = RandomNumberGenerator.GetBytes(16);
        File.AppendAllText(Ruta,
            $"{nombre}|{Hashear(clave, salt)}|{Convert.ToBase64String(salt)}|{rol}{Environment.NewLine}");
    }

    
    public static string Validar(string nombre, string clave)
    {
        if (!File.Exists(Ruta)) return null;
        foreach (var linea in File.ReadAllLines(Ruta))
        {
            var p = linea.Split('|');
            if (p.Length != 4 || !p[0].Equals(nombre, StringComparison.OrdinalIgnoreCase)) continue;
            var calculado = Convert.FromBase64String(Hashear(clave, Convert.FromBase64String(p[2])));
            return CryptographicOperations.FixedTimeEquals(calculado, Convert.FromBase64String(p[1])) ? p[3] : null;
        }
        return null;
    }
}