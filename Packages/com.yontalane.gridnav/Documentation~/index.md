# Grid Nav

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



If you don't want path asynchronously, you can instead do:

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