# Changelog

## [1.0.70] - 2026.07.12

### Fixed

- Found the actual root cause the 1.0.68/1.0.69 fallbacks never reached: `root.Query<VisualElement>()` in DelayedFocusElement's child search includes the menu's own root as its first result (the same gotcha already called out in RegisterDynamicElement's docs), and for a menu with no other interactive descendants RegisterEmptyMenuFallback has already made that root focusable by the time this search runs. So the loop matched and focused the root at its very first iteration, every time, before ever reaching a real child or either ScrollView-focused fallback -- toggling the ScrollView's focusable in UXML could never have mattered, since the search never got that far. The loop now explicitly skips the root itself, so the 1.0.69 ScrollView fallback (and RegisterScrollViewKeyScrolling) can finally run when appropriate.

## [1.0.69] - 2026.07.12

### Fixed

- Corrected the 1.0.68 fallback: focusing a ScrollView's Scroller didn't actually work, since in this project's UI templates the Scroller and everything inside it (RepeatButtons, drag thumb) are focusable="false" by default, and UI Toolkit's ScrollView doesn't scroll itself in response to focus plus directional input regardless. DelayedFocusElement's last-resort fallback now focuses the plain ScrollView itself instead (forcing it focusable at runtime rather than requiring UXML changes), only when it actually has overflow to scroll. A new RegisterScrollViewKeyScrolling moves scrollOffset directly in response to NavigationMoveEvent and handles Cancel/Back, mirrored from the previous Scroller-focused version. Still only engages when a menu has no other focusable content, and still leaves ScrollViewAuto's existing focus-stepping-between-list-items behavior untouched.

## [1.0.68] - 2026.07.12

### Added

- Menus with no focusable content of their own (e.g. a screen that's just a block of scrollable text, with no buttons or fields) now fall back to focusing a ScrollView's Scroller as a last resort in DelayedFocusElement, so directional/controller navigation can still scroll it. Previously the Scroller was unconditionally excluded from auto-focus (to keep it from stealing focus away from real content), which left such screens with nothing focusable at all -- only mouse-dragging the scrollbar worked. Also registers a Cancel handler on each Scroller so Back still works if it ends up focused this way.

## [1.0.67] - 2026.07.08

### Fixed

- DelayedFocusElement now bails out immediately if its own menu root is no longer displayed by the time it resumes (e.g. a later SetMenu call in the same frame, such as ReturnToPreviousMenu firing right after Activate's own SetMenu, hides it first). Previously the child-search loop only checked each child's own canGrabFocus, which doesn't account for an ancestor's display:none, so it could still find and focus an invisible child -- confirmed via [NavDiag] logging, which caught it focusing a button with a zeroed-out layout rect. That stray focus call could also trigger the focused element's own focus-driven side effects (e.g. Getaway's HookUpButtonPreview calling StopAllCoroutines), silently killing a second DelayedFocusElement coroutine already queued to focus the menu actually on screen.

## [1.0.66] - 2026.07.07

### Removed

- Temporary [NavDiag] diagnostic logging added in 1.0.65. The [NavDiag] trace showed m_ignoreFocus was already True at every FocusInEvent during the menu-appearance sound, proving the sound wasn't coming from this package's navigation-sound path at all -- root cause was in game code (a Toggle.value assignment during settings initialization firing an unguarded click sound). The 1.0.64 focus-guard fix is still correct and stays in place.

## [1.0.65] - 2026.07.07

### Debug

- Temporary [NavDiag] logging around Awake, SetMenu, DelayedFocusElement, and the navigation FocusInEvent handler, to find the source of a menu-appearance navigation sound that survived the 1.0.64 fix. To be removed once the root cause is confirmed.

## [1.0.64] - 2026.07.07

### Fixed

- A menu's auto-focus on first appearing no longer plays the navigation sound. DelayedFocusElement and SetFocus now set the ignore-focus guard before calling EventSystem.SetSelectedGameObject (which could itself trigger a real focus change) instead of only around the explicit Focus() call after it, and clear the guard explicitly afterward instead of relying on the first FocusInEvent to consume it, so a second real focus event in the same programmatic pass can't slip through unguarded.

## [1.0.63] - 2026.07.07

### Added

- Added method to mute audio 

## [1.0.62] - 2026.07.07

### Removed

- Temporary [NavDiag] diagnostic logging from DelayedFocusElement, added in 1.0.60 to track down the root-focus bug fixed in 1.0.61

## [1.0.61] - 2026.07.07

### Fixed

- RegisterDynamicElement now explicitly recomputes the empty-menu fallback's root.focusable state after registering a dynamically added element, instead of relying solely on AttachToPanelEvent bubbling (confirmed via [NavDiag] logging: root.focusable was staying true even after 31 real controls were added to a ScrollViewAuto, causing DelayedFocusElement to auto-focus the menu root itself instead of any real content -- Query<VisualElement>() on a menu's root includes the root as its own first result).

## [1.0.60] - 2026.07.07

### Debug

- Temporary [NavDiag] logging in DelayedFocusElement to diagnose why menus with dynamically-added ScrollViewAuto content aren't receiving auto-focus on entry. To be removed once the root cause is confirmed.

## [1.0.59] - 2026.07.07

### Fixed

- DelayedFocusElement's auto-focus-on-menu-entry search now excludes a ScrollView's internal Scroller elements (same exclusion HasInteractiveDescendant already used), so entering a menu whose content is entirely inside a ScrollViewAuto focuses the first real control instead of an invisible scrollbar element

## [1.0.58] - 2026.07.07

### Added

- Public MenuManager.RegisterDynamicElement(menuName, element), registering click/cancel/left-right navigation for a single element added to a menu after Awake-time registration already ran (e.g. UI built at runtime from game code). Factored the existing per-BindableElement cancel/left-right registration out of RegisterClick(Menu) into a shared RegisterBindableNavigation helper so both paths use identical logic.

## [1.0.57] - 2026.07.07

### Fixed

- HasInteractiveDescendant now also requires a candidate element to be focusable and to not belong to a ScrollView's internal Scroller, so a menu containing only non-focusable items (e.g. a Back button meant to be reached only via cancel) or only a ScrollViewAuto's always-present scrollbar internals is still correctly treated as empty for the cancel/focus fallback

## [1.0.56] - 2026.07.07

### Added

- Menus with no focusable items now respond to the cancel shortcut, by making the menu's root VisualElement the fallback focus/cancel target only while the menu has no interactive descendants, kept in sync at runtime as items are added or removed so the root never lingers as a phantom cancel or navigation target

## [1.0.55] - 2026.07.07

### Fixed

- ScrollViewAuto navigation now checks canGrabFocus instead of focusable when searching for the next child, so children hidden via conditional visibility (e.g. display: none) are correctly skipped instead of receiving focus while invisible

## [1.0.54] - 2026.07.07

### Fixed

- ScrollViewAuto.NavigationMoveListener no longer lets handled navigation events fall through to the panel's default (geometry-based) navigation, which could hijack focus away from the intended target, especially at wrap-around list boundaries

## [1.0.53] - 2026.07.05

### Fixed

- ScrollViewAuto navigation and auto-scroll no longer land on non-focusable children (e.g. section-header labels)

## [1.0.52] - 2026.04.11

### Added

- Allowed overriding of query choice button instantiation

## [1.0.51] - 2026.04.04

### Changed

- Initiating query with no response deselects all other objects

## [1.0.50] - 2026.04.04

### Added

- Can create Query without giving focus

## [1.0.49] - 2026.02.13

### Changed

- ScrollViewAuto.ScrollToChild() is now public

## [1.0.48] - 2026.02.12

### Fixed

- Updated Editor with new OnNavInput

## [1.0.47] - 2026.02.12

### Added

- MenuManager broadcasts OnNavInput event

## [1.0.46] - 2026.01.29

### Changed

- Undid previous update

## [1.0.45] - 2026.01.29

### Changed

- ScrollViewAuto updates focus after a delay

## [1.0.44] - 2026.01.29

### Changed

- Undid previous update

## [1.0.43] - 2026.01.29

### Changed

- MenuManager navigation is based on order of menu items 

## [1.0.42] - 2025.12.29

### Fixed

- ScrollViewAuto was working inconsistently

## [1.0.41] - 2025.12.29

### Fixed

- ScrollViewAuto confirms scrolled-to element is child

## [1.0.40] - 2025.12.04

### Added

- Signed package

## [1.0.39] - 2025.08.30

### Added

- Menu navigation can be manually overridden.

### Changed

- Clicking menu item that has been defined in the MenuManager inspector but has no target no longer hides the menu.

## [1.0.38] - 2025.08.29

### Added

- Sounds can be muted.

## [1.0.37] - 2025.08.29

### Added

- MenuManager can be activated after start.

## [1.0.36] - 2025.08.28

### Changed

- More robust navigation listening.

## [1.0.35] - 2025.08.28

### Added

- Menu event broadcasters and optional audio clips.

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
