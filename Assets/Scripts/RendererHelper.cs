using UnityEngine;

public static class RendererHelper
{
    private static Renderer cachedRenderer;

    public static Renderer GetTargetRenderer()
    {
        if (cachedRenderer == null)
        {
            GameObject plane = GameObject.Find("Plane");
            if (plane != null)
            {
                cachedRenderer = plane.GetComponent<Renderer>();
                if (cachedRenderer == null)
                {
                    Debug.LogError("El GameObject 'Plane' no tiene un componente Renderer.");
                }
            }
            else
            {
                Debug.LogError("No se encontró un GameObject llamado 'Plane' en la jerarquía.");
            }
        }
        return cachedRenderer;
    }
}