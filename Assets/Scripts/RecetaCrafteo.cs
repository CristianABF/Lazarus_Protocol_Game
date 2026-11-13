using UnityEngine;

[CreateAssetMenu(fileName = "NuevaReceta", menuName = "LazarusProtocol/Receta de Crafteo")]
public class RecetaCrafteo : ScriptableObject
{
    [Header("Datos del Ítem")]
    public string idEnLlaveInglesa;
    public string nombreItem;
    public Sprite imagenPlano;

    [Header("Costo y Resultado")]
    public int costoChatarra = 50;
    public GameObject prefabAEntregar;
}