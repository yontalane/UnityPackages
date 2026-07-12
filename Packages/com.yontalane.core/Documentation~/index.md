# Core

Core is a grab-bag of small, mostly-independent utilities and components shared across Yontalane's other packages and sample projects: logging, math and collection helpers, audio and music playback, physics/collision helpers, UI feedback, and a few custom property drawers. Most entries below are self-contained—skim for whatever you need rather than reading top to bottom.

## Logger

A thin wrapper around `UnityEngine.Debug`'s logging methods. The **Log**/**Log Warning**/**Log Error** methods only run when the `ENABLE_LOGS` scripting define is set, so you can leave debug logging calls in your code and have them compile out of release builds without deleting them. The **Editor Log** variants instead only run inside the editor, regardless of any scripting defines.

### Static Methods

| Name                   | Description                                                  |
| ---------------------- | ------------------------------------------------------------ |
| **Log**                | Invoke UnityEngine.Debug.Log, but only if the conditional ENABLE_LOGS is active. |
| **Log Warning**        | Invoke UnityEngine.Debug.LogWarning, but only if the conditional ENABLE_LOGS is active. |
| **Log Error**          | Invoke UnityEngine.Debug.LogError, but only if the conditional ENABLE_LOGS is active. |
| **Editor Log**         | Invoke UnityEngine.Debug.Log, but only if in the editor. Note that outside the editor, this function still calls an empty method. |
| **Editor Log Warning** | Invoke UnityEngine.Debug.LogWarning, but only if in the editor. Note that outside the editor, this function still calls an empty method. |
| **Editor Log Error**   | Invoke UnityEngine.Debug.LogError, but only if in the editor. Note that outside the editor, this function still calls an empty method. |

## Math

A collection of small vector and angle utility methods that Unity doesn't provide out of the box: converting an angle to a direction vector, rotating a vector by radians or degrees, setting a single axis of a vector, and converting a float-based vector to its integer equivalent.

### Static Methods

| Name | Description |
| --- | ---|
| **Radian To Vector2** | Create a vector representation of an angle. |
| **Rotate By Radians** | Rotate a vector by radians. |
| **Rotate By Degrees** | Rotate a vector by degrees. |
| **Set X** | Set a single value of a vector. |
| **Set Y** | Set a single value of a vector. |
| **Set Z** | Set a single value of a vector. |
| **To Vector2 Int** | Convert a float-based vector to an integer-based vector. |
| **To Vector3 Int** | Convert a float-based vector to an integer-based vector. |

## Utility

Small helper methods that don't belong to any single component. **Highlight** lets you set a UI Selectable as the current EventSystem selection from code, and **Refresh Layout Groups Immediate And Recursive** works around a common Unity quirk where nested Vertical/Horizontal Layout Groups don't resize immediately after their content changes.

### Properties

| Name | Description |
| --- | ---|
| **Layer Count** | Gets the total number of sorting layers defined in the project. |

### Static Methods

| Name | Description |
| --- | ---|
| **Highlight** | Select the target Selectable. |
| **Refresh Layout Groups Immediate And Recursive** | Sometimes Vertical or Horizontal LayoutGroups don't adjust to match the size of their content. This method forces them to adjust. |

## List Extensions

Extension methods for `List<T>`.

### Static Methods

| Name | Description |
| --- | ---|
| **Shuffle** | Shuffles the list in place, into a random order. |

## Serializable Dictionary

A generic, ordered dictionary type that—unlike `System.Collections.Generic.Dictionary`—can be serialized and edited directly in the Inspector.

In order to be serialized in the Inspector, SerializableDictionary needs to be inherited and specifically called out as Serializable. For example:

```
[System.Serializable]
public class StringTextureDictionary : SerializableDictionary<string, Texture> { }

public StringTextureDictionary dict = new StringTextureDictionary();
```

### Properties

| Name | Description |
| --- | ---|
| **Count** | The number of key/value pairs in the dictionary. |

### Public Methods

| Name | Description |
| --- | ---|
| **Get Key At** | Get the key at the provided index. |
| **Get Value At** | Get the value at the provided index. |
| **Get At** | Get the key/value pair at the provided index. |
| **Try Get** | If the key exists, set the `out` parameter to the key's associated value and return true. Otherwise, return false. |
| **Get** | Return the value for the key. |
| **Add** | Add a key/value pair. |
| **Insert** | Insert a key/value pair at the provided index. |
| **Remove** | Remove the provided key and its associated value. |
| **Remove At** | Remove the key/value pair at the provided index. |
| **Clear** | Clear the dictionary. |
| **Set** | Set the value associated with the provided key. |
| **Set At** | Set the key/value pair at the provided index. |
| **Index Of** | If the dictionary contains the provided key, return its index; otherwise, return -1. |
| **Contains** | Return true if the dictionary contains the provided key. Otherwise, return false. |

