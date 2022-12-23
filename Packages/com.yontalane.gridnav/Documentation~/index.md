# Grid Nav

### To use this library, create a GridNavigator.

```c#
GridNavigator navigator = new GridNavigator();
```



### Listen for the navigator's completion event.

```c#
navigator.OnFoundPath += Navigator_OnFoundPath;
```



### Call FindPath() on the navigator.

```c#
navigator.FindPath(startCoord, endCoord, grid);
```



`startCoord` and `endCoord` are Vector2Int objects. `grid` is a two-dimensional array of IGridNode. (IGridNode is any Component that implements `Vector2Int Coordinate { get; set; }`.)



### When the navigator finishes pathing, it will invoke its completion event.

```c#
private void Navigator_OnFoundPath(bool pathExists)
{
  if (pathExists)
  {
    Debug.Log($"Path exists with a count of {navigator.PathCount}.");
  }
  else
  {
    Debug.Log($"Path does not exist.");
  }
}
```

