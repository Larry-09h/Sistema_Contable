namespace SistemaContable;

public record AsientoVista(int Id, DateTime Fecha, string Descripcion, List<Movimiento> Lineas);

public record CuentaTVista(Cuenta Cuenta, List<decimal> Debes, List<decimal> Haberes);

public record FilaMayor(DateTime Fecha, int Asiento, string Descripcion, decimal Debe, decimal Haber, decimal Saldo);

public record FilaBalanza(string Codigo, string Nombre, decimal Debe, decimal Haber, decimal Deudor, decimal Acreedor);

public record FilaSaldo(string Codigo, string Nombre, decimal Saldo);

public record ResultadoEstado(List<FilaSaldo> Ingresos, List<FilaSaldo> Costos, List<FilaSaldo> Gastos)
{
    public decimal TotalIngresos => Ingresos.Sum(f => f.Saldo);
    public decimal TotalCostos => Costos.Sum(f => f.Saldo);
    public decimal TotalGastos => Gastos.Sum(f => f.Saldo);
    public decimal UtilidadBruta => TotalIngresos - TotalCostos;
    public decimal UtilidadNeta => UtilidadBruta - TotalGastos;
}

public record ResultadoBalance(
    List<FilaSaldo> ActivoCorriente, List<FilaSaldo> ActivoNoCorriente,
    List<FilaSaldo> PasivoCorriente, List<FilaSaldo> PasivoNoCorriente,
    List<FilaSaldo> Capital, decimal Utilidad)
{
    public decimal TotalActivoCorriente => ActivoCorriente.Sum(f => f.Saldo);
    public decimal TotalActivoNoCorriente => ActivoNoCorriente.Sum(f => f.Saldo);
    public decimal TotalActivo => TotalActivoCorriente + TotalActivoNoCorriente;
    public decimal TotalPasivoCorriente => PasivoCorriente.Sum(f => f.Saldo);
    public decimal TotalPasivoNoCorriente => PasivoNoCorriente.Sum(f => f.Saldo);
    public decimal TotalPasivo => TotalPasivoCorriente + TotalPasivoNoCorriente;
    public decimal TotalCapital => Capital.Sum(f => f.Saldo) + Utilidad;
    public bool Cuadra => TotalActivo == TotalPasivo + TotalCapital;
}

public static class ServicioReportes
{
    public static List<Movimiento> Filtrar(DateTime? desde, DateTime? hasta) =>
        Datos.Movimientos
            .Where(m => (desde == null || m.Fecha >= desde) && (hasta == null || m.Fecha <= hasta))
            .ToList();

    public static List<AsientoVista> LibroDiario(DateTime? desde, DateTime? hasta) =>
        Filtrar(desde, hasta)
            .GroupBy(m => m.AsientoId)
            .OrderBy(g => g.First().Fecha)
            .ThenBy(g => g.Key)
            .Select(g => new AsientoVista(g.Key, g.First().Fecha, g.First().Descripcion, g.ToList()))
            .ToList();

    private static List<Movimiento> DeCuenta(string codigo, DateTime? desde, DateTime? hasta) =>
        Filtrar(desde, hasta)
            .Where(m => m.Cuenta == codigo)
            .OrderBy(m => m.Fecha)
            .ThenBy(m => m.AsientoId)
            .ToList();

    public static CuentaTVista CuentaT(string codigo, DateTime? desde, DateTime? hasta)
    {
        var cuenta = Datos.Buscar(codigo) ?? throw new ErrorValidacion($"La cuenta {codigo} no existe");
        var movimientos = DeCuenta(codigo, desde, hasta);
        return new CuentaTVista(
            cuenta,
            movimientos.Where(m => m.Debe > 0).Select(m => m.Debe).ToList(),
            movimientos.Where(m => m.Haber > 0).Select(m => m.Haber).ToList());
    }

