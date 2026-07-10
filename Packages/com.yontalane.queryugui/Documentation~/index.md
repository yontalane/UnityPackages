# Query UGUI

A UGUI implementation of Yontalane's query system.

For information about how the query system itself works — starting a query, `QueryProcessor`, `QueryEventData`, `IQueryUI`, etc. — see the documentation in the Query package.

This package's `QueryUI` (`Yontalane.QueryUGUI.QueryUI`) is a singleton `MonoBehaviour` that implements `IQueryUI` using Unity's UGUI. Attach it to the same GameObject as `QueryProcessor`, or reference it from `QueryProcessor`'s `Query UI` field, to give your queries an on-screen UI.

### Variables

* `ShowType` **Show Type**: How to show and hide the query window. `Animator` (default) drives an `Animator` bool parameter named `Query Visible`; `SetActive` toggles a root GameObject's active state; `None` does neither.
* `GameObject` **Root Object**: The root object to show/hide when `Show Type` is `SetActive`.
* `Animator` **Animator**: The Animator that controls the query window when `Show Type` is `Animator`. Must have a `Query Visible` boolean parameter.
* `TMP_Text` **Text**: The field for displaying the query's prompt text.
* `TMP_Text` **Description**: An optional field for displaying the query's description text.
* `RectTransform` **Response Container**: The location to instantiate response buttons.
* `AudioClip` **Button Click**: An optional audio clip to play when a response button is clicked.
* `Button` **Primary Response Button Prefab**: An optional prefab used for the first (primary/default) response button. If left blank, `Response Button Prefab` is used for every response.
* `Button` **Response Button Prefab**: The default prefab to use for response buttons.

### Functions

* `void` **Close()**: Closes the query window.
* `void` **Initialize(string text, string description, string[] responses, int initialSelection, Action&lt;QueryEventData&gt; callback, Action&lt;QueryEventData&gt; selectCallback)**: Builds and shows the query window. This is the `IQueryUI` method invoked by `QueryProcessor`; you generally won't call it directly — start a query via `QueryProcessor.Initiate(...)` instead.

### Customizing response buttons

Subscribe to the static delegate `QueryUI.OnOverrideChoiceInstantiation` to take full control over how each response `Button` is instantiated — for example, to use a custom prefab per response or a pooling scheme. It receives a `ChoiceInstantiationData` struct describing the response being created (`index`, `buttonText`, `queryText`, `queryDescription`, `buttonContainer`, `isPrimary`, `primaryButtonPrefab`, `buttonPrefab`) and should return the created `Button`. `QueryUI` still names the button, sets up its `Navigation`, and adds its own click listener afterward.

Subscribe to the static delegate `QueryUI.OnQueryUILoaded` to be notified whenever a query UI has finished loading and been shown.
