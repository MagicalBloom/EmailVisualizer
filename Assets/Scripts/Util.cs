using UnityEngine;

public class Util {

    public static Vector3 CreatePositionRandom(float range)
    {
        Vector3 returnedPosition = Vector3.zero;

        returnedPosition.x = Random.Range(Mathf.Clamp(range * -1, range * -1, 0), Mathf.Clamp(range, 0, range));
        returnedPosition.y = Random.Range(Mathf.Clamp(range * -1, range * -1, 0), Mathf.Clamp(range, 0, range));
        returnedPosition.z = Random.Range(Mathf.Clamp(range * -1, range * -1, 0), Mathf.Clamp(range, 0, range));

        return returnedPosition;
    }

    public static float CalcRepulsion(Vector3 from, Vector3 to, float repulsion)
    {
        float distance = Mathf.Max(Vector3.Distance(from, to), 0.1f);
        return -repulsion * repulsion / Mathf.Max(distance, 0.1f); // Fruchterman & Reingold
    }

    public static float CalcAttraction(Vector3 from, Vector3 to, float attraction)
    {
        float distance = Mathf.Max(Vector3.Distance(from, to), 0.1f);
        return Mathf.Max((distance * distance) / attraction, 0.1f); // Fruchterman & Reingold
    }
}