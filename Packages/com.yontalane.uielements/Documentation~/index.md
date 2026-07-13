# UI Elements

A library of menu, dialog, and control components built on Unity's UI Toolkit (`UnityEngine.UIElements`). Where the `com.yontalane.menus` package drives `UnityEngine.UI` (uGUI) menus, this package serves the same purpose for UI Toolkit projects: interconnected menus with controller/keyboard navigation, laid out in UXML and wired up by subclassing an abstract Menu Manager. It also includes a handful of standalone controls (Selectable Button, Icon Button, Toggle Button, Scroll View Auto) and overlay elements (Notification, Query) usable on their own, without the menu system.

## Menu Manager

Abstract base class for managing a UIDocument's menus: navigation, focus, sound and event feedback, and dynamically added ("addable") items. Subclass it and implement **OnClick** to react to menu item clicks — or use Simple Menu Manager if you'd rather wire everything up through UnityEvents in the Inspector instead of subclassing.

On Awake, Menu Manager finds the scene's UIDocument (via `FindAnyObjectByType`), validates its configured subordinates and menu items, and — for every Menu in **Menus** — finds that Menu's root VisualElement by name (its UXML id) and registers click, cancel, and left/right-navigation handling for every Button, Toggle, and other bindable control inside it. On Start, it calls **Activate** automatically unless **ActivateOnStart** is overridden to return false.

A Menu Manager can have **subordinate** Menu Managers (for example, a HUD manager with a pause menu subordinate to it). A menu item's **Menu Item Type** determines what selecting it does: a `Normal` item switches to another menu within the same Menu Manager; a `Subordinate` item switches to a menu within one of this Menu Manager's subordinates; a `Dominant` item switches to a menu within the Menu Manager this one is subordinate to.

An optional **Global Menu** (e.g. a persistent tab strip) can be shown across every Menu that opts in via its own **hasGlobalMenu** setting, and can be tabbed through with **Input**'s configured tab-left/tab-right actions.

When switching menus, Menu Manager also handles focus automatically: it remembers the last-focused item per menu (**Menu.focusItem**) and, when a menu has no remembered focus, focuses the first focusable control inside it — falling back to a scrollable block of text, and finally to the menu's own root, for menus with nothing else focusable.

### Properties

| Name                 | Description                                                  |
| --------------------- | ------------------------------------------------------------ |
| **Global Menu**       | The optional global menu shown across every Menu that opts in. See Global Menu below. |
| **Menus**             | The regular menus (and subordinate Menu Managers) managed by this Menu Manager. See Menu Collection below. |
| **Input**             | The input actions driving navigation, tabbing, and button events. See Control Input below. |
| **Listeners**         | UnityEvents for menu actions and interactions. See Listeners below. |
| **Sounds**            | Optional sound effects for menu actions and feedback. See Sounds below. |
| **Document**          | The UIDocument this Menu Manager controls, found automatically on Awake. |
| **Root**              | The root VisualElement of **Document**.                       |
| **InputActions**      | The InputActionAsset assigned in **Input**.                    |
| **ActivateOnStart**   | Whether this Menu Manager should activate itself automatically on Start. Override to return false to activate it manually instead. |

### Public Methods

