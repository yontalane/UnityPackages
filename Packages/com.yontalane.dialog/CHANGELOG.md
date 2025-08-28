# Changelog

## [1.0.78] - 2025.08.28

### Added

- IDialogResponder can generate new LineData at runtime.

## [1.0.77] - 2025.08.26

### Added

- Added ability to add to dialog count value.

## [1.0.76] - 2025.08.26

### Added

- Added ability to set dialog count value.

## [1.0.75] - 2025.08.07

### Fixed

- Dialog text displayed for one frame.

### Changed

- Using query system that separates backend from display.

## [1.0.74] - 2025.08.07

### Changed

- DataStorage does not seek list by ref.

### Added

- DataStorage can return pairs as dictionary.

## [1.0.73] - 2025.08.07

### Added

- Added back functionality to DataStorage class.

## [1.0.72] - 2025.08.07

### Changed

- This package's obsolete DialogUI implements the IDialogUI interface.

## [1.0.71] - 2025.08.07

### Changed

- Marked this package's DialogUI class as obsolete; transitioning to the DialogUGUI package.

## [1.0.70] - 2025.08.07

### Changed

- Moved display UI to a separate package

## [1.0.69] - 2025.07.30

### Added

- Inline images

## [1.0.68] - 2025.07.27

### Fixed

- Skipping line does not show all text

## [1.0.67] - 2025.07.27

### Fixed

- Dialog breaks when receiving an empty portrait name

## [1.0.66] - 2025.07.27

### Changed

- UI uses maxVisibleCharacters for cleaner line wrapping
- Supports standard rich text tag notation

## [1.0.65] - 2025.07.27

### Added

- Fields and methods from MonoBehaviour DialogAgent now in ScriptableDialogAgent as well

## [1.0.64] - 2025.07.26

### Added

- Can import and export dialog data

### Changed

- Dialog storage data format (behind the scenes)

## [1.0.63] - 2025.07.26

### Added

- Can play looping audio while text is being typed

## [1.0.62] - 2025.07.25

### Added

- Text data includes voice and other speaker display/audio

### Changed

- Sample uses text data rather than json

## [1.0.61] - 2025.07.25

### Fixed

- Portraits were not displaying.

## [1.0.60] - 2025.07.25

### Change

- Supporting Unity 6+.

### Added

- Optional typing sound delay.

## [1.0.59] - 2025.07.21

### Changed

* Removed rewind functionality for now.

## [1.0.58] - 2025.07.20

### Fixed

* Fixed issue in which rewind history broke on functional (non-text) dialog nodes.

## [1.0.57] - 2025.07.19

### Fixed

* Made rewind button accessible in inspector.

## [1.0.56] - 2025.07.19

### Added

* Rewind button functionality.

## [1.0.55] - 2025.03.25

### Fixed

* Line joiner code caused infinite loop. Fixed.

## [1.0.54] - 2025.03.24

### Added

* TextDataConverter supports line joiners for splitting a single line over multiple lines.

## [1.0.53] - 2025.03.11

### Changed

* TextDataConverter confirms an dialog line before entering response mode.

## [1.0.52] - 2025.03.01

### Added

* DialogAgent's TextData and StaticText are publicly accessible.

## [1.0.51] - 2025.02.20

### Fixed

* Supports query box description text.

## [1.0.50] - 2025.02.20

### Added

* Supports query box description text.

## [1.0.49] - 2025.02.20

### Added

* Supports query box description text.

## [1.0.48] - 2025.02.18

### Fixed

* Parsing error in query from text script.

## [1.0.47] - 2025.02.18

### Fixed

* Script text file is visible in the inspector.

## [1.0.46] - 2025.02.18

### Changed

* Allowing earlier Unity version.

## [1.0.45] - 2025.02.18

### Added

* You can initiate a query popup from simple script.

## [1.0.44] - 2024.09.05

### Changed

* Fixed bug with optional speaker field.

## [1.0.43] - 2024.09.05

### Added

* Dialog UI supports an optional speaker name text field.

## [1.0.42] - 2024.08.29

### Added

* |: creates a mid-text linebreak.

## [1.0.41] - 2024.06.12

### Added

* Can set static text at runtime.

