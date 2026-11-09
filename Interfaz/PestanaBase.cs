namespace SistemaContable.Interfaz;

public abstract class PestanaBase : UserControl
{
    protected readonly TableLayoutPanel Raiz = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 0,
        Padding = new Padding(10)
    };

    protected PestanaBase()
    {
        Dock = DockStyle.Fill;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        Raiz.BackColor = Tema.Fondo;
        Raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(Raiz);
    }

    public static string Dinero(decimal valor) => valor.ToString("N2", CultureInfo.InvariantCulture);

    protected void Fila(Control control, bool expandir = false)
    {
        Raiz.RowStyles.Add(expandir ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.AutoSize));
        control.Dock = DockStyle.Fill;
        Raiz.RowCount++;
        Raiz.Controls.Add(control, 0, Raiz.RowCount - 1);
    }

    protected ListView CrearTabla(string[] columnas, int[] anchos, int[] derecha = null)
    {
        var tabla = new TablaOscura();
        for (int i = 0; i < columnas.Length; i++)
        {
            var alineacion = derecha != null && derecha.Contains(i) && i > 0
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left;
            tabla.Columns.Add(columnas[i], anchos[i], alineacion);
        }
        Fila(tabla, true);
        return tabla;
    }

    protected static void Limpiar(ListView tabla) => tabla.Items.Clear();

    protected static ListViewItem Insertar(ListView tabla, params string[] valores)
    {
        var item = new ListViewItem(valores);
        tabla.Items.Add(item);
        return item;
    }

    protected static ListViewItem InsertarNegrita(ListView tabla, params string[] valores)
    {
        var item = Insertar(tabla, valores);
        item.Font = new Font(tabla.Font, FontStyle.Bold);
        return item;
    }

    protected static Control Campo(string etiqueta, Control control)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Margin = new Padding(0, 0, 8, 0)
        };
        panel.Controls.Add(new Label { Text = etiqueta, AutoSize = true });
        panel.Controls.Add(control);
        return panel;
    }

    protected static Control CampoBoton(Button boton)
    {
        var panel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true };
        panel.Controls.Add(new Label { Text = " ", AutoSize = true });
        panel.Controls.Add(boton);
        return panel;
    }

    protected static GroupBox Grupo(string titulo, params Control[] controles)
    {
        var flujo = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true
        };
        flujo.Controls.AddRange(controles);
        var grupo = new GrupoOscuro
        {
            Text = titulo,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8)
        };
        grupo.Controls.Add(flujo);
        return grupo;
    }

    protected static ComboBox CrearComboCuentas(int ancho) => new()
    {
        Width = ancho,
        DropDownStyle = ComboBoxStyle.DropDown,
        AutoCompleteMode = AutoCompleteMode.SuggestAppend,
        AutoCompleteSource = AutoCompleteSource.ListItems
    };

    protected static void CargarCuentas(ComboBox combo, IEnumerable<Cuenta> cuentas)
    {
        var texto = combo.Text;
        combo.Items.Clear();
        foreach (var c in cuentas)
            combo.Items.Add($"{c.Codigo} - {c.Nombre}");
        combo.Text = combo.Items.Contains(texto) ? texto : "";
    }

    protected static string CodigoDe(ComboBox combo) =>
        combo.Items.Contains(combo.Text) ? combo.Text.Split(" - ")[0] : "";

    protected void AgregarExportar(ListView tabla, string archivo)
    {
        var boton = new Button { Text = "Exportar a CSV", AutoSize = true };
        boton.Click += (s, e) => ExportarCsv(tabla, archivo);
        var contenedor = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
        contenedor.Controls.Add(boton);
        Fila(contenedor);
    }

    private static string Csv(string texto)
    {
        texto ??= "";
        if (Regex.IsMatch(texto, @"^-?[0-9,]+\.[0-9]{2}$"))
            texto = texto.Replace(",", "");
        if (texto.Contains(',') || texto.Contains('"') || texto.Contains('\n'))
            texto = "\"" + texto.Replace("\"", "\"\"") + "\"";
        return texto;
    }

    private static void ExportarCsv(ListView tabla, string archivo)
    {
        using var dialogo = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"{archivo}_{DateTime.Today:yyyyMMdd}.csv"
        };
        if (dialogo.ShowDialog() != DialogResult.OK)
            return;

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", tabla.Columns.Cast<ColumnHeader>().Select(c => Csv(c.Text))));
        foreach (ListViewItem item in tabla.Items)
            sb.AppendLine(string.Join(",", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => Csv(s.Text))));

        try
        {
            File.WriteAllText(dialogo.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show("Archivo exportado correctamente", "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (IOException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo exportar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public virtual void Actualizar() { }
}
