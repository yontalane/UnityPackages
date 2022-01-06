# Menus

The Menus library provides a cross-platform game menu system that supports controller as well as keyboard and mouse. The layout of multiple menus (e.g. an interconnected main menu, about screen, and settings screen)--as well as navigation bridges between those menus (a "Settings" button in the main menu and a "Back" button in the Settings menu)--can be set up entirely in the Hierarchy and Inspector. An event system allows for easy access to menu events within script.

In this package, a "menu" is a list of items or controls. A common game start flow might contain a main menu, an about screen, a settings screen, an a credits screen. Each of these four screens is a single menu.

A "menu item" is an individual, selectable item within a menu. It is most commonly a button, though it might also be a slider or toggle (in a settings menu) or a scrollable text box (in an about screen).

**Note:** Only one menu, and only one menu item, can be active at a time.

## Menu Manager

Place a MenuManager on a GameObject, beneath which you will place all of your interconnected menus.

### Variables

* **`Menu[]` Menus:** You can assign menus to the `Menus` array. If you don't do this, MenuManager will attempt to auto-fill the array at runtime.
* **`int` ActiveMenu:** The `ActiveMenu` value determines the initially active menu.
* **`struct` Connections:** The `Connections` array is how you manage navigation between menus. Each entry in the array contains a `SourceButton`, a `SourceMenu`, and a `TargetMenu`. When the user clicks the source button on the source menu, the MenuManager will active the target menu. For example, to handle a connection from a main menu to an about screen, you might have:

```
SourceButton = "About Button"
SourceMenu = "Main Menu"
TargetMenu = "About Menu"
```

## Menu Input

When you add a MenuManager to a GameObject, a MenuInput will automatically be added as well.

### Variables

* **`bool` AllowHoldDownMove:** If this is set to true, you can hold down a navigation button to continually select the next item in the Menu. Otherwise, you need to repeatedly press the navigation button.

### Input Functions

MenuInput doesn't detect input entirely on its own; you have to use an event system to trigger its input listeners. The listener functions are as follows:

* OnMove()
* OnSubmit()
* OnCancel()
* OnScroll()
* OnGamepadFaceButtonNorth()
* OnGamepadFaceButtonEast()
* OnGamepadFaceButtonSouth()
* OnGamepadFaceButtonWest()
* OnGamepadShoulderLeft()
* OnGamepadShoulderRight()
* OnGamepadStart()
* OnGamepadSelect()

It's essential that you call OnMove(), OnSubmit(), and OnCancel(). The rest are optional, depending on your menu's functionality.

The intended usage is that you call these functions using a PlayerInput component or an InputActionAsset; keep in mind that both require appropriately named mappings. Or you can call the functions from your own C# script.

## Animator

You have the option to add an Animator to a MenuManager GameObject. If you do, whenever the MenuManager activates a new menu, it will set the an integer parameter `Active Menu` in the Animator to the index of the new menu.

## Menu

Place a Menu on a GameObject. That GameObject needs to contain one or more Selectable objects and must be a descendent of a MenuManager.

**Note:** Menu is generally assumed to be oriented vertically.

### Variables

* **`Selectable[]` Selectables:** You can assign menu items to this array. If you don't assign them, Menu will attempt to auto-fill the array at runtime.
* **`int` ActiveSelectable:** The initially selected Selectable.
* **`bool` UseNavigation:** Whether or not to use navigation (joystick or arrow keys to move between menu items).
* **`bool` WrapNavigation:** Whether or not to wrap navigation. If wrapping is on, clicking next while selecting the last item moves your selection to the first item. Otherwise, it will do nothing.
* **`bool` RememberHighlight:** If you exit a Menu and then return to it, whether or not the item that had been selected will still be selected.

## Selectable

Menu items all inherit from `UnityEngine.UI.Selectable`. Any Selectable--or anything that inherits from Selectable--that is added to a Menu component's `Selectables` array is a menu item. Buttons and other common Unity UI elements inherit from Selectable. In addition, a couple proprietary Selectable-based classes are built into this package:

### Cycle Selector

The CycleSelector allows you to choose an option from a list.

### Variables

* **`string[]` Items:** The options within the CycleSelector.
* **`int` Index:** The initially selected option.
* **`Text` Text:** The Text UI element that displays the selected option. If left null, defaults to a Text component attached to the CycleSelector GameObject.
* **`Button` PreviousButton:** If you assign a button to this field, CycleSelector will set up the OnClick event so you don't have to. Without a button assigned here, you can still navigate using a controller.
* **`Button` NextButton:** If you assign a button to this field, CycleSelector will set up the OnClick event so you don't have to. Without a button assigned here, you can still navigate using a controller.

### Text Area

The TextArea displays a scrollable block of text. You scroll through the text using a different directional input than the one used to navigate through the menu. (In a typical controller setup, the left analog stick or d-pad would be used to navigate the menu, and the right analog stick would be used to scroll the text.)

### Variables

* **`float` ScrollSpeed:** The speed of scrolling.
* **`bool` Reverse:** Whether or not to invert the Y axis for the purpose of scrolling.

## Menu Navigation Override

A MenuNavigationOverride can optionally be added to a menu item. This component allows you to override the Menu's `UseNavigation` setting on a case-by-case basis.

### Variables

* **`bool` IsNavigable:** Whether or not to use navigation for this menu item.