## [1.0.40] - 2024.06.12

### Added

* Can manually clear dialog data in agent.

## [1.0.39] - 2024.06.10

### Added

* Running portrait text through replaceInLineText().
* If simple-text portrait is [] then it is replaced by the speaker name.

## [1.0.38] - 2024.06.10

### Fixed

* Response button navigation would sometimes break.

## [1.0.37] - 2024.06.09

### Added

* Can set portrait from simple text.

## [1.0.36] - 2024.05.29

### Added

* Can use custom event to set portrait.

## [1.0.35] - 2024.05.29

### Fixed

* Response button rotation is zeroed out on instantiation.

## [1.0.34] - 2024.05.19

### Changed

* Updated dependency versions.

## [1.0.33] - 2024.05.19

### Fixed

* Fixed null reference in sample scene.

## [1.0.32] - 2024.05.17

### Changed

* Updated dependency versions.

## [1.0.31] - 2024.04.30

### Fixed

* Minor documentation fix.

## [1.0.30] - 2024.04.25

### Changed

* Updated dependency versions.

## [1.0.29] - 2024.04.10

### Fixed

* DO now works without a parameter.

## [1.0.28] - 2024.04.10

### Fixed

* SetData() resets the current data.

## [1.0.27] - 2024.04.10

### Changed

* SET uses equal sign instead of colon.

## [1.0.26] - 2024.04.10

### Changed

* DO uses comma instead of colon to delineate between function and parameter.

## [1.0.25] - 2024.04.10

### Fixed

* Multiple dialogs were all using the same script. Removed 1.0.23 addition, at least for now.

## [1.0.24] - 2024.04.10

### Added

* Invoking events on start and end typing line.

## [1.0.23] - 2024.04.09

### Changed

* Not re-parsing simple text data if already done.

## [1.0.22] - 2024.04.09

### Added

* Documentation for speakerless text.

## [1.0.21] - 2024.04.09

### Fixed

* Custom start node wasn't working.

## [1.0.20] - 2024.04.09

### Added

* Documentation for simplified text data format.

## [1.0.19] - 2024.04.09

### Added

* DialogAgent can accept a new, simplified text data format.

## [1.0.18] - 2024.04.03

### Added

* DialogAgent Data can be set publically.

## [1.0.17] - 2023.09.05

### Added

* More styling options for the dialog speaker.

## [1.0.16] - 2023.07.28

### Fixed

* Set Editor AssemblyDef to target the editor only.

## [1.0.15] - 2023.05.08

### Changed

* When writing out dialog, Dialog UI waits one frame before highlighting skip button.

## [1.0.14] - 2023.05.02

### Changed

* Dialog Data text field is now a text area.

## [1.0.13] - 2023.04.30

### Added

* Dialog Agent can accept data in additional formats.

## [1.0.12] - 2023.01.30

### Fixed

* Null check for dialog function result.

## [1.0.11] - 2023.01.30

### Added

* More speaker color options.

## [1.0.10] - 2023.01.19

### Fixed

* Refresh layout groups not working on skip.

## [1.0.9] - 2023.01.19

### Added

* Option to automatically refresh layout groups.

## [1.0.8] - 2023.01.19

### Added

* Added option for line break after speaker name.

## [1.0.7] - 2023.01.19

### Added

* You can customize the string that is displayed between the speaker's name and the dialog text.
* Added new line command: `exit`. Exits dialog when set to true.

## [1.0.6] - 2023.01.18

### Changed

* DialogUI now has a static Instance accessor.
* DialogProcessor automatically calls Close() and InstantiateLine() in DialogUI.
* DialogProcessor will not try to process a line if NodeData is null.
* Add DisplayName field to DialogAgent.

## [1.0.5] - 2023.01.17

### Added

* Added ScriptableObject variation of DialogAgent.

## [1.0.4] - 2023.01.17

### Changed

* Changed abstract class DialogResponder to interface IDialogResponder.

## [1.0.3] - 2022.12.30

### Changed

* Switched to Text Mesh Pro.

## [1.0.2] - 2022.01.15

## [1.0.1] - 2022.01.15

## [1.0.0] - 2022.01.05

### Added

* Dialog
* DialogProcessor has public `KillDialog()` function.