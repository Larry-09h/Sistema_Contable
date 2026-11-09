using System.Runtime.InteropServices;

namespace SistemaContable.Interfaz;

public class FormLogin : Form
{
    // COLORES (rojo, verde, azul)
    static readonly Color ColorBarraSuperior = Color.FromArgb(25, 55, 110);    
    static readonly Color ColorTextoBarra    = Color.White;                  
    static readonly Color ColorBoton         = Color.FromArgb(25, 55, 110);   
    static readonly Color ColorFondo         = Color.FromArgb(244, 246, 250);  
    static readonly Color ColorCampo         = Color.FromArgb(222, 232, 247);  
    static readonly Color ColorTexto         = Color.FromArgb(25, 40, 70);     
    
    readonly TextBox txtUsuario = new()
    {
        Left = 40, Top = 50, Width = 300,
        PlaceholderText = "Usuario"
    };

    readonly TextBox txtClave = new()
    {
        Left = 40, Top = 105, Width = 300,
        PlaceholderText = "Contraseña",
        UseSystemPasswordChar = true
    };

    readonly CheckBox chkMostrar = new()
    {
        Left = 40, Top = 140,
        AutoSize = true,
        Text = "Mostrar contraseña",
        Cursor = Cursors.Hand
    };

    readonly Button btnEntrar = new()
    {
        Left = 40, Top = 178, Width = 300, Height = 42,
        Text = "Entrar"
    };

    int intentos = 0;
    const int MaximoIntentos = 3;
    public string Rol { get; private set; } = "";

    public FormLogin()
    {
        Text = "Iniciar sesión";
        ClientSize = new Size(380, 255);          // tamaño de la ventana
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ColorFondo;
        ForeColor = ColorTexto;
        Font = new Font("Segoe UI", 11F);

        
        txtUsuario.BackColor = ColorCampo;
        txtClave.BackColor = ColorCampo;
        txtUsuario.BorderStyle = BorderStyle.FixedSingle;
        txtClave.BorderStyle = BorderStyle.FixedSingle;

        
        chkMostrar.CheckedChanged += (_, _) =>
            txtClave.UseSystemPasswordChar = !chkMostrar.Checked;

       
        btnEntrar.BackColor = ColorBoton;
        btnEntrar.ForeColor = Color.White;
        btnEntrar.FlatStyle = FlatStyle.Flat;
        btnEntrar.FlatAppearance.BorderSize = 0;
        btnEntrar.Cursor = Cursors.Hand;

        Controls.AddRange(new Control[] { txtUsuario, txtClave, chkMostrar, btnEntrar });
        AcceptButton = btnEntrar;
        btnEntrar.Click += (_, _) => IntentarEntrar();
    }

    void IntentarEntrar()
    {
        string usuario = txtUsuario.Text.Trim();
        string clave = txtClave.Text;

        string rol = ServicioAutenticacion.Validar(usuario, clave);

        if (rol != null)
        {
            Rol = rol;
            DialogResult = DialogResult.OK;
            return;
        }

        intentos++;

        if (intentos >= MaximoIntentos)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        MessageBox.Show("Usuario o contraseña incorrectos.", "Error");
        txtClave.Clear();
        txtClave.Focus();
    }

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr ventana, int atributo, ref int valor, int tamano);

    const int ATRIBUTO_BARRA_OSCURA = 20;
    const int ATRIBUTO_COLOR_BORDE  = 34;
    const int ATRIBUTO_COLOR_BARRA  = 35;
    const int ATRIBUTO_COLOR_TEXTO  = 36;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        int oscuro = 1;
        DwmSetWindowAttribute(Handle, ATRIBUTO_BARRA_OSCURA, ref oscuro, sizeof(int));

        int barra = ColorTranslator.ToWin32(ColorBarraSuperior);
        DwmSetWindowAttribute(Handle, ATRIBUTO_COLOR_BARRA, ref barra, sizeof(int));
        DwmSetWindowAttribute(Handle, ATRIBUTO_COLOR_BORDE, ref barra, sizeof(int));

        int letras = ColorTranslator.ToWin32(ColorTextoBarra);
        DwmSetWindowAttribute(Handle, ATRIBUTO_COLOR_TEXTO, ref letras, sizeof(int));
    }
}