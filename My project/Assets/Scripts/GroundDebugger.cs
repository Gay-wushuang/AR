using UnityEngine;

public class GroundDebugger : MonoBehaviour
{
    public float rayDistance = 10f;

    void Update()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, Color.green);

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance))
        {
            Debug.Log($"✅ 检测到地面: {hit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, Distance: {hit.distance}");
            Debug.DrawRay(rayOrigin, Vector3.down * hit.distance, Color.red);
        }
        else
        {
            Debug.Log($"❌ 未检测到地面");
        }
    }
}
