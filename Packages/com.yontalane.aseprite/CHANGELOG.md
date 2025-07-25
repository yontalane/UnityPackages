# Changelog

## [1.0.8] - 2025.07.25

### Change

- Supporting Unity 6+.

## [1.0.7] - 2025.07.23

### Change

- AsepriteAnimationBridge.Play() now defaults to not restarting a looping animation, but this can be overridden

## [1.0.6] - 2025.07.23

### Fixed

- Animation start event was at end of animation instead of start

## [1.0.5] - 2025.07.23

### Added

- AsepriteAnimationBridge can play animations starting at a normalized offset

### Changed

- AsepriteAnimationBridge lifecycle events use a single struct parameter

### Changed

- Updated documentation

## [1.0.4] - 2025.07.22

### Changed

- Updated documentation

## [1.0.3] - 2025.07.22

### Added

- AnimationBridge can be used to play animations
- AnimationBridge can be used to detect current animation

### Changed

- MotionReceiver is now AnimationBridge

## [1.0.2] - 2025.07.22

### Added

- Hat in sample project

### Fixed

- Collision boxes and points were deactivating in certain frames

## [1.0.1] - 2025.07.22

### Added

- Sample project

### Fixed

- Collision boxes on first frame not displaying
- Root motion wrong on first or last frame

## [1.0.0] - 2025.07.22

### Added

- Custom Aseprite Importer
  - Import collision boxes
  - Import null points
  - Import root motion