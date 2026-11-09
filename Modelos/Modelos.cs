namespace SistemaContable;

public class Cuenta
{
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Tipo { get; set; }
}

public class Movimiento
{
    public int AsientoId { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; }
    public string Cuenta { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}

public static class Datos
{
    public static List<Cuenta> Cuentas = new();
    public static List<Movimiento> Movimientos = new();

    public static Cuenta Buscar(string codigo) => Cuentas.FirstOrDefault(c => c.Codigo == codigo);

    public static string Nombre(string codigo) => Buscar(codigo)?.Nombre ?? "(cuenta desconocida)";

    public static int SiguienteAsiento() => Movimientos.Count == 0 ? 1 : Movimientos.Max(m => m.AsientoId) + 1;
}

public static class Contabilidad
{
    public static readonly string[] Tipos = { "ACTIVO", "PASIVO", "CAPITAL", "INGRESO", "GASTO" };

    public static bool EsDeudora(string tipo) => tipo == "ACTIVO" || tipo == "GASTO";

    public static decimal Saldo(Cuenta cuenta, decimal debe, decimal haber) =>
        EsDeudora(cuenta.Tipo) ? debe - haber : haber - debe;

    public static string Dinero(decimal valor) => valor.ToString("N2", CultureInfo.InvariantCulture);

    public static string Naturaleza(Cuenta cuenta) => EsDeudora(cuenta.Tipo) ? "deudora" : "acreedora";

    public static Dictionary<string, (decimal Debe, decimal Haber)> Totales(IEnumerable<Movimiento> movimientos) =>
        movimientos
            .GroupBy(m => m.Cuenta)
            .ToDictionary(g => g.Key, g => (Debe: g.Sum(x => x.Debe), Haber: g.Sum(x => x.Haber)));
}
