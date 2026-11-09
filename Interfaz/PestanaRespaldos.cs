using System.Diagnostics;

namespace SistemaContable.Interfaz;

public class PestanaRespaldos : PestanaBase
{
    private readonly Action alCambiar;
    private readonly ListView tabla;
    private readonly Label ruta = new() { AutoSize = true, ForeColor = Color.DimGray };

    public PestanaRespaldos(Action alCambiar)
    {
        this.alCambiar = alCambiar;

        var crear = new Button { Text = "Crear respaldo ahora", AutoSize = true };
        crear.Click += (s, e) => Crear();
        var restaurar = new Button { Text = "Restaurar respaldo seleccionado", AutoSize = true };
        restaurar.Click += (s, e) => Restaurar();
        var abrir = new Button { Text = "Abrir carpeta de datos", AutoSize = true };
        abrir.Click += (s, e) => Abrir(Almacenamiento.Carpeta);
        var abrirRespaldos = new Button { Text = "Abrir carpeta de respaldos", AutoSize = true };
        abrirRespaldos.Click += (s, e) => Abrir(Almacenamiento.CarpetaRespaldos);

        var botones = new FlowLayoutPanel { AutoSize = true };
        botones.Controls.AddRange(new Control[] { crear, restaurar, abrir, abrirRespaldos });
        Fila(botones);

        ruta.Text = $"Datos (base de datos cifrada): {Almacenamiento.Carpeta}";
        Fila(ruta);

        tabla = CrearTabla(new[] { "Respaldos disponibles" }, new[] { 500 });
        Actualizar();
    }

    private static void Abrir(string carpeta)
    {
        Directory.CreateDirectory(carpeta);
        Process.Start(new ProcessStartInfo { FileName = carpeta, UseShellExecute = true });
    }

    private void Crear()
    {
        var carpeta = Almacenamiento.CrearRespaldo();
        Dialogo.Info($"Respaldo creado en:\n{carpeta}");
        Actualizar();
    }

    private void Restaurar()
    {
        if (tabla.SelectedItems.Count == 0)
        {
            Dialogo.Info("Seleccione un respaldo de la lista.", "Respaldos");
            return;
        }
        var carpeta = (string)tabla.SelectedItems[0].Tag;
        if (!Dialogo.Confirmar("Se reemplazarán los datos actuales (antes se crea un respaldo automático).\n¿Desea continuar?"))
            return;
        Almacenamiento.Restaurar(carpeta);
        Dialogo.Info("Respaldo restaurado correctamente.");
        alCambiar();
    }

    public override void Actualizar()
    {
        Limpiar(tabla);
        foreach (var carpeta in Almacenamiento.ListarRespaldos())
            Insertar(tabla, Path.GetFileName(carpeta)).Tag = carpeta;
    }
}
