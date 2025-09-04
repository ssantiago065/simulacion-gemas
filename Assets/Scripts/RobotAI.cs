using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

[RequireComponent(typeof(Movement), typeof(Grab))]
public class RobotAI : MonoBehaviour
{
    public Movement mover;
    public Grab grab;

    public Vector2Int gridSize = new Vector2Int(8, 8); // tamaño del grid

    // Casillas de entrega por color (dos posiciones por color)
    private static readonly Dictionary<string, Vector2Int[]> deliveryPositions = new Dictionary<string, Vector2Int[]>()
    {
        { "GemaAzul", new Vector2Int[]{ new Vector2Int(7,0), new Vector2Int(6,0) } },
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
        if (SimulationStats.simulationEnded) return; // detener lógica de robots

        if (mover == null || mover.targetRenderer == null)
        {
            Debug.LogWarning($"{name}: Movement o targetRenderer no configurados.");
            return;
        }

        GridManager.RenderGrid();

        Perceive();

        // 2️⃣ Posición actual
        Vector2Int myPos = mover.GetCurrentCell();
        Debug.Log($"{name} en {myPos}");


        // 3️⃣ Decisión
        if (grab.IsCarryingTag(grab.myGemTag))
        {
            //Cortar

            Vector2Int delivery = GetDeliveryCell();
            Debug.Log($"{name} lleva gema {grab.myGemTag}, objetivo entrega en {delivery}");
            MoveAlongPath(myPos, delivery, true);
            SimulationStats.RegisterMove();
        }
        else
        {
            HashSet<Vector2Int> knownGems = Blackboard.knownGems[grab.myGemTag];
            if (knownGems.Count == 0)
            {
                Debug.Log($"{name} no conoce gemas de su color {grab.myGemTag}");
                return;
            }



            Vector2Int targetGem = GetClosestGem(myPos, knownGems, grab.myGemTag);
            Debug.Log($"{name} busca gema en {targetGem}");
            MoveAlongPath(myPos, targetGem, false);
            SimulationStats.RegisterMove();
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
            Debug.LogWarning($"{name} no encontró camino desde {from} a {target}, moviendose a una casilla disponible cercana.");
            GridManager.RenderGrid();
            GameObject izquierda = GridManager.GetObjectAt(from + Vector2Int.left);
            GameObject derecha = GridManager.GetObjectAt(from + Vector2Int.right);
            GameObject arriba = GridManager.GetObjectAt(from + Vector2Int.up);
            GameObject abajo = GridManager.GetObjectAt(from + Vector2Int.down);
            if (izquierda == null && from.x > 0) mover.MoveToCell(from.x - 1, from.y);
            else if (derecha == null && from.x < mover.columns - 1) mover.MoveToCell(from.x + 1, from.y);
            else if (arriba == null && from.y < mover.rows - 1) mover.MoveToCell(from.x, from.y + 1);
            else if (abajo == null && from.y > 0) mover.MoveToCell(from.x, from.y - 1);
            else Debug.LogWarning($"{name} no puede moverse, todas las casillas adyacentes están ocupadas.");
            return;
        }


        if (path.Count == 1 && from == target)
        {
            if (isDelivery)
            {
                Debug.Log($"{name} entregó {grab.myGemTag} en {target}");
                Vector3 worldPos = grab.baseTarget.GetComponent<BaseZone>().dropPoint.position;
                grab.DropAt(worldPos);
            }
            else if (!isDelivery && GridManager.GetObjectAt(target) != null && GridManager.GetObjectAt(target).CompareTag(grab.myGemTag))
            {
                grab.PickUp(GridManager.GetObjectAt(target));
                Blackboard.RemoveGem(grab.myGemTag, target);
                Debug.Log($"{name} recogió la gema {grab.myGemTag} en {target}");
            }
            else
            {
                Debug.LogWarning($"{name} ya está en {target} pero no hay nada que hacer.");
            }
            return;
        }

        if (target == from){
            Debug.LogWarning($"{name} ya está en {target}, no se mueve.");
            GameObject gem = GridManager.GetObjectAt(target);
            grab.PickUp(gem);
            Blackboard.RemoveGem(grab.myGemTag, target);
            return;
        }

        Vector2Int nextStep = path[1];
        Debug.Log($"{name} se mueve de {from} hacia {nextStep} (meta {target})");

        // Permitir moverse sobre su propia gema
        GameObject objAtNext = GridManager.GetObjectAt(nextStep);
        if (objAtNext == null || objAtNext.CompareTag(grab.myGemTag))
        {
            mover.MoveToCell(nextStep.x, nextStep.y);

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
                Vector3 worldPos = grab.baseTarget.GetComponent<BaseZone>().dropPoint.position;
                grab.DropAt(worldPos);
            }
        }
        else
        {
            Debug.Log($"{name} no puede moverse a {nextStep}, ocupado por {objAtNext.name}");
            // Intentar moverse a una casilla adyacente libre
            GameObject izquierda = GridManager.GetObjectAt(from + Vector2Int.left);
            GameObject derecha = GridManager.GetObjectAt(from + Vector2Int.right);
            GameObject arriba = GridManager.GetObjectAt(from + Vector2Int.up);
            GameObject abajo = GridManager.GetObjectAt(from + Vector2Int.down);
            if (izquierda == null && from.x > 0) mover.MoveToCell(from.x - 1, from.y);
            else if (derecha == null && from.x < mover.columns - 1) mover.MoveToCell(from.x + 1, from.y);
            else if (arriba == null && from.y < mover.rows - 1) mover.MoveToCell(from.x, from.y + 1);
            else if (abajo == null && from.y > 0) mover.MoveToCell(from.x, from.y - 1);
            else
            {
                Debug.LogWarning($"{name} no puede moverse, todas las casillas adyacentes están ocupadas.");
                Debug.LogWarning($"{izquierda} {derecha} {arriba} {abajo}");
                GridManager.RenderGrid();
            }
            return;
        }
    }


    List<Vector2Int> BFS(Vector2Int start, Vector2Int goal)
    {
        // Si el objetivo está fuera de la grilla (ej. delivery en y=-1),
        // lo ajustamos a la celda más cercana válida dentro del grid.
        Vector2Int adjustedGoal = goal;
        if (goal.y < 0) adjustedGoal = new Vector2Int(goal.x, 0);

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

            if (current == adjustedGoal)
                return ReconstructPath(cameFrom, current);

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;

                if (neighbor.x < 0 || neighbor.x >= mover.columns || neighbor.y < 0 || neighbor.y >= mover.rows)
                    continue;

                if (cameFrom.ContainsKey(neighbor))
                    continue;

                GameObject obj = GridManager.GetObjectAt(neighbor);

                if (obj != null && obj != gameObject &&
                    (obj.CompareTag("RobotAzul") || obj.CompareTag("RobotRojo") || obj.CompareTag("RobotVerde")))
                    continue;

                if (obj != null && (obj.CompareTag("GemaAzul") || obj.CompareTag("GemaRoja") || obj.CompareTag("GemaVerde")))
                {
                    if (neighbor != adjustedGoal && !obj.CompareTag(grab.myGemTag))
                        continue;
                }

                queue.Enqueue(neighbor);
                cameFrom[neighbor] = current;
            }
        }

        Debug.LogWarning($"{name} no pudo calcular ruta de {start} a {goal}");
        return null;
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

    Vector2Int GetClosestGem(Vector2Int myPos, HashSet<Vector2Int> gems, string gemTag)
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
        if (!deliveryPositions.ContainsKey(gemTag))
        {
            Debug.LogError($"{name}: No hay posiciones de entrega definidas para {gemTag}");
            return myPos;
        }

        Vector2Int[] options = deliveryPositions[gemTag];
        Vector2Int best = options.OrderBy(p => Vector2Int.Distance(myPos, p)).First();
        Debug.Log($"{name} eligió delivery {best} para {gemTag}");
        return best;
    }

    Vector2Int GetDeliveryCell()
        {
            if (!grab.baseTarget)
            {
                Debug.LogError($"{name}: baseTarget no asignado");
                return mover.GetCurrentCell();
            }

            // Tomamos la posición del baseTarget
            Vector3 basePos = grab.baseTarget.position;

            // Convertimos a coordenada de grilla
            Vector2Int approx = GridManager.WorldToGridPosition(
                basePos,
                mover.targetRenderer.bounds,
                mover.columns,
                mover.rows
            );

            // Clampeamos dentro de la matriz [0..columns-1, 0..rows-1]
            int x = Mathf.Clamp(approx.x, 0, mover.columns - 1);
            int y = Mathf.Clamp(approx.y, 0, mover.rows - 1);

            // Esto garantiza que siempre sea una celda navegable del borde
            return new Vector2Int(x, y);
        }
}



