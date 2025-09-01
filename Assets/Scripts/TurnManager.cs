using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public List<RobotAI> robots; // Lista de robots en la escena
    private int currentRobotIndex = 0;
    private Renderer targetRenderer;

    void Start()
    {
        targetRenderer = RendererHelper.GetTargetRenderer();
        if (targetRenderer == null)
        {
            Debug.LogError("No se pudo asignar el targetRenderer en TurnManager.");
        }

        Debug.Log("TurnManager iniciado con " + robots.Count + " robots");
        // Inicializar la lista de robots si no está configurada manualmente
        if (robots == null || robots.Count == 0)
        {
            robots = new List<RobotAI>(FindObjectsOfType<RobotAI>());
        }

        // Comenzar el turno del primer robot
        StartTurn();
    }

    void StartTurn()
    {
        if (robots.Count == 0) return;

        // Llamar al método TakeTurn del robot actual
        robots[currentRobotIndex].TakeTurn();

        // Pasar al siguiente robot después de un pequeño retraso
        Invoke(nameof(NextTurn), 1f); // 1 segundo entre turnos
    }

    void NextTurn()
    {
        // Avanzar al siguiente robot
        currentRobotIndex = (currentRobotIndex + 1) % robots.Count;

        // Comenzar el turno del siguiente robot
        StartTurn();
    }

    void Update()
    {
        if (!targetRenderer)
        {
            GameObject plane = GameObject.Find("Plane");
            if (plane != null) targetRenderer = plane.GetComponent<Renderer>();
        }
    }
}
