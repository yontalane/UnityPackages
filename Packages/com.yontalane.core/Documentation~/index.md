# Core

## Logger

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

### Static Methods

| Name | Description |
| --- | ---|
| **Highlight** | Select the target Selectable. |
| **Refresh Layout Groups Immediate And Recursive** | Sometimes Vertical or Horizontal LayoutGroups don't adjust to match the size of their content. This method forces them to adjust. |

## Serializable Dictionary

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

### Properties

| Name | Description |
| --- | ---|
| **key** | The key. |
| **value** | The value. |

## Animation Event Transform

### Public Methods

| Name                 | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| **Move**             | Translate the object based on data in the Animation Event. Expects a string parameter formatted as `##,##,##`. |
| **Set Position**     | Set the object's position based on data in the Animation Event. Expects a string parameter formatted as `##,##,##`. |
| **Set Euler Angles** | Set the object's rotation based on data in the Animation Event. Expects a string parameter formatted as `##,##,##`. |
| **Rotate**           | Rotate the object based on data in the Animation Event. Expects a string parameter formatted as `##,##,##`. Alternatively, can accept a float parameter—in which case, the object will rotate around the Y axis. |
| **Set Scale**        | Set the object's local scale based on data in the Animation Event. Expects a string parameter formatted as `##,##,##`. |

## Character Bump Controller

Adding this component to a game object with a Character Controller allows the Character Controller to push other objects that use the Rigidbody physics system. (Ordinarily, the Character Controller does not interact with Unity's physics.)

### Properties

| Name     | Description                                                  |
| -------- | ------------------------------------------------------------ |
| **Mass** | Used for scaling force created by collisions between this object and a Rigidbody. |

## Bump Listener

### Properties

| Name                                     | Description                                                  |
| ---------------------------------------- | ------------------------------------------------------------ |
| **On Land**                              | Info for handling bottom collisions.                         |
| **On Bump**                              | Info for handling side collisions.                           |
| **Listen for Character Controller Bump** | Whether or not to listen for collisions produced by Character Controllers. |
| **On Character Controller Bump**         | Info for handling collisions produced by Character Controllers. (Relies on Character Bump Controller.) |

## Bump Info

### Properties

| Name                  | Description                                     |
| --------------------- | ----------------------------------------------- |
| **Layer Mask**        | Collisions are filtered by this layer mask.     |
| **Required Velocity** | Ignore collisions with less velocity than this. |
| **On Collision**      | Event to invoke on collision.                   |
| **Audio Clip**        | Sound to play on collision.                     |

## Clamp Attribute

Can be used with Float Range or Int Range. Similar to Range Attribute on a standard float or int. Clamps the allowed upper and lower limits of the field.

### Properties

| Name    | Description                          |
| ------- | ------------------------------------ |
| **min** | The allowed lower limit input value. |
| **max** | The allowed upper limit input value. |

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
| **Float Range** | Creates a new Int Range. |

### Inherited Members
### Public Methods

| Name | Description |
| --- | ---|
| **To String** | Returns the min and max value in the form of a string. |

## Leash Transform

### Properties

| Name               | Description                                                  |
| ------------------ | ------------------------------------------------------------ |
| **target**         | What is this object being leashed to?                        |
| **positionConfig** | Configuration for leashed position.                          |
| **rotationConfig** | Configuration for leashed rotation.                          |
| **scaleConfig**    | Configuration for leashed scale (local space only).          |
| **updateType**     | How frequently to update the leashed object's position.      |
| **useRigidbody**   | Whether to leash via the Move method on a Rigidbody, assuming a Rigidbody is present. Affects position and rotation, but not scale. |

### Public Methods

| Name           | Description                                                  |
| -------------- | ------------------------------------------------------------ |
| **Initialize** | Initialize or reset Leash Transform. Automatically invoked on Start. |

## Leash Transform Config

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

### Properties

| Name                  | Description                                         |
| --------------------- | --------------------------------------------------- |
| **onChangeSelection** | Trigger this event when the Selectable is selected. |
| **clip**              | Play this sound when the Selectable is selected.    |
| **volume**            | If playing a sound, use this volume.                |
