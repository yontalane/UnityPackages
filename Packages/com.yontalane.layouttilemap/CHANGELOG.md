# Changelog

## [1.0.26] - 2025.08.27

### Added

- Option to not load resources from tile names.

## [1.0.25] - 2025.08.26

### Added

- EntityData has localPosition.

## [1.0.24] - 2025.08.26

### Fixed

- MapBuilder tried to destroy prefab asset and not instance.

## [1.0.23] - 2025.08.26

### Added

- Tooltips and summaries.

## [1.0.22] - 2025.08.26

### Fixed

- Using DestroyImmediate instead of Destroy.

## [1.0.21] - 2025.08.03

### Fixed

- Editor code was including in runtime.

## [1.0.20] - 2025.07.25

### Changed

- Supporting Unity 6+.

## [1.0.19] - 2023.01.09

### Added

- `MapBuilder.LoadMap()` can now accept a Grid instance as a parameter.

### Changed

### Fixed

## [1.0.18] - 2023.01.04

### Added

### Changed

- Removed entity tags. Styling based on name.

### Fixed

## [1.0.17] - 2023.01.03

### Added

### Changed

### Fixed

- Map entity project settings are now handled properly.

## [1.0.16] - 2023.01.03

### Added

### Changed

### Fixed

- Map entity project settings are now stored in ProjectSettings.

## [1.0.15] - 2023.01.03

### Added

### Changed

- Map entity settings is now a part of Project Settings.

### Fixed

## [1.0.14] - 2022.12.31

### Added

- Sample scene.

### Changed

- Visually dynamic map entities.

### Fixed

## [1.0.13] - 2022.12.13

### Added

- Map data contains tile data.

### Changed

### Fixed

- Map entity icon was sometimes not centered.

## [1.0.12] - 2022.07.25

### Added

### Changed

### Fixed

- Fixed - Editor scripts in build.

## [1.0.11] - 2022.07.02

### Added

- Passing map parent Transform in MapData.

### Changed

### Fixed

## [1.0.10] - 2022.06.27

### Added

### Changed

### Fixed

- Persistent object retains scale.

## [1.0.9] - 2022.06.27

### Added

- Persistent Object to be copied wholesale from the source map into the final.

### Changed

### Fixed

## [1.0.8] - 2022.04.09

### Added

### Changed

### Fixed

* Added null check when loading map entity resources.

## [1.0.6] - 2022.02.13

### Added

* Added map name to MapData.

### Changed

### Fixed

## [1.0.5] - 2022.02.13

### Added

* Entity draws collider bounds as gizmo.

### Changed

* Entity data contains map properties as dictionary rather than component.

### Fixed

* Entity bounds not exporting properly.

## [1.0.4] - 2022.02.13

### Added

* Exporting additional entity data.

### Changed

### Fixed

## [1.0.3] - 2022.02.12

### Added

* Exporting additional entity data.

### Changed

### Fixed

## [1.0.2] - 2022.02.12

### Added

* Map bounds to build callback event data.

### Changed

### Fixed

## [1.0.1] - 2022.02.09

### Added

* Gizmo icon.

### Changed

### Fixed

## [1.0.0] - 2022.02.09

### Added

* Core functionality (building prefab-based level layout using 2D UnityEngine.Tilemap as blueprint).

### Changed

### Fixed
