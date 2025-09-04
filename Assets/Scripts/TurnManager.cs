using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(0)]
public class TurnManager : MonoBehaviour
{
    public Spawner spawner;          // arrástralo en el Inspector (o se resuelve solo)
    public List<RobotAI> robots;     // se rellena con los del Spawner
    private int currentRobotIndex = 0;

    void Start()
    {
        StartCoroutine(Bootstrap());
    }

    void Update()
    {
        SimulationStats.elapsedTime += Time.deltaTime;
    }

    System.Collections.IEnumerator Bootstrap()
    {
        // Espera un frame a que el Spawner instancie e inyecte referencias
        yield return null;

        if (!spawner) spawner = FindObjectOfType<Spawner>();
        robots = spawner ? new List<RobotAI>(spawner.GetRobots()) : new List<RobotAI>();

        // Diagnóstico: ver quién está listo
        foreach (var ai in robots)
        {
            bool hasMover = ai && ai.mover != null;
            bool hasTR = hasMover && ai.mover.targetRenderer != null;
            Debug.Log($"[TurnManager] {ai.name}: mover={hasMover}, targetRenderer={hasTR}");
        }

        if (robots.Count == 0)
        {
            Debug.LogError("No hay robots en escena.");
            yield break;
        }

        Debug.Log("TurnManager iniciado con " + robots.Count + " robots");
        StartTurn();
    }

    void StartTurn()
    {
        if (robots == null || robots.Count == 0) return;

        int tried = 0;
        while (tried < robots.Count)
        {
            var ai = robots[currentRobotIndex];

            // Avanza índice circular para la próxima vuelta
            currentRobotIndex = (currentRobotIndex + 1) % robots.Count;

            bool ready = (ai != null && ai.mover != null && ai.mover.targetRenderer != null);
            if (ready)
            {
                ai.TakeTurn();
                // Programa el siguiente turno con delay (evita recursión inmediata)
                Invoke(nameof(StartTurn), 1f);
                return;
            }

            tried++;
        }

        // Si nadie está listo aún, espera un poco y reintenta
        Debug.LogWarning("[TurnManager] No hay robots listos; reintentando...");
        Invoke(nameof(StartTurn), 0.25f);
    }
}
