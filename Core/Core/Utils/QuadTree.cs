using System;
using System.Collections.Generic;
using System.Drawing;
using QuantumCore.Game.World;

namespace QuantumCore.Core.Utils
{
    public class QuadTree<T> where T : Entity
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public uint Capacity { get; private set; }
        public Rectangle Bounds { get; private set; }
        public bool Subdivided { get; private set; }
        public List<T> Objects { get; } = new List<T>();

        private QuadTree<T> _nw;
        private QuadTree<T> _ne;
        private QuadTree<T> _sw;
        private QuadTree<T> _se;

        public QuadTree(int x, int y, int width, int height, uint capacity)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Capacity = capacity;
            Bounds = new Rectangle(X, Y, Width, Height);
        }
        
        public bool Insert(T obj)
        {
            if (!Bounds.Contains(obj.PositionX, obj.PositionY)) return false;

            if (Objects.Count < Capacity && !Subdivided)
            {
                // We still have places in the quad and are not subdivided yet
                Objects.Add(obj);
                return true;
            }

            if (!Subdivided)
            {
                // No place left but we aren't subdivded yet
                Subdivide();
            }

            // Add the object to one of our quadrants
            return _nw.Insert(obj) || _ne.Insert(obj) || _sw.Insert(obj) || _se.Insert(obj);
        }

        public void QueryAround(List<T> objects, int x, int y, int radius)
        {
            // Check if the circle is in our bounds
            if (!CircleIntersects(x, y, radius)) return;

            // If we are divided ask our child quadrants
            if (Subdivided)
            {
                _ne.QueryAround(objects, x, y, radius);
                _nw.QueryAround(objects, x, y, radius);
                _se.QueryAround(objects, x, y, radius);
                _sw.QueryAround(objects, x, y, radius);
            }
            else
            {
                // Go through all objects and check if their position is inside the circle
                foreach (var obj in Objects)
                {
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
            var halfWidth = Width / 2;
            var halfHeight = Height / 2;
            
            _nw = new QuadTree<T>(X, Y, halfWidth, halfHeight, Capacity);
            _ne = new QuadTree<T>(X, Y + halfHeight, halfWidth, halfHeight, Capacity);
            _sw = new QuadTree<T>(X + halfWidth, Y, halfWidth, halfHeight, Capacity);
            _se = new QuadTree<T>(X + halfWidth, Y + halfHeight, halfWidth, halfHeight, Capacity);
            Subdivided = true;
            
            // Move our own objects to our children
            foreach (var entity in Objects)
            {
                if (_nw.Insert(entity) || _ne.Insert(entity) || _sw.Insert(entity) || _se.Insert(entity)) ;
            }
            Objects.Clear();
        }
    }
}