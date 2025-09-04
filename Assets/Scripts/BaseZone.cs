using UnityEngine;

public class BaseZone : MonoBehaviour
{
    public string acceptsGemTag;

    public Transform dropPoint;

    void OnTriggerEnter(Collider other)
    {
        var collector = other.GetComponentInParent<Grab>();
        if (collector == null) return;

        if (!collector.IsCarryingTag(acceptsGemTag)) return;

        Vector3 pos = dropPoint ? dropPoint.position : transform.position;
        collector.DropAt(pos);
    }
}
