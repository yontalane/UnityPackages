# Layout Tilemap

Enables creation of prefab-based level layouts using 2D `UnityEngine.Tilemap` objects as blueprints. Paint a level with Unity's 2D Tilemap tools, save it as a prefab in a Resources folder, then use `MapBuilder` to reconstruct it as a 3D scene at runtime: every tile and entity is replaced by an instantiated prefab that shares its name.

To use this package:

* Build a level using `UnityEngine.Tilemap`—optionally adding `MapEntity` components for characters/items and `MapProperties` components for custom per-tile or per-entity data—and save it as a prefab under a Resources folder.
* For every distinct tile name and entity name used in your Tilemap, create a same-named prefab (also under a Resources folder). `MapBuilder` will load and instantiate it in place of the 2D tile or entity.
* Add a `MapBuilder` component to a GameObject in your scene, and call `LoadMap`.

```c#
private void Start()
{
    MapBuilder builder = GetComponent<MapBuilder>();
    builder.LoadMap(OnMapLoaded);
}

private void OnMapLoaded(MapData data)
{
    foreach (EntityData entity in data.entities)
    {
        if (entity.properties.TryGet("Tilt", out MapPropertyValue tilt) && tilt.boolValue)
        {
            entity.gameObject.transform.Rotate(5f, 0f, 0f);
        }
    }
}
```

## Map Builder

MapBuilder is a MonoBehaviour that loads and builds maps from Unity Tilemaps: it scrapes through all tiles and entities within the map, replaces them with instantiated prefabs, and then deletes the original Tilemap.

### Methods

| Name | Description |
| --- | --- |
| **LoadMap** | Loads and builds a map—either the one configured on this component (its `mapToLoad` field), a specific named map from Resources, or an already-loaded Grid—invoking the given callback with the resulting MapData once finished. |

## Map Data

Holds all relevant data for a built map. Passed to MapBuilder's `LoadMap` callback.

### Properties

| Name | Description |
| --- | --- |
| **name** | The name of the map. |
| **tileDataCollection** | The collection of tile data for all tilemaps in the map. |
| **entities** | The list of entities placed within the map. |
| **properties** | Custom properties associated with the map, from a MapProperties component on the source Grid. |
| **bounds** | The bounding box that encapsulates the map area. |
| **mapParent** | The parent transform under which the map was built. |

## Tile Data

Represents data for a single tile in the map.

### Properties

| Name | Description |
| --- | --- |
| **exists** | Whether a tile exists at this location. |
| **name** | The name of the tile, matching its source Tile asset's name. |
| **worldPosition** | The world position of the tile. |

## Entity Data

Contains information about an entity placed on the map.

### Properties

| Name | Description |
| --- | --- |
| **name** | The name of the entity. |
| **position** | The world position of the entity. |
| **localPosition** | The local position of the entity within the source Tilemap. |
| **eulerAngles** | The rotation of the entity. |
| **hasBounds** | Whether this entity has valid bounds information, taken from a Collider on its MapEntity. |
| **bounds** | The bounding box of the entity, if available. |
| **entity** | The source MapEntity component. |
| **properties** | Custom properties associated with the entity, from its MapProperties component. |
| **gameObject** | The instantiated GameObject representing the entity, if entity resources are being loaded. |

## Tile Data Collection

Manages a hierarchical collection of Tile Data, organized by tilemap, row, and column. Used internally by MapBuilder and exposed on Map Data for querying the finished map's layout.

### Methods

| Name | Description |
| --- | --- |
| **AddTilemap** | Adds a new, empty tilemap to the collection. |
| **AddRow** | Adds a new row to the most recently added tilemap. |
| **AddTileData** | Adds a tile to the most recently added row. |
| **GetTilemapCount** | Gets the number of tilemaps in the collection. |
| **GetRowCount** | Gets the number of rows in a given tilemap. |
| **GetColumnCount** | Gets the number of columns in a given row. |
| **TryGetTileData** | Tries to get the Tile Data at the given tilemap/row/column coordinate. |
| **GetTileData** | Gets the Tile Data at the given tilemap/row/column coordinate, or default if out of range. |
| **GetNamesArray** | Returns a 2D array of tile names for the given tilemap. |
| **GetExistArray** | Returns a 2D array of whether a tile exists, for the given tilemap. |

## Map Entity

MapEntity marks a GameObject in a source Tilemap as an entity to be replaced by a matching prefab on load. In the Editor, it also draws a gizmo representing its position and facing.

### Properties

| Name | Description |
| --- | --- |
| **MapProperties** | The MapProperties component attached to this MapEntity. |

## Map Properties

Stores a collection of custom key/value property pairs for a map element—a Grid, or a Tilemap entity—allowing flexible assignment and retrieval of custom data at runtime or in the editor.

### Properties

| Name | Description |
| --- | --- |
| **Properties** | The collection of key/value property pairs for this map element. |

## Map Property Dictionary

A serializable dictionary mapping string keys to Map Property Value, used by Map Properties. In addition to standard dictionary operations, it exposes `TryGet(key, out value)` for looking up a property by name.

## Map Property Value

Represents a single property's value, tagged with a type so it can hold a string, float, int, bool, or Unity Object reference.

### Properties

| Name | Description |
| --- | --- |
| **type** | The type of value stored in this property (String, Float, Int, Bool, or Object). |
| **stringValue** | The string value, used if type is String. |
| **floatValue** | The float value, used if type is Float. |
| **intValue** | The integer value, used if type is Int. |
| **boolValue** | The boolean value, used if type is Bool. |
| **objectReference** | The Unity Object reference value, used if type is Object. |

## Persistent Object

A tag component. Any GameObject within the source Tilemap that has a PersistentObject component is duplicated as-is into the final map, rather than being replaced by a name-matched prefab.

## Map Utilities

Extension methods for working with maps and tilemaps.

### Methods

| Name | Description |
| --- | --- |
| **MapLocalToGridLocal** | Converts a local position within a Tilemap to a local position within the grid, accounting for map bounds and cell swizzle order. |

## Settings

Yontalane's Layout Tilemap settings are available under **Project Settings > Yontalane > Layout Tilemap**. They control the default appearance of Map Entity gizmos in the Scene view, plus optional per-name overrides.

### Properties

| Name | Description |
| --- | --- |
| **defaultData** | The default visual and pointer configuration data for map entity gizmos. |
| **specialCaseData** | An array of special-case map entity data, each associated with a specific name. |

### Methods

| Name | Description |
| --- | --- |
| **Save** | Saves the current settings asset to disk. |
| **GetData** | Retrieves the Map Entity Data associated with the given name, falling back to defaultData if no special case matches. |

## Map Entity Data

Contains visual and pointer configuration data for a map entity's Scene view gizmo.

### Properties

| Name | Description |
| --- | --- |
| **scale** | The overall scale of the gizmo. |
| **outerColor** | The color used for the outer part of the gizmo. |
| **innerColor** | The color used for the inner part of the gizmo. |
| **pointerLength** | The length of the pointer (direction indicator). |
| **pointerScale** | The scale of the pointer (direction indicator). |
| **thickness** | The thickness of the gizmo's outline. |

## Named Map Entity Data

Associates a name with a specific set of Map Entity Data, for special-case entity gizmo appearance.

### Properties

| Name | Description |
| --- | --- |
| **name** | The name associated with this set of data. |
| **data** | The Map Entity Data for this named entry. |
