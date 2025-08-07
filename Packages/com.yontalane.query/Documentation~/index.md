# Query UI

A dialog input box. It displays a message and allows for any number of user response buttons that are generated on the fly. It is designed to work with controller as well as keyboard and mouse.

**Note:** QueryUI is handled through a separate package.

### Variables

* **`Animator` Animator:** The Animator for controlling the dialog. Must have a `Query Visible` boolean parameter.
* **`Text` Text:** The field for displaying the dialog's message text.
* **`RectTransform` ResponseContainer:** The location to instantiate response buttons.
* **`AudioClip` ButtonClick:** An optional audio clip to play when clicking buttons.
* **`Button` ResponseButtonPrefab:** The prefab to use for the response buttons.

### Functions

* **`void` Close():** Close the window. QueryUI relies on the Animator to do the closing.
* **`static void` Initiate(`string` text, `string[]` responses, `Action<string>` callback):** Initiate a query. QueryUI sets up the query window using the parameters and relies on the Animator to open the window.