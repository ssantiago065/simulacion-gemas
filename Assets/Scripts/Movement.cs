using UnityEngine;

public class Movement : MonoBehaviour
{
    public Renderer targetRenderer;
    public int columns = 8;
    public int rows = 8;

    private Vector2Int currentCell;

    public float moveSpeed = 6f;
    public float turnSpeed = 540f;
    public float stopDistance = 0.02f;

    Vector3 targetPos;
    bool moving;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        targetPos = transform.position;
        currentCell = GridManager.WorldToGridPosition(transform.position, targetRenderer.bounds, columns, rows);
        GridManager.SetObjectAt(currentCell, gameObject);
    }

    void Update()
    {
        if (!moving) return;

        Vector3 to = (targetPos - transform.position);
        to.y = 0f;
        float dist = to.magnitude;

        if (dist <= stopDistance)
        {
            moving = false;
        }

        if (to.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
            Quaternion newRot = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
            if (rb) rb.MoveRotation(newRot); else transform.rotation = newRot;
        }

        Vector3 step = transform.forward * moveSpeed * Time.deltaTime;

        if (step.magnitude > dist) step = step.normalized * dist;

        if (rb)
            rb.MovePosition(rb.position + step);
        else
            transform.position += step;
    }

    public void MoveToCell(int col, int row)
    {
        // 🔹 limpiar celda actual
        GridManager.ClearObjectAt(currentCell);

        // 🔹 mover en el mundo
        Vector3 newPos = GridManager.GridToWorldPosition(new Vector2Int(col, row), targetRenderer.bounds, columns, rows);
        transform.position = newPos;
        
        // Movimiento suave
        /*
        Bounds b = targetRenderer.bounds;
        float w = b.size.x, h = b.size.z;
        float cellX = w / columns, cellZ = h / rows;

        Vector3 start = new Vector3(
            b.min.x + cellX * 0.5f,
            transform.position.y,       
            b.min.z + cellZ * 0.5f
        );
        targetPos = start + new Vector3(col * cellX, 0f, row * cellZ);
        moving = true;
        */



        // 🔹 actualizar celda
        currentCell = new Vector2Int(col, row);
        GridManager.SetObjectAt(currentCell, gameObject);
    }

    public Vector2Int GetCurrentCell()
    {
        return currentCell;
    }
}