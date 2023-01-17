# Dialog

A dialog system. This incorporates branching dialog functionality and dialog display UI.

## Structure

For the dialog system to work, you need two singletons in your scene: DialogProcessor and DialogUI. DialogProcessor manages the overall flow; you use it to initiate dialog, it handles on-the-fly changes to dialog text, it manages branching dialog and stored variables, it manages player response input, and it provides callbacks to external functions depending on the state of your dialog tree. DialogUI handles the visual representation of the dialog: It works with an Animator to show and hide your text box, it plays sounds, it allows you to control whether letters type out in sequence or display all at once, et cetera.

You also need one or more DialogAgent objects. The DialogAgent stores the text script for the dialog, which is stored as a DialogData object. If you're making an RPG and you can talk to various NPCs, presumably each NPC would contain a DialogAgent component.

Finally, you have the option to include IDialogResponder objects in your scene. IDialogResponder works with the DialogProcessor and can modify dialog text or run actions based on keywords within the text. DialogAgent inherits from IDialogResponder, but non-agents can be responders as well. For example, a line of dialog might read: `My name is <<speaker>> and I see that you have a lot of <<most common item>>.` The DialogProcessor takes each keyword--designated by double angle brackets--and checks how they should be modified by each IDialogResponder. The DialogAgent, which is a type of responder, replaces `<<speaker>>` with its name. And an inventory manager singleton that also inherits from IDialogResponder might replace `<<most common item>>` with whatever item the player has the most of. So, after processing, the dialog could read: `My name is Lulu and I see that you have a lot of fish.`

## Dialog Data Format

Each DialogAgent has a DialogData object. DialogData, which is read in as JSON, is composed of multiple nodes that form together to establish a branching dialog tree. Each node has text data and information for which node to go to next. For example, in its first node, an NPC might ask you for your fish ("May I please have a fish?"). The DialogProcessor checks first if you have fish on you, and if you do, gives you an opportunity to respond "Yes" or "No." So you might have three nodes in total:


```
  "May I please have a fish?"

IF YOU HAVE A FISH AND SAY YES:

  "Thank you, this is the happiest day of my life! Here's something in return!" (PLAYER LOSES ONE FISH AND GAINS ONE JEWEL)

IF YOU HAVE A FISH AND SAY NO:

  "I understand. I am not worthy of your fish."

IF YOU DON'T HAVE A FISH:

  "You don't have a fish!? What a cruel world!"
```

DialogData contains an array of NodeData. NodeData contains an array of LineData, and LineData is where most of the dialog information is stored. (Because games often display dialog one line at a time, dialog is divided into discrete, line-by-line items rather than being a single, giant wall of text.)

LineData contains the following data for displaying dialog:

* string **speaker**: Game dialog is generally presented as a conversation between the player and an NPC. So for each line, we check if the speaker is the player or the DialogAgent (NPC). The dialog system also supports conversations involving multiple NPCs.
* string **text**: The line of dialog.
* string **portrait**: The name of the portrait graphic to display.
* string **typing**: The name of the sound effect to play as each character is typed out.
* string **sound**: The name of the sound to play when the line begins.
* string **voice**: The name of the voice line audio clip to play.
* string **link**: The name of the next dialog node to go to when this one is finished.
* ResponseData[] **responses**: The player's response options. Each ResponseData object contains **text** (the text of the response) and **link** (the dialog node to go to if this response is chosen).
* string **data**: Optional data to be used in custom code.

LineData is sometimes purely functional, almost acting as lines of code. Functional LineData won't use `speaker` or `text`, but instead will make use of the following:

* string **ifDialogCount**: If you have spoken to this DialogAgent *x* times. The value of this string takes the format [operator][number], e.g. `>2`.
* string **ifFunction**: Send a query to all your IDialogResponder objects. Takes the format [query name]::[parameter]=[desired result], e.g. `Possesses::Apple=true`.
* string **ifVar**: Check whether the DialogProcessor has set a particular variable. [variable name]=[desired result], e.g. `joinedTheTeam=true`.
* bool **elseIf**: Setting this to true makes a LineData object act purely as an "else if" line in code, ignoring all other LineData fields. Must follow an `if` LineData object.
* bool **endIf**: Setting this to true makes a LineData object at purely as an "end if" line in code, ignoring all other LineData fields. Must follow an `if` or `elseIf` LineData object.
* VarData **setVar**: VarData contains a **key** string and a **value** string. Have the DialogProcessor store a value.
* QueryData **query**: Displays a modal input dialog to the player. QueryData contains a **text** string (the input box's prompt) and a ResponseData array called **responses**.
* string **callFunction**: Calls a function in your IDialogResponder objects. Takes the format [function name]::[parameter], e.g. `GiveToPlayer::Apple`.

The demo scene embedded in this package includes dialog data that shows all of this functionality in action.