# Dialog

A dialog system. This incorporates branching dialog functionality; dialog display UI is handled through a separate package.

## Structure

For the dialog system to work, you need two singletons in your scene: DialogProcessor and DialogUI. DialogProcessor manages the overall flow; it handles on-the-fly changes to dialog text, it manages branching dialog and stored variables, it manages player response input, and it provides callbacks to external functions depending on the state of your dialog tree. DialogUI handles the visual representation of the dialog: It works with an Animator to show and hide your text box, it plays sounds, it allows you to control whether letters type out in sequence or display all at once, et cetera.

(As mentioned above, note that **dialog display UI is handloed through a separate package**.)

You also need one or more DialogAgent objects. The DialogAgent initiates dialog, and it stores the dialog's text script, formatted as a DialogData object. If you're making an RPG and you can talk to various NPCs, each NPC might contain a DialogAgent component. Alternatively, you might have a singleton DialogAgent and switch its script on the fly.

Finally, you have the option to include IDialogResponder objects in your scene. IDialogResponder works with the DialogProcessor and can modify dialog text or run actions based on keywords within the text. DialogAgent inherits from IDialogResponder, but non-agents can be responders as well. For example, a line of dialog might read: `My name is <<speaker>> and I see that you have a lot of <<most common item>>.` The DialogProcessor takes each keyword--designated by double angle brackets--and checks how they should be modified by each IDialogResponder. The DialogAgent, which is a type of responder, replaces `<<speaker>>` with its name. And an inventory manager singleton that also inherits from IDialogResponder might replace `<<most common item>>` with whatever item the player has the most of. So, after processing, the dialog could read: `My name is Lulu and I see that you have a lot of fish.`

Another DialogResponder feature is the ability to replace pieces of text with inline images. Add entries to the `Inline Image Replacement Info` list on a DialogAgent or other responder. For example, you might have an entry where the text is `%S` and the image is an icon of the sun. Now, anywhere in your dialog where `%S` appears, it will be replaced with the sun.

## Dialog Script Format

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
* string **typingLoop**: The name of the sound effect to loop while the characters are typed out.
* string **sound**: The name of the sound to play when the line begins.
* string **voice**: The name of the voice line audio clip to play.
* string **link**: The name of the next dialog node to go to when this one is finished.
* ResponseData[] **responses**: The player's response options. Each ResponseData object contains **text** (the text of the response) and **link** (the dialog node to go to if this response is chosen).
* string **data**: Optional data to be used in custom code.

LineData is sometimes purely functional, almost acting as lines of code. Functional LineData won't use `speaker` or `text`, but instead will make use of the following:

* string **ifDialogCount**: If you have spoken to this DialogAgent *x* times. The value of this string takes the format [operator][number], e.g. `>2`.
* string **ifFunction**: Send a query to all your IDialogResponder objects. Takes the format [query name]::[parameter]=[desired result], e.g. `Possesses::Apple=true`.
  * `ifFunction` also supports basic mathematical comparisons. For example, you can say: `Compare::AppleCount>3=true`. Either operand (in this case `AppleCount` and `3`) can be a defined variable or a literal value. Accepted operators are `==`, `!=`, `<=`, `>=`, `<`. and `>`. You can call `Compare` for float values and `CompareInt` for integer values.
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
  * `callFunction` also supports basic arithmetic. For example, you can say: `Math::AppleCount=AppleCount+3`. The item on the left side of the equal sign (`AppleCount`) is a variable. The two operands on the right side of the equal sign (`AppleCount` and `3`) can be defined variables or literal values. Accepted operators are `=`, `+`, `-`, `*`, `/`. `^`. and `%`. You can call `Math` for float values and `MathInt` for integer values.
  * The example above also supports the notation `Math::AppleCount+=3`. All operators are accepted.

* string **lineBuilderFunction**: Calls a function in your IDialogResponder objects. The IDialogResponder can build and return new LineData, thus changing the script on the fly.

The demo scene embedded in this package includes dialog data that shows all of this functionality in action.

### Simple Text Format

The DialogData object has an alternate simplified text format that is not JSON. Although it's not quite as versatile as the JSON format, you can still do a lot with it, and it's easier to read and understand.

It looks like this:

```
// This is a comment.

// This indicates the starting node:
==> Basic Convo

// This is a node:
#Basic Convo

  // ifVar is IF: VAR = VAL => NODE
  IF: talked to=true => Already Talked
  
  // setVar is SET: VAR = VAL
  SET: talked to=true

  // Basic dialog is SPEAKER: TEXT
  // If you don't want a speaker, just start the line with a colon.
  Arm Dude: Hey there.

  // callFunction is DO: FUNC, PARAM
  DO: Cutscene, Arm Dude

  // portait can be set by SPEAKER [PORTRAIT]: TEXT
  Arm Dude [Arm Dude Contemplative]: I know what you're thinking.

  // Responses are - TEXT => NODE
  // An empty portrait [] is replaced by the speaker name text
  // Dialog supports rich text
  
  Arm Dude []: Yes, this is a metal arm. No, I am not "happy to see you."
    - That's <i>weird</i>, man. => Deep Convo
    - cya. => Done

#Deep Convo

  Arm Dude: Cold, bro. Cold.

  // A modal query popup is ?: TEXT && RESPONSE => NODE && RESPONSE => NODE...
  
  ?: Just leave? && Yes => Done && No => Mind

#Mind

  // You can still use double brackets.
  // |: creates a mid-text linebreak.
  
  <<player>>: Hey, look.|:Gotta speak my mind.

  Arm Dude: You're all right.

#Done

  // Using key/value pairs, you can populate the speaker brackets
  // with any or all of the following display data:
  // * Portrait graphic asset name
  // * Voice audio clip asset name
  // * Sound audio clip asset name
  // * Typing audio clip asset name
  // * Typing Loop audio clip asset name
  
  Arm Dude [Portrait = Arm Dude Tired, Voice = Allrighty, Sound = Bang, Typing = TelegraphClick, Typing Loop = TelegraphHum]: All righty.
  
  // This jumps to another node:
  => Done Part Two

#Done Part Two

  Arm Dude: Bye now.

#Already Talked

  // ifDialogCount is COUNT OPERATOR VAL => NODE
  COUNT > 3 => Other Already Talked

  // ifFunction is IF FUNCTION: FUNC, PARAM = RESULT => NODE
  // -- joins lines together. Use this to break up a single line.
  
  IF FUNCTION:
  -- Randomness, time = true =>
  -- Other Already Talked

  Arm Dude: Later.

#Other Already Talked

  Arm Dude: G'day.
```

## Data Storage

Dialog variables from `ifVar`, `setVar`, and `ifDialogCount` are stored behind the scenes by the DataStorage static class. These variables are universal; this means that if you `setVar` in one DialogAgent's dialog, and then check `ifVar` on the same variable name in another DialogAgent's dialog, you will get the result set in the initial script.

If you want to save and load the dialog state, use `DataStorage.ExportToJson()` and `DataStorage.ImportFromJson(string json)`.