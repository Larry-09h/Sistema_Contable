namespace SistemaContable.Interfaz;

public static class Dialogo
{
    public static string Pedir(string titulo, string etiqueta, string inicial = "")
    {
        using var formulario = new Form
        {
            Text = titulo,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ClientSize = new Size(400, 112)
        };
        var texto = new Label { Text = etiqueta, Left = 12, Top = 12, AutoSize = true };
        var campo = new TextBox { Left = 12, Top = 36, Width = 376, Text = inicial };
        var aceptar = new Button { Text = "Aceptar", Left = 232, Top = 72, Width = 75, DialogResult = DialogResult.OK };
        var cancelar = new Button { Text = "Cancelar", Left = 313, Top = 72, Width = 75, DialogResult = DialogResult.Cancel };
        formulario.Controls.AddRange(new Control[] { texto, campo, aceptar, cancelar });
        formulario.AcceptButton = aceptar;
        formulario.CancelButton = cancelar;
        formulario.BackColor = Tema.Fondo;
        Tema.Aplicar(formulario);
        return formulario.ShowDialog() == DialogResult.OK ? campo.Text : null;
    }

    public static void Error(string mensaje, string titulo = "Dato no válido") =>
        MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static void Info(string mensaje, string titulo = "Listo") =>
        MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static bool Confirmar(string mensaje, string titulo = "Confirmar") =>
        MessageBox.Show(mensaje, titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}
