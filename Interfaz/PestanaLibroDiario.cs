namespace SistemaContable.Interfaz;

public class PestanaLibroDiario : PestanaBase
{
    private readonly FiltroFechas filtro;
    private readonly ListView tabla;

    public PestanaLibroDiario(FiltroFechas filtro)
    {
        this.filtro = filtro;
        tabla = CrearTabla(
            new[] { "N°", "Fecha", "Descripción", "Cuenta", "Debe", "Haber" },
            new[] { 50, 90, 240, 260, 100, 100 },
            new[] { 4, 5 });
        AgregarExportar(tabla, "libro_diario");
        Actualizar();
    }

    public override void Actualizar()
    {
        Limpiar(tabla);
        decimal granDebe = 0, granHaber = 0;
        foreach (var asiento in ServicioReportes.LibroDiario(filtro.Desde, filtro.Hasta))
        {
            for (int i = 0; i < asiento.Lineas.Count; i++)
            {
                var m = asiento.Lineas[i];
                var primero = i == 0;
                Insertar(tabla,
                    primero ? asiento.Id.ToString() : "",
                    primero ? asiento.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
                    primero ? asiento.Descripcion : "",
                    (m.Haber > 0 ? "      " : "") + $"{m.Cuenta} - {Datos.Nombre(m.Cuenta)}",
                    m.Debe > 0 ? Dinero(m.Debe) : "",
                    m.Haber > 0 ? Dinero(m.Haber) : "");
            }
            var sumaDebe = asiento.Lineas.Sum(m => m.Debe);
            var sumaHaber = asiento.Lineas.Sum(m => m.Haber);
            InsertarNegrita(tabla, "", "", "", "Sumas iguales", Dinero(sumaDebe), Dinero(sumaHaber));
            granDebe += sumaDebe;
            granHaber += sumaHaber;
        }
        if (tabla.Items.Count > 0)
            InsertarNegrita(tabla, "", "", "", "TOTAL DEL LIBRO DIARIO", Dinero(granDebe), Dinero(granHaber));
    }
}
