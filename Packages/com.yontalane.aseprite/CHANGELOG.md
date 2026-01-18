# Changelog

## [1.0.37] - 2026.01.18

### Added

- Option to not clear output list when getting activation periods

## [1.0.36] - 2026.01.18

### Fixed

- Bug calculating single-frame duration

## [1.0.35] - 2026.01.18

### Added

- SpriteObjectAnimationInfo has animation length in seconds

## [1.0.34] - 2026.01.04

### Changed

- Reverted previous change

## [1.0.33] - 2026.01.04

### Changed

- When trying to play an animation that's already playing, Bridge will instead only change the playback time

## [1.0.32] - 2025.12.21

### Added

- Bridge can play multiple animators in sync

## [1.0.31] - 2025.12.15

### Changed

- Bridge stores extras data as a list, not an array

## [1.0.30] - 2025.12.15

### Changed

- Breaking change: MotionTree uses a new data type that allows clip lookup by name

## [1.0.29] - 2025.12.04

### Added

- Signed package

## [1.0.28] - 2025.11.01

### Fixed

- Removed editor package build requirement

## [1.0.27] - 2025.09.30

### Added

- Bridge stores list of frames where attached objects, such as colliders, are activated.

## [1.0.26] - 2025.09.11

### Fixed

- Removed duplicate Bridge.TryPlay() method.

## [1.0.25] - 2025.09.07

### Fixed

- Properly time MotionTree animation.

## [1.0.24] - 2025.09.07

### Changed

- Refactored MotionTree code.

## [1.0.23] - 2025.09.06

### Fixed

- Undid previous change because it caused a crash.
- Not setting time when playing MotionTree clip in Bridge LateUpdate().

## [1.0.22] - 2025.09.06

### Added

- Extras.TryGetAnimation() and Bridge.TryGetAnimationClip() now export startTime for playing clips from motion trees.

## [1.0.21] - 2025.09.06

### Changed

- KeyFloatPair is a class instead of a struct so that it is passed by ref.

## [1.0.20] - 2025.09.06

### Added

- Bridge invokes MotionTreeValue events.

## [1.0.19] - 2025.09.06

### Changed

- Clamping MotionTree animation index appropriately.

## [1.0.18] - 2025.09.06

### Added

- Can filter debug logs.

## [1.0.17] - 2025.09.06

### Added

- More debug logs.

## [1.0.16] - 2025.09.06

### Added

- Debug logs in Bridge.

## [1.0.15] - 2025.09.05

### Added

- AsepriteAnimationExtras and MotionTrees.

## [1.0.14] - 2025.09.04

### Changed

- Reimplemented generating colliders from Aseprite layers.

### Added

- AsepriteAnimationBridge stores references to colliders and point objects.
- Gizmo displays of colliders and point objects.

## [1.0.13] - 2025.08.20

### Changed

- Changed root motion event data. Passing local animation frame.

## [1.0.12] - 2025.08.20

### Added

- Added more data to the root motion event.

## [1.0.11] - 2025.08.20

### Changed

- Switched root motion event to using an event data struct rather than directly passing a vector.

## [1.0.10] - 2025.08.18

### Added

- Added Sprite-related fields to the Bridge.

## [1.0.9] - 2025.08.15

### Fixed

- Aseprite import takes into account animation curves that enable and disable the SpriteRenderer.

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