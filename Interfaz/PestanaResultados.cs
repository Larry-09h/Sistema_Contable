namespace SistemaContable.Interfaz;

public class PestanaResultados : PestanaBase
{
    private readonly FiltroFechas filtro;
    private readonly ListView tabla;

    public PestanaResultados(FiltroFechas filtro)
    {
        this.filtro = filtro;
        tabla = CrearTabla(new[] { "Concepto", "Monto" }, new[] { 480, 160 }, new[] { 1 });
        AgregarExportar(tabla, "estado_resultados");
        Actualizar();
    }

    private void Seccion(string titulo, List<FilaSaldo> detalle, string etiquetaTotal, decimal total)
    {
        InsertarNegrita(tabla, titulo, "");
        foreach (var f in detalle)
            Insertar(tabla, $"    {f.Codigo} - {f.Nombre}", Dinero(f.Saldo));
        InsertarNegrita(tabla, etiquetaTotal, Dinero(total));
    }

    public override void Actualizar()
    {
        Limpiar(tabla);
        var r = ServicioReportes.EstadoResultados(filtro.Desde, filtro.Hasta);
        Seccion("INGRESOS", r.Ingresos, "Total de ingresos", r.TotalIngresos);
        Seccion("(-) COSTOS", r.Costos, "Total de costos", r.TotalCostos);
        InsertarNegrita(tabla, r.UtilidadBruta >= 0 ? "UTILIDAD BRUTA" : "PÉRDIDA BRUTA", Dinero(r.UtilidadBruta));
        Seccion("(-) GASTOS DE OPERACIÓN Y OTROS", r.Gastos, "Total de gastos", r.TotalGastos);
        InsertarNegrita(tabla, r.UtilidadNeta >= 0 ? "UTILIDAD NETA DEL PERIODO" : "PÉRDIDA NETA DEL PERIODO", Dinero(r.UtilidadNeta));
    }
}
