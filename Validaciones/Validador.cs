namespace SistemaContable;

public class ErrorValidacion : Exception
{
    public ErrorValidacion(string mensaje) : base(mensaje) { }
}

public static class Validador
{
    private static readonly Regex PatronTexto = new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9 .,\-]+\z");
    private static readonly Regex PatronCodigo = new(@"^[0-9]{3,10}\z");
    private static readonly Regex PatronMonto = new(@"^[0-9]+(\.[0-9]{1,2})?\z");

    public const decimal MontoMaximo = 999999999.99m;
    public const int AnioMinimo = 2000;
    public const int DiasFuturoMaximo = 365;

    public static string Texto(string valor, string campo, int maximo = 100)
    {
        valor = (valor ?? "").Trim();
        if (valor.Length == 0)
            throw new ErrorValidacion($"{campo}: no puede estar vacío");
        if (valor.Length > maximo)
            throw new ErrorValidacion($"{campo}: máximo {maximo} caracteres");
        if (!PatronTexto.IsMatch(valor))
            throw new ErrorValidacion($"{campo}: solo letras, números, espacios y los signos . , -");
        return valor;
    }

    public static string Codigo(string valor)
    {
        valor = (valor ?? "").Trim();
        if (!PatronCodigo.IsMatch(valor))
            throw new ErrorValidacion("Código: solo números, de 3 a 10 dígitos");
        return valor;
    }

    public static string Tipo(string valor)
    {
        valor = (valor ?? "").Trim().ToUpperInvariant();
        if (!Contabilidad.Tipos.Contains(valor))
            throw new ErrorValidacion($"Tipo: debe ser uno de {string.Join(", ", Contabilidad.Tipos)}");
        return valor;
    }

    public static decimal Monto(string valor, string campo)
    {
        valor = (valor ?? "").Trim();
        if (!PatronMonto.IsMatch(valor))
            throw new ErrorValidacion($"{campo}: número positivo con punto decimal y máximo 2 decimales (ej. 1500.50)");
        var monto = decimal.Parse(valor, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
        if (monto > MontoMaximo)
            throw new ErrorValidacion($"{campo}: monto demasiado grande");
        return monto;
    }

    public static DateTime Fecha(string valor)
    {
        valor = (valor ?? "").Trim();
        string[] formatos = { "dd/MM/yyyy", "d/M/yyyy" };
        if (!DateTime.TryParseExact(valor, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            throw new ErrorValidacion("Fecha: use el formato dd/mm/aaaa con una fecha real");
        if (fecha.Year < AnioMinimo)
            throw new ErrorValidacion($"Fecha: no puede ser anterior al año {AnioMinimo}");
        if (fecha > DateTime.Today.AddDays(DiasFuturoMaximo))
            throw new ErrorValidacion("Fecha: no puede estar más de un año en el futuro");
        return fecha;
    }

    public static DateTime? FechaOpcional(string valor)
    {
        valor = (valor ?? "").Trim();
        return valor.Length == 0 ? null : Fecha(valor);
    }
}
