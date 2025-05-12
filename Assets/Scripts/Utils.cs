using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector3 RoundNextPosToGrid(Vector3 position, Vector2 velocity)
    {
        Vector3 tPosition = position;
        if (velocity.x != 0)
        {
            tPosition.x = (float) Math.Round(position.x) + (velocity.x > 0 ? 0.5f : -0.5f);
        }
        if (velocity.y != 0)
            tPosition.y = (float) (velocity.y > 0 ? Math.Ceiling(position.y) : Math.Floor(position.y));
        return tPosition;
    }

    public static Vector3 RoundOffToGrid(Vector3 position)
    {
        Vector3 tPosition = position;
        tPosition.x = (float) Math.Floor(position.x) + 0.5f;
        tPosition.y = (float) Math.Round(position.y);
        return tPosition;
    }
}
