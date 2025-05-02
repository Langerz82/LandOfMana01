using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float GetNextRoundedPosition(float value, float round, bool roundUp = true)
    {
        float tval = 1 / round;

        float roundedValue = (float) ((roundUp ? Math.Ceiling(value * tval) : Math.Floor(value * tval)) / tval);

        return roundedValue;
    }

    public static Vector3 RoundToGrid(Vector3 position, Vector2 velocity)
    {
        Vector3 tPosition = position;
        if (velocity.x != 0)
        {
            tPosition.x = Utils.GetNextRoundedPosition(position.x, 0.5f, velocity.x > 0);
            if (tPosition.x % 1 == 0)
            {
                tPosition.x = tPosition.x + (velocity.x > 0 ? 0.5f : -0.5f);
            }
        }
        if (velocity.y != 0)
            tPosition.y = Utils.GetNextRoundedPosition(position.y, 1f, velocity.y > 0);
        return tPosition;
    }
}
