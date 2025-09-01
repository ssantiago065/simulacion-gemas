using UnityEngine;
using System.Collections.Generic;

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

    private List<Vector3> gridCenters;

    void Start()
    {
        gridCenters = BuildGridCenters();
        SpawnAll();
    }

    private List<Vector3> BuildGridCenters()
    {

        Bounds b = targetRenderer.bounds;
        float width  = b.size.x; 
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
        {
            for (int z = 0; z < rows; z++)
            {
                list.Add(start + new Vector3(x * cellX, 0f, z * cellZ));
            }
        }
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

        var goA = Instantiate(robotAzul,  TakeCell(), Quaternion.identity, transform);
        var colA = goA.GetComponent<Grab>();
        var movA = goA.GetComponent<Movement>();
        colA.baseTarget = baseAzul;
        colA.mover      = movA;
        movA.targetRenderer = targetRenderer;   

        var goR = Instantiate(robotRojo, TakeCell(), Quaternion.identity, transform);
        var colR = goR.GetComponent<Grab>();
        var movR = goR.GetComponent<Movement>();
        colR.baseTarget = baseRoja;
        colR.mover      = movR;
        movR.targetRenderer = targetRenderer;

        var goV = Instantiate(robotVerde, TakeCell(), Quaternion.identity, transform);
        var colV = goV.GetComponent<Grab>();
        var movV = goV.GetComponent<Movement>();
        colV.baseTarget = baseVerde;
        colV.mover      = movV;
        movV.targetRenderer = targetRenderer;

        for (int i = 0; i < 2; i++)
        {
            if (gemaAzul)  Instantiate(gemaAzul,  TakeCell(), Quaternion.Euler(0, 90, 0), transform);
            if (gemaRoja)  Instantiate(gemaRoja,  TakeCell(), Quaternion.Euler(0, 90, 0), transform);
            if (gemaVerde) Instantiate(gemaVerde, TakeCell(), Quaternion.Euler(0, 90, 0), transform);
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
