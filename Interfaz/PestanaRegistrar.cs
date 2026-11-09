namespace SistemaContable.Interfaz;

public class PestanaRegistrar : PestanaBase
{
    private readonly Action alCambiar;
    private readonly List<(string Codigo, string Debe, string Haber)> lineas = new();
    private readonly TextBox fecha = new() { Width = 110, Text = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) };
    private readonly TextBox descripcion = new() { Width = 450 };
    private readonly ComboBox combo = CrearComboCuentas(340);
    private readonly TextBox debe = new() { Width = 100 };
    private readonly TextBox haber = new() { Width = 100 };
    private readonly Label naturaleza = new() { AutoSize = true, ForeColor = Color.DimGray };
    private readonly Label total = new() { Text = "Debe: 0.00    Haber: 0.00", AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly ListView tabla;

    public PestanaRegistrar(Action alCambiar)
    {
        this.alCambiar = alCambiar;

        Fila(Grupo("Datos de la operación",
            Campo("Fecha (dd/mm/aaaa)", fecha),
            Campo("Descripción", descripcion)));

        var agregar = new Button { Text = "Agregar línea", AutoSize = true };
        agregar.Click += (s, e) => AgregarLinea();

        Fila(Grupo("Agregar línea al asiento",
            Campo("Cuenta (escriba para buscar)", combo),
            Campo("Debe", debe),
            Campo("Haber", haber),
            CampoBoton(agregar)));
        Fila(naturaleza);

        tabla = CrearTabla(new[] { "Cuenta", "Debe", "Haber" }, new[] { 420, 130, 130 }, new[] { 1, 2 });

        var registrar = new Button { Text = "Registrar asiento", AutoSize = true };
        registrar.Click += (s, e) => Registrar();
        var limpiar = new Button { Text = "Limpiar todo", AutoSize = true };
        limpiar.Click += (s, e) => LimpiarFormulario();
        var quitar = new Button { Text = "Quitar línea", AutoSize = true };
        quitar.Click += (s, e) => QuitarLinea();

        var botones = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.Right };
        botones.Controls.AddRange(new Control[] { quitar, limpiar, registrar });

        var pie = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, AutoSize = true };
        pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pie.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pie.Controls.Add(total, 0, 0);
        pie.Controls.Add(botones, 1, 0);
        Fila(pie);

        combo.TextChanged += (s, e) => MostrarNaturaleza();
        Actualizar();
    }

    private void MostrarNaturaleza()
    {
        var cuenta = Datos.Buscar(CodigoDe(combo));
        naturaleza.Text = cuenta == null
            ? ""
            : $"Naturaleza {Contabilidad.Naturaleza(cuenta)}: " +
              (Contabilidad.EsDeudora(cuenta.Tipo)
                  ? "aumenta por el Debe y disminuye por el Haber."
                  : "aumenta por el Haber y disminuye por el Debe.");
    }

    private void AgregarLinea()
    {
        var codigo = CodigoDe(combo);
        if (codigo.Length == 0)
        {
            Dialogo.Error("Elija una cuenta de la lista.");
            return;
        }
        var d = string.IsNullOrWhiteSpace(debe.Text) ? "0" : debe.Text.Trim();
        var h = string.IsNullOrWhiteSpace(haber.Text) ? "0" : haber.Text.Trim();
        try
        {
            var montoDebe = Validador.Monto(d, "Debe");
            var montoHaber = Validador.Monto(h, "Haber");
            if (montoDebe > 0 && montoHaber > 0)
                throw new ErrorValidacion("Un movimiento no puede tener Debe y Haber a la vez");
            if (montoDebe == 0 && montoHaber == 0)
                throw new ErrorValidacion("Un movimiento debe tener un monto mayor a cero");
        }
        catch (ErrorValidacion ex)
        {
            Dialogo.Error(ex.Message);
            return;
        }
        lineas.Add((codigo, d, h));
        debe.Text = "";
        haber.Text = "";
        combo.Text = "";
        MostrarLineas();
        combo.Focus();
    }

    private void QuitarLinea()
    {
        if (tabla.SelectedIndices.Count > 0)
        {
            lineas.RemoveAt(tabla.SelectedIndices[0]);
            MostrarLineas();
        }
    }

    private void MostrarLineas()
    {
        Limpiar(tabla);
        decimal totalDebe = 0, totalHaber = 0;
        foreach (var (codigo, d, h) in lineas)
        {
            var montoDebe = Validador.Monto(d, "Debe");
            var montoHaber = Validador.Monto(h, "Haber");
            Insertar(tabla, $"{codigo} - {Datos.Nombre(codigo)}", Dinero(montoDebe), Dinero(montoHaber));
            totalDebe += montoDebe;
            totalHaber += montoHaber;
        }
        total.Text = $"Debe: {Dinero(totalDebe)}    Haber: {Dinero(totalHaber)}" +
                     (totalDebe == totalHaber ? "" : $"    Diferencia: {Dinero(Math.Abs(totalDebe - totalHaber))}");
    }

    private void Registrar()
    {
        int id;
        try
        {
            id = ServicioContable.RegistrarAsiento(fecha.Text, descripcion.Text, lineas);
        }
        catch (ErrorValidacion ex)
        {
            Dialogo.Error(ex.Message, "No se pudo registrar");
            return;
        }
        Dialogo.Info($"Asiento N° {id} registrado correctamente.");
        LimpiarFormulario();
        alCambiar();
    }

    private void LimpiarFormulario()
    {
        lineas.Clear();
        descripcion.Text = "";
        MostrarLineas();
    }

    public override void Actualizar() => CargarCuentas(combo, Datos.Cuentas.OrderBy(c => c.Codigo, StringComparer.Ordinal));
}
