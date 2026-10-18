using UnityEngine;

[RequireComponent(typeof(Rigidbody))]   // kinematic + collider Is Trigger
public class TrafficCar : MonoBehaviour
{
    public float Speed = 12f;           // m/s, siempre menor que el bus
    void Update() => transform.position += Vector3.forward * (Speed * Time.deltaTime);
}