using Microsoft.Data.Sqlite;

namespace SistemaContable;

public static class Almacenamiento
{
    public static string Carpeta => Path.Combine(AppContext.BaseDirectory, "datos");
    public static string CarpetaRespaldos => Path.Combine(AppContext.BaseDirectory, "respaldos");
    public static string ArchivoBD => Path.Combine(Carpeta, "contabilidad.db");

    // Conexión cifrada (SQLCipher): la clave sale de Seguridad/ClaveBaseDatos.cs
    private static string Conexion => ConexionCifrada(ArchivoBD);

    private static string ConexionCifrada(string archivo) =>
        new SqliteConnectionStringBuilder
        {
            DataSource = archivo,
            Password = ClaveBaseDatos.Obtener()
        }.ToString();

    // Solo para leer bases antiguas que todavía no estaban cifradas.
    private static string ConexionSinClave(string archivo) => $"Data Source={archivo}";

    // Archivos de texto de la versión anterior (solo se usan para migrar).
    private static string TxtCuentas => Path.Combine(Carpeta, "cuentas.txt");
    private static string TxtMovimientos => Path.Combine(Carpeta, "movimientos.txt");

    public static void Cargar()
    {
        Directory.CreateDirectory(Carpeta);

        // Si la base es de una versión anterior (sin cifrar), se cifra automáticamente.
        CifrarSiEsNecesario(ArchivoBD);

        var bdNueva = !File.Exists(ArchivoBD);
        CrearTablas(Conexion);

        if (bdNueva && File.Exists(TxtCuentas))
        {
            // Migración automática desde los .txt viejos.
            Datos.Cuentas = LeerCuentasTxt();
            Datos.Movimientos = LeerMovimientosTxt();
            GuardarCuentas();
            GuardarMovimientos();
            return;
        }

        Datos.Cuentas = LeerCuentas(Conexion);
        Datos.Movimientos = LeerMovimientos(Conexion);
        if (bdNueva)
            CargarCatalogoInicial();
    }

    // ---- Cifrado de bases antiguas ----

    // Una base SQLite sin cifrar siempre empieza con el texto "SQLite format 3".
    // Una base cifrada empieza con bytes al azar.
    private static bool EsBasePlana(string ruta)
    {
        using var fs = File.OpenRead(ruta);
        var cabecera = new byte[15];
        var leidos = fs.Read(cabecera, 0, cabecera.Length);
        return leidos == 15 && Encoding.ASCII.GetString(cabecera) == "SQLite format 3";
    }

    private static void CifrarSiEsNecesario(string ruta)
    {
        if (!File.Exists(ruta) || !EsBasePlana(ruta))
            return;

        var cuentas = LeerCuentas(ConexionSinClave(ruta));
        var movimientos = LeerMovimientos(ConexionSinClave(ruta));
        SqliteConnection.ClearAllPools();

        var temporal = ruta + ".cifrada";
        if (File.Exists(temporal))
            File.Delete(temporal);

        var cs = ConexionCifrada(temporal);
        CrearTablas(cs);
        EscribirCuentas(cs, cuentas);
        EscribirMovimientos(cs, movimientos);

        // Se comprueba la copia cifrada ANTES de reemplazar la original.
        var cuentasCifradas = LeerCuentas(cs).Count;
        var movimientosCifrados = LeerMovimientos(cs).Count;
        SqliteConnection.ClearAllPools();
        if (cuentasCifradas != cuentas.Count || movimientosCifrados != movimientos.Count)
        {
            File.Delete(temporal);
            throw new InvalidDataException(
                "No se pudo cifrar la base de datos. No se modificó nada.");
        }

        File.Move(temporal, ruta, overwrite: true);
    }

    // ---- Tablas, lectura y escritura ----

