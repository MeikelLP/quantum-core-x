using System.Diagnostics;

namespace QuantumCore.Core.Utils;

public class Grid<T> where T : class?
{
    public uint Width { get; private set; }
    public uint Height { get; private set; }

    private T[,] _grid;

    public Grid(uint width, uint height)
    {
        Width = width;
        Height = height;
        _grid = new T[Width,Height];
    }

    public void Resize(uint width, uint height)
    {
        Width = width;
        Height = height;
        _grid = new T[Width,Height];
    }

    public T? Get(uint x, uint y)
    {
        if (x >= Width || y >= Height)
            return null;

        return _grid[x, y];
    }

    public void Set(uint x, uint y, T value)
    {
        if (x >= Width || y >= Height)
            return;

        _grid[x, y] = value;
    }

    public void SetBlock(uint x, uint y, uint width, uint height, T value)
    {
        for (var x2 = x; x2 < x + width && x < Width; x2++)
        {
            for (var y2 = y; y2 < y + height && y < Height; y2++)
            {
                Set(x2, y2, value);
            }
        }
    }

    public (long, long) GetFreePosition(uint width, uint height)
    {
        Debug.Assert(width > 0);
        Debug.Assert(height > 0);

        for (uint y = 0; y < Height - height + 1; y++)
        {
            for (uint x = 0; x < Width - width + 1; x++)
            {
                if (Get(x, y) is not null)
                {
                    continue;
                }

                var isFree = true;
                for (var y2 = y; y2 < y + height; y2++)
                {
                    for (var x2 = x; x2 < x + width; x2++)
                    {
                        isFree = Get(x2, y2) is null;
                        if (!isFree)
                        {
                            break;
                        }
                    }

                    if (!isFree)
                    {
                        break;
                    }
                }

                if (isFree)
                {
                    return (x, y);
                }
            }
        }

        return (-1, -1);
    }
}
