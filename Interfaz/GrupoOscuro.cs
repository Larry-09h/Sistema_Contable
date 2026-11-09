namespace SistemaContable.Interfaz;

public class GrupoOscuro : GroupBox
{
    public GrupoOscuro()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                 | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Tema.Fondo;
        ForeColor = Tema.AcentoClaro;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Tema.Fondo);
        var tamano = TextRenderer.MeasureText(Text, Font);
        var marco = new Rectangle(0, tamano.Height / 2, Width - 1, Height - tamano.Height / 2 - 1);
        using (var lapiz = new Pen(Tema.Borde))
            g.DrawRectangle(lapiz, marco);
        using (var pincel = new SolidBrush(Tema.Fondo))
            g.FillRectangle(pincel, new Rectangle(8, 0, tamano.Width + 6, tamano.Height));
        TextRenderer.DrawText(g, Text, Font, new Point(10, 0), ForeColor);
    }
}
