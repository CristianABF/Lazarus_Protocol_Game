using System.Collections.Generic;

[System.Serializable]
public class DatosEnemigo
{
    public string nombrePrefab;
    public float posX;
    public float posY;
    public float posZ;
    public float rotY;
}

[System.Serializable]
public class ListaEnemigosGuardados
{
    public List<DatosEnemigo> enemigos = new List<DatosEnemigo>();
}