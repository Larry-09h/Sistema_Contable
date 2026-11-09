namespace SistemaContable.Interfaz;

public class PestanaCuentas : PestanaBase
{
    private readonly Action alCambiar;
    private readonly TextBox codigo = new() { Width = 100, MaxLength = 10 };
    private readonly TextBox nombre = new() { Width = 280 };
    private readonly ComboBox tipo = new() { Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox buscar = new() { Width = 220 };
    private readonly ListView tabla;

    public PestanaCuentas(Action alCambiar)
    {
        this.alCambiar = alCambiar;

        tipo.Items.AddRange(Contabilidad.Tipos);
        tipo.SelectedIndex = 0;

        var guardar = new Button { Text = "Guardar cuenta", AutoSize = true };
        guardar.Click += (s, e) => Guardar();

        Fila(Grupo("Nueva cuenta",
            Campo("Código (3 a 10 dígitos)", codigo),
            Campo("Nombre", nombre),
            Campo("Tipo", tipo),
            CampoBoton(guardar)));

        var modificar = new Button { Text = "Modificar nombre", AutoSize = true };
        modificar.Click += (s, e) => Modificar();
        var eliminar = new Button { Text = "Eliminar cuenta", AutoSize = true };
        eliminar.Click += (s, e) => Eliminar();
        var catalogo = new Button { Text = "Cargar catálogo inicial", AutoSize = true };
        catalogo.Click += (s, e) => CargarCatalogo();

        var barra = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
        barra.Controls.Add(new Label { Text = "Buscar:", AutoSize = true, Margin = new Padding(0, 8, 6, 0) });
        barra.Controls.Add(buscar);
        barra.Controls.AddRange(new Control[] { modificar, eliminar, catalogo });
        Fila(barra);
        buscar.TextChanged += (s, e) => Actualizar();

        tabla = CrearTabla(new[] { "Código", "Nombre", "Tipo" }, new[] { 100, 420, 120 });
        Actualizar();
    }

    private void Guardar()
    {
        try
        {
            ServicioContable.CrearCuenta(codigo.Text, nombre.Text, tipo.Text);
        }
        catch (ErrorValidacion ex)
        {
            Dialogo.Error(ex.Message);
            return;
        }
        codigo.Text = "";
        nombre.Text = "";
        alCambiar();
    }

    private ListViewItem Seleccionada()
    {
        if (tabla.SelectedItems.Count == 0)
        {
            Dialogo.Info("Seleccione una cuenta de la tabla.", "Cuentas");
            return null;
        }
        return tabla.SelectedItems[0];
    }

    private void Modificar()
    {
        var item = Seleccionada();
        if (item == null)
            return;
        var codigoCuenta = item.SubItems[0].Text;
        var nuevo = Dialogo.Pedir("Modificar cuenta", $"Nuevo nombre para la cuenta {codigoCuenta}:", item.SubItems[1].Text);
        if (nuevo == null)
            return;
        try
        {
            ServicioContable.ModificarNombre(codigoCuenta, nuevo);
        }
        catch (ErrorValidacion ex)
        {
            Dialogo.Error(ex.Message);
            return;
        }
        alCambiar();
    }

    private void Eliminar()
    {
        var item = Seleccionada();
        if (item == null)
            return;
        var codigoCuenta = item.SubItems[0].Text;
        if (!Dialogo.Confirmar($"¿Eliminar la cuenta {codigoCuenta} - {item.SubItems[1].Text}?"))
            return;
        try
        {
            ServicioContable.EliminarCuenta(codigoCuenta);
        }
        catch (ErrorValidacion ex)
        {
            Dialogo.Error(ex.Message, "No se pudo eliminar");
            return;
        }
        alCambiar();
    }

    private void CargarCatalogo()
    {
        if (!Dialogo.Confirmar("Se agregarán las cuentas del catálogo que todavía no existan.\nLas que ya tiene no se modifican. ¿Continuar?"))
            return;
        var agregadas = Almacenamiento.CargarCatalogoInicial();
        Dialogo.Info($"Se agregaron {agregadas} cuentas.");
        alCambiar();
    }

    public override void Actualizar()
    {
        Limpiar(tabla);
        var texto = buscar.Text.Trim();
        foreach (var c in Datos.Cuentas.OrderBy(x => x.Codigo, StringComparer.Ordinal))
        {
            if (texto.Length > 0
                && !c.Codigo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                && !c.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase))
                continue;
            Insertar(tabla, c.Codigo, c.Nombre, c.Tipo);
        }
    }
}
