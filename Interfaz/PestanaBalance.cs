namespace SistemaContable.Interfaz;

public class PestanaBalance : PestanaBase
{
    private readonly FiltroFechas filtro;
    private readonly ListView tabla;

    public PestanaBalance(FiltroFechas filtro)
    {
        this.filtro = filtro;
        tabla = CrearTabla(new[] { "Concepto", "Monto" }, new[] { 480, 160 }, new[] { 1 });
        AgregarExportar(tabla, "balance_general");
        Actualizar();
    }

    private decimal Seccion(string titulo, List<FilaSaldo> detalle, string etiquetaTotal)
    {
        InsertarNegrita(tabla, titulo, "");
        foreach (var f in detalle)
            Insertar(tabla, $"    {f.Codigo} - {f.Nombre}", Dinero(f.Saldo));
        var total = detalle.Sum(f => f.Saldo);
        InsertarNegrita(tabla, etiquetaTotal, Dinero(total));
        return total;
    }

    public override void Actualizar()
    {
        Limpiar(tabla);
        var b = ServicioReportes.BalanceGeneral(filtro.Hasta);

        InsertarNegrita(tabla, "ACTIVO", "");
        Seccion("  Activo corriente", b.ActivoCorriente, "  Total activo corriente");
        Seccion("  Activo no corriente", b.ActivoNoCorriente, "  Total activo no corriente");
        InsertarNegrita(tabla, "TOTAL ACTIVO", Dinero(b.TotalActivo));

        InsertarNegrita(tabla, "PASIVO", "");
        Seccion("  Pasivo corriente", b.PasivoCorriente, "  Total pasivo corriente");
        Seccion("  Pasivo no corriente", b.PasivoNoCorriente, "  Total pasivo no corriente");
        InsertarNegrita(tabla, "TOTAL PASIVO", Dinero(b.TotalPasivo));

        InsertarNegrita(tabla, "CAPITAL", "");
        foreach (var f in b.Capital)
            Insertar(tabla, $"    {f.Codigo} - {f.Nombre}", Dinero(f.Saldo));
        Insertar(tabla, b.Utilidad >= 0 ? "    Utilidad del periodo" : "    Pérdida del periodo", Dinero(b.Utilidad));
        InsertarNegrita(tabla, "TOTAL CAPITAL", Dinero(b.TotalCapital));

        InsertarNegrita(tabla, "TOTAL PASIVO + CAPITAL", Dinero(b.TotalPasivo + b.TotalCapital));
        InsertarNegrita(tabla, "Resultado", b.Cuadra ? "El balance CUADRA" : "El balance NO CUADRA");
    }
}
