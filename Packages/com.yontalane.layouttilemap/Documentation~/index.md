# Layout Tilemap

Enables creation of prefab-based level layouts using 2D UnityEngine.Tilemap objects as blueprints.

Create a tilemap using UnityEngine.Tilemap and save it as a prefab in a Resources folder. Call `MapBuilder.LoadMap([onFinishedLoading function], [prefab name])`. The map will be reconstructed in 3D; MapBuilder will load prefab resources that have the same name as each tile.