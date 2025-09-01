using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Movement))]
public class RobotAI : MonoBehaviour
{
    private Movement mover;

    void Awake()
    {
        mover = GetComponent<Movement>();
    }

    public void TakeTurn()
    {
        if (mover == null || mover.targetRenderer == null)
        {
            Debug.LogWarning("Movement o targetRenderer no asignado en " + gameObject.name);
            return;
        }

        // Obtener posición actual en la grid
        Vector2Int myGridPos = GridManager.WorldToGridPosition(
            transform.position,
            mover.targetRenderer.bounds,
            mover.columns,
            mover.rows
        );

        // Definir direcciones posibles
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        Vector2Int nextPos = myGridPos;

        // Intentar moverse a una celda libre al azar
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            Vector2Int candidate = myGridPos + dir;

            // Verificar límites
            if (candidate.x < 0 || candidate.x >= mover.columns || candidate.y < 0 || candidate.y >= mover.rows)
                continue;

            // Verificar si la celda está libre
            if (GridManager.GetObjectAt(candidate) == null)
            {
                nextPos = candidate;
                break;
            }
        }

        // Mover el robot y actualizar grid
        if (nextPos != myGridPos)
        {
            mover.MoveToCell(nextPos.x, nextPos.y);
            GridManager.ClearObjectAt(myGridPos);
            GridManager.SetObjectAt(nextPos, gameObject);
        }
    }
}
