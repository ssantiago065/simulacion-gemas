using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class Spawner : MonoBehaviour
{
    public Renderer targetRenderer;
    public int columns = 8;
    public int rows = 8;

    public GameObject robotAzul;
    public GameObject robotRojo;
    public GameObject robotVerde;

    public GameObject gemaAzul;
    public GameObject gemaRoja;
    public GameObject gemaVerde;

    public Transform baseAzul;
    public Transform baseRoja;
    public Transform baseVerde;

    // 👉 Lista de robots creados por este spawner
    private readonly List<RobotAI> spawnedRobots = new List<RobotAI>();
    public IReadOnlyList<RobotAI> GetRobots() => spawnedRobots;

    void Awake()
    {
        Blackboard.Init(columns, rows);
        // Fallback por si olvidamos asignarlo en el Inspector.
        if (targetRenderer == null)
        {
            var plane = GameObject.Find("Plane"); // cambia el nombre si tu tablero no se llama "Plane"
            if (plane) targetRenderer = plane.GetComponent<Renderer>();
            if (targetRenderer == null)
                Debug.LogError("[Spawner] targetRenderer sigue null. Asigna el MeshRenderer del tablero en el Inspector.");
        }
    }

    private List<Vector3> gridCenters;

    void Start()
    {
        
        gridCenters = BuildGridCenters();
        SpawnAll();
    }

    private List<Vector3> BuildGridCenters()
    {
        Bounds b = targetRenderer.bounds;
        float width = b.size.x;
        float height = b.size.z;

        float cellX = width / columns;
        float cellZ = height / rows;

        Vector3 start = new Vector3(
            b.min.x + cellX * 0.5f,
            b.max.y,
            b.min.z + cellZ * 0.5f
        );

        var list = new List<Vector3>(columns * rows);
        for (int x = 0; x < columns; x++)
            for (int z = 0; z < rows; z++)
                list.Add(start + new Vector3(x * cellX, 0f, z * cellZ));
        return list;
    }

    private void SpawnAll()
    {
        var cells = new List<Vector3>(gridCenters);
        Shuffle(cells);

        Vector3 TakeCell()
        {
            Vector3 p = cells[0];
            cells.RemoveAt(0);
            return p;
        }

        void RegisterInGrid(GameObject obj, Vector3 position)
        {
            Vector2Int gridPosition =
                GridManager.WorldToGridPosition(position, targetRenderer.bounds, columns, rows);
            GridManager.SetObjectAt(gridPosition, obj);
        }

        // AZUL
        var goA = Instantiate(robotAzul, TakeCell(), Quaternion.identity, transform);
        RegisterInGrid(goA, goA.transform.position);
        var colA = goA.GetComponent<Grab>();
        var movA = goA.GetComponent<Movement>();
        var aiA  = goA.GetComponent<RobotAI>();
        colA.baseTarget = baseAzul;
        colA.mover = movA;
        movA.targetRenderer = targetRenderer; // inyección del renderer del tablero
        aiA.mover = movA;                     // inyección del Movement a la IA
        spawnedRobots.Add(aiA);

        // ROJO
        var goR = Instantiate(robotRojo, TakeCell(), Quaternion.identity, transform);
        RegisterInGrid(goR, goR.transform.position);
        var colR = goR.GetComponent<Grab>();
        var movR = goR.GetComponent<Movement>();
        var aiR  = goR.GetComponent<RobotAI>();
        colR.baseTarget = baseRoja;
        colR.mover = movR;
        movR.targetRenderer = targetRenderer;
        aiR.mover = movR;
        spawnedRobots.Add(aiR);

        // VERDE
        var goV = Instantiate(robotVerde, TakeCell(), Quaternion.identity, transform);
        RegisterInGrid(goV, goV.transform.position);
        var colV = goV.GetComponent<Grab>();
        var movV = goV.GetComponent<Movement>();
        var aiV  = goV.GetComponent<RobotAI>();
        colV.baseTarget = baseVerde;
        colV.mover = movV;
        movV.targetRenderer = targetRenderer;
        aiV.mover = movV;
        spawnedRobots.Add(aiV);

        // Gemas
        for (int i = 0; i < 2; i++)
        {
            if (gemaAzul)
            {
                var gemA = Instantiate(gemaAzul, TakeCell(), Quaternion.Euler(0, 90, 0), transform);
                RegisterInGrid(gemA, gemA.transform.position);
            }
            if (gemaRoja)
            {
                var gemR = Instantiate(gemaRoja, TakeCell(), Quaternion.Euler(0, 90, 0), transform);
                RegisterInGrid(gemR, gemR.transform.position);
            }
            if (gemaVerde)
            {
                var gemV = Instantiate(gemaVerde, TakeCell(), Quaternion.Euler(0, 90, 0), transform);
                RegisterInGrid(gemV, gemV.transform.position);
            }
        }
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