## Serializable Key Value Pair

A serializable key/value pair returned by, and used to modify, a Serializable Dictionary entry—for example by its **Get At** and **Set At** methods.

### Properties

| Name | Description |
| --- | ---|
| **key** | The key. |
| **value** | The value. |

## Animation Event Transform

Lets Animation Events move, rotate, or scale the GameObject directly, without a dedicated script per animation. Attach it alongside an Animator, then call one of its methods from an Animation Event, passing the desired value as the event's string parameter.

### Public Methods

| Name                 | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| **Move**             | Translate the object based on data in the Animation Event. Expects a string parameter formatted as `##,##,##`. |
| **Set Position**     | Set the object's position based on data in the Animation Event. Expects a string parameter formatted as `##,##,##`. |
| **Set Euler Angles** | Set the object's rotation based on data in the Animation Event. Expects a string parameter formatted as `##,##,##`. |
| **Rotate**           | Rotate the object based on data in the Animation Event. Expects a string parameter formatted as `##,##,##`. Alternatively, can accept a float parameter—in which case, the object will rotate around the Y axis. |
| **Set Scale**        | Set the object's local scale based on data in the Animation Event. Expects a string parameter formatted as `##,##,##`. |

## Animation Event Broadcaster

Broadcasts animation events from an Animator to multiple listeners at once: a UnityEvent for wiring up in the Inspector, an instance-level delegate for local script subscriptions, and a static delegate for global subscriptions from anywhere in the project. Attach it to a GameObject with an Animator, then call its **Animation Event** method from an Animation Event.

### Delegates

| Name | Description |
| --- | ---|
| **OnAnimationEventLocal** | Instance event invoked when an animation event occurs on this GameObject. |
| **OnAnimationEvent** | Static event invoked when an animation event occurs on any GameObject with an Animation Event Broadcaster. |

### Public Methods

| Name | Description |
| --- | ---|
| **Animation Event** | Invokes all subscribed listeners—Inspector-configured, instance, and static—with the given AnimationEvent. Call this from an Animation Event. |

## Animation Sound Player

Plays an AudioClip or AudioPack from an Animation Event. Attach it to a GameObject, then call its **Play** method from an Animation Event, passing the AudioClip or AudioPack as the event's object reference parameter (and, optionally, a volume as the event's float parameter when passing an AudioClip).

### Public Methods

| Name | Description |
| --- | ---|
| **Play** | Plays the AudioClip or AudioPack referenced by the Animation Event's object reference parameter. |

## Audio Pack

A ScriptableObject that bundles a list of AudioClips with a shared volume, so a single asset can represent one logical sound—such as a footstep or hit—with several variations. Create one via **Assets > Create > Yontalane > AudioPack**, then play it with **Play** or **Try Play**. Playback happens through pooled, auto-managed Audio Pack Players, so you don't need to add an AudioSource of your own.

### Properties

| Name | Description |
| --- | ---|
| **clips** | The list of audio clips to play. |
| **volume** | The volume of the audio clips. |

### Public Methods

| Name | Description |
| --- | ---|
| **Can Play** | Returns whether this pack has any clips to play. |
| **Play** (instance) | Plays a random clip from the pack, or, given an index, plays the clip at that index. |

### Static Methods

| Name | Description |
| --- | ---|
| **Try Play** | Tries to play the given AudioPack, returning whether it played. |
| **Play** (AudioPack) | Plays the given AudioPack. |
| **Play** (AudioClip) | Plays the given AudioClip through a pooled Audio Pack Player, with an optional volume. |

## Audio Pack Player

The pooled component that Audio Pack uses internally to play a single clip and then return itself to the pool once playback finishes. You generally don't need to interact with this directly—Audio Pack manages it for you.

### Public Methods

| Name | Description |
| --- | ---|
| **Initiate** | Configures this player's AudioSource with the given clip and volume, plays it, and invokes the given callback once playback finishes. |

## Singleton

