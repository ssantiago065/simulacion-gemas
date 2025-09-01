using UnityEngine;

public class Detect : MonoBehaviour
{
    Grab owner;

    public void Init(Grab r) { owner = r; }

    void OnTriggerEnter(Collider other)
    {
        if (owner) owner.OnDetected(other);
    }
}