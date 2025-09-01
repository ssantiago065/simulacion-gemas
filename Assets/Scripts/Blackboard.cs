using UnityEngine;
using System.Collections.Generic;

public static class Blackboard
{
    // Posiciones conocidas de gemas por color
    public static Dictionary<string, HashSet<Vector2Int>> knownGems = new Dictionary<string, HashSet<Vector2Int>>()
    {
        { "GemaAzul", new HashSet<Vector2Int>() },
        { "GemaRoja", new HashSet<Vector2Int>() },
        { "GemaVerde", new HashSet<Vector2Int>() }
    };

    // Posiciones actuales de robots
    public static Dictionary<RobotAI, Vector2Int> robotPositions = new Dictionary<RobotAI, Vector2Int>();

    // Obstáculos dinámicos: robots y gemas de otros colores
    public static GameObject[,] obstacleGrid;

    public static void Init(int columns, int rows)
    {
        obstacleGrid = new GameObject[columns, rows];
        robotPositions.Clear();
        foreach (var key in knownGems.Keys)
            knownGems[key].Clear();
    }

    // Actualizar la posición de un robot
    public static void UpdateRobotPosition(RobotAI robot, Vector2Int pos)
    {
        robotPositions[robot] = pos;
        obstacleGrid[pos.x, pos.y] = robot.gameObject;
    }

    // Actualizar una gema conocida (cuando un robot la percibe)
    public static void UpdateKnownGem(string tag, Vector2Int pos)
    {
        if (!knownGems.ContainsKey(tag))
            knownGems[tag] = new HashSet<Vector2Int>();

        knownGems[tag].Add(pos);
    }


    // Marcar que la gema ya fue recogida
    public static void RemoveGem(string tag, Vector2Int pos)
    {
        knownGems[tag].Remove(pos);
        obstacleGrid[pos.x, pos.y] = null;
    }
}
