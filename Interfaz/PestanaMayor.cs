namespace SistemaContable.Interfaz;

public class PestanaMayor : PestanaBase
{
    private readonly FiltroFechas filtro;
    private readonly ComboBox combo = CrearComboCuentas(380);
    private readonly ListView tabla;

    public PestanaMayor(FiltroFechas filtro)
    {
        this.filtro = filtro;

        var barra = new FlowLayoutPanel { AutoSize = true };
        barra.Controls.Add(new Label { Text = "Cuenta (escriba para buscar):", AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
        barra.Controls.Add(combo);
        Fila(barra);

        tabla = CrearTabla(
            new[] { "Fecha", "Asiento", "Descripción", "Debe", "Haber", "Saldo" },
            new[] { 90, 70, 280, 110, 110, 110 },
            new[] { 3, 4, 5 });
        AgregarExportar(tabla, "libro_mayor");

        combo.TextChanged += (s, e) => Mostrar();
        Actualizar();
    }

    public override void Actualizar()
    {
        CargarCuentas(combo, Datos.Cuentas.OrderBy(c => c.Codigo, StringComparer.Ordinal));
        Mostrar();
    }

    private void Mostrar()
    {
        Limpiar(tabla);
        var codigo = CodigoDe(combo);
        if (codigo.Length == 0)
            return;
        var (saldoInicial, filas) = ServicioReportes.LibroMayor(codigo, filtro.Desde, filtro.Hasta);
        if (filtro.Desde != null)
            InsertarNegrita(tabla, "", "", "Saldo anterior al periodo", "", "", Dinero(saldoInicial));
        foreach (var f in filas)
            Insertar(tabla, f.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), f.Asiento.ToString(),
                f.Descripcion, f.Debe > 0 ? Dinero(f.Debe) : "", f.Haber > 0 ? Dinero(f.Haber) : "", Dinero(f.Saldo));
        if (filas.Count > 0)
            InsertarNegrita(tabla, "", "", "Totales", Dinero(filas.Sum(f => f.Debe)), Dinero(filas.Sum(f => f.Haber)),
                Dinero(filas[^1].Saldo));
    }
}
