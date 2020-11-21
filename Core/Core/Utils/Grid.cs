namespace QuantumCore.Core.Utils
{
    public class Grid<T> where T : class
    {
        public uint Width { get; private set; }
        public uint Height { get; private set; }
        
        private T[,] _grid;
        
        public Grid(uint width, uint height)
        {
            Resize(width, height);
            Width = width;
            Height = height;
        }

        public void Resize(uint width, uint height)
        {    
            Width = width;
            Height = height;
            _grid = new T[Width,Height];
        }

        public T Get(uint x, uint y)
        {
            return _grid[x, y];
        }

        public void Set(uint x, uint y, T value)
        {
            _grid[x, y] = value;
        }
    }
}