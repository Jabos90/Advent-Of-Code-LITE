using Common.Enums;

namespace Common.Extensions;

public static class Enums
{   /// <summary>
    /// Applies movement vector of the current facing to the current position
    /// </summary>
    /// <param name="position">The origin position</param>
    /// <param name="direction">The direction to move</param>
    /// <param name="set">Whether or not to directly update the given position</param>
    /// <returns>The new position</returns>
    public static (int X, int Y) Move(this ref (int X, int Y) position, Cardinal direction, bool set = true)
    {
        var vector = direction.MovementVector();
        var newPosition = position.Add(vector);
        if (set) position = newPosition;
        return newPosition;
    }

    /// <summary>
    /// Get the movement vector for a given direction
    /// </summary>
    /// <param name="direction">The direction to get a movement vector for</param>
    /// <returns>The movement vector for the given direction</returns>
    private static (int X, int Y) MovementVector(this Cardinal direction) =>
        direction switch
        {
            Cardinal.North => (0, -1),
            Cardinal.East => (1, 0),
            Cardinal.South => (0, 1),
            Cardinal.West => (-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
}