# Menus

Menus provides a cross-platform game menu system, driven by Unity UI (`UnityEngine.UI`), that supports controller as well as keyboard and mouse. The layout of multiple menus (e.g. an interconnected main menu, about screen, and settings screen) — as well as navigation bridges between those menus (a "Settings" button in the main menu and a "Back" button in the Settings menu) — can be set up entirely in the Hierarchy and Inspector.

In this package, a "menu" is a list of items or controls. A common game start flow might contain a main menu, an about screen, a settings screen, and a credits screen — each of these four screens is a single menu. A "menu item" is an individual, selectable item within a menu: most commonly a button, though it might also be a slider, toggle, or Cycle Selector (in a settings menu), or a scrollable Text Area (in an about screen). Menu items are all `UnityEngine.UI.Selectable`-derived components.

**Note:** Only one menu, and only one menu item within that menu, can be active at a time.

## Menu Manager

Place a Menu Manager on a GameObject, beneath which you place all of your interconnected Menus. Adding a Menu Manager automatically adds a required Menu Input alongside it. On Awake, Menu Manager initializes every child Menu and activates whichever one is configured as its initially active menu.

If a Menu Manager's GameObject also has an Animator, Menu Manager sets that Animator's `Active Menu` integer parameter to the index of whichever Menu is newly activated, so you can drive transition animations from the Animator Controller.

### Properties

| Name              | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| **Menus**         | The child Menu components managed by this Menu Manager. If left unassigned, populated automatically at runtime with all child Menu components. |
| **Active Menu**   | The initially active Menu, set in the Inspector as an index into **Menus**. |
| **Connections**   | Buttons that navigate from one Menu to another. See Connection below. |
| **Instance**      | The singleton instance of the component, set on Awake.       |
| **ActiveMenu**    | Gets the currently active Menu, or null if none is active.   |

### Delegates

