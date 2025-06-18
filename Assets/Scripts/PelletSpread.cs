using UnityEngine;

public static class PelletSpread
{
    public static Vector3[] Generate(Vector3 forward, Vector3 up, Vector3 right, float spread)
    {
        Vector3[] directions = new Vector3[9];
        int index = 0;

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector3 offset = (right * x + up * y) * spread;
                directions[index++] = (forward + offset).normalized;
            }
        }

        return directions;
    }
}

