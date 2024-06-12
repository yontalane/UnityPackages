# Changelog

## 1.0.41 - 2024.06.12

**Added**

* Can set static text at runtime.

## 1.0.40 - 2024.06.12

**Added**

* Can manually clear dialog data in agent.

## 1.0.39 - 2024.06.10

**Added**

* Running portrait text through replaceInLineText().
* If simple-text portrait is [] then it is replaced by the speaker name.

## 1.0.38 - 2024.06.10

**Fixed**

* Response button navigation would sometimes break.

## 1.0.37 - 2024.06.09

**Added**

* Can set portrait from simple text.

## 1.0.36 - 2024.05.29

**Added**

* Can use custom event to set portrait.

## 1.0.35 - 2024.05.29

**Fixed**

* Response button rotation is zeroed out on instantiation.

## 1.0.34 - 2024.05.19

**Changed**

* Updated dependency versions.

## 1.0.33 - 2024.05.19

**Fixed**

* Fixed null reference in sample scene.

## 1.0.32 - 2024.05.17

**Changed**

* Updated dependency versions.

## 1.0.31 - 2024.04.30

**Fixed**

* Minor documentation fix.

## 1.0.30 - 2024.04.25

**Changed**

* Updated dependency versions.

## 1.0.29 - 2024.04.10

**Fixed**

* DO now works without a parameter.

## 1.0.28 - 2024.04.10

**Fixed**

* SetData() resets the current data.

## 1.0.27 - 2024.04.10

**Changed**

* SET uses equal sign instead of colon.

## 1.0.26 - 2024.04.10

**Changed**

* DO uses comma instead of colon to delineate between function and parameter.

## 1.0.25 - 2024.04.10

**Fixed**

* Multiple dialogs were all using the same script. Removed 1.0.23 addition, at least for now.

## 1.0.24 - 2024.04.10

**Added**

* Invoking events on start and end typing line.

## 1.0.23 - 2024.04.09

**Changed**

* Not re-parsing simple text data if already done.

## 1.0.22 - 2024.04.09

**Added**

* Documentation for speakerless text.

## 1.0.21 - 2024.04.09

**Fixed**

* Custom start node wasn't working.

## 1.0.20 - 2024.04.09

**Added**

* Documentation for simplified text data format.

## 1.0.19 - 2024.04.09

**Added**

* DialogAgent can accept a new, simplified text data format.

## 1.0.18 - 2024.04.03

**Added**

* DialogAgent Data can be set publically.

## 1.0.17 - 2023.09.05

**Added**

* More styling options for the dialog speaker.

## 1.0.16 - 2023.07.28

**Fixed**

* Set Editor AssemblyDef to target the editor only.

## 1.0.15 - 2023.05.08

**Changed**

* When writing out dialog, Dialog UI waits one frame before highlighting skip button.

## 1.0.14 - 2023.05.02

**Changed**

* Dialog Data text field is now a text area.

## 1.0.13 - 2023.04.30

**Added**

* Dialog Agent can accept data in additional formats.

## 1.0.12 - 2023.01.30

**Fixed**

* Null check for dialog function result.

## 1.0.11 - 2023.01.30

**Added**

* More speaker color options.

## 1.0.10 - 2023.01.19

**Fixed**

* Refresh layout groups not working on skip.

## 1.0.9 - 2023.01.19

**Added**

* Option to automatically refresh layout groups.

## 1.0.8 - 2023.01.19

**Added**

* Added option for line break after speaker name.

## 1.0.7 - 2023.01.19

**Added**

* You can customize the string that is displayed between the speaker's name and the dialog text.
* Added new line command: `exit`. Exits dialog when set to true.

## 1.0.6 - 2023.01.18

**Changed**

* DialogUI now has a static Instance accessor.
* DialogProcessor automatically calls Close() and InstantiateLine() in DialogUI.
* DialogProcessor will not try to process a line if NodeData is null.
* Add DisplayName field to DialogAgent.

## 1.0.5 - 2023.01.17

**Added**

* Added ScriptableObject variation of DialogAgent.

## 1.0.4 - 2023.01.17

**Changed**

* Changed abstract class DialogResponder to interface IDialogResponder.

## 1.0.3 - 2022.12.30

**Changed**

* Switched to Text Mesh Pro.

## 1.0.2 - 2022.01.15

## 1.0.1 - 2022.01.15

## 1.0.0 - 2022.01.05

**Added**

* Dialog
* DialogProcessor has public `KillDialog()` function.