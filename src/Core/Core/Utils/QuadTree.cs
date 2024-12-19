using System.Diagnostics;
using System.Drawing;
using QuantumCore.API.Core.Utils;
using QuantumCore.API.Game.World;

namespace QuantumCore.Core.Utils
{
    [DebuggerDisplay("{Bounds}")]
    public class QuadTree : IQuadTree
    {
        private const int MinQuadSize = 16;
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public uint Capacity { get; private set; }
        public Rectangle Bounds { get; private set; }
        public bool Subdivided { get; private set; }
        public List<IEntity> Objects { get; } = new List<IEntity>();

        private QuadTree _nw = null!;
        private QuadTree _ne = null!;
        private QuadTree _sw = null!;
        private QuadTree _se = null!;

        public QuadTree(int x, int y, int width, int height, uint capacity)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Capacity = capacity;
            Bounds = new Rectangle(X, Y, Width, Height);
        }

        public bool Insert(IEntity obj)
        {
            if (!Bounds.Contains(obj.PositionX, obj.PositionY)) return false;

            if (Objects.Count < Capacity && !Subdivided)
            {
                // We still have places in the quad and are not subdivided yet
                obj.LastPositionX = obj.PositionX;
                obj.LastPositionY = obj.PositionY;
                obj.LastQuadTree = this;

                Objects.Add(obj);
                return true;
            }

            if (!Subdivided)
            {
                if (Width > MinQuadSize && Height > MinQuadSize)
                {
                    // No place left but we aren't subdivded yet
                    Subdivide();
                }
                else
                {
                    // quad is too small to subdivide - increase capacity
                    Capacity++;
                    return Insert(obj);
                }
            }

            // Add the object to one of our quadrants
            return _nw.Insert(obj) || _ne.Insert(obj) || _sw.Insert(obj) || _se.Insert(obj);
        }

        public bool Remove(IEntity obj)
        {
            if (Subdivided)
            {
                return _nw.Remove(obj) || _ne.Remove(obj) || _sw.Remove(obj) || _se.Remove(obj);
            }

            if (Objects.Remove(obj))
            {
                obj.LastQuadTree = null;
                return true;
            }

            return false;
        }

        public void QueryAround(List<IEntity> objects, int x, int y, int radius, EEntityType? filter = null)
        {
            // Check if the circle is in our bounds
            if (!CircleIntersects(x, y, radius)) return;

            // If we are divided ask our child quadrants
            if (Subdivided)
            {
                _ne.QueryAround(objects, x, y, radius, filter);
                _nw.QueryAround(objects, x, y, radius, filter);
                _se.QueryAround(objects, x, y, radius, filter);
                _sw.QueryAround(objects, x, y, radius, filter);
            }
            else
            {
                // Go through all objects and check if their position is inside the circle
                foreach (var obj in Objects)
                {
                    if (filter != null && obj.Type != filter)
                    {
                        continue;
                    }

                    if (Math.Pow(obj.PositionX - x, 2) + Math.Pow(obj.PositionY - y, 2) <= Math.Pow(radius, 2))
                    {
                        objects.Add(obj);
                    }
                }
            }
        }

        private bool CircleIntersects(int x, int y, int radius)
        {
            var halfWidth = Width / 2;
            var halfHeight = Height / 2;
            var centerX = X + halfWidth;
            var centerY = Y + halfHeight;

            var xDist = Math.Abs(centerX - x);
            var yDist = Math.Abs(centerY - y);

            var edges = Math.Pow(xDist - halfWidth, 2) + Math.Pow(yDist - halfHeight, 2);
            if (xDist > radius + halfWidth || yDist > radius + halfHeight)
            {
                return false;
            }

            if (xDist <= halfWidth || yDist <= halfHeight)
            {
                return true;
            }

            return edges <= Math.Pow(radius, 2);
        }

        private void Subdivide()
        {
            var halfWidth1 = Width / 2;
            var halfHeight1 = Height / 2;

            // when we have a none dividable number we have to make one quadrant bigger than the other to avoid gaps
            var halfWidth2 = Width > 2 && Width % 2 > 0 ? Width / 2 + Width % 2 : halfWidth1;
            var halfHeight2 = Height > 2 && Height % 2 > 0 ? Height / 2 + Height % 2 : halfHeight1;

            _nw = new QuadTree(X, Y, halfWidth1, halfHeight1, Capacity);
            _ne = new QuadTree(X, Y + halfHeight1, halfWidth1, halfHeight2, Capacity);
            _sw = new QuadTree(X + halfWidth1, Y, halfWidth2, halfHeight1, Capacity);
            _se = new QuadTree(X + halfWidth1, Y + halfHeight1, halfWidth2, halfHeight2, Capacity);
            Subdivided = true;

            // Move our own objects to our children
            foreach (var entity in Objects)
            {
                entity.LastQuadTree = null;
                var addedOnNw = false;
                var addedOnNe = false;
                var addedOnSw = false;
                var addedOnSe = false;
                addedOnNw = _nw.Insert(entity);
                if (!addedOnNw)
                {
                    addedOnNe = _ne.Insert(entity);
                }

                if (!addedOnNe)
                {
                    addedOnSw = _sw.Insert(entity);
                }

                if (!addedOnSw)
                {
                    addedOnSe = _se.Insert(entity);
                }

                Debug.Assert(addedOnNw || addedOnNe || addedOnSw || addedOnSe, "Entity must be added to any quadrant");
            }

            Objects.Clear();
        }

        public void UpdatePosition(IEntity entity)
        {
            if (entity.LastQuadTree == null)
            {
                Insert(entity);
                return;
            }

            if (entity.LastQuadTree is QuadTree qt)
            {
                if (!qt.Bounds.Contains(entity.PositionX, entity.PositionY))
                {
                    qt.Remove(entity);
                    Insert(entity);
                }
            }
        }
    }
}
