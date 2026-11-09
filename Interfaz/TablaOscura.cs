namespace SistemaContable.Interfaz;

public class TablaOscura : ListView
{
    private int minimoUltima;

    public TablaOscura()
    {
        View = View.Details;
        FullRowSelect = true;
        GridLines = false;
        HideSelection = false;
        MultiSelect = false;
        OwnerDraw = true;
        DoubleBuffered = true;
        BorderStyle = BorderStyle.None;
        BackColor = Tema.Panel;
        ForeColor = Tema.Texto;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Tema.OscurecerControl(this, "DarkMode_Explorer");
        AjustarUltimaColumna();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        AjustarUltimaColumna();
    }

    private void AjustarUltimaColumna()
    {
        if (Columns.Count == 0 || !IsHandleCreated)
            return;
        var ultima = Columns[Columns.Count - 1];
        if (minimoUltima == 0)
            minimoUltima = ultima.Width;
        var otros = 0;
        for (int i = 0; i < Columns.Count - 1; i++)
            otros += Columns[i].Width;
        var disponible = ClientSize.Width - otros - SystemInformation.VerticalScrollBarWidth;
        var nuevo = Math.Max(minimoUltima, disponible);
        if (ultima.Width != nuevo)
            ultima.Width = nuevo;
    }

    private static TextFormatFlags Alineacion(HorizontalAlignment alineacion) =>
        (alineacion == HorizontalAlignment.Right ? TextFormatFlags.Right : TextFormatFlags.Left)
        | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

    protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
    {
        using (var fondo = new SolidBrush(Tema.Superficie))
            e.Graphics.FillRectangle(fondo, e.Bounds);
        using (var lapiz = new Pen(Tema.Acento))
            e.Graphics.DrawLine(lapiz, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        using var fuente = new Font(Font, FontStyle.Bold);
        TextRenderer.DrawText(e.Graphics, e.Header.Text, fuente, Rectangle.Inflate(e.Bounds, -6, 0),
            Tema.AcentoClaro, Alineacion(e.Header.TextAlign));
    }

    protected override void OnDrawItem(DrawListViewItemEventArgs e)
    {
    }

    protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
    {
        var seleccionado = e.Item.Selected;
        var fondo = seleccionado ? Tema.AcentoOscuro : (e.ItemIndex % 2 == 0 ? Tema.Panel : Tema.PanelAlterno);
        using (var pincel = new SolidBrush(fondo))
            e.Graphics.FillRectangle(pincel, e.Bounds);
        using (var lapiz = new Pen(Tema.Borde))
            e.Graphics.DrawLine(lapiz, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, Rectangle.Inflate(e.Bounds, -6, 0),
            seleccionado ? Color.White : Tema.Texto, Alineacion(e.Header.TextAlign));
    }
}
