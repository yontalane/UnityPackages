# Query

A modal query/input-box system. It displays a prompt (and, optionally, a description) along with any number of response options generated on the fly, and invokes a callback with the response the player chose.

This package's logic is UI-agnostic; the actual on-screen presentation is provided by a separate package that implements `IQueryUI` — for example `com.yontalane.queryugui` (UGUI) or `com.yontalane.dialoguielements` (UI Toolkit).

## Structure

To use the query system, you need a singleton `QueryProcessor` somewhere in your scene. Assign a GameObject containing a component that implements `IQueryUI` to `QueryProcessor`'s `Query UI` field, or attach the `IQueryUI` component directly to the same GameObject as `QueryProcessor`.

Start a query with one of the static `QueryProcessor.Initiate(...)` methods (or `QueryProcessor.InitiateWithDescription(...)` if you also want a description shown under the main prompt). `QueryProcessor` forwards the call to your `IQueryUI` implementation's `Initialize(...)` method, which is responsible for building and displaying the on-screen UI and, once the player responds, invoking `QueryProcessor.Callback` (and, as the player highlights different responses along the way, `QueryProcessor.SelectCallback`).

For a simple confirm/cancel or single-button "OK" prompt, use `QueryProcessor.Alert(...)`, a convenience wrapper around `Initiate(...)`.

### QueryProcessor

* `static void` **Initiate(...)**: Starts a query and displays it via the assigned `IQueryUI`. This comes in many overloads: with or without a caller-supplied `id` (a string identifying this particular query, useful for telling multiple queries apart; if omitted, the processor's current `Id` is reused), with or without a `description` (use `InitiateWithDescription` for the long form), with or without an `initialSelection` index (which response is highlighted by default; defaults to `0`), and with a callback that receives either the full `QueryEventData` or just the chosen response's text as a plain string.
* `static void` **InitiateWithDescription(...)**: Same as `Initiate`, but always takes a `description` string to display underneath the main prompt.
* `static void` **Alert(...)**: Convenience overloads for a simple confirm/cancel (or single "OK" button) prompt, built on top of `Initiate`.
* `string` **Id**: The active query's unique ID — the `id` value most recently passed to `Initiate`.

### IQueryUI

Implement this interface on the component that actually displays a query's UI on-screen:

* `void` **Initialize(string text, string description, string[] responses, int initialSelection, Action&lt;QueryEventData&gt; callback, Action&lt;QueryEventData&gt; selectCallback)**: Called by `QueryProcessor` to build and show the query UI. Invoke `callback` once the player chooses a response, and, optionally, `selectCallback` whenever the highlighted response changes but hasn't yet been chosen.

### QueryEventData

The data passed to a query's callbacks:

* string **prompt**: The query's main prompt text.
* string **description**: The query's description text, if any.
* string[] **responses**: All of the response options that were offered.
* string **chosenResponse**: The text of the response that was chosen (or, for `selectCallback`, the one currently highlighted).
* string **queryId**: The query's unique ID.

### ShowType

Used by `IQueryUI` implementations to decide how to show and hide the query window: `None`, `Animator` (drive an `Animator` bool parameter named `Query Visible`), or `SetActive` (toggle a root GameObject's active state).

## Deprecated QueryUI

This package also contains a legacy `QueryUI` MonoBehaviour (a UGUI implementation of `IQueryUI`). It is marked `Obsolete` and will be removed from this package in a future update. For a UGUI query UI, use the `com.yontalane.queryugui` package's `QueryUI` instead; for UI Toolkit, use `com.yontalane.dialoguielements`.
