using System;

public struct CubeCoord
{
    public int x;
    public int y;
    public int z;

    public CubeCoord(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public int Distance(CubeCoord other)
    {
        return (Math.Abs(x - other.x) + Math.Abs(y - other.y) + Math.Abs(z - other.z)) / 2;
    }

    public static implicit operator CubeCoord(AxialCoord axial)
    {
        return new CubeCoord(axial.x, -axial.x - axial.z, axial.z);
    }
}