    public static (decimal SaldoInicial, List<FilaMayor> Filas) LibroMayor(string codigo, DateTime? desde, DateTime? hasta)
    {
        var cuenta = Datos.Buscar(codigo) ?? throw new ErrorValidacion($"La cuenta {codigo} no existe");
        decimal saldoInicial = 0;
        if (desde != null)
        {
            foreach (var m in Datos.Movimientos.Where(x => x.Cuenta == codigo && x.Fecha < desde))
                saldoInicial += Contabilidad.Saldo(cuenta, m.Debe, m.Haber);
        }
        var saldo = saldoInicial;
        var filas = new List<FilaMayor>();
        foreach (var m in DeCuenta(codigo, desde, hasta))
        {
            saldo += Contabilidad.Saldo(cuenta, m.Debe, m.Haber);
            filas.Add(new FilaMayor(m.Fecha, m.AsientoId, m.Descripcion, m.Debe, m.Haber, saldo));
        }
        return (saldoInicial, filas);
    }

    public static List<FilaBalanza> Balanza(DateTime? desde, DateTime? hasta)
    {
        var totales = Contabilidad.Totales(Filtrar(desde, hasta));
        var filas = new List<FilaBalanza>();
        foreach (var cuenta in Datos.Cuentas.OrderBy(c => c.Codigo, StringComparer.Ordinal))
        {
            if (!totales.TryGetValue(cuenta.Codigo, out var t))
                continue;
            var neto = t.Debe - t.Haber;
            filas.Add(new FilaBalanza(cuenta.Codigo, cuenta.Nombre, t.Debe, t.Haber,
                neto > 0 ? neto : 0, neto < 0 ? -neto : 0));
        }
        return filas;
    }

    private static List<FilaSaldo> Detalle(
        Dictionary<string, (decimal Debe, decimal Haber)> totales, Func<Cuenta, bool> filtro)
    {
        var lista = new List<FilaSaldo>();
        foreach (var cuenta in Datos.Cuentas.OrderBy(c => c.Codigo, StringComparer.Ordinal))
        {
            if (filtro(cuenta) && totales.TryGetValue(cuenta.Codigo, out var t))
                lista.Add(new FilaSaldo(cuenta.Codigo, cuenta.Nombre, Contabilidad.Saldo(cuenta, t.Debe, t.Haber)));
        }
        return lista;
    }

    private static bool EsCosto(Cuenta c) => c.Codigo.StartsWith("51");

    private static bool EsActivoNoCorriente(Cuenta c) => c.Codigo.StartsWith("15") || c.Codigo.StartsWith("16");

    private static bool EsPasivoNoCorriente(Cuenta c) => c.Codigo.StartsWith("22");

    public static ResultadoEstado EstadoResultados(DateTime? desde, DateTime? hasta)
    {
        var totales = Contabilidad.Totales(Filtrar(desde, hasta));
        return new ResultadoEstado(
            Detalle(totales, c => c.Tipo == "INGRESO"),
            Detalle(totales, c => c.Tipo == "GASTO" && EsCosto(c)),
            Detalle(totales, c => c.Tipo == "GASTO" && !EsCosto(c)));
    }

    public static ResultadoBalance BalanceGeneral(DateTime? hasta)
    {
        var totales = Contabilidad.Totales(Filtrar(null, hasta));
        var utilidad = Detalle(totales, c => c.Tipo == "INGRESO").Sum(f => f.Saldo)
                     - Detalle(totales, c => c.Tipo == "GASTO").Sum(f => f.Saldo);
        return new ResultadoBalance(
            Detalle(totales, c => c.Tipo == "ACTIVO" && !EsActivoNoCorriente(c)),
            Detalle(totales, c => c.Tipo == "ACTIVO" && EsActivoNoCorriente(c)),
            Detalle(totales, c => c.Tipo == "PASIVO" && !EsPasivoNoCorriente(c)),
            Detalle(totales, c => c.Tipo == "PASIVO" && EsPasivoNoCorriente(c)),
            Detalle(totales, c => c.Tipo == "CAPITAL"),
            utilidad);
    }

    public static List<Movimiento> Movimientos(string codigoCuenta, int? asiento, DateTime? desde, DateTime? hasta) =>
        Filtrar(desde, hasta)
            .Where(m => string.IsNullOrEmpty(codigoCuenta) || m.Cuenta == codigoCuenta)
            .Where(m => asiento == null || m.AsientoId == asiento)
            .OrderBy(m => m.Fecha)
            .ThenBy(m => m.AsientoId)
            .ToList();
}
