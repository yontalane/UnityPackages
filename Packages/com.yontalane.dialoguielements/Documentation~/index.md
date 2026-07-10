# Dialog UI Toolkit

A UI Toolkit (UI Elements) implementation of Yontalane's dialog and query systems.

For information about how the dialog system itself works — DialogProcessor, DialogAgent, the dialog script format, keywords, etc. — see the documentation in the Dialog package. For information about how the query system itself works, see the documentation in the Query package.

## DialogPane

`DialogPane` (`Yontalane.DialogUIElements.DialogPane`) is a custom `VisualElement` that renders a single line of dialog: speaker name, dialog text (with typewriter support), a portrait image, a Continue button, a Skip button, a Close button, and a container for response buttons. Add it to a UXML document like any other UI Toolkit element, or build one in the UI Builder.

DialogPane exposes a number of properties for cosmetic customization, including `Speaker`, `SpeakerTextAfter`, `SpeakerIsInline`, `SpeakerIsBold`, `SpeakerIsCapitalized`, `SpeakerColor`, `Text`, `ContinueButtonLabel`, `Portrait`, `PortraitVisible`, `SkipButtonActive`, `ContinueButtonInteractable`, `SkipButtonInteractable`, and `VisibilityStyleClass` (a style class to toggle instead of `DisplayStyle` when showing/hiding the pane).

DialogPane requires a stylesheet resource named `DialogPane` (i.e. a `Resources/DialogPane.uss` asset somewhere in your project) and loads it automatically when constructed.

## DialogUI

`DialogUI` (`Yontalane.DialogUIElements.DialogUI`) is the singleton `MonoBehaviour` that implements `IDialogUI` for this package. It drives a `DialogPane` found within a `UIDocument`: assign the `UIDocument` (or leave it blank and it will look for one on the same GameObject or elsewhere in the scene) and the name of the `DialogPane` element to control (`Dialog Pane Name`).

Like the other `IDialogUI` implementations, it supports a per-character typewriter effect, per-line typing/typing-loop/voice/sound audio (with optional override callbacks), response buttons, portrait overrides, and per-speaker text coloring.

## QueryUI

This package also includes a UI Toolkit implementation of the query system: `Yontalane.DialogUIElements.QueryUI`. It implements `IQueryUI` and, when a query is initiated, creates a `Yontalane.UIElements.Query` visual element inside a `UIDocument`. Assign the `UIDocument` and the name of the VisualElement that should act as the query's parent (`Query Root`); if left blank, the query is added directly to the document's root. See the Query package documentation for how the query system itself works (starting a query, `QueryProcessor`, `QueryEventData`, etc.).

## Pausing Dialog

Set `DialogUI.Instance.IsPaused` to `true` to pause dialog, and back to `false` to resume it. Setting `IsPaused` to `true` also pauses the typing effect if a line is in the middle of being typed out (including its looping typing sound); the line resumes typing exactly where it left off when `IsPaused` is set back to `false`. While paused, initiating a new dialog or advancing to the next line is deferred until `IsPaused` is set back to `false`.

By default, pausing also disables interaction with the skip and continue buttons, so the player can't advance the paused line. Uncheck `Disable Interaction When Paused` in the inspector if you'd rather leave those buttons interactable while paused.
