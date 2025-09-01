using UnityEngine;

public class Movement : MonoBehaviour
{
    public Renderer targetRenderer; 
    public int columns = 8;
    public int rows = 8;

    public float moveSpeed = 3f; 
    public float turnSpeed = 540f;      
    public float stopDistance = 0.02f;

    Vector3 targetPos;
    bool moving;
    Rigidbody rb;

    void Awake()
    {
        if (!targetRenderer)
        {
            targetRenderer = RendererHelper.GetTargetRenderer();
            if (targetRenderer == null)
            {
                Debug.LogError("No se pudo asignar el targetRenderer en Movement.");
            }
        }
    }

    void Start()
    {
        targetPos = transform.position;
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
    }
}
