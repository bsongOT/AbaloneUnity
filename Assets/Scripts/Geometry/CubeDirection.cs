public enum CubeDirection
{
    Left = 0,
    TopLeft,
    TopRight,
    Right,
    BottomRight,
    BottomLeft
}

public static class CubeDirectionExtensions
{
    public static CubeCoord[] directionToCoordMap = new CubeCoord[] {
        new CubeCoord(-1, 1, 0), new CubeCoord(0, 1, -1), new CubeCoord(1, 0, -1),
        new CubeCoord(1, -1, 0), new CubeCoord(0, -1, 1), new CubeCoord(-1, 1, 1)
    };

    public static CubeCoord ToCoord(this CubeDirection direction)
    {
        return directionToCoordMap[(int)direction];
    }
}