    private static void CrearTablas(string conexion)
    {
        using var cn = new SqliteConnection(conexion);
        cn.Open();
        var cmd = cn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS cuentas(
                codigo TEXT PRIMARY KEY,
                nombre TEXT NOT NULL,
                tipo   TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS movimientos(
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                asiento_id  INTEGER NOT NULL,
                fecha       TEXT NOT NULL,
                descripcion TEXT NOT NULL,
                cuenta      TEXT NOT NULL,
                debe        TEXT NOT NULL,
                haber       TEXT NOT NULL);";
        cmd.ExecuteNonQuery();
    }

    public static void GuardarCuentas() => EscribirCuentas(Conexion, Datos.Cuentas);

    public static void GuardarMovimientos() => EscribirMovimientos(Conexion, Datos.Movimientos);

    private static void EscribirCuentas(string conexion, IEnumerable<Cuenta> cuentas)
    {
        using var cn = new SqliteConnection(conexion);
        cn.Open();
        using var tx = cn.BeginTransaction();

        var borrar = cn.CreateCommand();
        borrar.Transaction = tx;
        borrar.CommandText = "DELETE FROM cuentas";
        borrar.ExecuteNonQuery();

        foreach (var c in cuentas.OrderBy(c => c.Codigo, StringComparer.Ordinal))
        {
            var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "INSERT INTO cuentas(codigo,nombre,tipo) VALUES($c,$n,$t)";
            cmd.Parameters.AddWithValue("$c", c.Codigo);
            cmd.Parameters.AddWithValue("$n", c.Nombre);
            cmd.Parameters.AddWithValue("$t", c.Tipo);
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    private static void EscribirMovimientos(string conexion, IEnumerable<Movimiento> movimientos)
    {
        using var cn = new SqliteConnection(conexion);
        cn.Open();
        using var tx = cn.BeginTransaction();

        var borrar = cn.CreateCommand();
        borrar.Transaction = tx;
        borrar.CommandText = "DELETE FROM movimientos";
        borrar.ExecuteNonQuery();

        foreach (var m in movimientos)
        {
            var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"INSERT INTO movimientos(asiento_id,fecha,descripcion,cuenta,debe,haber)
                                VALUES($a,$f,$d,$c,$de,$ha)";
            cmd.Parameters.AddWithValue("$a", m.AsientoId);
            cmd.Parameters.AddWithValue("$f", m.Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("$d", m.Descripcion);
            cmd.Parameters.AddWithValue("$c", m.Cuenta);
            cmd.Parameters.AddWithValue("$de", m.Debe.ToString("0.00", CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("$ha", m.Haber.ToString("0.00", CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    private static List<Cuenta> LeerCuentas(string conexion)
    {
        var lista = new List<Cuenta>();
        using var cn = new SqliteConnection(conexion);
        cn.Open();
        var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT codigo,nombre,tipo FROM cuentas ORDER BY codigo";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            lista.Add(new Cuenta { Codigo = r.GetString(0), Nombre = r.GetString(1), Tipo = r.GetString(2) });
        return lista;
    }

    private static List<Movimiento> LeerMovimientos(string conexion)
    {
        var lista = new List<Movimiento>();
        using var cn = new SqliteConnection(conexion);
        cn.Open();
        var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT asiento_id,fecha,descripcion,cuenta,debe,haber FROM movimientos ORDER BY id";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            lista.Add(new Movimiento
            {
                AsientoId = r.GetInt32(0),
                Fecha = DateTime.ParseExact(r.GetString(1), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Descripcion = r.GetString(2),
                Cuenta = r.GetString(3),
                Debe = decimal.Parse(r.GetString(4), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                Haber = decimal.Parse(r.GetString(5), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)
            });
        return lista;
    }

    // ---- Lectura de los .txt viejos (solo para migrar) ----

    private static List<Cuenta> LeerCuentasTxt()
    {
        var lista = new List<Cuenta>();
        var lineas = File.ReadAllLines(TxtCuentas, Encoding.UTF8);
        for (int i = 0; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i]))
                continue;
            var partes = lineas[i].Split('|');
            if (partes.Length != 3 || !Contabilidad.Tipos.Contains(partes[2]))
                throw new InvalidDataException($"cuentas.txt: la línea {i + 1} está dañada.");
            lista.Add(new Cuenta { Codigo = partes[0], Nombre = partes[1], Tipo = partes[2] });
        }
        return lista;
    }

    private static List<Movimiento> LeerMovimientosTxt()
    {
        var lista = new List<Movimiento>();
        if (!File.Exists(TxtMovimientos))
            return lista;
        var lineas = File.ReadAllLines(TxtMovimientos, Encoding.UTF8);
        for (int i = 0; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i]))
                continue;
            var p = lineas[i].Split('|');
            try
            {
                if (p.Length != 6)
                    throw new FormatException();
                lista.Add(new Movimiento
                {
                    AsientoId = int.Parse(p[0], CultureInfo.InvariantCulture),
                    Fecha = DateTime.ParseExact(p[1], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Descripcion = p[2],
                    Cuenta = p[3],
                    Debe = decimal.Parse(p[4], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                    Haber = decimal.Parse(p[5], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)
                });
            }
            catch (FormatException)
            {
                throw new InvalidDataException($"movimientos.txt: la línea {i + 1} está dañada.");
            }
        }
        return lista;
    }

    // ---- Catálogo, respaldos y restauración ----

    public static int CargarCatalogoInicial()
    {
        int agregadas = 0;
        foreach (var (codigo, nombre, tipo) in CatalogoInicial.Cuentas)
        {
            if (Datos.Buscar(codigo) != null
                || Datos.Cuentas.Any(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                continue;
            Datos.Cuentas.Add(new Cuenta { Codigo = codigo, Nombre = nombre, Tipo = tipo });
            agregadas++;
        }
        GuardarCuentas();
        return agregadas;
    }

    // El respaldo es una copia de la base ya cifrada.
    // Para restaurarlo en otra computadora o usuario también se necesita clave.bin.
    public static string CrearRespaldo()
    {
        GuardarCuentas();
        GuardarMovimientos();
        var destino = Path.Combine(CarpetaRespaldos, $"respaldo_{DateTime.Now:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(destino);
        SqliteConnection.ClearAllPools();
        File.Copy(ArchivoBD, Path.Combine(destino, "contabilidad.db"), true);
        return destino;
    }

    public static List<string> ListarRespaldos()
    {
        if (!Directory.Exists(CarpetaRespaldos))
            return new List<string>();
        return Directory.GetDirectories(CarpetaRespaldos)
            .Where(d => File.Exists(Path.Combine(d, "contabilidad.db")))
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToList();
    }

    public static void Restaurar(string carpeta)
    {
        CrearRespaldo();
        SqliteConnection.ClearAllPools();

        // Se prepara y se comprueba una copia antes de tocar la base actual.
        var temporal = ArchivoBD + ".restaurar";
        File.Copy(Path.Combine(carpeta, "contabilidad.db"), temporal, true);
        try
        {
            CifrarSiEsNecesario(temporal);          // respaldos viejos sin cifrar
            LeerCuentas(ConexionCifrada(temporal)); // falla si la clave no coincide
            SqliteConnection.ClearAllPools();
            File.Move(temporal, ArchivoBD, overwrite: true);
        }
        catch
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(temporal))
                File.Delete(temporal);
            throw;
        }

        Datos.Cuentas = LeerCuentas(Conexion);
        Datos.Movimientos = LeerMovimientos(Conexion);
    }
}
