using System.Collections.Generic;
using UnityEngine;

namespace Yontalane.GridNav
{
    public class GridNavigator<T> where T : class, IGridNode
    {
        #region Structs & Enums
        private struct VisitedNode
        {
            public T node;
            public int visited;
        }

        private enum Direction
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }
        #endregion

        #region Delegates
        public delegate void PathingCompleteHandler(bool pathExists);
        public PathingCompleteHandler OnComplete;
        #endregion

        #region Private Variables
        private VisitedNode[,] m_visitedNodes;
        private Vector2Int m_startCoord;
        private Vector2Int m_endCoord;
        private readonly List<VisitedNode> m_path;
        #endregion

        public GridNavigator() => m_path = new List<VisitedNode>();

        /// <summary>
        /// The number of nodes along the path to reach the goal.
        /// </summary>
        public int PathCount => m_path.Count;

        /// <summary>
        /// The node at the given index along the path to reach the goal.
        /// </summary>
        public T GetPathNode(int index) => index >= 0 && index < m_path.Count ? m_path[index].node : null;

        /// <summary>
        /// Find a path to the goal. Invokes delegate when finding is complete.
        /// </summary>
        public void FindPath(Vector2Int start, Vector2Int end, T[,] gridNodes)
        {
            m_startCoord = start;
            m_endCoord = end;
            GenerateGrid(gridNodes);
            SetDistance();
            SetPath();
        }

        private void GenerateGrid(T[,] gridNodes)
        {
            m_visitedNodes = new VisitedNode[gridNodes.GetLength(0), gridNodes.GetLength(1)];

            for (int x = 0; x < gridNodes.GetLength(0); x++)
            {
                for (int y = 0; y < gridNodes.GetLength(1); y++)
                {
                    m_visitedNodes[x, y] = new VisitedNode()
                    {
                        node = gridNodes[x, y],
                        visited = -1
                    };
                }
            }
        }

        private void SetDistance()
        {
            InitialSetup();
            Vector2Int coord = m_startCoord;
            int[] testArray = new int[m_visitedNodes.GetLength(0) * m_visitedNodes.GetLength(1)];
            for (int step = 1; step < testArray.Length; step++)
            {
                foreach (VisitedNode obj in m_visitedNodes)
                {
                    if (obj.node != null && obj.visited == step - 1)
                    {
                        TestFourDirections(obj.node.GetCoordinate(), step);
                    }
                }
            }
        }

        private void SetPath()
        {
            int step;
            Vector2Int coord = m_endCoord;
            List<VisitedNode> tempList = new List<VisitedNode>();
            m_path.Clear();
            if (coord.x >= 0 && coord.x < m_visitedNodes.GetLength(0) && coord.y >= 0 && coord.y < m_visitedNodes.GetLength(1) && m_visitedNodes[m_endCoord.x, m_endCoord.y].node != null && m_visitedNodes[m_endCoord.x, m_endCoord.y].visited > 0)
            {
                m_path.Insert(0, m_visitedNodes[coord.x, coord.y]);
                step = m_visitedNodes[coord.x, coord.y].visited - 1;
            }
            else
            {
                OnComplete?.Invoke(false);
                return;
            }
            for (int i = step; step > -1; step--)
            {
                if (TestDirection(coord, step, Direction.Up))
                {
                    tempList.Add(m_visitedNodes[coord.x, coord.y + 1]);
                }
                if (TestDirection(coord, step, Direction.Right))
                {
                    tempList.Add(m_visitedNodes[coord.x + 1, coord.y]);
                }
                if (TestDirection(coord, step, Direction.Down))
                {
                    tempList.Add(m_visitedNodes[coord.x, coord.y - 1]);
                }
                if (TestDirection(coord, step, Direction.Left))
                {
                    tempList.Add(m_visitedNodes[coord.x - 1, coord.y]);
                }

                VisitedNode tempObj = FindClosest(m_visitedNodes[m_endCoord.x, m_endCoord.y], tempList);
                m_path.Insert(0, tempObj);
                coord = tempObj.node.GetCoordinate();
                tempList.Clear();
            }
            OnComplete?.Invoke(true);
        }

        private void InitialSetup()
        {
            for (int x = 0; x < m_visitedNodes.GetLength(0); x++)
            {
                for (int y = 0; y < m_visitedNodes.GetLength(1); y++)
                {
                    if (m_visitedNodes[x, y].node != null)
                    {
                        m_visitedNodes[x, y].visited = -1;
                    }
                }
            }
            m_visitedNodes[m_startCoord.x, m_startCoord.y].visited = 0;
        }

        private bool TestDirection(Vector2Int coord, int step, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    if (coord.y + 1 < m_visitedNodes.GetLength(1) && m_visitedNodes[coord.x, coord.y + 1].node != null && m_visitedNodes[coord.x, coord.y + 1].visited == step)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Right:
                    if (coord.x + 1 < m_visitedNodes.GetLength(0) && m_visitedNodes[coord.x + 1, coord.y].node != null && m_visitedNodes[coord.x + 1, coord.y].visited == step)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Down:
                    if (coord.y - 1 >= 0 && m_visitedNodes[coord.x, coord.y - 1].node != null && m_visitedNodes[coord.x, coord.y - 1].visited == step)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Left:
                    if (coord.x - 1 >= 0 && m_visitedNodes[coord.x - 1, coord.y].node != null && m_visitedNodes[coord.x - 1, coord.y].visited == step)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
            return false;
        }

        private void TestFourDirections(Vector2Int coord, int step)
        {
            if (TestDirection(coord, -1, Direction.Up))
            {
                SetVisited(coord + Vector2Int.up, step);
            }
            if (TestDirection(coord, -1, Direction.Right))
            {
                SetVisited(coord + Vector2Int.right, step);
            }
            if (TestDirection(coord, -1, Direction.Down))
            {
                SetVisited(coord + Vector2Int.down, step);
            }
            if (TestDirection(coord, -1, Direction.Left))
            {
                SetVisited(coord + Vector2Int.left, step);
            }
        }

        private void SetVisited(Vector2Int coord, int step)
        {
            if (coord.x >= 0 && coord.x < m_visitedNodes.GetLength(0))
            {
                if (coord.y >= 0 && coord.y < m_visitedNodes.GetLength(1))
                {
                    if (m_visitedNodes[coord.x, coord.y].node != null)
                    {
                        m_visitedNodes[coord.x, coord.y].visited = step;
                    }
                }
            }
        }

        private VisitedNode FindClosest(VisitedNode targetLocation, List<VisitedNode> list)
        {
            float closestDistance = Mathf.Infinity;
            int closestIndex = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].node != null)
                {
                    float distance = Vector2.Distance(targetLocation.node.GetCoordinate(), list[i].node.GetCoordinate());
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestIndex = i;
                    }
                }
            }
            return list[closestIndex];
        }
    }
}