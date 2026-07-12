# Grid Nav

Grid Nav is a lightweight pathfinding library for games built on a 2D grid, such as tactics games, roguelikes, or any tile-based world. Its main class, `GridNavigator`, finds a route between two grid coordinates without needing to know anything about how your grid is represented internally — you provide simple callback functions that answer "is this cell pathable?" and `GridNavigator` handles the rest.

`GridNavigator` finds the shortest path using a flood-fill search that expands outward from the start coordinate one step at a time, rather than a heuristic-guided search like A*. This keeps the implementation simple and guarantees the shortest path, but search cost scales with the size of the grid rather than the distance to the goal, so it's best suited to small or moderately sized grids.

To use this library, create a function to determine if a node is pathable.

```c#
private bool IsPathable(int x, int y)
{
  return grid[x, y] != null;
}
```

Create a GridNavigator. Pass it a grid size and the callback function.

```c#
GridNavigator navigator = new GridNavigator(grid.GetLength(0), grid.GetLength(1), IsPathable);
```

Listen for the navigator's completion event.

```c#
navigator.OnComplete += Navigator_OnComplete;
```

Call FindPath() on the navigator.

```c#
navigator.FindPath(startX, startY, endX, endY);
```

When the navigator finishes pathing, it will invoke its completion event.

```c#
private void Navigator_OnComplete(bool pathExists)
{
  if (pathExists)
  {
    Debug.Log($"Path exists.");
    for (int i = 0; i < navigator.PathCount; i++)
    {
      Debug.Log($"Go to {navigator.GetPathNode(i)}.");
    }
  }
  else
  {
    Debug.Log($"Path does not exist.");
  }
}
```

If you don't want to path asynchronously, you can instead do:

```c#
bool pathExists = navigator.FindPathSynchronous(startX, startY, endX, endY);

if (pathExists)
{
  Debug.Log($"Path exists.");
  for (int i = 0; i < navigator.PathCount; i++)
  {
    Debug.Log($"Go to {navigator.GetPathNode(i)}.");
  }
}
else
{
  Debug.Log($"Path does not exist.");
}
```

## Grid Navigator

### Constructors

| Name | Description |
| --- | ---|
| **Grid Navigator** | Creates a new GridNavigator for a grid of the given size, using the provided node (and, optionally, step) validation callbacks. |

### Properties

| Name | Description |
| --- | ---|
| **Path Count** | The number of nodes along the path to the goal. |

### Public Methods

| Name | Description |
| --- | ---|
| **Find Path** | Asynchronously finds a path to the goal, invoking the completion delegate when finished. |
| **Find Path Synchronous** | Finds a path to the goal immediately and returns whether a path was found. |
| **Get Path Node** | Gets the coordinate at the given index along the path to the goal. |

### Delegates

| Name | Description |
| --- | ---|
| **Pathing Complete Handler** | Invoked when pathfinding completes. Passes whether a valid path was found. |

### Events

| Name | Description |
| --- | ---|
| **On Complete** | Event invoked when the pathfinding process is complete. |
