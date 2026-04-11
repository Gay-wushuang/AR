using UnityEngine;

public class QuickMoonGround : MonoBehaviour
{
    public float groundSize = 50f;
    public Color groundColor = new Color(0.3f, 0.3f, 0.3f);

    void Start()
    {
        CreateMoonGround();
    }

    void CreateMoonGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "MoonGround";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(groundSize / 10f, 1f, groundSize / 10f);

        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = groundColor;
        }

        Collider collider = ground.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        ground.layer = LayerMask.NameToLayer("Default");
    }
}