| Name                      | Description                                                  |
| -------------------------- | ------------------------------------------------------------ |
| **Activate**               | Hides all subordinate Menu Managers' menus, then displays this Menu Manager's first menu (per Menu Collection's **firstMenu**). |
| **RegisterDynamicElement** | Registers click/cancel/navigation handling for a Button, Toggle, or other bindable element added to a menu at runtime, so it behaves the same as elements present when the menu was first registered at Awake. |
| **GetMenus**               | Returns the array of menus managed by this Menu Manager.       |
| **TryGetMenu**             | Attempts to retrieve a Menu by name — optionally also its root VisualElement and (for addable items) its container VisualElement. |
| **TryGetContainer**        | Attempts to retrieve the addable-item container VisualElement for a menu by name. |
| **TryGetActiveMenu**       | Attempts to retrieve the currently active Menu and its root VisualElement. |
| **IndexOfActiveMenu**      | Returns the index of the currently active menu in **Menus**, or -1 if none is active. |
| **SetEnabled**             | Sets the enabled state of a named UI element, within a given menu or the currently active one. |
| **AddClass** / **RemoveClass** | Adds or removes a USS class from a named UI element within a given menu. |
| **SetText**                | Sets the text of a named TextElement, within a given menu or the root. |
| **SetFocus**               | Sets keyboard/gamepad focus to a named UI element within the currently active menu. |
| **Clear**                  | Removes all addable items from a menu's container.            |
| **Add**                    | Adds an addable UI item to a menu's container, appending it at the end. See Addable Item Data below. |
| **Insert**                 | Adds an addable UI item to a menu's container at a specific index. |
| **AddRange**               | Adds a range of addable UI items to a menu's container.        |
| **Remove**                 | Removes a named addable item from a menu's container.          |
| **SetMute**                | Mutes or unmutes this Menu Manager's sound effects.            |

### Protected Members

These are meant to be called or overridden from a subclass — see Simple Menu Manager for a subclass that exposes them as UnityEvents instead.

| Name                  | Description                                                  |
| ---------------------- | ------------------------------------------------------------ |
| **SetMenu**            | Activates the menu with the given name (or hides all menus if null/empty), updates remembered focus, and updates the global menu's tab highlight. |
| **HideAllMenus**       | Hides every menu managed by this Menu Manager, optionally including the global menu. |
| **OnClick**            | Abstract. Called when a menu item (Button or Toggle) is clicked. Implement to define your game's response. |
| **OnCancel**           | Virtual. Called when the user cancels/backs out of a menu or item with no **cancelTarget** configured. Set `blockEvent` true to stop the cancel from also reaching UI Toolkit's own handling. |
| **OnSideNavigation**   | Virtual. Called on left/right navigation within a menu item.  |
| **OnDisplayMenu**      | Virtual. Called whenever a menu becomes visible.               |
| **OnButtonInput**      | Virtual. Called when one of **Input**'s configured buttons is performed. |

## Simple Menu Manager

A ready-made Menu Manager subclass that exposes menu interactions as UnityEvents instead of requiring you to subclass and override **OnClick**.

### Properties

| Name           | Description                                                  |
| --------------- | ------------------------------------------------------------ |
| **OnMenuItem**  | Event invoked when a menu item is interacted with, passing the menu name and item name. |
| **OnButton**    | Event invoked when a menu item is interacted with and isn't otherwise wired to navigate between menus (i.e. `ClickData.inUse` is false), passing the item name. |

## Menu

Represents a single UI Toolkit menu: a UXML element (identified by name/uxml id), its items, and its navigation behavior. An entry in Menu Collection's **menus** array, or Global Menu's **menu**.

### Properties

| Name                          | Description                                                  |
| ------------------------------ | ------------------------------------------------------------ |
| **name**                       | The unique name (uxml id) of the menu.                        |
| **items**                      | The menu items contained in this menu. See Menu Item below.  |
| **hasCancelTarget**            | Whether this menu has a cancel target defined.                |
| **cancelTarget**               | The menu item to target when cancel is triggered.             |
| **blockSideNavigation**        | Whether to block side navigation (e.g. left/right tabbing) in this menu. |
| **hasGlobalMenu**              | Whether this menu shows the Global Menu while active.         |
| **addableContainer**           | The uxml id of the container for addable items in this menu. Defaults to the menu's own root if unset. |
| **defaultAddableItemTemplate** | The default VisualTreeAsset template for addable items in this menu. |
| **focusItem**                  | The uxml id of the item to focus when this menu next becomes active. Updated automatically as focus changes, and can be set to seed the initially focused item. |

## Menu Item

Represents a single item within a Menu — typically a Button or Toggle found by name inside the menu's root VisualElement.

### Properties

| Name                    | Description                                                  |
| ------------------------ | ------------------------------------------------------------ |
| **name**                 | The unique name (uxml id) of this menu item.                 |
| **type**                 | This item's Menu Item Type (Normal, Subordinate, or Dominant). |
| **targetMenu**           | The name of the menu to activate when this item is selected.  |
| **targetSubordinate**    | The index, into Menu Collection's **subordinates**, of the subordinate Menu Manager to activate a menu on (used when **type** is Subordinate). |
| **navigationOverrides**  | Optional per-direction focus overrides for this item. See Navigation Override below. |

## Menu Item Type

The behavior of a Menu Item when it's selected.

| Name             | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| **Normal**        | Activates another menu within the same Menu Manager.          |
| **Subordinate**   | Activates a menu within one of this Menu Manager's subordinates. |
| **Dominant**      | Activates a menu within the Menu Manager this one is subordinate to. |

## Menu Collection

The set of menus (and subordinate Menu Managers) managed by a Menu Manager.

### Properties

| Name              | Description                                                  |
| ------------------ | ------------------------------------------------------------ |
| **firstMenu**      | The index, into **menus**, of the menu shown when Menu Manager's **Activate** is called. Use -1 for none. |
| **subordinates**   | The subordinate Menu Managers managed by this collection.     |
| **menus**          | The menus included in this collection. See Menu above.        |

## Global Menu

The optional persistent menu (e.g. a tab strip) shown across every Menu that opts in via its own **hasGlobalMenu** setting.

### Properties

| Name             | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| **menu**          | The Menu that makes up the global menu.                       |
| **resetFocus**    | Whether to reset focus to the default item when the global menu is used to switch to a new menu. |

## Navigation Override

Lets a specific Menu Item override where focus goes for one particular directional input, instead of relying on UI Toolkit's default geometry-based navigation. An entry in Menu Item's **navigationOverrides** array.

### Properties

| Name          | Description                                                  |
| ------------- | ------------------------------------------------------------ |
| **direction** | The direction of this navigation override. See Directions below. |
| **target**    | The uxml id of the UI element that gets focus when this navigation is triggered. |

## Directions

The four cardinal directions used by Navigation Override.

| Name        | Description             |
| ----------- | ------------------------ |
| **Up**      | Upward navigation.       |
| **Right**   | Rightward navigation.    |
| **Down**    | Downward navigation.     |
| **Left**    | Leftward navigation.     |

## Control Input

The input configuration used by a Menu Manager for menu navigation and button actions.

### Properties

| Name                 | Description                                                  |
| --------------------- | ------------------------------------------------------------ |
| **actions**           | The InputActionAsset containing all input actions for menu navigation. |
| **directionalInput**  | The action name or path for basic navigation.                 |
| **tabLeft**           | The action name or path for navigating to the previous tab.   |
| **tabRight**          | The action name or path for navigating to the next tab.       |
| **buttons**           | Action names or paths for menu button actions (e.g. submit, cancel), each raised through **OnButtonInput**. |

## Listeners

UnityEvents for menu actions and interactions, used by Menu Manager and (via its own equivalents) Simple Menu Manager.

### Properties

| Name                  | Description                                                  |
| ---------------------- | ------------------------------------------------------------ |
| **onClick**            | Broadcast on menu item clicking. See Click Data below.        |
| **onNavigationInput**  | Broadcast on raw directional navigation input, passing the direction as a Vector2Int. |
| **onNavigation**       | Broadcast whenever a menu item gains focus via navigation.     |
| **onTabNavigation**    | Broadcast on Global Menu tab navigation.                       |
| **onCancel**           | Broadcast when the user presses the cancel key to back out of a menu. |

## Sounds

Optional sound effects for menu actions and feedback.

### Properties

| Name             | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| **mute**          | Toggles whether sounds should play.                            |
| **click**         | Sound effect for menu item clicking.                            |
| **navigation**    | Sound effect for menu item navigation.                          |
| **tab**           | Sound effect for menu tab navigation.                            |
| **cancel**        | Sound effect for when the user presses the cancel key to back out of a menu. |

## Click Data

Passed to Listeners' **onClick** event (and Menu Manager's **OnClick** override) whenever a menu item is clicked.

