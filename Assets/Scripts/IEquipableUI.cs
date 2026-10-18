public enum TipoObjetoUI
{
    Arma,
    Herramienta,
    Otro
}

public interface IEquipableUI
{
    TipoObjetoUI GetTipoObjeto();
    string GetNombreObjeto();

    // Para armas
    int GetMunicionActual();
    int GetMaxMunicion();
    int GetCargadores();

    // Para herramientas (como la Llave Inglesa / Construcción)
    int GetCantidadDisponible();
    int GetCantidadMaxima();
}