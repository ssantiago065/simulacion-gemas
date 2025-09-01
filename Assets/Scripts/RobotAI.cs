using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Movement), typeof(Grab))]
public class RobotAI : MonoBehaviour
{
    public Movement mover;
    public Grab grab;

    public Vector2Int gridSize = new Vector2Int(10, 10); // tamaño del grid

    // Casillas de entrega por color (dos posiciones por color)
    private static readonly Dictionary<string, Vector2Int[]> deliveryPositions = new Dictionary<string, Vector2Int[]>()
    {
        { "GemaAzul", new Vector2Int[]{ new Vector2Int(0,0), new Vector2Int(1,0) } },
        { "GemaRoja", new Vector2Int[]{ new Vector2Int(3,0), new Vector2Int(4,0) } },
        { "GemaVerde", new Vector2Int[]{ new Vector2Int(6,0), new Vector2Int(7,0) } }
    };

    void Awake()
    {
        if (!mover) mover = GetComponent<Movement>();
        if (!grab) grab = GetComponent<Grab>();
    }

    public void TakeTurn()
    {
        if (mover == null || mover.targetRenderer == null) 
        {
            Debug.LogWarning($"{name}: Movement o targetRenderer no configurados.");
            return;
        }

        // 1️⃣ Percibir SIEMPRE todas las gemas
        Perceive();

        // 2️⃣ Posición actual
        Vector2Int myPos = GridManager.WorldToGridPosition(transform.position, mover.targetRenderer.bounds, mover.columns, mover.rows);
        Debug.Log($"{name} en {myPos}");

        // 3️⃣ Decisión
        if (grab.IsCarryingTag(grab.myGemTag))
        {
            Vector2Int delivery = GetClosestDelivery(myPos, grab.myGemTag);
            Debug.Log($"{name} lleva gema {grab.myGemTag}, objetivo entrega en {delivery}");
            MoveAlongPath(myPos, delivery, true);
        }
        else
        {
            HashSet<Vector2Int> knownGems = Blackboard.knownGems[grab.myGemTag];
            if (knownGems.Count == 0) 
            {
                Debug.Log($"{name} no conoce gemas de su color {grab.myGemTag}");
                return;
            }

            Vector2Int targetGem = GetClosestGem(myPos, knownGems);
            Debug.Log($"{name} busca gema en {targetGem}");
            MoveAlongPath(myPos, targetGem, false);
        }
    }

    void Perceive()
    {
        for (int x = 0; x < mover.columns; x++)
        {
            for (int y = 0; y < mover.rows; y++)
            {
                GameObject obj = GridManager.GetObjectAt(new Vector2Int(x, y));
                if (obj == null) continue;

                if (obj.CompareTag("GemaAzul") || obj.CompareTag("GemaRoja") || obj.CompareTag("GemaVerde"))
                {
                    Blackboard.UpdateKnownGem(obj.tag, new Vector2Int(x, y));
                    Debug.Log($"{name} percibe {obj.tag} en ({x},{y})");
                }
            }
        }
    }

    void MoveAlongPath(Vector2Int from, Vector2Int target, bool isDelivery)
{
    List<Vector2Int> path = BFS(from, target);

    if (path == null || path.Count <= 1)
    {
        Debug.LogWarning($"{name} no encontró camino desde {from} a {target}");
        return;
    }

    Vector2Int nextStep = path[1];
    Debug.Log($"{name} se mueve de {from} hacia {nextStep} (meta {target})");

    // Permitir moverse sobre su propia gema
    GameObject objAtNext = GridManager.GetObjectAt(nextStep);
    if (objAtNext == null || objAtNext.CompareTag(grab.myGemTag))
    {
        mover.MoveToCell(nextStep.x, nextStep.y);
        GridManager.ClearObjectAt(from);
        GridManager.SetObjectAt(nextStep, gameObject);
        Blackboard.UpdateRobotPosition(this, nextStep);

        // Recolectar la gema si es el objetivo
        if (!isDelivery && nextStep == target && objAtNext != null && objAtNext.CompareTag(grab.myGemTag))
        {
            grab.PickUp(objAtNext);
            Blackboard.RemoveGem(grab.myGemTag, nextStep);
            Debug.Log($"{name} recogió la gema {grab.myGemTag} en {nextStep}");
        }
        else if (isDelivery && nextStep == target)
        {
            Debug.Log($"{name} entregó {grab.myGemTag} en {target}");
            Vector3 worldPos = GridManager.GridToWorldPosition(target, mover.targetRenderer.bounds, mover.columns, mover.rows);
            worldPos.y = transform.position.y;
            grab.DropAt(worldPos);
        }
    }
    else
    {
        Debug.Log($"{name} no puede moverse a {nextStep}, ocupado por {objAtNext.name}");
    }
}


    // --- 🔵 BFS Pathfinding ---
    List<Vector2Int> BFS(Vector2Int start, Vector2Int goal)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int?> cameFrom = new Dictionary<Vector2Int, Vector2Int?>();

        queue.Enqueue(start);
        cameFrom[start] = null;

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;

                if (neighbor.x < 0 || neighbor.x >= mover.columns || neighbor.y < 0 || neighbor.y >= mover.rows)
                    continue;

                if (cameFrom.ContainsKey(neighbor))
                    continue;

                GameObject obj = GridManager.GetObjectAt(neighbor);

                // 🚧 Celda ocupada por otro robot (cualquier color)
                if (obj != null && obj != gameObject &&
                    (obj.CompareTag("RobotAzul") || obj.CompareTag("RobotRojo") || obj.CompareTag("RobotVerde")))
                    continue;

               // 🚧 Celda ocupada por gema que no es mía (salvo que sea de mi color)
                if (obj != null && (obj.CompareTag("GemaAzul") || obj.CompareTag("GemaRoja") || obj.CompareTag("GemaVerde")))
                {
                    // si no es la meta y no es de mi color, la trato como obstáculo
                    if (neighbor != goal && !obj.CompareTag(grab.myGemTag))
                        continue;

                    // si es de mi color, permitir mover sobre ella
                }

                queue.Enqueue(neighbor);
                cameFrom[neighbor] = current;
            }
        }

        Debug.LogWarning($"{name} no pudo calcular ruta de {start} a {goal}");
        return null; // No se encontró camino
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int?> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        while (cameFrom.ContainsKey(current) && cameFrom[current] != null)
        {
            path.Add(current);
            current = cameFrom[current].Value;
        }

        path.Add(current); // agregar start
        path.Reverse();
        return path;
    }

    Vector2Int GetClosestGem(Vector2Int myPos, HashSet<Vector2Int> gems)
    {
        Vector2Int closest = default;
        float minDist = float.MaxValue;
        foreach (var g in gems)
        {
            float dist = Vector2Int.Distance(myPos, g);
            if (dist < minDist)
            {
                minDist = dist;
                closest = g;
            }
        }
        Debug.Log($"{name} eligió gema más cercana en {closest}");
        return closest;
    }

    Vector2Int GetClosestDelivery(Vector2Int myPos, string gemTag)
    {
        Vector2Int[] options = deliveryPositions[gemTag];
        Vector2Int best = options.OrderBy(p => Vector2Int.Distance(myPos, p)).First();
        Debug.Log($"{name} eligió delivery {best} para {gemTag}");
        return best;
    }
}
