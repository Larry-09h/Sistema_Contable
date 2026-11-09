namespace SistemaContable.Interfaz;

public class PanelT : Panel
{
    private CuentaTVista datos;

    public PanelT()
    {
        DoubleBuffered = true;
        AutoScroll = true;
        BackColor = Tema.Panel;
        BorderStyle = BorderStyle.None;
    }

    public void Mostrar(CuentaTVista vista)
    {
        datos = vista;
        var filas = vista == null ? 0 : Math.Max(vista.Debes.Count, vista.Haberes.Count);
        AutoScrollMinSize = new Size(0, 190 + filas * 24);
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (datos == null)
            return;

        var g = e.Graphics;
        g.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);

        var ancho = Math.Min(ClientSize.Width - 40, 560);
        var x0 = Math.Max(10, (ClientSize.Width - ancho) / 2);
        var mitad = x0 + ancho / 2;
        var mitadAncho = ancho / 2;

        using var fuente = new Font("Segoe UI", 11f);
        using var negrita = new Font("Segoe UI", 11f, FontStyle.Bold);
        using var titulo = new Font("Segoe UI", 13f, FontStyle.Bold);
        using var lapiz = new Pen(Tema.Acento, 2);
        using var pincel = new SolidBrush(Tema.Texto);
        using var acento = new SolidBrush(Tema.AcentoClaro);
        using var centro = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var derecha = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };

        var filas = Math.Max(datos.Debes.Count, datos.Haberes.Count);
        var totalDebe = datos.Debes.Sum();
        var totalHaber = datos.Haberes.Sum();

        g.DrawString($"{datos.Cuenta.Codigo} - {datos.Cuenta.Nombre}", titulo, acento,
            new RectangleF(x0, 10, ancho, 34), centro);

        var yEncabezado = 52;
        g.DrawString("DEBE", negrita, acento, new RectangleF(x0, yEncabezado, mitadAncho, 26), centro);
        g.DrawString("HABER", negrita, acento, new RectangleF(mitad, yEncabezado, mitadAncho, 26), centro);

        var yLinea = yEncabezado + 30;
        g.DrawLine(lapiz, x0, yLinea, x0 + ancho, yLinea);

        var y = yLinea + 6;
        for (int i = 0; i < filas; i++)
        {
            if (i < datos.Debes.Count)
                g.DrawString(Contabilidad.Dinero(datos.Debes[i]), fuente, pincel,
                    new RectangleF(x0, y, mitadAncho - 14, 24), derecha);
            if (i < datos.Haberes.Count)
                g.DrawString(Contabilidad.Dinero(datos.Haberes[i]), fuente, pincel,
                    new RectangleF(mitad, y, mitadAncho - 14, 24), derecha);
            y += 24;
        }

        y += 6;
        g.DrawLine(lapiz, x0, y, x0 + ancho, y);
        g.DrawLine(lapiz, mitad, yEncabezado, mitad, y);

        y += 4;
        g.DrawString(Contabilidad.Dinero(totalDebe), negrita, pincel,
            new RectangleF(x0, y, mitadAncho - 14, 26), derecha);
        g.DrawString(Contabilidad.Dinero(totalHaber), negrita, pincel,
            new RectangleF(mitad, y, mitadAncho - 14, 26), derecha);

        var neto = totalDebe - totalHaber;
        var etiqueta = neto > 0 ? "Saldo deudor" : neto < 0 ? "Saldo acreedor" : "Saldo";
        y += 40;
        g.DrawString($"{etiqueta}: {Contabilidad.Dinero(Math.Abs(neto))}", negrita, acento,
            new RectangleF(x0, y, ancho, 28), centro);
    }
}
