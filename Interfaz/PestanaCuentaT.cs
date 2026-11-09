namespace SistemaContable.Interfaz;

public class PestanaCuentaT : PestanaBase
{
    private readonly FiltroFechas filtro;
    private readonly ComboBox combo = CrearComboCuentas(380);
    private readonly Label info = new() { AutoSize = true, ForeColor = Color.DimGray };
    private readonly PanelT panel = new();

    public PestanaCuentaT(FiltroFechas filtro)
    {
        this.filtro = filtro;

        var barra = new FlowLayoutPanel { AutoSize = true };
        barra.Controls.Add(new Label { Text = "Cuenta (escriba para buscar):", AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
        barra.Controls.Add(combo);
        Fila(barra);
        Fila(info);
        Fila(panel, true);

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
        var codigo = CodigoDe(combo);
        if (codigo.Length == 0)
        {
            info.Text = "Elija una cuenta para ver su cuenta T.";
            panel.Mostrar(null);
            return;
        }
        var vista = ServicioReportes.CuentaT(codigo, filtro.Desde, filtro.Hasta);
        info.Text = $"{vista.Cuenta.Tipo}, naturaleza {Contabilidad.Naturaleza(vista.Cuenta)}";
        panel.Mostrar(vista);
    }
}
