using System;

[Serializable]
public class Fare
{
    public int Origin;
    public int Destination;
    public int Passengers;
    public float Distance;        // metros entre paradas
    public float ParTime;         // segundos "justos" de viaje
    public float PickedAt = -1f;  // Time.time al subir
}
