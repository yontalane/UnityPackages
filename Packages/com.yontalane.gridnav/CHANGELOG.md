# Changelog

## [1.0.8] - 2025.12.04

### Added

- Signed package

## [1.0.7] - 2025.07.25

### Change

- Supporting Unity 6+.

## [1.0.6] - 2024.02.09

### Changed

* Removed wall support for now.
* Improved demo scene.

## [1.0.5] - 2024.02.09

### Added

* Now supports grid tiles with walls.

## [1.0.4] - 2022.12.24

### Added

* Option for synchronous (not just asynchronous) path finding.

### Changed

* Callback takes integer parameters instead of Vector2Int.

## [1.0.3] - 2022.12.23

### Added

* Override constructor and FindPath() to not use Vector2Int.

## [1.0.2] - 2022.12.23

### Changed

* Undid generic class definition.
* Got rid of IGridNode.
* GridNav navigates using a callback to determine if a coordinate is pathable.

## [1.0.1] - 2022.12.23

### Changed

* IGridNode no longer requires a transform.
* GridNavigator is defined using an IGridNode implementation (```GridNavigator<T> where T : IGridNode```).

## [1.0.0] - 2022.12.23

### Added

* Grid Nav