### Properties

| Name         | Description                                                  |
| ------------ | ------------------------------------------------------------ |
| **menu**     | The name of the menu where the click event occurred.          |
| **item**     | The name of the item that was clicked within the menu.        |
| **inUse**    | Whether the clicked item was already handled by Menu Manager itself — i.e. it's a Normal, Subordinate, or Dominant menu item with a **targetMenu** configured. |

## Addable Item Data

Describes a UI item to dynamically add to a menu via Menu Manager's **Add**/**Insert**/**AddRange**.

### Properties

| Name          | Description                                                  |
| ------------- | ------------------------------------------------------------ |
| **name**      | The unique uxml id for the new UI element instance.            |
| **text**      | The display text to assign to the UI element (e.g. a button label). |
| **label**     | The uxml id of the label element within the template to set the text on. Leave unset to set text directly on the instantiated element (if it's a TextElement) instead. |
| **template**  | The VisualTreeAsset template to instantiate for this item. Leave null to use the menu's **defaultAddableItemTemplate**. |
| **onAdd**     | Optional callback invoked with the newly added element once it's added to the menu. |

## Selectable Button

A UI Toolkit Button that tracks its own visual state (Normal, Hover, Active) based on focus and pointer events, broadcasting every state change through a static event so other systems — such as Query, or a sound-effect listener — can react without subscribing to each button individually.

### Delegates

| Name              | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| **OnButtonEvent** | Static event invoked whenever any Selectable Button's focus or pointer state changes. See Selectable Button Event Info below. |

## Selectable Button Event Info

Passed to Selectable Button's static **OnButtonEvent** delegate.

### Properties

| Name         | Description                                                  |
| ------------ | ------------------------------------------------------------ |
| **target**   | The Selectable Button that triggered the event.                |
| **type**     | The type of event that occurred. See Selectable Button Event Type below. |
| **hasFocus** | Whether the button currently has focus.                        |
| **state**    | The button's current visual state. See Button State below.     |

## Selectable Button Event Type

| Name              | Description                                    |
| ------------------ | ------------------------------------------------ |
| **None**           | No event.                                        |
| **FocusIn**        | The button has received focus.                   |
| **FocusOut**       | The button has lost focus.                       |
| **PointerEnter**   | The pointer has entered the button area.         |
| **PointerExit**    | The pointer has exited the button area.          |
| **PointerDown**    | The pointer is pressed down on the button.       |
| **PointerUp**      | The pointer is released from the button.          |
| **Cancel**         | The pointer interaction was canceled.             |

## Button State

The visual state of a Selectable Button.

| Name        | Description                     |
| ----------- | ---------------------------------- |
| **Normal**  | The button is in its normal state. |
| **Hover**   | The button is being hovered over.   |
| **Active**  | The button is in its active (pressed) state. |

## Icon Button

A Selectable Button that displays an icon and/or a text label, laid out via UXML attributes.

### Properties

| Name             | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| **Text**          | The text displayed on the button.                              |
| **IconSize**      | The size of the icon.                                          |
| **Icon**          | The icon sprite.                                                |
| **Gap**           | The spacing between the icon and the text, when both are present. |
| **IconOnLeft**    | Whether the icon appears on the left side of the text (true) or the right (false). |
| **HasIcon**       | Whether this button currently has an icon assigned.            |
| **HasText**       | Whether this button currently has text assigned.                |

## Toggle Button

An Icon Button that can operate as a toggle, adding a "checked" style class when on.

### Properties

| Name       | Description                                                  |
| ---------- | ------------------------------------------------------------ |
| **Value**  | The current checked state of the toggle button. Setting it updates the visual state and invokes **OnChange**. |

### Delegates

| Name         | Description                              |
| ------------ | ------------------------------------------ |
| **OnChange** | Invoked when **Value** changes. See Toggle Button Change Event below. |

### Public Methods

| Name                          | Description                                    |
| ------------------------------ | ------------------------------------------------ |
| **SetValueWithoutNotify**      | Sets **Value** without invoking **OnChange**.    |

## Toggle Button Change Event

Passed to Toggle Button's **OnChange** delegate.

### Properties

| Name          | Description                                        |
| ------------- | ----------------------------------------------------- |
| **oldValue**  | The value of the toggle before the change occurred.    |
| **newValue**  | The value of the toggle after the change occurred.     |
| **target**    | The Toggle Button that triggered the change event.      |

## Scroll View Auto

A ScrollView that automatically registers its child elements — including any added via its own **Add**/**Insert**/**CloneTreeAsset** methods — so that directional navigation steps focus between them, similarly to how Menu Manager wires up its own menus. Useful for a scrollable list whose items should be navigable via directional input without registering each one by hand.

### Properties

| Name           | Description                                              |
| -------------- | ----------------------------------------------------------- |
| **ChildCount** | The number of child elements contained in the scroll view.    |

### Public Methods

| Name                 | Description                                                  |
| --------------------- | ------------------------------------------------------------ |
| **Children**          | Returns the child VisualElements contained within the scroll view. |
| **ScrollToChild**     | Scrolls the view to bring the given child into view, centering it vertically if possible. |
| **Clear**             | Removes all child elements from the scroll view.               |
| **Add**               | Adds an element to the scroll view and registers it for navigation. |
| **Insert**            | Inserts an element at the specified index and registers it for navigation. |
| **AddRange**          | Adds a range of elements to the scroll view.                    |
| **Remove**            | Removes an element from the scroll view, by reference or by name, and unregisters it from navigation. |
| **RemoveAt**          | Removes the child element at the specified index, and unregisters it from navigation. |
| **CloneTreeAsset**    | Clones a VisualTreeAsset into the scroll view and registers the added elements for navigation. |

## Notification

A push-notification-style message pane: a header with optional body text that slides/fades in, stays for a configurable duration, then dismisses itself. Multiple Notifications on the same UIDocument stack into distinct slots automatically.

### Properties

| Name          | Description                                                  |
| ------------- | ------------------------------------------------------------ |
| **Header**    | The header text displayed at the top of the notification.      |
| **Text**      | The main body text of the notification. Automatically hides the body label if set to null or empty. |
| **Duration**  | How long, in seconds, the notification is displayed before dismissing itself. Defaults to 4. |

### Constructors

| Name              | Description                                                  |
| ------------------ | ------------------------------------------------------------ |
| **Notification**   | Creates a Notification with a header and, optionally, body text and/or a UIDocument to attach to and auto-dismiss from. Without a UIDocument, the notification shows immediately and does not auto-dismiss. |

### Public Methods

| Name        | Description                                                  |
| ----------- | ------------------------------------------------------------ |
| **Remove**  | Removes this notification from its parent VisualElement, dismissing it immediately. |

## Query

A modal alert or dialog box: a header, optional body text, and one or more response buttons, navigable via keyboard/gamepad and dismissible via a configurable Cancel response.

### Properties

| Name               | Description                                                  |
| ------------------- | ------------------------------------------------------------ |
| **Header**          | The header text displayed at the top of the query dialog.      |
| **Text**            | The main body text displayed in the query dialog.               |
| **ButtonLabel**     | The label of the last response button. If there are no responses, gets or sets the default response's label instead. |
| **Responses**       | The response button labels for the query dialog. Setting this rebuilds the response buttons and their navigation/cancel behavior. |
| **ResponseButtons** | The instantiated Selectable Button response buttons.            |
| **CanCancel**       | Whether the query can be dismissed via a cancel input (e.g. Escape). |

### Delegates

| Name                          | Description                                                  |
| ------------------------------ | ------------------------------------------------------------ |
| **OnOverrideChoiceInstantiation** | Static delegate that lets you customize how each response's Selectable Button is instantiated (e.g. adding style classes or custom label text). See Choice Instantiation Data below. |
| **OnChangeSelectedButton**     | Instance event invoked whenever a different response button becomes focused.  |

### Constructors

| Name      | Description                                                  |
| --------- | ------------------------------------------------------------ |
| **Query** | Creates a Query with a header and, optionally, body text, a list (or single) response label, an initially focused response, and a callback invoked once the user responds. |

### Public Methods

| Name              | Description                                                  |
| ------------------ | ------------------------------------------------------------ |
| **SetCallback**    | Sets the callback invoked when a response is chosen.           |
| **SetOnNavigate**  | Sets the callback invoked when navigation moves focus between responses. |
| **Focus**          | Gives the Query dialog focus, focusing the first (or a specified) response button. |

## Query Event

Passed to Query's response and navigation callbacks.

### Properties

| Name                    | Description                                                  |
| ------------------------ | ------------------------------------------------------------ |
| **chosenResponseText**   | The text of the response chosen (or currently focused) by the user. |
| **chosenResponseIndex**  | The index of that response within **allResponses**.            |
| **allResponses**         | All possible response texts presented to the user.              |

## Choice Instantiation Data

Passed to Query's static **OnOverrideChoiceInstantiation** delegate.

### Properties

| Name                | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| **index**            | The index of the Selectable Button to be added.                |
| **buttonText**       | The label text of the Selectable Button to be added.            |
| **queryText**        | The title text of the query.                                     |
| **queryDescription** | The description text of the query.                               |
| **buttonContainer**  | The location in which Selectable Buttons are expected to be added. |
| **isPrimary**        | Whether the current choice is the primary/default one.          |

## Sound Player

Static helper for playing one-off audio clips (e.g. menu sound effects) without needing a dedicated AudioSource in the scene — each call spawns a temporary, self-destroying player positioned at the main camera.

### Static Methods

| Name      | Description                                                  |
| --------- | ------------------------------------------------------------ |
| **Play**  | Plays the given AudioClip via a temporary, self-destroying AudioSource. Does nothing if the clip is null. |
