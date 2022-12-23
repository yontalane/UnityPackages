# Grid Nav

### To use this library, implement IGridNode.

```c#
using UnityEngine;

public class GridNode : MonoBehaviour, IGridNode
{
  public Vector2Int coordinate;
  public Vector2Int GetCoordinate() => coordinate;
}
```



### Create a GridNavigator for your node type.

```c#
GridNavigator navigator = new GridNavigator<GridNode>();
```



### Listen for the navigator's completion event.

```c#
navigator.OnFoundPath += Navigator_OnFoundPath;
```



### Call FindPath() on the navigator.

```c#
navigator.FindPath(startCoord, endCoord, grid);
```



`startCoord` and `endCoord` are Vector2Int objects. `grid` is a two-dimensional array of grid nodes.



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

