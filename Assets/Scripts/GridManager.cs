using UnityEngine;
using System.Collections.Generic;

public static class GridManager
{
    public static GameObject[,] grid = new GameObject[8, 8];

    // Convierte una posición en el mundo a una posición en la cuadrícula
    public static Vector2Int WorldToGridPosition(Vector3 worldPosition, Bounds bounds, int columns, int rows)
    {
        float cellWidth = bounds.size.x / columns;
        float cellHeight = bounds.size.z / rows;

        int x = Mathf.FloorToInt((worldPosition.x - bounds.min.x) / cellWidth);
        int z = Mathf.FloorToInt((worldPosition.z - bounds.min.z) / cellHeight);

        return new Vector2Int(x, z);
    }

    public static Vector3 GridToWorldPosition(Vector2Int gridPosition, Bounds bounds, int columns, int rows)
    {
        float cellWidth = bounds.size.x / columns;
        float cellHeight = bounds.size.z / rows;

        float x = bounds.min.x + gridPosition.x * cellWidth + cellWidth / 2f;
        float z = bounds.min.z + gridPosition.y * cellHeight + cellHeight / 2f;
        float y = bounds.max.y; // Mantener la altura original  
        return new Vector3(x, y, z);
    }

    // Establece un objeto en una posición específica de la cuadrícula
    public static void SetObjectAt(Vector2Int gridPosition, GameObject obj)
    {
        if (gridPosition.x >= 0 && gridPosition.x < 8 && gridPosition.y >= 0 && gridPosition.y < 8)
        {
            grid[gridPosition.x, gridPosition.y] = obj;
        }
    }

    // Obtiene un objeto en una posición específica de la cuadrícula
    public static GameObject GetObjectAt(Vector2Int gridPosition)
    {
        if (gridPosition.x >= 0 && gridPosition.x < 8 && gridPosition.y >= 0 && gridPosition.y < 8)
        {
            return grid[gridPosition.x, gridPosition.y];
        }
        return null;
    }

    // Limpia un objeto en una posición específica de la cuadrícula
    public static void ClearObjectAt(Vector2Int gridPosition)
    {
        if (gridPosition.x >= 0 && gridPosition.x < 8 && gridPosition.y >= 0 && gridPosition.y < 8)
        {
            grid[gridPosition.x, gridPosition.y] = null;
        }
    }

    // Obtiene todas las posiciones de un tipo específico de objeto
    public static List<Vector2Int> GetPositionsByTag(string tag)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject obj = grid[x, y];
                if (obj != null && obj.CompareTag(tag))
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }

        return positions;
    }

    public static void RenderGrid()
    {
        string gridRepresentation = "";

        for (int y = 7; y >= 0; y--) // Iterar desde la fila superior hacia abajo
        {
            for (int x = 0; x < 8; x++)
            {
                GameObject obj = grid[x, y];
                if (obj == null)
                {
                    gridRepresentation += ". "; // Celda vacía
                }
                else if (obj.CompareTag("RobotAzul"))
                {
                    gridRepresentation += "A "; // Robot azul
                }
                else if (obj.CompareTag("RobotRojo"))
                {
                    gridRepresentation += "R "; // Robot rojo
                }
                else if (obj.CompareTag("RobotVerde"))
                {
                    gridRepresentation += "V "; // Robot verde
                }
                else if (obj.CompareTag("GemaAzul"))
                {
                    gridRepresentation += "GA "; // Gema azul
                }
                else if (obj.CompareTag("GemaRoja"))
                {
                    gridRepresentation += "GR "; // Gema roja
                }
                else if (obj.CompareTag("GemaVerde"))
                {
                    gridRepresentation += "GV "; // Gema verde
                }
                else
                {
                    gridRepresentation += "? "; // Objeto desconocido
                }
            }
            gridRepresentation += "\n"; // Nueva línea al final de cada fila
        }

        Debug.Log(gridRepresentation);
    }

    // Devuelve toda la matriz
    public static GameObject[,] GetGrid()
    {
        return grid;
    }
}