A base class for creating simple singleton components. Deriving `MyManager : Singleton<MyManager>` gives you a static **Instance** property that's set to `this` on Awake. Note that `Singleton<T>` doesn't guard against or clean up duplicate instances itself—subclasses that need that behavior (such as Music Manager) handle it in their own Awake override.

### Properties

| Name | Description |
| --- | ---|
| **Instance** | The singleton instance of the component, set on Awake. |

## Optional Singleton

Like Singleton, but lets each instance decide for itself—via an abstract `IsSingleton` property that subclasses must implement—whether it should register itself as the singleton **Instance**. Useful when a shared `Instance` reference should exist, but not every instance of the component in the scene should claim it.

### Properties

| Name | Description |
| --- | ---|
| **Instance** | The singleton instance of the component, set by whichever instance's `IsSingleton` returns true. |

## Music Manager

A persistent, cross-scene background music system. Play, stop, or cross-fade AudioClips (or AudioPacks) from anywhere via static methods, with no need to find or reference a specific GameObject. Music Manager creates its own persistent instance the first time it's used, survives scene loads, and manages two internal AudioSources so it can cross-fade between whatever is currently playing and whatever plays next.

### Properties

| Name | Description |
| --- | ---|
| **GlobalVolume** | The volume that all music played through Music Manager is scaled by. Defaults to 1. |
| **MusicVolume** | The volume of the currently playing clip, before **GlobalVolume** scaling. |
| **CurrentVolume** | The volume of the currently playing clip, after **GlobalVolume** scaling (**MusicVolume** × **GlobalVolume**). |
| **IsOn** | Whether Music Manager will play music at all. Setting this to false stops any currently playing music. |
| **CurrentSource** | The Music Source that started the currently playing clip, if any. |
| **CurrentClip** | The currently playing AudioClip, or null if nothing is playing. |
| **CurrentClipName** | The name of the currently playing AudioClip, or an empty string. |
| **IsPlaying** | Whether music is currently playing. |
| **IsFadingOut** | Whether a fade-out is currently in progress. |

### Static Methods

| Name | Description |
| --- | ---|
| **Play** (AudioClip) | Plays the given AudioClip, with an optional volume scale and fade-in. |
| **Play** (AudioPack) | Plays a random clip from the given AudioPack, with an optional fade-in. |
| **Stop** | Stops the currently playing music, with an optional fade-out. |

## Music Source

A component you place in a scene to play background music through Music Manager, typically on scene start. Because Music Manager persists across scene loads, Music Source lets you declare "this scene's music" without worrying about whether music is already playing from a previous scene.

### Properties

| Name | Description |
| --- | ---|
| **Clip** | The AudioClip assigned to this persistent music source. |
| **volume** | The playback volume for this music source (0 to 1). |
| **playSetting** | Specifies when and how the music should play when this GameObject starts. See Play Setting Options below. |
| **stopOnDestroy** | If true, stops the music when this source is destroyed, if it's the currently active source. |

### Play Setting Options

| Name | Description |
| --- | ---|
| **Don't Play** | No music will be played automatically. |
| **Play On Start** | Play music on start, regardless of existing music. |
| **Play On Start If No Music Is Playing** | Play music on start only if no music is currently playing. |
| **Fade In On Start** | Fade in music on start, regardless of existing music. |
| **Fade In On Start If No Music Is Playing** | Fade in music on start only if no music is currently playing. |
| **Fade In On Start If Music Is Playing, Otherwise Start Immediately** | Fade in on start only if music is already playing or fading out; otherwise, start immediately without a fade. |

### Public Methods

| Name | Description |
| --- | ---|
| **Play** | Plays this source's clip via Music Manager, fading in if indicated by its Play Setting. |

## Remote Music Player

Downloads and plays one or more audio tracks from URLs via `UnityWebRequest`, rather than requiring the AudioClips to be bundled with the project. Useful for music or audio that's fetched at runtime instead of shipped in the build.

### Properties

| Name | Description |
| --- | ---|
| **tracks** | The list of tracks to be played. See Remote Music Player Track below. |
| **autoPlay** | If true, playback starts automatically on Start. |
| **MusicVolume** | Gets the volume of the music. Override in a subclass to hook into your own volume system. Defaults to 1. |
| **MusicOn** | Gets whether music is enabled. Override in a subclass to hook into your own settings system. Defaults to true. |

### Public Methods

| Name | Description |
| --- | ---|
| **Play** | Stops any current playback, then downloads and plays each track. |
| **Stop** | Stops all playback and cleans up any runtime-created AudioSources. |

