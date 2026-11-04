using System;
using System.Collections;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    public Collider sphereCollider;

    void Awake()
    {
        if (sphereCollider == null) sphereCollider = GetComponent<Collider>();
    }

    public void MoveTowardsBus(Vector3 busPosition, float speed = 4f, Action onArrived = null)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(busPosition, speed, onArrived));
    }

    IEnumerator MoveRoutine(Vector3 target, float speed, Action onArrived)
    {
        while (Vector3.Distance(transform.position, target) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        onArrived?.Invoke();
    }
}
