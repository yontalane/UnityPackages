# Dialog UGUI

A UGUI implementation of Yontalane's dialog system.

For information about how to work with the dialog system, see the documentation in the Dialog package.

## Pausing Dialog

Set `DialogUI.Instance.IsPaused` to `true` to pause dialog, and back to `false` to resume it. Setting `IsPaused` to `true` also pauses the typing effect if a line is in the middle of being typed out (including its looping typing sound); the line resumes typing exactly where it left off when `IsPaused` is set back to `false`. While paused, initiating a new dialog or advancing to the next line is deferred until `IsPaused` is set back to `false`.

By default, pausing also disables interaction with the skip and continue buttons, so the player can't advance the paused line. Uncheck `Disable Interaction When Paused` in the inspector if you'd rather leave those buttons interactable while paused.