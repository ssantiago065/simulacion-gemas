using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Movement), typeof(Grab))]
public class RobotAI : MonoBehaviour
{
    public Movement mover;
    public Grab grab;

    // Casillas de entrega por color (fila -1 para estar fuera del grid)
    private static readonly Dictionary<string, Vector2Int[]> deliveryPositions = new Dictionary<string, Vector2Int[]>()
    {
        { "GemaAzul", new Vector2Int[]{ new Vector2Int(-1,0), new Vector2Int(-1,1) } },
        { "GemaRoja", new Vector2Int[]{ new Vector2Int(-1,3), new Vector2Int(-1,4) } },
        { "GemaVerde", new Vector2Int[]{ new Vector2Int(-1,6), new Vector2Int(-1,7) } }
    };

    void Awake()
    {
        if (!mover) mover = GetComponent<Movement>();
        if (!grab) grab = GetComponent<Grab>();
    }

    public void TakeTurn()
    {
        if (mover == null || mover.targetRenderer == null) return;

        // 1️⃣ Percepción: siempre registrar todas las gemas
        Perceive();

        // 2️⃣ Obtener posición actual
        Vector2Int myPos = GridManager.WorldToGridPosition(
            transform.position,
            mover.targetRenderer.bounds,
            mover.columns,
            mover.rows
        );

        // 3️⃣ Acción según estado
        if (grab.IsCarryingTag(grab.myGemTag))
        {
            // Lleva una gema → ir hacia delivery
            Vector2Int delivery = GetNextDeliveryPosition(grab.myGemTag);
            MoveOneStep(myPos, delivery, true);
        }
        else
        {
            // No lleva gema → moverse hacia la gema más cercana de su color
            HashSet<Vector2Int> knownGems = Blackboard.knownGems[grab.myGemTag];
            if (knownGems.Count > 0)
            {
                Vector2Int targetGem = GetClosestGem(myPos, knownGems);
                MoveOneStep(myPos, targetGem, false);
            }
            else
            {
                MoveRandomly(myPos);
            }
        }
    }

    void Perceive()
    {
        Vector2Int myPos = GridManager.WorldToGridPosition(
            transform.position,
            mover.targetRenderer.bounds,
            mover.columns,
            mover.rows
        );

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            Vector2Int.up + Vector2Int.left, Vector2Int.up + Vector2Int.right,
            Vector2Int.down + Vector2Int.left, Vector2Int.down + Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int check = myPos + dir;
            if (check.x < 0 || check.x >= mover.columns || check.y < 0 || check.y >= mover.rows)
                continue;

            GameObject obj = GridManager.GetObjectAt(check);
            if (obj != null && (obj.CompareTag("GemaAzul") || obj.CompareTag("GemaRoja") || obj.CompareTag("GemaVerde")))
            {
                Blackboard.UpdateKnownGem(obj.tag, check);
                Debug.Log($"{name} percibió gema {obj.tag} en {check}");
            }
        }
    }

    void MoveRandomly(Vector2Int myPos)
    {
        Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            Vector2Int candidate = myPos + dir;

            if (candidate.x < 0 || candidate.x >= mover.columns || candidate.y < 0 || candidate.y >= mover.rows)
                continue;

            if (GridManager.GetObjectAt(candidate) == null)
            {
                mover.MoveToCell(candidate.x, candidate.y);
                GridManager.ClearObjectAt(myPos);
                GridManager.SetObjectAt(candidate, gameObject);
                Blackboard.UpdateRobotPosition(this, candidate);
                Debug.Log($"{name} se movió aleatoriamente a {candidate}");
                break;
            }
        }
    }

    Vector2Int GetNextDeliveryPosition(string gemTag)
    {
        foreach (var pos in deliveryPositions[gemTag])
        {
            // Tomamos la primera posición de entrega disponible
            return pos;
        }
        return deliveryPositions[gemTag][0];
    }

    void MoveOneStep(Vector2Int from, Vector2Int to, bool isDelivery)
    {
        // Pathfinding simple: paso en dirección de X y luego Y evitando obstáculos
        Vector2Int step = from;

        if (from.x < to.x) step.x++;
        else if (from.x > to.x) step.x--;

        if (from.y < to.y) step.y++;
        else if (from.y > to.y) step.y--;

        // Verificar celda libre dentro del grid
        if (step.x >= 0 && step.x < mover.columns && step.y >= 0 && step.y < mover.rows)
        {
            if (GridManager.GetObjectAt(step) == null)
            {
                mover.MoveToCell(step.x, step.y);
                GridManager.ClearObjectAt(from);
                GridManager.SetObjectAt(step, gameObject);
                Blackboard.UpdateRobotPosition(this, step);
                Debug.Log($"{name} se movió hacia {(isDelivery ? "delivery" : "gema")} a {step}");
            }
        }

        // Recoger o soltar según corresponda
        if (!isDelivery && step == to)
        {
            GameObject gem = GridManager.GetObjectAt(step);
            if (gem != null && gem.CompareTag(grab.myGemTag))
            {
                grab.PickUp(gem);
                Blackboard.RemoveGem(grab.myGemTag, step);
                Debug.Log($"{name} recogió gema {grab.myGemTag} en {step}");
            }
        }
        else if (isDelivery && Vector2Int.Distance(step, to) <= 1)
        {
            Vector3 worldPos = GridManager.GridToWorldPosition(to, mover.targetRenderer.bounds, mover.columns, mover.rows);
            worldPos.y = transform.position.y;
            grab.DropAt(worldPos);
            Debug.Log($"{name} entregó gema {grab.myGemTag} en {to}");
        }
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
        return closest;
    }
}