using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yontalane.GridNav
{
    /// <summary>
    /// Provides pathfinding functionality on a 2D grid, allowing navigation from a start to an end coordinate
    /// using customizable node and step validation logic.
    /// </summary>
    public class GridNavigator
    {
        #region Structs & Enums
        /// <summary>
        /// Represents a node that has been visited during pathfinding, storing its coordinate and visit count.
        /// </summary>
        private struct VisitedNode
        {
            public Vector2Int coordinate;
            public int visited;
        }

        /// <summary>
        /// Enumerates the possible movement directions on the grid.
        /// </summary>
        private enum Direction
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }
        #endregion

        #region Delegates
        /// <summary>
        /// Delegate for handling the completion of the pathfinding process.
        /// </summary>
        /// <param name="pathExists">Indicates whether a valid path was found.</param>
        public delegate void PathingCompleteHandler(bool pathExists);

        /// <summary>
        /// Event invoked when the pathfinding process is complete.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GridNavigator"/> class with the specified grid size and validation functions.
        /// </summary>
        /// <param name="gridSize">The size of the grid as a <see cref="Vector2Int"/>.</param>
        /// <param name="nodeIsValid">A function that determines if a node at (x, y) is valid.</param>
        /// <param name="stepIsValid">An optional function that determines if a step from (x1, y1) to (x2, y2) is valid.</param>
        public GridNavigator(Vector2Int gridSize, Func<int, int, bool> nodeIsValid, Func<int, int, int, int, bool> stepIsValid = null)
        {
            m_gridSize = gridSize;
            m_nodeIsValid = nodeIsValid;
            m_stepIsValid = stepIsValid;
            m_path = new List<VisitedNode>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridNavigator"/> class with the specified grid width, height, and validation functions.
        /// </summary>
        /// <param name="gridWidth">The width of the grid.</param>
        /// <param name="gridHeight">The height of the grid.</param>
        /// <param name="nodeIsValid">A function that determines if a node at (x, y) is valid.</param>
        /// <param name="stepIsValid">An optional function that determines if a step from (x1, y1) to (x2, y2) is valid.</param>
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


        /// <summary>
        /// Initializes the grid of visited nodes, setting each node's coordinate and marking it as unvisited.
        /// </summary>
        private void GenerateGrid()
        {
            m_visitedNodes = new VisitedNode[m_gridSize.x, m_gridSize.y];

            // Populate the grid with VisitedNode instances, each with its coordinate and visited set to -1 (unvisited)
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

        /// <summary>
        /// Performs a breadth-first search from the start node, setting the distance (step count) to each reachable node.
        /// </summary>
        private void SetDistance()
        {
            InitialSetup();
            Vector2Int coord = m_startCoord;
            // The testArray is only used to determine the maximum number of steps possible in the grid
            int[] testArray = new int[m_visitedNodes.GetLength(0) * m_visitedNodes.GetLength(1)];
            for (int step = 1; step < testArray.Length; step++)
            {
                foreach (VisitedNode visitedNode in m_visitedNodes)
                {
                    // For each node visited in the previous step, try to expand to its neighbors
                    if (m_nodeIsValid(visitedNode.coordinate.x, visitedNode.coordinate.y) && visitedNode.visited == step - 1)
                    {
                        TestFourDirections(visitedNode.coordinate, step);
                    }
                }
            }
        }

        /// <summary>
        /// Reconstructs the path from the end node to the start node, if possible.
        /// Optionally invokes the OnComplete callback.
        /// </summary>
        /// <param name="invokeCallback">Whether to invoke the OnComplete callback.</param>
        /// <returns>True if a path was found, false otherwise.</returns>
        private bool SetPath(bool invokeCallback)
        {
            int step = 0;
            Vector2Int coord = m_endCoord;
            List<VisitedNode> tempList = new();
            m_path.Clear();
            Vector2Int test = m_visitedNodes[m_endCoord.x, m_endCoord.y].coordinate;

            // Check if the end coordinate is valid and reachable
            if (coord.x >= 0 && coord.x < m_visitedNodes.GetLength(0) &&
                coord.y >= 0 && coord.y < m_visitedNodes.GetLength(1) &&
                m_nodeIsValid(test.x, test.y) &&
                m_visitedNodes[m_endCoord.x, m_endCoord.y].visited > 0)
            {
                // Start the path with the end node
                m_path.Insert(0, m_visitedNodes[coord.x, coord.y]);
                step = m_visitedNodes[coord.x, coord.y].visited - 1;
            }
            else
            {
                // No path found
                if (invokeCallback)
                {
                    OnComplete?.Invoke(false);
                }
                return false;
            }

            // Backtrack from the end node to the start node
            for (; step > -1; step--)
            {
                // Check all four directions for the next node in the path
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

                // Choose the closest node to the end as the next step in the path
                VisitedNode tempObj = FindClosest(m_visitedNodes[m_endCoord.x, m_endCoord.y], tempList);
                m_path.Insert(0, tempObj);
                coord = tempObj.coordinate;
                tempList.Clear();
            }

            // Path found, invoke callback if requested
            if (invokeCallback)
            {
                OnComplete?.Invoke(true);
            }
            return true;
        }

        /// <summary>
        /// Resets the visited state of all valid nodes and marks the start node as visited (step 0).
        /// </summary>
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
            // Mark the start node as visited at step 0
            m_visitedNodes[m_startCoord.x, m_startCoord.y].visited = 0;
        }

        /// <summary>
        /// Checks if a neighbor in the given direction from the specified coordinate is valid and was visited at the given step.
        /// </summary>
        /// <param name="coord">The current coordinate.</param>
        /// <param name="step">The step number to check for.</param>
        /// <param name="direction">The direction to check.</param>
        /// <returns>True if the neighbor is valid and visited at the given step; otherwise, false.</returns>
        private bool TestDirection(Vector2Int coord, int step, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    // Check if the node above is within bounds, valid, and visited at the given step
                    if (coord.y + 1 < m_visitedNodes.GetLength(1) &&
                        m_nodeIsValid(m_visitedNodes[coord.x, coord.y + 1].coordinate.x, m_visitedNodes[coord.x, coord.y + 1].coordinate.y) &&
                        m_visitedNodes[coord.x, coord.y + 1].visited == step)
                    {
                        // Uncomment below to use step validation
                        //if (m_stepIsValid != null && !m_stepIsValid(...)) return false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Right:
                    // Check if the node to the right is within bounds, valid, and visited at the given step
                    if (coord.x + 1 < m_visitedNodes.GetLength(0) &&
                        m_nodeIsValid(m_visitedNodes[coord.x + 1, coord.y].coordinate.x, m_visitedNodes[coord.x + 1, coord.y].coordinate.y) &&
                        m_visitedNodes[coord.x + 1, coord.y].visited == step)
                    {
                        // Uncomment below to use step validation
                        //if (m_stepIsValid != null && !m_stepIsValid(...)) return false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Down:
                    // Check if the node below is within bounds, valid, and visited at the given step
                    if (coord.y - 1 >= 0 &&
                        m_nodeIsValid(m_visitedNodes[coord.x, coord.y - 1].coordinate.x, m_visitedNodes[coord.x, coord.y - 1].coordinate.y) &&
                        m_visitedNodes[coord.x, coord.y - 1].visited == step)
                    {
                        // Uncomment below to use step validation
                        //if (m_stepIsValid != null && !m_stepIsValid(...)) return false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Left:
                    // Check if the node to the left is within bounds, valid, and visited at the given step
                    if (coord.x - 1 >= 0 &&
                        m_nodeIsValid(m_visitedNodes[coord.x - 1, coord.y].coordinate.x, m_visitedNodes[coord.x - 1, coord.y].coordinate.y) &&
                        m_visitedNodes[coord.x - 1, coord.y].visited == step)
                    {
                        // Uncomment below to use step validation
                        //if (m_stepIsValid != null && !m_stepIsValid(...)) return false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
            return false;
        }

        /// <summary>
        /// Attempts to visit all four neighboring nodes (up, right, down, left) from the given coordinate at the specified step.
        /// </summary>
        /// <param name="coord">The current coordinate.</param>
        /// <param name="step">The step number to assign to visited neighbors.</param>
        private void TestFourDirections(Vector2Int coord, int step)
        {
            // Try to visit the node above
            if (TestDirection(coord, -1, Direction.Up))
            {
                SetVisited(coord + Vector2Int.up, step);
            }
            // Try to visit the node to the right
            if (TestDirection(coord, -1, Direction.Right))
            {
                SetVisited(coord + Vector2Int.right, step);
            }
            // Try to visit the node below
            if (TestDirection(coord, -1, Direction.Down))
            {
                SetVisited(coord + Vector2Int.down, step);
            }
            // Try to visit the node to the left
            if (TestDirection(coord, -1, Direction.Left))
            {
                SetVisited(coord + Vector2Int.left, step);
            }
        }

        /// <summary>
        /// Marks the specified coordinate as visited at the given step, if it is within bounds and valid.
        /// </summary>
        /// <param name="coord">The coordinate to mark as visited.</param>
        /// <param name="step">The step number to assign.</param>
        private void SetVisited(Vector2Int coord, int step)
        {
            // Check if the coordinate is within the grid bounds
            if (coord.x >= 0 && coord.x < m_visitedNodes.GetLength(0))
            {
                if (coord.y >= 0 && coord.y < m_visitedNodes.GetLength(1))
                {
                    Vector2Int test = m_visitedNodes[coord.x, coord.y].coordinate;
                    // Only mark as visited if the node is valid
                    if (m_nodeIsValid(test.x, test.y))
                    {
                        m_visitedNodes[coord.x, coord.y].visited = step;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the node in the given list that is closest to the target location.
        /// </summary>
        /// <param name="targetLocation">The target node to compare distances to.</param>
        /// <param name="list">The list of candidate nodes.</param>
        /// <returns>The node in the list that is closest to the target location.</returns>
        private VisitedNode FindClosest(VisitedNode targetLocation, List<VisitedNode> list)
        {
            float closestDistance = Mathf.Infinity;
            int closestIndex = 0;
            for (int i = 0; i < list.Count; i++)
            {
                Vector2Int test = list[i].coordinate;
                // Only consider valid nodes
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