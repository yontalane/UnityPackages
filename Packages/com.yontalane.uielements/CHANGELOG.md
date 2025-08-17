# Changelog

## [1.0.34] - 2025.08.16

### Fixed

- Added UI Toolkit panel settings style theme to sample.

## [1.0.33] - 2025.08.09

### Changed

- Query uses selectable button and supports on-selection-changed callbacks.

## [1.0.32] - 2025.08.09

### Fixed

- An icon button can only have a gap if there is both an icon and text.
- Selecting first response in query.

## [1.0.31] - 2025.08.09

### Fixed

- Uxml attributes written with Unity 6 syntax.

### Added

- Selectable button.
- Icon button.

### Changed

- Toggle button inherits from icon button, inherits from selectable button.

## [1.0.30] - 2025.07.25

### Changed

- Supporting Unity 6+.

## [1.0.29] - 2023.08.04

### Changed

- MenuManager OnEnabled and OnDisabled are now protected virtual.

## [1.0.28] - 2023.07.28

### Fixed

- Set the Editor AssemblyDef to target the editor only.

## [1.0.27] - 2023.07.28

### Fixed

- Package was missing required dependencies.

## [1.0.26] - 2023.07.19

### Added

- MenuManager can accept arbitrary button input.

## [1.0.25] - 2023.07.19

### Fixed

- MenuManager private methods are private.

## [1.0.24] - 2023.07.18

### Added

- Menus can override left & right navigation.
- Separate menu method for overriding cancel.

## [1.0.23] - 2023.07.17

### Changed

- ToggleButton style uses class name.

## [1.0.22] - 2023.07.16

### Changed

- ToggleButton OnChanged delegate is no longer static.

## [1.0.21] - 2023.07.16

### Changed

- Cleaned up new version of ToggleButton.

### Fixed

- ScrollViewAuto wraps properly.

## [1.0.20] - 2023.07.16

### Changed

- ToggleButton inherits from Button instead of Toggle.

## [1.0.19] - 2023.07.14

### Fixed

- ToggleButton could not gain focus.

## [1.0.18] - 2023.07.14

### Fixed

- If a ToggleButton's icon is empty, its display is now none.

## [1.0.17] - 2023.07.14

### Fixed

- When a menu's AddableItem is a Toggle, set its label value with text data rather than its text value.

## [1.0.16] - 2023.07.14

### Added

- Icon on ToggleButton.

### Changed

- Simplified ToggleButton layout and style.

## [1.0.15] - 2023.07.11

### Fixed

- ToggleButton USS no longer directly references ToggleButton.

## [1.0.14] - 2023.07.09

### Fixed

- Leave ToggleButton height auto.

## [1.0.13] - 2023.07.09

### Fixed

- ToggleButton is navigable.

## [1.0.12] - 2023.07.09

### Fixed

- Removed minimum width from ToggleButton label.

## [1.0.11] - 2023.07.09

### Added

- Using ToggleButton in menu.

## [1.0.10] - 2023.07.09

### Added

- ToggleButton is a button that has a toggle state.

## [1.0.9] - 2023.07.07

### Changed

- ScrollViewAuto keeps elements in the middle.

## [1.0.8] - 2023.07.06

### Added

- Callback when addin element.

## [1.0.7] - 2023.07.06

### Added

- Exposed MenuManager.InputAsset and OnDisplayMenu().

## [1.0.6] - 2023.07.05

### Fixed

- Tabs can select subordinate Menu Manager.

## [1.0.5] - 2023.07.05

### Fixed

- ScrollViewAuto now works with children that were added prior to runtime.

## [1.0.4] - 2023.07.05

### Added

- ScrollViewAuto is a ScrollView that automatically scrolls to the focused element.

## [1.0.3] - 2023.07.05

### Added

- When you return to a menu, the focused element is still what it was when you left.

## [1.0.2] - 2023.07.03

### Fixed

- Null check on control inputs

## [1.0.1] - 2023.07.02

### Changed

- Changed required Unity version

## [1.0.0] - 2023.06.30

### Added

- Menu Manager
- Notification
- Query
