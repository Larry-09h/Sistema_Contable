namespace SistemaContable.Interfaz;

public class VentanaPrincipal : Form
{
    private readonly List<PestanaBase> pestanas = new();
    private readonly List<Button> botones = new();
    private readonly FiltroFechas filtro = new();
    private readonly TextBox desde = new() { Width = 100 };
    private readonly TextBox hasta = new() { Width = 100 };

    public VentanaPrincipal()
    {
        Text = "Sistema Contable Empresarial";
        Size = new Size(1150, 720);
        MinimumSize = new Size(1000, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        Font = new Font("Segoe UI", 9.5f);

        var definicion = new (string Titulo, PestanaBase Pestana)[]
        {
            ("Cuentas", new PestanaCuentas(Refrescar)),
            ("Registrar operación", new PestanaRegistrar(Refrescar)),
            ("Cuentas T", new PestanaCuentaT(filtro)),
            ("Libro diario", new PestanaLibroDiario(filtro)),
            ("Libro mayor", new PestanaMayor(filtro)),
            ("Balanza de comprobación", new PestanaBalanza(filtro)),
            ("Estado de resultados", new PestanaResultados(filtro)),
            ("Balance general", new PestanaBalance(filtro)),
            ("Movimientos", new PestanaMovimientos(filtro)),
            ("Respaldos", new PestanaRespaldos(Refrescar))
        };

        var anfitrion = new Panel { Dock = DockStyle.Fill };
        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Tema.Panel
        };
        menu.Controls.Add(new Label
        {
            Text = "SISTEMA CONTABLE",
            Width = 220,
            Height = 70,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Tema.AcentoClaro,
            BackColor = Tema.Panel,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            Margin = new Padding(0)
        });

        for (int i = 0; i < definicion.Length; i++)
        {
            var (titulo, pestana) = definicion[i];
            var indice = i;
            var boton = CrearBotonMenu(titulo);
            boton.Click += (s, e) => Seleccionar(indice);
            menu.Controls.Add(boton);
            botones.Add(boton);

            pestana.Dock = DockStyle.Fill;
            pestana.Visible = false;
            anfitrion.Controls.Add(pestana);
            pestanas.Add(pestana);
        }

        var salir = CrearBotonMenu("Salir");
        salir.Dock = DockStyle.Bottom;
        salir.Height = 46;
        salir.Click += (s, e) => Close();

        var lateral = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Tema.Panel };
        lateral.Controls.Add(menu);
        lateral.Controls.Add(salir);

        var aplicar = new Button { Text = "Aplicar", AutoSize = true };
        aplicar.Click += (s, e) => Aplicar();
        var quitar = new Button { Text = "Quitar filtro", AutoSize = true };
        quitar.Click += (s, e) =>
        {
            desde.Text = "";
            hasta.Text = "";
            Aplicar();
        };

        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(10, 10, 10, 4)
        };
        barra.Controls.Add(new Label { Text = "Periodo de reportes - Desde (dd/mm/aaaa):", AutoSize = true, Margin = new Padding(0, 7, 4, 0) });
        barra.Controls.Add(desde);
        barra.Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Margin = new Padding(8, 7, 4, 0) });
        barra.Controls.Add(hasta);
        barra.Controls.Add(aplicar);
        barra.Controls.Add(quitar);

        var contenido = new Panel { Dock = DockStyle.Fill };
        contenido.Controls.Add(anfitrion);
        contenido.Controls.Add(barra);
        Tema.Aplicar(contenido);

        Controls.Add(contenido);
        Controls.Add(lateral);

        Seleccionar(0);
    }

    private static Button CrearBotonMenu(string texto)
    {
        var boton = new Button
        {
            Text = texto,
            Width = 220,
            Height = 44,
            Margin = new Padding(0),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(18, 0, 0, 0),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = Tema.Panel,
            ForeColor = Tema.Texto,
            Cursor = Cursors.Hand
        };
        boton.FlatAppearance.BorderSize = 0;
        boton.FlatAppearance.MouseOverBackColor = Tema.Superficie;
        boton.FlatAppearance.MouseDownBackColor = Tema.AcentoOscuro;
        return boton;
    }

    private void Seleccionar(int indice)
    {
        for (int i = 0; i < pestanas.Count; i++)
        {
            var activa = i == indice;
            pestanas[i].Visible = activa;
            botones[i].BackColor = activa ? Tema.AcentoOscuro : Tema.Panel;
            botones[i].ForeColor = activa ? Color.White : Tema.Texto;
        }
        pestanas[indice].Actualizar();
    }

    private void Aplicar()
    {
        try
        {
            var d = Validador.FechaOpcional(desde.Text);
            var h = Validador.FechaOpcional(hasta.Text);
            if (d != null && h != null && d > h)
                throw new ErrorValidacion("La fecha Desde no puede ser posterior a la fecha Hasta");
            filtro.Desde = d;
            filtro.Hasta = h;
        }
        catch (ErrorValidacion ex)
        {
            Dialogo.Error(ex.Message);
            return;
        }
        Refrescar();
    }

    public void Refrescar()
    {
        foreach (var pestana in pestanas)
            pestana.Actualizar();
    }
}
