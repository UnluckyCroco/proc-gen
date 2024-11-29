using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionEnum : MonoBehaviour
{
    public static Direction ReverseDirection(Direction dir)
    {
        if (dir == Direction.North)
            return Direction.South;
        else if (dir == Direction.South)
            return Direction.North;
        else if (dir == Direction.East)
            return Direction.West;
        else 
            return Direction.East;
    }
}

public enum Direction { North, West, South, East };
