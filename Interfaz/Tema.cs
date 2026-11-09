using System.Runtime.InteropServices;

namespace SistemaContable.Interfaz;

public static class Tema
{
    public static readonly Color Fondo = Color.FromArgb(18, 18, 21);
    public static readonly Color Panel = Color.FromArgb(20, 22, 21);
    public static readonly Color PanelAlterno = Color.FromArgb(26, 29, 27);
    public static readonly Color Superficie = Color.FromArgb(32, 36, 34);
    public static readonly Color Borde = Color.FromArgb(52, 60, 56);
    public static readonly Color Texto = Color.FromArgb(222, 228, 224);
    public static readonly Color TextoSuave = Color.FromArgb(138, 150, 144);
    public static readonly Color Acento = Color.FromArgb(76, 140, 104);
    public static readonly Color AcentoOscuro = Color.FromArgb(34, 70, 52);
    public static readonly Color AcentoClaro = Color.FromArgb(124, 180, 146);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hwnd, string aplicacion, string ids);

    public static void OscurecerControl(Control control, string tema)
    {
        try
        {
            SetWindowTheme(control.Handle, tema, null);
        }
        catch (Exception)
        {
        }
    }

    public static void EstiloBoton(Button boton)
    {
        boton.UseVisualStyleBackColor = false;
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderColor = Acento;
        boton.FlatAppearance.BorderSize = 1;
        boton.FlatAppearance.MouseOverBackColor = AcentoOscuro;
        boton.FlatAppearance.MouseDownBackColor = Acento;
        boton.BackColor = Superficie;
        boton.ForeColor = Texto;
        boton.Cursor = Cursors.Hand;
        boton.Padding = new Padding(8, 2, 8, 2);
    }

    public static void Aplicar(Control control)
    {
        switch (control)
        {
            case ListView:
                break;
            case GrupoOscuro:
                control.BackColor = Fondo;
                control.ForeColor = AcentoClaro;
                break;
            case Button boton:
                EstiloBoton(boton);
                break;
            case TextBox texto:
                texto.BackColor = Superficie;
                texto.ForeColor = Texto;
                texto.BorderStyle = BorderStyle.FixedSingle;
                break;
            case ComboBox combo:
                combo.BackColor = Superficie;
                combo.ForeColor = Texto;
                combo.FlatStyle = FlatStyle.Flat;
                combo.HandleCreated += (s, e) => OscurecerControl(combo, "DarkMode_CFD");
                if (combo.IsHandleCreated)
                    OscurecerControl(combo, "DarkMode_CFD");
                break;
            case Label etiqueta:
                etiqueta.BackColor = Color.Transparent;
                etiqueta.ForeColor = etiqueta.ForeColor == Color.DimGray || etiqueta.ForeColor == TextoSuave
                    ? TextoSuave
                    : Texto;
                break;
            default:
                control.BackColor = Fondo;
                control.ForeColor = Texto;
                break;
        }
        foreach (Control hijo in control.Controls)
            Aplicar(hijo);
    }
}
