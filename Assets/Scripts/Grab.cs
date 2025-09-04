using UnityEngine;

public class Grab : MonoBehaviour
{
    public string myGemTag;

    public Transform detector;           
    public Transform carryPoint;         
    public Transform baseTarget;          
    public Movement mover;         

    GameObject carrying;

    void Reset()
    {
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void Awake()
    {
        if (!mover) mover = GetComponent<Movement>();
        if (detector && !detector.GetComponent<Detect>())
            detector.gameObject.AddComponent<Detect>().Init(this);
    }

    public void OnDetected(Collider other)
    {
        if (carrying != null) return;              
        if (!other.CompareTag(myGemTag)) return;   

        PickUp(other.gameObject);

        //if (baseTarget && mover)
            //Ir a la base
    }

    public void PickUp(GameObject gem)
    {
        carrying = gem;

        if (gem.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        if (gem.TryGetComponent<Collider>(out var col)) col.enabled = false;

        if (!carryPoint) carryPoint = transform;
        gem.transform.SetParent(carryPoint);
        gem.transform.localPosition = Vector3.zero;
        gem.transform.localRotation = Quaternion.identity;
    }

    public bool IsCarryingTag(string tag) =>
        carrying != null && carrying.CompareTag(tag);

    public void DropAt(Vector3 worldPos)
    {
        if (carrying == null) return;

        var gem = carrying;
        carrying = null;

        gem.transform.SetParent(null);
        gem.transform.position = worldPos;
        SimulationStats.RegisterDelivery();

        if (gem.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = false;
        if (gem.TryGetComponent<Collider>(out var col)) col.enabled = true;
    }
}