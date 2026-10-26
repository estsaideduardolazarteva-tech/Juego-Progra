using UnityEngine;

public class CamaraSigue : MonoBehaviour
{
    public Transform objetivo;
    public float minX, maxX;

    Camera cam;

    void Awake() { cam = GetComponent<Camera>(); }

    void LateUpdate()
    {
        if (objetivo == null) return;
        float mitad = cam.orthographicSize * cam.aspect;
        float x = Mathf.Clamp(objetivo.position.x + 1.5f, minX + mitad, maxX - mitad);
        Vector3 p = transform.position;
        p.x = Mathf.Lerp(p.x, x, 10f * Time.deltaTime);
        p.y = 0f;
        p.z = -10f;
        transform.position = p;
    }
}
