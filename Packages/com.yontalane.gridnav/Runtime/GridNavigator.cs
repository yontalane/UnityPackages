using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yontalane.GridNav
{
    public class GridNavigator
    {
        #region Structs & Enums
        private struct VisitedNode
        {
            public Vector2Int coordinate;
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
        private Vector2Int m_gridSize;
        private readonly Func<int, int, bool> m_nodeIsValid;
        private readonly Func<int, int, int, int, bool> m_stepIsValid;
        #endregion

        public GridNavigator(Vector2Int gridSize, Func<int, int, bool> nodeIsValid, Func<int, int, int, int, bool> stepIsValid = null)
        {
            m_gridSize = gridSize;
            m_nodeIsValid = nodeIsValid;
            m_stepIsValid = stepIsValid;
            m_path = new List<VisitedNode>();
        }

        public GridNavigator(int gridWidth, int gridHeight, Func<int, int, bool> nodeIsValid, Func<int, int, int, int, bool> stepIsValid = null) : this(new Vector2Int(gridWidth, gridHeight), nodeIsValid, stepIsValid)
        {

        }

        /// <summary>
        /// The number of nodes along the path to reach the goal.
        /// </summary>
        public int PathCount => m_path.Count;

        /// <summary>
        /// The node at the given index along the path to reach the goal.
        /// </summary>
        public Vector2Int GetPathNode(int index) => m_path[index].coordinate;

        /// <summary>
        /// Find a path to the goal. Invokes delegate when finding is complete.
        /// </summary>
        public void FindPath(Vector2Int start, Vector2Int end)
        {
            m_startCoord = start;
            m_endCoord = end;
            GenerateGrid();
            SetDistance();
            _ = SetPath(true);
        }

        /// <summary>
        /// Find a path to the goal and return whether path was found.
        /// </summary>
        public bool FindPathSynchronous(Vector2Int start, Vector2Int end)
        {
            m_startCoord = start;
            m_endCoord = end;
            GenerateGrid();
            SetDistance();
            return SetPath(false);
        }

        /// <summary>
        /// Find a path to the goal. Invokes delegate when finding is complete.
        /// </summary>
        public void FindPath(int startX, int startY, int endX, int endY) => FindPath(new Vector2Int(startX, startY), new Vector2Int(endX, endY));

        /// <summary>
        /// Find a path to the goal and return whether path was found.
        /// </summary>
        public bool FindPathSynchronous(int startX, int startY, int endX, int endY) => FindPathSynchronous(new Vector2Int(startX, startY), new Vector2Int(endX, endY));

        private void GenerateGrid()
        {
            m_visitedNodes = new VisitedNode[m_gridSize.x, m_gridSize.y];

            for (int x = 0; x < m_visitedNodes.GetLength(0); x++)
            {
                for (int y = 0; y < m_visitedNodes.GetLength(1); y++)
                {
                    m_visitedNodes[x, y] = new VisitedNode()
                    {
                        coordinate = new Vector2Int(x, y),
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
                foreach (VisitedNode visitedNode in m_visitedNodes)
                {
                    if (m_nodeIsValid(visitedNode.coordinate.x, visitedNode.coordinate.y) && visitedNode.visited == step - 1)
                    {
                        TestFourDirections(visitedNode.coordinate, step);
                    }
                }
            }
        }

        private bool SetPath(bool invokeCallback)
        {
            int step = 0;
            Vector2Int coord = m_endCoord;
            List<VisitedNode> tempList = new();
            m_path.Clear();
            Vector2Int test = m_visitedNodes[m_endCoord.x, m_endCoord.y].coordinate;
            if (coord.x >= 0 && coord.x < m_visitedNodes.GetLength(0) && coord.y >= 0 && coord.y < m_visitedNodes.GetLength(1) && m_nodeIsValid(test.x, test.y) && m_visitedNodes[m_endCoord.x, m_endCoord.y].visited > 0)
            {
                m_path.Insert(0, m_visitedNodes[coord.x, coord.y]);
                step = m_visitedNodes[coord.x, coord.y].visited - 1;
            }
            else
            {
                if (invokeCallback)
                {
                    OnComplete?.Invoke(false);
                }
                return false;
            }

            for (; step > -1; step--)
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
                coord = tempObj.coordinate;
                tempList.Clear();
            }
            if (invokeCallback)
            {
                OnComplete?.Invoke(true);
            }
            return true;
        }

        private void InitialSetup()
        {
            Vector2Int test;
            for (int x = 0; x < m_visitedNodes.GetLength(0); x++)
            {
                for (int y = 0; y < m_visitedNodes.GetLength(1); y++)
                {
                    test = m_visitedNodes[x, y].coordinate;
                    if (m_nodeIsValid(test.x, test.y))
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
                    if (coord.y + 1 < m_visitedNodes.GetLength(1) && m_nodeIsValid(m_visitedNodes[coord.x, coord.y + 1].coordinate.x, m_visitedNodes[coord.x, coord.y + 1].coordinate.y) && m_visitedNodes[coord.x, coord.y + 1].visited == step)
                    {
                        //if (m_stepIsValid != null && !m_stepIsValid(m_visitedNodes[coord.x, coord.y + 1].coordinate.x, m_visitedNodes[coord.x, coord.y + 1].coordinate.y, m_visitedNodes[coord.x, coord.y].coordinate.x, m_visitedNodes[coord.x, coord.y].coordinate.y))
                        //{
                        //    return false;
                        //}
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Right:
                    if (coord.x + 1 < m_visitedNodes.GetLength(0) && m_nodeIsValid(m_visitedNodes[coord.x + 1, coord.y].coordinate.x, m_visitedNodes[coord.x + 1, coord.y].coordinate.y) && m_visitedNodes[coord.x + 1, coord.y].visited == step)
                    {
                        //if (m_stepIsValid != null && !m_stepIsValid(m_visitedNodes[coord.x + 1, coord.y].coordinate.x, m_visitedNodes[coord.x + 1, coord.y].coordinate.y, m_visitedNodes[coord.x, coord.y].coordinate.x, m_visitedNodes[coord.x, coord.y].coordinate.y))
                        //{
                        //    return false;
                        //}
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Down:
                    if (coord.y - 1 >= 0 && m_nodeIsValid(m_visitedNodes[coord.x, coord.y - 1].coordinate.x, m_visitedNodes[coord.x, coord.y - 1].coordinate.y) && m_visitedNodes[coord.x, coord.y - 1].visited == step)
                    {
                        //if (m_stepIsValid != null && !m_stepIsValid(m_visitedNodes[coord.x, coord.y - 1].coordinate.x, m_visitedNodes[coord.x, coord.y - 1].coordinate.y, m_visitedNodes[coord.x, coord.y].coordinate.x, m_visitedNodes[coord.x, coord.y].coordinate.y))
                        //{
                        //    return false;
                        //}
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Left:
                    if (coord.x - 1 >= 0 && m_nodeIsValid(m_visitedNodes[coord.x - 1, coord.y].coordinate.x, m_visitedNodes[coord.x - 1, coord.y].coordinate.y) && m_visitedNodes[coord.x - 1, coord.y].visited == step)
                    {
                        //if (m_stepIsValid != null && !m_stepIsValid(m_visitedNodes[coord.x - 1, coord.y].coordinate.x, m_visitedNodes[coord.x - 1, coord.y].coordinate.y, m_visitedNodes[coord.x, coord.y].coordinate.x, m_visitedNodes[coord.x, coord.y].coordinate.y))
                        //{
                        //    return false;
                        //}
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
                    Vector2Int test = m_visitedNodes[coord.x, coord.y].coordinate;
                    if (m_nodeIsValid(test.x, test.y))
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
                Vector2Int test = list[i].coordinate;
                if (m_nodeIsValid(test.x, test.y))
                {
                    float distance = Vector2.Distance(targetLocation.coordinate, list[i].coordinate);
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