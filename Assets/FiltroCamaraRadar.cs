using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FiltroCamaraRadar : MonoBehaviour
{
    public Shader shaderDeReemplazo;

    void OnEnable()
    {
        if (shaderDeReemplazo != null)
        {
            GetComponent<Camera>().SetReplacementShader(shaderDeReemplazo, "");
        }
    }

    void OnDisable()
    {
        GetComponent<Camera>().ResetReplacementShader();
    }
}