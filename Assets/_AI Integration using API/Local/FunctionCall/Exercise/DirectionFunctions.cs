using UnityEngine;
// This class contains static methods that return different directions as Vector3
public class DirectionFunctions
{
    public static int MoveLeft()
    {
        return -1;  // Returns (-1, 0, 0)
    }

    public static int MoveRight()
    {
        return 1; // Returns (1, 0, 0)
    }
    public static int NoDirectionsMentioned()
    {
        return 0;  // Returns (0, 0, 0) when no direction is specified
    }
}
