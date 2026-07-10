# Dialog UGUI

A UGUI implementation of Yontalane's dialog system.

For information about how the dialog system itself works — DialogProcessor, DialogAgent, the dialog script format, keywords, etc. — see the documentation in the Dialog package.

This package's `DialogUI` (`Yontalane.DialogUGUI.DialogUI`) is the singleton `MonoBehaviour` that implements `IDialogUI` using Unity's UGUI. It needs to exist somewhere in your scene alongside `DialogProcessor`, and its `Animator` field needs a `Dialog Visible` boolean parameter to show and hide the dialog box.

### Variables

**Cosmetics**

* Color **Player Color**: Color of the player's name in the dialog header text.
* Color **Speaker Color**: Color of the dialog agent's own name (i.e. `<<self>>`) in the dialog header text.
* ColorSet[] **Speaker Colors**: Per-speaker-name color overrides, for speakers other than the player or `self`.
* Color **Fallback Speaker Color**: Color used for a speaker name that doesn't match the player, `self`, or an entry in Speaker Colors.
* bool **Speaker Bold**: Whether the speaker name is bold.
* bool **Speaker Caps**: Whether the speaker name is capitalized.
* string **Speaker Separator**: Text that appears between the speaker name and the dialog text (default `": "`).
* int **Speaker Line Breaks**: Number of line breaks between the speaker name and the dialog text (0–2).
* bool **Use Type Character Interval**: Whether to write text out character-by-character (typewriter effect) instead of showing it all at once.
* float **Type Character Interval**: Seconds to wait between characters when writing out text.
* float **Type Sound Delay**: How long to wait before another typing sound effect can play, as a fraction of the typing sound's length. `0` plays a sound as soon as a new character appears; `1` waits for the previous sound to finish; `0.5` allows a new sound once the previous is at least halfway done.
* bool **Auto Continue**: Whether to advance to the next line automatically once it finishes writing/playing, instead of waiting for player input.

**Pausing**

* bool **Disable Interaction When Paused**: See "Pausing Dialog," below.

**Scene UI**

* RectTransform **Dialog Root**: The root container of the dialog UI. Used to recursively refresh child layout groups while typing.
* bool **Refresh Layout Groups**: Whether to force layout groups to refresh while typing.
* Animator **Animator**: The Animator that controls the dialog UI. Must have a `Dialog Visible` boolean parameter.
* TMP_Text **Text Field**: The field for displaying dialog text.
* TMP_Text **Speaker Field**: An optional field for the speaker's name. If left blank, the speaker's name is shown inline within Text Field instead.
* Button **Skip Button**: Button for skipping the text-writing sequence. May be the same object as Continue Button.
* Button **Continue Button**: Button for proceeding to the next line of dialog. May be the same object as Skip Button.
* ButtonActiveMethod **Continue Button Active Method**: Whether the Continue Button's shown/hidden state is controlled via its `interactable` property (`Interactable`) or by toggling its GameObject's active state (`Active`).
* RectTransform **Response Container**: The object within which response button prefabs are instantiated.
* RectTransform **Portrait Container**: The object that contains the portrait Image. Having a reference to both the container and the portrait helps with layout.
* Image **Portrait**: The Image that displays the speaker's portrait, if any.
* AudioClip **Button Click**: Sound effect for clicking a response button.
* AudioClip **Typing Default**: Default typing sound effect, used when a line doesn't specify its own `typing` clip.
* AudioClip **Typing Loop Default**: Default typing-loop sound effect, used when a line doesn't specify its own `typingLoop` clip.
* bool **Stop Blast After Text**: Whether a line's `sound`/`voice` clip should be cut short if the text finishes typing before the clip finishes playing.

**Prefabs**

* Button **Response Button Prefab**: The prefab to use for response buttons.

**Overrides**

* SetSpriteAction **Set Portrait**: A callback for setting a custom portrait instead of the one requested by the dialog data. Set Portrait overrides Get Portrait, which in turn overrides the dialog data.
* GetSpriteAction **Get Portrait**: A callback for supplying a custom portrait instead of the one requested by the dialog data.
* GetAudioClipAction **Get Sound**: A callback for supplying a custom audio clip instead of the one requested by the dialog data's `sound` field.
* GetAudioClipAction **Get Voice**: A callback for supplying a custom audio clip instead of the one requested by the dialog data's `voice` field.

**Callbacks**

* UnityEvent **On Start Typing**: Invoked when a line starts being typed out.
* UnityEvent **On Type**: Invoked for each character revealed while a line is being typed out.
* UnityEvent **On Display Line**: Invoked once a line is fully typed out (or shown all at once).
* ResponseClickHandler **On Clicked Response Button**: Invoked, with the response's index and its `ResponseData`, when a response button is clicked.

### Public Properties

* float **TypeCharacterInterval**: Runtime accessor for the Type Character Interval field, above.
* bool **IsPaused**: See "Pausing Dialog," below.

## Pausing Dialog

Set `DialogUI.Instance.IsPaused` to `true` to pause dialog, and back to `false` to resume it. Setting `IsPaused` to `true` also pauses the typing effect if a line is in the middle of being typed out (including its looping typing sound); the line resumes typing exactly where it left off when `IsPaused` is set back to `false`. While paused, initiating a new dialog or advancing to the next line is deferred until `IsPaused` is set back to `false`.

By default, pausing also disables interaction with the skip and continue buttons, so the player can't advance the paused line. Uncheck `Disable Interaction When Paused` in the inspector if you'd rather leave those buttons interactable while paused.
