using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Cosmos.QuadTree
{
    /// <summary>
    /// �Ĳ�����
    /// </summary>
    /// <typeparam name="T">�Ĳ����Զ��������ģ��</typeparam>
    public class QuadTree<T>
    {
        /// <summary>
        ///��ǰ���ڵ����飻
        /// </summary>
        public QuadTreeRect Area { get; private set; }
        /// <summary>
        /// ��ǰ���ڵ��������󼯺ϣ�
        /// </summary>
        readonly HashSet<T> objectSet;
        /// <summary>
        /// �Ĳ����ж������Ч�߽��ȡ�ӿڣ�
        /// </summary>
        readonly IObjecRectangletBound<T> objectRectangleBound;
        /// <summary>
        /// �Ƿ�����ӽڵ㣻
        /// </summary>
        bool hasChildren;
        /// <summary>
        /// ��ǰ��ȣ�
        /// </summary>
        public int CurrentDepth { get; private set; }
        /// <summary>
        /// �����ȣ�
        /// </summary>
        public int MaxDepth { get; private set; }
        /// <summary>
        /// ��ǰRect�Ķ���������
        /// </summary>
        public int ObjectCapacity { get; private set; }
        /// <summary>
        /// TopRight Quadrant1
        /// </summary>
        QuadTree<T> treeTR;
        /// <summary>
        /// TopLeft Quadrant2
        /// </summary>
        QuadTree<T> treeTL;
        /// <summary>
        /// BottomLeft Quadrant3
        /// </summary>
        QuadTree<T> treeBL;
        /// <summary>
        /// BottomRight Quadrant4
        /// </summary>
        QuadTree<T> treeBR;
        public QuadTree(float x, float y, float width, float height, IObjecRectangletBound<T> quadTreebound, int objectCapacity = 10, int maxDepth = 5, int currentDepth = 0)
        {
            Area = new QuadTreeRect(x, y, width, height);
            objectSet = new HashSet<T>();
            this.objectRectangleBound = quadTreebound;
            this.CurrentDepth = currentDepth;
            this.MaxDepth = maxDepth;
            this.ObjectCapacity = objectCapacity;
            hasChildren = false;
        }
        public QuadTree(float width, float height, IObjecRectangletBound<T> objectBound, int maxObject = 10, int maxDepth = 5, int currentDepth = 0)
            : this(0, 0, width, height, objectBound, maxObject, maxDepth, currentDepth) { }
        /// <summary>
        /// �и��ĵȷ֣�
        /// </summary>
        public bool Insert(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (!IsObjectInside(obj)) return false;
            if (hasChildren)
            {
                if (treeTL.Insert(obj)) return true;
                if (treeTR.Insert(obj)) return true;
                if (treeBL.Insert(obj)) return true;
                if (treeBR.Insert(obj)) return true;
            }
            else
            {
                objectSet.Add(obj);
                if (objectSet.Count > ObjectCapacity)
                {
                    Quarter();
                }
            }
            return true;
        }
        public void Quarter()
        {
            if (CurrentDepth >= MaxDepth) return;
            int nextDepth = CurrentDepth + 1;
            hasChildren = true;
            treeTL = new QuadTree<T>(Area.X, Area.Y, Area.HalfWidth, Area.HalfHeight, objectRectangleBound, ObjectCapacity, MaxDepth, nextDepth);
            treeTR = new QuadTree<T>(Area.CenterX, Area.Y, Area.HalfWidth, Area.HalfHeight, objectRectangleBound, ObjectCapacity, MaxDepth, nextDepth);
            treeBL = new QuadTree<T>(Area.X, Area.CenterY, Area.HalfWidth, Area.HalfHeight, objectRectangleBound, ObjectCapacity, MaxDepth, nextDepth);
            treeBR = new QuadTree<T>(Area.CenterX, Area.CenterY, Area.HalfWidth, Area.HalfHeight, objectRectangleBound, ObjectCapacity, MaxDepth, nextDepth);
            foreach (var obj in objectSet)
            {
                Insert(obj);
            }
            objectSet.Clear();
        }
        public void Clear()
        {
            if (hasChildren)
            {
                treeTR.Clear();
                treeTR = null;
                treeTL.Clear();
                treeTL = null;
                treeBR.Clear();
                treeBR = null;
                treeBL.Clear();
                treeBL = null;
            }
            objectSet.Clear();
            hasChildren = false;
            Area.IsOverlapped = false;
        }
        public int Count()
        {
            int count = 0;
            if (hasChildren)
            {
                count += treeTR.Count();
                count += treeTL.Count();
                count += treeBR.Count();
                count += treeBL.Count();
            }
            else
            {
                count = objectSet.Count;
            }
            return count;
        }
        /// <summary>
        /// ��ȡ�����е����ж���
        /// </summary>
        /// <param name="rect">rect����</param>
        /// <returns>�����е����ж���</returns>
        public T[] FindObjects(QuadTreeRect rect)
        {
            List<T> foundObjects = new List<T>();
            if (hasChildren)
            {
                foundObjects.AddRange(treeTR.FindObjects(rect));
                foundObjects.AddRange(treeTL.FindObjects(rect));
                foundObjects.AddRange(treeBR.FindObjects(rect));
                foundObjects.AddRange(treeBL.FindObjects(rect));
            }
            else
            {
                if (IsOverlapping(rect))
                {
                    foundObjects.AddRange(objectSet);
                }
            }
            HashSet<T> result = new HashSet<T>();
            result.UnionWith(foundObjects);
            return result.ToArray();
        }
        /// <summary>
        /// ��ȡ��������������������ͬ��ȵĶ���
        /// </summary>
        /// <param name="obj">���еĶ���</param>
        /// <returns>��ͬ����Ķ���</returns>
        public T[] FindObjects(T obj)
        {
            return FindObjects(new QuadTreeRect(objectRectangleBound.GetPositonX(obj), objectRectangleBound.GetPositonY(obj), objectRectangleBound.GetWidth(obj), objectRectangleBound.GetHeight(obj)));
        }
        /// <summary>
        ///����һ�����󼯺ϣ� 
        /// </summary>
        public void InsertRange(IEnumerable<T> objects)
        {
            foreach (T obj in objects)
            {
                Insert(obj);
            }
        }
        public QuadTreeRect[] GetGrid()
        {
            List<QuadTreeRect> grid = new List<QuadTreeRect> { Area };
            if (hasChildren)
            {
                grid.AddRange(treeTR.GetGrid());
                grid.AddRange(treeTL.GetGrid());
                grid.AddRange(treeBR.GetGrid());
                grid.AddRange(treeBL.GetGrid());
            }
            return grid.ToArray();
        }
        /// <summary>
        ///�Ƿ���ȫ�ص��� 
        /// </summary>
        bool IsOverlapping(QuadTreeRect rect)
        {
            if (rect.Right < Area.Left || rect.Left > Area.Right) return false;
            if (rect.Top > Area.Bottom || rect.Bottom < Area.Top) return false;
            Area.IsOverlapped = true;
            return true;
        }
        /// <summary>
        /// �����Ƿ�����ڵ�ǰrect�У�
        /// </summary>
        bool IsObjectInside(T go)
        {
            var x = objectRectangleBound.GetPositonX(go);
            var y = objectRectangleBound.GetPositonY(go);
            var width = objectRectangleBound.GetWidth(go);
            var height = objectRectangleBound.GetHeight(go);
            var top = y + height * 0.5f;
            var bottom = y - height * 0.5f;
            var left = x - width * 0.5f;
            var right = x + width * 0.5f;
            if (top > Area.Bottom) return false;
            if (bottom < Area.Top) return false;
            if (left >Area.Right) return false;
            if (right < Area.Left) return false;
            return true;
        }
    }
}