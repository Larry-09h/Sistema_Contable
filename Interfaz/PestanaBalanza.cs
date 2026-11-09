namespace SistemaContable.Interfaz;

public class PestanaBalanza : PestanaBase
{
    private readonly FiltroFechas filtro;
    private readonly ListView tabla;
    private readonly Label estado = new() { AutoSize = true, Margin = new Padding(0, 8, 0, 8) };

    public PestanaBalanza(FiltroFechas filtro)
    {
        this.filtro = filtro;
        tabla = CrearTabla(
            new[] { "Código", "Cuenta", "Debe", "Haber", "Saldo deudor", "Saldo acreedor" },
            new[] { 80, 250, 110, 110, 110, 110 },
            new[] { 2, 3, 4, 5 });
        Fila(estado);
        AgregarExportar(tabla, "balanza_comprobacion");
        Actualizar();
    }

    public override void Actualizar()
    {
        Limpiar(tabla);
        var filas = ServicioReportes.Balanza(filtro.Desde, filtro.Hasta);
        foreach (var f in filas)
            Insertar(tabla, f.Codigo, f.Nombre, Dinero(f.Debe), Dinero(f.Haber),
                f.Deudor > 0 ? Dinero(f.Deudor) : "", f.Acreedor > 0 ? Dinero(f.Acreedor) : "");

        if (filas.Count == 0)
        {
            estado.Text = "No hay movimientos para mostrar.";
            return;
        }
        var debe = filas.Sum(f => f.Debe);
        var haber = filas.Sum(f => f.Haber);
        var deudor = filas.Sum(f => f.Deudor);
        var acreedor = filas.Sum(f => f.Acreedor);
        InsertarNegrita(tabla, "", "SUMAS", Dinero(debe), Dinero(haber), Dinero(deudor), Dinero(acreedor));
        estado.Text = debe == haber && deudor == acreedor
            ? "La balanza CUADRA: débitos = créditos y saldos deudores = saldos acreedores."
            : "La balanza NO CUADRA. Revise los asientos.";
    }
}