## Remote Music Player Track

A single track entry for a Remote Music Player: its URL, playback settings, and, optionally, which AudioSource should play it.

### Properties

| Name | Description |
| --- | ---|
| **url** | The URL of the audio file to play. |
| **volume** | The volume of the audio file. |
| **loop** | Should the track loop when finished? |
| **audioSource** | The AudioSource to play this track. If unassigned, a new AudioSource is created automatically. |

## Character Bump Controller

Adding this component to a game object with a Character Controller allows the Character Controller to push other objects that use the Rigidbody physics system. (Ordinarily, the Character Controller does not interact with Unity's physics.)

### Properties

| Name     | Description                                                  |
| -------- | ------------------------------------------------------------ |
| **Mass** | Used for scaling force created by collisions between this object and a Rigidbody. |

## Bump Listener

Listens for physical collisions—and, optionally, CharacterController bumps via a Character Bump Controller—and classifies each one as either a "land" (mostly downward impact) or a "bump" (mostly sideways impact), invoking a distinct event and optionally playing a distinct sound for each.

### Properties

| Name                                     | Description                                                  |
| ---------------------------------------- | ------------------------------------------------------------ |
| **On Land**                              | Info for handling bottom collisions.                         |
| **On Bump**                              | Info for handling side collisions.                           |
| **Listen for Character Controller Bump** | Whether or not to listen for collisions produced by Character Controllers. |
| **On Character Controller Bump**         | Info for handling collisions produced by Character Controllers. (Relies on Character Bump Controller.) |

## Bump Info

The event configuration used by Bump Listener for each of its three event categories (on land, on bump, and on Character Controller bump). A collision must match the layer mask and meet a minimum velocity before the associated event and sound are triggered.

### Properties

| Name                  | Description                                     |
| --------------------- | ----------------------------------------------- |
| **Layer Mask**        | Collisions are filtered by this layer mask.     |
| **Required Velocity** | Ignore collisions with less velocity than this. |
| **On Collision**      | Event to invoke on collision.                   |
| **Audio Clip**        | Sound to play on collision.                     |

## Clamp Attribute

Can be used with Float Range or Int Range. Similar to Range Attribute on a standard float or int. Clamps the allowed upper and lower limits of the field.

```c#
[Clamp(0f, 10f)]
public FloatRange damage;

[Clamp(0, 100)]
public IntRange spawnCount;
```

### Properties

| Name    | Description                          |
| ------- | ------------------------------------ |
| **min** | The allowed lower limit input value. |
| **max** | The allowed upper limit input value. |

## Layer Attribute

A property attribute that displays a Layer dropdown menu in the Inspector, instead of a plain input field. Apply it to an `int` field that should store a Unity layer index.

```c#
[Layer]
public int targetLayer;
```

## Tag Attribute

A property attribute that displays a Tag dropdown menu in the Inspector, instead of a plain input field. Apply it to an `int` or `string` field that should store a Unity tag.

```c#
[Tag]
public string targetTag;
```

## Collision Listener

Add this to an object to allow other objects to listen for its collision events.

### Properties

| Name       | Description                                                  |
| ---------- | ------------------------------------------------------------ |
| **filter** | For any event to fire, the colliding object must have a layer that falls within this Layer Mask. |

### Delegates

| Name | Description |
| --- | ---|
| **OnCollision** | Invokes when entering or exiting collision. Passes the state (enter or exit) and Collision object. |
| **OnCollision2D** | Invokes when entering or exiting 2D collision. Passes the state (enter or exit) and Collision2D object. |
| **OnTrigger** | Invokes when entering or exiting trigger. Passes the state (enter or exit) and other Collider. |
| **OnTrigger2D** | Invokes when entering or exiting 2D trigger. Passes the state (enter or exit) and other Collider2D. |
| **OnControllerHit** | Invokes when the Character Controller is hit. Passes the other Collider. |

## Float Range

A serializable min/max range of float values, useful for exposing a tunable range (such as damage, delay, or speed) in the Inspector. Provides the midpoint, a random value within the range, and linear interpolation between the two bounds.

### Properties

| Name | Description |
| --- | ---|
| **min** | One of the two values in the range. |
| **max** | One of the two values in the range. |
| **Mid** | The midpoint between the range's min and max values. |
| **Random** | A random float between the range's min and max values. |

### Constructors

| Name | Description |
| --- | ---|
| **Float Range** | Creates a new Float Range. |

### Inherited Members
### Public Methods

| Name | Description |
| --- | ---|
| **To String** | Returns the min and max value in the form of a string. |

## Int Range

A serializable min/max range of int values, useful for exposing a tunable range (such as a spawn count or ammo capacity) in the Inspector. Provides the midpoint, a random value within the range, and linear interpolation between the two bounds, all rounded to the nearest integer.

### Properties

| Name | Description |
| --- | ---|
| **min** | One of the two values in the range. |
| **max** | One of the two values in the range. |
| **Mid** | The midpoint (rounded to the nearest integer) between the range's min and max values. |
| **Random** | A random integer between the range's min and max values. |

### Constructors

| Name | Description |
| --- | ---|
| **Int Range** | Creates a new Int Range. |

### Inherited Members
### Public Methods

| Name | Description |
| --- | ---|
| **To String** | Returns the min and max value in the form of a string. |

## Leash Transform

Keeps this GameObject's position, rotation, and/or scale following a target Transform, with each property leashed independently and configurably (world/local/parent space, smoothing, and slack distance before the leash engages, plus optional bounds clamping for position). Useful for cameras, UI elements, or attachments that should track a target without being rigidly parented to it.

### Properties

| Name               | Description                                                  |
| ------------------ | ------------------------------------------------------------ |
| **target**         | What is this object being leashed to?                        |
| **positionConfig** | Configuration for leashed position.                          |
| **rotationConfig** | Configuration for leashed rotation.                          |
| **scaleConfig**    | Configuration for leashed scale (local space only).          |
| **updateType**     | How frequently to update the leashed object's position.      |
| **useRigidbody**   | Whether to leash via the Move method on a Rigidbody, assuming a Rigidbody is present. Affects position and rotation, but not scale. |
| **Rigidbody**      | The Rigidbody attached to this GameObject, if any. Set automatically on Initialize. |
| **useBounds**      | Whether to bound positional leashing to a fixed world-space area. |
| **bounds**         | If **useBounds** is true, positional leashing stops the leashed object from following the target outside these world bounds. |

### Public Methods

| Name           | Description                                                  |
| -------------- | ------------------------------------------------------------ |
| **Initialize** | Initialize or reset Leash Transform. Automatically invoked on Start. |

## Leash Transform Config

Per-property (position, rotation, or scale) settings used by Leash Transform, controlling whether that property is leashed, its offset from the target, the space it's leashed in, how much slack to allow before leashing kicks in, and how smoothly it catches up.

### Properties

| Name            | Description                                                  |
| --------------- | ------------------------------------------------------------ |
| **shouldLeash** | Should this transformation be leashed?                       |
| **offset**      | Should the offset between the target object and the leashed object be determined by their starting transformations or should it be manually set? |
| **offsetValue** | The offset between the target object and the leashed object. |
| **space**       | Should the leashing use world space, local space (least common), or parent space? |
| **slack**       | If the objects are this close, don't leash.                  |
| **smoothDamp**  | Should the leashed object snap to the desired transformation or should it move there smoothly? Set this to zero for snapping. |

## Selectable Listener

Plays an audio clip and invokes an event whenever the attached Selectable is selected—via UI navigation or pointer hover—letting you add UI feedback sounds without wiring up a dedicated script per button.

### Properties

| Name                  | Description                                         |
| --------------------- | --------------------------------------------------- |
| **onChangeSelection** | Trigger this event when the Selectable is selected. |
| **clip**              | Play this sound when the Selectable is selected.    |
| **volume**            | If playing a sound, use this volume.                |

## Animation Fixer

Editor window for fixing broken property paths in AnimationClips. Open it via **Window > Yontalane > Animation Fixer**.

If the Animation window is open and targeting a clip, Animation Fixer operates on that clip. Otherwise, it operates on all AnimationClips currently selected in the Project pane. If neither applies, its controls are disabled.

### Controls

| Name | Description |
| --- | ---|
| **Automatic Fix** | Attempts to resolve broken property paths by matching them against the hierarchy of the object targeted by the Animation window. Only enabled when the Animation window is targeting a clip on a live GameObject; paths with no unambiguous match are left untouched. |
| **Find** | Text to search for within each curve's property path. |
| **New** | Replacement text used by Replace, or the text prepended by Prepend. |
| **Replace** | Replaces every occurrence of Find with New in the targeted clip(s)' property paths. |
| **Prepend** | Inserts New at the very start of every property path in the targeted clip(s), ignoring Find. |
