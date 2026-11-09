namespace SistemaContable.Interfaz;

public class PestanaMovimientos : PestanaBase
{
    private readonly FiltroFechas filtro;
    private readonly ComboBox combo = CrearComboCuentas(380);
    private readonly TextBox asiento = new() { Width = 80 };
    private readonly ListView tabla;

    public PestanaMovimientos(FiltroFechas filtro)
    {
        this.filtro = filtro;

        var buscar = new Button { Text = "Buscar", AutoSize = true };
        buscar.Click += (s, e) => Actualizar();
        var todos = new Button { Text = "Mostrar todos", AutoSize = true };
        todos.Click += (s, e) =>
        {
            combo.Text = "";
            asiento.Text = "";
            Actualizar();
        };

        Fila(Grupo("Consultar movimientos (deje vacío para no filtrar; el periodo se toma de la barra superior)",
            Campo("Cuenta", combo),
            Campo("N° de asiento", asiento),
            CampoBoton(buscar),
            CampoBoton(todos)));

        tabla = CrearTabla(
            new[] { "Asiento", "Fecha", "Descripción", "Cuenta", "Debe", "Haber" },
            new[] { 70, 90, 240, 260, 100, 100 },
            new[] { 4, 5 });
        AgregarExportar(tabla, "movimientos");
        Actualizar();
    }

    public override void Actualizar()
    {
        CargarCuentas(combo, Datos.Cuentas.OrderBy(c => c.Codigo, StringComparer.Ordinal));
        Limpiar(tabla);

        int? numero = null;
        if (asiento.Text.Trim().Length > 0)
        {
            if (!int.TryParse(asiento.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var n))
            {
                Dialogo.Error("El número de asiento debe ser un entero positivo.");
                return;
            }
            numero = n;
        }

        var movimientos = ServicioReportes.Movimientos(CodigoDe(combo), numero, filtro.Desde, filtro.Hasta);
        foreach (var m in movimientos)
            Insertar(tabla, m.AsientoId.ToString(), m.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                m.Descripcion, $"{m.Cuenta} - {Datos.Nombre(m.Cuenta)}",
                m.Debe > 0 ? Dinero(m.Debe) : "", m.Haber > 0 ? Dinero(m.Haber) : "");
        if (movimientos.Count > 0)
            InsertarNegrita(tabla, "", "", $"Movimientos: {movimientos.Count}", "TOTALES",
                Dinero(movimientos.Sum(m => m.Debe)), Dinero(movimientos.Sum(m => m.Haber)));
    }
}
