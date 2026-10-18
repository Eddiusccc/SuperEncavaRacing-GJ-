using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] bool lockY = true;   // true: solo gira en horizontal (no se acuesta)
    Transform cam;

    void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main != null) cam = Camera.main.transform;
            else return;
        }

        Vector3 f = cam.forward;
        if (lockY) f.y = 0f;
        if (f.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(f);
    }
}
