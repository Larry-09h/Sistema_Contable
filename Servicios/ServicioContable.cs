namespace SistemaContable;

public static class ServicioContable
{
    public static void CrearCuenta(string codigo, string nombre, string tipo)
    {
        codigo = Validador.Codigo(codigo);
        nombre = Validador.Texto(nombre, "Nombre", 60);
        tipo = Validador.Tipo(tipo);
        if (Datos.Buscar(codigo) != null)
            throw new ErrorValidacion($"Ya existe la cuenta {codigo}");
        if (Datos.Cuentas.Any(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            throw new ErrorValidacion($"Ya existe una cuenta llamada {nombre}");

        var cuenta = new Cuenta { Codigo = codigo, Nombre = nombre, Tipo = tipo };
        Datos.Cuentas.Add(cuenta);
        try
        {
            Almacenamiento.GuardarCuentas();
        }
        catch
        {
            Datos.Cuentas.Remove(cuenta);
            throw;
        }
    }

    public static void ModificarNombre(string codigo, string nombre)
    {
        var cuenta = Datos.Buscar(codigo) ?? throw new ErrorValidacion($"La cuenta {codigo} no existe");
        nombre = Validador.Texto(nombre, "Nombre", 60);
        if (Datos.Cuentas.Any(c => c != cuenta && c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            throw new ErrorValidacion($"Ya existe una cuenta llamada {nombre}");

        var anterior = cuenta.Nombre;
        cuenta.Nombre = nombre;
        try
        {
            Almacenamiento.GuardarCuentas();
        }
        catch
        {
            cuenta.Nombre = anterior;
            throw;
        }
    }

    public static void EliminarCuenta(string codigo)
    {
        var cuenta = Datos.Buscar(codigo) ?? throw new ErrorValidacion($"La cuenta {codigo} no existe");
        if (Datos.Movimientos.Any(m => m.Cuenta == codigo))
            throw new ErrorValidacion("No se puede eliminar: la cuenta tiene movimientos registrados");

        Datos.Cuentas.Remove(cuenta);
        try
        {
            Almacenamiento.GuardarCuentas();
        }
        catch
        {
            Datos.Cuentas.Add(cuenta);
            throw;
        }
    }

    public static int RegistrarAsiento(string fechaTexto, string descripcionTexto,
        IEnumerable<(string Codigo, string Debe, string Haber)> lineas)
    {
        var fecha = Validador.Fecha(fechaTexto);
        var descripcion = Validador.Texto(descripcionTexto, "Descripción", 120);
        var id = Datos.SiguienteAsiento();
        var nuevos = new List<Movimiento>();

        foreach (var (codigoTexto, debeTexto, haberTexto) in lineas)
        {
            var codigo = Validador.Codigo(codigoTexto);
            if (Datos.Buscar(codigo) == null)
                throw new ErrorValidacion($"La cuenta {codigo} no existe");
            var debe = Validador.Monto(debeTexto, "Debe");
            var haber = Validador.Monto(haberTexto, "Haber");
            if (debe > 0 && haber > 0)
                throw new ErrorValidacion("Un movimiento no puede tener Debe y Haber a la vez");
            if (debe == 0 && haber == 0)
                throw new ErrorValidacion("Un movimiento debe tener un monto mayor a cero");
            nuevos.Add(new Movimiento
            {
                AsientoId = id,
                Fecha = fecha,
                Descripcion = descripcion,
                Cuenta = codigo,
                Debe = debe,
                Haber = haber
            });
        }

        if (nuevos.Count < 2)
            throw new ErrorValidacion("El asiento necesita al menos 2 movimientos");
        if (nuevos.Select(m => m.Cuenta).Distinct().Count() < 2)
            throw new ErrorValidacion("El asiento necesita al menos 2 cuentas distintas");
        var totalDebe = nuevos.Sum(m => m.Debe);
        var totalHaber = nuevos.Sum(m => m.Haber);
        if (totalDebe != totalHaber)
            throw new ErrorValidacion($"El asiento no cuadra: Debe {totalDebe:N2} ≠ Haber {totalHaber:N2}");

        Datos.Movimientos.AddRange(nuevos);
        try
        {
            Almacenamiento.GuardarMovimientos();
        }
        catch
        {
            foreach (var m in nuevos)
                Datos.Movimientos.Remove(m);
            throw;
        }
        return id;
    }
}