| Name                  | Description                                                  |
| --------------------- | ------------------------------------------------------------ |
| **OnActivateMenu**    | Static event invoked whenever a Menu Manager activates a new Menu. |
| **OnMenuItemClick**   | Static event invoked whenever a menu item is clicked (or its equivalent input button is pressed while it's highlighted). |
| **OnMenuItemSelect**  | Static event invoked whenever a menu item becomes highlighted, via pointer hover or navigation. |
| **OnMenuButton**      | Static event invoked whenever a named input button (e.g. "Accept", "Cancel") is pressed while a Menu Manager is active. |

### Public Methods

| Name                   | Description                                                  |
| ---------------------- | ------------------------------------------------------------ |
| **TryGetMenu**         | Search for a Menu within this Menu Manager by name, returning whether it was found. |
| **GetMenu**            | Get the Menu with the given name within this Menu Manager, or null if not found. |
| **ActivateMenu**       | Activate the Menu with the given name.                       |
| **TryGetActiveMenu**   | Get the currently active Menu, returning whether one is active. |

## Connection

An entry in Menu Manager's **Connections** array, describing a button that navigates from one Menu to another. For example, to handle a connection from a main menu to an about screen, you might set **Source Button** to "About Button", **Source Menu** to "Main Menu", and **Target Menu** to "About Menu".

### Properties

| Name             | Description      |
| ---------------- | ----------------- |
| **sourceButton** | Source button.    |
| **sourceMenu**   | Source menu.      |
| **targetMenu**   | Target menu.      |

## Menu Input

Automatically added alongside Menu Manager. Menu Input doesn't detect input entirely on its own — you have to use an event system to trigger its listener methods, either by wiring a PlayerInput component's Unity Events (or Send Messages) to them, an InputActionAsset assigned directly to Menu Input, or by calling them from your own script. It's essential that whichever approach you use calls **OnNavigate**, **OnSubmit** (or **OnAccept**/**OnConfirm**), and **OnCancel**; the rest are optional, depending on your menu's functionality.

### Properties

| Name                                              | Description                                                  |
| -------------------------------------------------- | ------------------------------------------------------------ |
| **Actions**                                        | An input config asset. If you don't use this, you'll need to receive input in a different way, such as by attaching a PlayerInput to this GameObject. |
| **Action Navigate Map**                            | The action used for directional navigation. Leave blank to ignore. |
| **Action Submit Map**                              | The action used to submit/accept. Leave blank to ignore.     |
| **Action Cancel Map**                              | The action used to cancel. Leave blank to ignore.            |
| **Action Scroll Map**                               | The action used for scrolling (e.g. a Text Area). Leave blank to ignore. |
| **Action Gamepad Face Button North/East/South/West Map** | The actions used for the four gamepad face buttons. Leave blank to ignore. |
| **Action Gamepad Shoulder Left/Right Map**         | The actions used for the gamepad shoulder buttons. Leave blank to ignore. |
| **Action Gamepad Start/Select Map**                | The actions used for the gamepad Start and Select buttons. Leave blank to ignore. |
| **Allow Hold Down Move**                           | If true, you can hold down a navigation button to continually select the next item in the Menu. Otherwise, you need to repeatedly press the navigation button. |

### Delegates

| Name             | Description                                                  |
| ---------------- | ------------------------------------------------------------ |
| **OnInputEvent** | Instance event invoked whenever Menu Input produces a navigation, scroll, or named-button input event. Menu Manager subscribes to this internally. |

### Public Methods

| Name                                                        | Description                                                  |
| ------------------------------------------------------------ | ------------------------------------------------------------ |
| **OnMove** / **OnNavigate**                                  | Input listener for directional navigation.                   |
| **OnScroll**                                                  | Input listener for scroll input (e.g. for a Text Area).      |
| **OnAccept** / **OnSubmit** / **OnConfirm**                  | Equivalent input listeners that raise the "Accept" button event — call whichever name matches your input setup. |
| **OnCancel**                                                  | Input listener that raises the "Cancel" button event.         |
| **OnGamepadFaceButtonNorth** / **East** / **South** / **West** | Input listeners that raise the corresponding named button event. |
| **OnGamepadShoulderLeft** / **OnGamepadShoulderRight**       | Input listeners that raise the corresponding named button event. |
| **OnGamepadStart** / **OnGamepadSelect**                      | Input listeners that raise the corresponding named button event. |

## Menu Component

Abstract base class shared by Menu Manager, Menu, and every menu item component. Provides convenient access to the Menu Input and Menu Manager anywhere above this component in the Hierarchy.

### Properties

| Name              | Description                                        |
| ----------------- | --------------------------------------------------- |
| **MenuInput**     | The Menu Input that affects this Menu Component.    |
| **MenuManager**   | The Menu Manager that affects this Menu Component.   |

## Menu

Place a Menu on a GameObject that contains one or more Selectable objects and is a descendant of a Menu Manager. A Menu is generally assumed to be oriented vertically.

Beyond its initial set of Selectables, a Menu can also have items added to (and removed from) it at runtime — for example, populating an inventory screen — by assigning an **Addable Item** template Selectable and using **Add**/**Remove**/**Clear**.

### Properties

| Name                     | Description                                                  |
| ------------------------ | ------------------------------------------------------------ |
| **selectables**          | The Selectable items within this Menu. If left unassigned, the Menu will attempt to find them at runtime. |
| **activeSelectable**     | The initially selected Selectable, by index into **selectables**. |
| **useNavigation**        | Whether or not to use navigation (joystick or arrow keys to move between menu items). |
| **wrapNavigation**       | Whether or not to wrap navigation. If wrapping is on, clicking next while selecting the last item moves your selection to the first item. Otherwise, it will do nothing. |
| **rememberHighlight**    | If you exit this Menu and then return to it, whether or not the item that had been selected will still be selected. |
| **IsActive**             | Whether this Menu is the currently active one.                |
| **Scroll Rect**          | Defaults to a ScrollRect attached to the Menu. If assigned, the Menu automatically scrolls to keep the highlighted item in view. |
| **Addable Item**         | If you can add new items to this Menu at runtime, this Selectable is used as the template. |

### Delegates

| Name        | Description                                                  |
| ----------- | ------------------------------------------------------------ |
| **OnClick** | Static event invoked whenever a menu item within any Menu is clicked. See Menu Action Event below. |

### Public Methods

| Name                 | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| **Initialize**       | Prepares this Menu's addable-item template, if any, for later use with **Add**. Automatically invoked by Menu Manager on Awake. |
| **Activate**         | Activates or deactivates this Menu, enabling or disabling its Selectables and, if now active, highlighting the appropriate one. Automatically invoked by Menu Manager when switching menus. |
| **HighlightNext**    | Moves the highlight to the next navigable Selectable, honoring **wrapNavigation**. |
| **HighlightPrevious** | Moves the highlight to the previous navigable Selectable, honoring **wrapNavigation**. |
| **TryGetItem**       | Search for a Selectable within this Menu by name, returning whether it was found. |
| **GetItem**          | Get the Selectable with the given name within this Menu, or null if not found. |
| **Add**              | Adds a new item to the Menu, instantiated from **Addable Item**. Overloads let you supply a name and label, a configuration callback, or a ready-made Selectable, and let you choose where it's inserted and whether it's scrolled to or highlighted. |
| **Remove**           | Removes a previously added item, by reference or by name.    |
| **Clear**            | Removes all previously added items from the Menu.             |

## Menu Action Event

Passed to Menu's static **OnClick** delegate whenever a menu item is clicked.

### Properties

| Name         | Description                              |
| ------------ | ----------------------------------------- |
| **item**     | The Selectable that was clicked.          |
| **itemName** | The name of the Selectable that was clicked. |
| **menuName** | The name of the Menu containing the item. |
| **menu**     | The Menu containing the item.             |

## Interactable Menu Item

Abstract base class for menu items that need to respond to Menu Input directly, beyond simple click handling — such as Cycle Selector and Text Area. While this GameObject is enabled and currently selected, input events from Menu Input are forwarded to the subclass's `OnInputEvent` method.

### Properties

| Name             | Description                                                          |
| ---------------- | ---------------------------------------------------------------------- |
| **IsSelected**   | Whether this menu item is the current EventSystem selection.          |

## Cycle Selector

A Selectable-based menu item that lets the user choose an option from a fixed list of strings, cycling through them via Previous/Next buttons or left/right navigation.

### Properties

| Name                    | Description                                                  |
| ----------------------- | ------------------------------------------------------------ |
| **Items**               | The options within the Cycle Selector.                        |
| **Index**               | The currently selected option, by index. Setting it invokes **OnChange**. |
| **Value**               | The currently selected option, by string. Setting it selects the matching entry in **Items**, if any. |
| **CycleInteractable**   | Whether the Previous/Next cycle buttons are interactable.      |
| **Text**                | The Text UI element that displays the selected option. If left null, defaults to a Text component attached to this GameObject. |
| **Previous Button**     | If assigned, Cycle Selector sets up this button's OnClick event so you don't have to. Without a button assigned here, you can still navigate using a controller. |
| **Next Button**         | If assigned, Cycle Selector sets up this button's OnClick event so you don't have to. Without a button assigned here, you can still navigate using a controller. |

### Delegates

| Name         | Description                                            |
| ------------ | -------------------------------------------------------- |
| **OnChange** | Invoked whenever **Index** changes, passing the new **Value**. |

### Public Methods

| Name                        | Description                                                  |
| ---------------------------- | ------------------------------------------------------------ |
| **SetValueWithoutNotify**    | Sets **Value** without invoking **OnChange**.                 |
| **SelectPrevious**           | Selects the previous option, wrapping to the last if already at the first. |
| **SelectNext**               | Selects the next option, wrapping to the first if already at the last. |

## Text Area

A Selectable-based menu item that displays a scrollable block of text, requiring a ScrollRect component. You scroll through the text using a different directional input than the one used to navigate through the Menu — in a typical controller setup, the left analog stick or d-pad would navigate the menu, and the right analog stick would scroll the Text Area (via Menu Input's scroll action).

### Properties

| Name        | Description                          |
| ----------- | -------------------------------------- |
| **Speed**   | Scroll speed.                          |
| **Reverse** | Whether or not to invert the Y axis for the purpose of scrolling. |

## Menu Navigation Override

Optionally add a Menu Navigation Override to a menu item to override its Menu's **useNavigation** setting on a case-by-case basis.

### Properties

| Name             | Description                                                  |
| ---------------- | ------------------------------------------------------------ |
| **IsNavigable**  | Whether or not this menu item is included in navigation, regardless of its Menu's **useNavigation** setting. |
