# Dialog

A dialog system. This incorporates branching dialog functionality and dialog display UI.

## Structure

For the dialog system to work, you need two singletons in your scene: DialogProcessor and DialogUI. DialogProcessor manages the overall flow; it handles on-the-fly changes to dialog text, it manages branching dialog and stored variables, it manages player response input, and it provides callbacks to external functions depending on the state of your dialog tree. DialogUI handles the visual representation of the dialog: It works with an Animator to show and hide your text box, it plays sounds, it allows you to control whether letters type out in sequence or display all at once, et cetera.

You also need one or more DialogAgent objects. The DialogAgent initiates dialog, and it stores the dialog's text script, formatted as a DialogData object. If you're making an RPG and you can talk to various NPCs, each NPC might contain a DialogAgent component. Alternatively, you might have a singleton DialogAgent and switch its script on the fly.

Finally, you have the option to include IDialogResponder objects in your scene. IDialogResponder works with the DialogProcessor and can modify dialog text or run actions based on keywords within the text. DialogAgent inherits from IDialogResponder, but non-agents can be responders as well. For example, a line of dialog might read: `My name is <<speaker>> and I see that you have a lot of <<most common item>>.` The DialogProcessor takes each keyword--designated by double angle brackets--and checks how they should be modified by each IDialogResponder. The DialogAgent, which is a type of responder, replaces `<<speaker>>` with its name. And an inventory manager singleton that also inherits from IDialogResponder might replace `<<most common item>>` with whatever item the player has the most of. So, after processing, the dialog could read: `My name is Lulu and I see that you have a lot of fish.`

## Dialog Data Format

There are two types of DialogAgent: DialogAgent, that inherits from MonoBehaviour, and SerializedDialogAgent, that inherits from SerializedObject. You can use either in your project; both contain the same methods and functionality.

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
  * When creating the code for `ifFunction` in a DialogResponder, note that `DialogFunction()` returns true if it can process the request and false if it cannot. If `DialogFunction()` is able to process a request and determines that the result is false, then `result` will be set to false, but the function will return true. See the example below.


```c#
public bool DialogFunction(string call, string parameter, out string result)
{
  switch (call)
  {
    case "Possesses":
      result = (Inventory.Contains(parameter)).ToString();
      return true;
      break;
  }
  result = null;
  return false;
}
```

* string **ifVar**: Check whether the DialogProcessor has set a particular variable. [variable name]=[desired result], e.g. `joinedTheTeam=true`.
* bool **elseIf**: Setting this to true makes a LineData object act purely as an "else if" line in code, ignoring all other LineData fields. Must follow an `if` LineData object.
* bool **endIf**: Setting this to true makes a LineData object act purely as an "end if" line in code, ignoring all other LineData fields. Must follow an `if` or `elseIf` LineData object.
* bool **exit**: Setting this to true exits the dialog as soon as this line is reached. Other LineData fields are ignored.
* VarData **setVar**: VarData contains a **key** string and a **value** string. Have the DialogProcessor store a value.
* QueryData **query**: Displays a modal input dialog to the player. QueryData contains a **text** string (the input box's prompt) and a ResponseData array called **responses**.
* string **callFunction**: Calls a function in your IDialogResponder objects. Takes the format [function name]::[parameter], e.g. `GiveToPlayer::Apple`.

The demo scene embedded in this package includes dialog data that shows all of this functionality in action.

### Simple Text Format

The DialogData object has an alternate simplified text format that is not JSON. Although it's not quite as versatile as the JSON format, you can still do a lot with it, and it's easier to read and understand.

It looks like this:

```
//This is a comment.

//This indicates the starting node.
=> Basic Convo

//This is a node.
#Basic Convo

  //ifVar is IF:[var]=[val]=>[new node]
  IF: talked to=true => Already Talked
  
  //setVar is SET:[var]=[val]
  SET: talked to=true

  //Basic dialog is [speaker]:[text]
  //Text with no speaker is :[text]
  Arm Dude: Hey there.

  //callFunction is DO:[func],[param]
  DO: Cutscene, Arm Dude

  //portait can be set by [speaker] [[portrait]]: [dialog]
  Arm Dude [Arm Dude Contemplative]: I know what you're thinking.

  //responses are - [text]=>[new node]
  //an empty portrait [] is replaced by the speaker name text
  Arm Dude []: Yes, this is a metal arm. No, I am not "happy to see you."
    - That's weird, man. => Deep Convo
    - cya. => Done

#Deep Convo

  Arm Dude: Cold, bro. Cold.

  //You can still use double brackets.
  //|: creates a mid-text linebreak.
  <<player>>: Hey, look.|:Gotta speak my mind.

  Arm Dude: You're all right.

#Done

  Arm Dude: Bye now.

#Already Talked

  //ifDialogCount is COUNT[operator][val]=>[new node]
  COUNT > 3 => Other Already Talked

  //ifFunction is IF FUNCTION:[func],[param]=[result]=>[new node]
  IF FUNCTION: Randomness, time = true => Other Already Talked

  Arm Dude: Later.

#Other Already Talked

  Arm Dude: G'day.
```