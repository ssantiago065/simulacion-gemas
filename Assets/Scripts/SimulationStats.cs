using UnityEngine;

public static class SimulationStats
{
    public static int totalMoves = 0;
    public static int totalDeliveries = 0;
    public static float elapsedTime = 0f;
    public static bool simulationEnded = false;
    private static float endTimer = 0f;

    public static void RegisterMove()
    {
        if (!simulationEnded)
            totalMoves++;
    }

    public static void RegisterDelivery()
    {
        if (!simulationEnded)
        {
            totalDeliveries++;
            if (totalDeliveries >= 6) // Hardcodeado
            {
                simulationEnded = true;
                Debug.Log($"Todas las gemas entregadas {totalDeliveries}");
                Debug.Log($"Tiempo total: {elapsedTime:F2} segundos");
                Debug.Log($"Movimientos totales: {totalMoves}");
            }
        }
    }

    public static void UpdateTimer()
    {
        if (!simulationEnded)
        {
            elapsedTime += Time.deltaTime;
        }
        else
        {
            endTimer += Time.deltaTime;
            if (endTimer > 5f) // dejar 5 segundos visibles
            {
                Debug.Log("Simulación finalizada.");
                UnityEditor.EditorApplication.isPlaying = false; // Solo en editor
            }
        }
    }
}
