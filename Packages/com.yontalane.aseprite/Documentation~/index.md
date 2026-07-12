# Aseprite

## Overview

Yontalane's Aseprite package extends Unity's 2D Aseprite Importer.

Adding the following layers in your Aseprite file will affect the import process:

* A layer with "collision" in its name.
* A layer with "trigger" in its name.
* A layer with "point" in its name.
* A layer with "root" in its name.

Upon import, these layers will be transformed into Unity objects.

Note that the layers **need to be marked visible**. However, their opacity can be set to zero.

## Collision

BoxCollider2D components will be generated for each **collision** layer—one collider for each layer. These colliders will be named the same as their source layer. The colliders' **offset** and **size** properties will animate to match the bounding box of the layer art. Its **enabled** state will animate as well; the collider will only be active on frames where its source layer contains art.

## Trigger

Identical to collision layers, except that the generated colliders will be marked as **triggers**.

## Point

Similar to collision layers, except the generated object is a GameObject with no extra components attached. The GameObject's **isActive** and **transform.localPosition** properties animate to match the center of its source layer's art.

## Root

This is a variation of the point layer that is modified to enable a type of [root motion](https://docs.unity3d.com/6000.1/Documentation/Manual/RootMotion.html).

Animate your character such that their position translates within Aseprite. Add a root layer, and, on each frame, draw a point that represents the character's **origin point on the ground**. Since the character is moving in Aseprite, this point should move as well.

Upon import into Unity, the animation will be adjusted such that the root point remains at 0,0 in local space. However, each frame at which the root would have moved, an `OnMotion` event is broadcast. You can have your game's character class listen for this event and move the character in world space.

(An AsepriteAnimationBridge component is added to the imported Aseprite file, and it is this component, specifically, that broadcasts `OnMotion`.)

## Aseprite Animation Bridge

The Aseprite Animation Bridge is a bridge between Aseprite animations and the Unity Animator. It is automatically added to the imported Aseprite file.

The Bridge facilitates interaction between Aseprite animations and Unity's Animator by handling root motion events, animation lifecycle events, and providing utility methods for animation control and querying.

### Delegates

| Name                          | Description                                                  |
| ----------------------------- | ------------------------------------------------------------ |
| **OnMotion**                  | Event invoked when root motion data is received from the Aseprite animation. |
| **OnStart**                   | Event invoked when an animation starts.                      |
| **OnComplete**                | Event invoked when an animation completes.                   |
| **OnRequestMotionTreeValue**  | Event invoked to request the current progress value (0 to 1) for whichever Motion Tree is currently playing. See [Motion Trees](#motion-trees) below. |

### Properties

| Name                       | Description                                                  |
| -------------------------- | ------------------------------------------------------------ |
| **Animator**                | The Animator component attached to the GameObject.          |
| **SpriteRenderer**           | The SpriteRenderer used to display the sprite.               |
| **Sprite**                   | The Sprite currently being rendered.                          |
| **Color**                    | The rendering color of the sprite.                            |
| **SortingOrder**             | The renderer's order within its sorting layer.                |
| **FlipX**                    | Whether the sprite is flipped on the X axis.                  |
| **FlipY**                    | Whether the sprite is flipped on the Y axis.                  |
| **CurrentAnimation**         | Gets the name of the currently playing animation.             |
| **Colliders**                | The BoxCollider2D components generated from Collision layers. |
| **Triggers**                 | The BoxCollider2D components generated from Trigger layers.   |
| **Points**                   | The Transforms generated from Point layers.                   |
| **SpriteObjectInfo**         | Metadata about the colliders, triggers, and points defined in Aseprite. See [Sprite Object Info](#sprite-object-info) below. |
| **AnimationLengths**         | The length, in seconds, of every animation on this GameObject. See [Sprite Object Info](#sprite-object-info) below. |
| **Extras**                   | Optional AsepriteAnimationExtra assets containing Motion Trees for this GameObject. See [Motion Trees](#motion-trees) below. |
| **SynchronizedAnimators**    | Additional Animators to play in sync with this GameObject's own Animator. |

### Methods

| Name                    | Description                                                  |
| ----------------------- | ------------------------------------------------------------ |
| **TryGetAnimationClip** | Tries to get an animation clip with the specified name.      |
| **HasAnimation**        | Checks if the Animator has an animation with the specified name. |
| **HasMotionTree**       | Checks if a Motion Tree with the specified name exists among this GameObject's Extras. |
| **TryGetMotionTree**    | Tries to get a Motion Tree with the specified name.           |
| **TryPlay**             | Tries to play an animation (or Motion Tree) with the specified name. |
| **TryPlayMotionTree**   | Tries to play the Motion Tree with the specified name.        |
| **Play**                | Plays an animation with the specified name.                   |
| **PlayMotionTree**      | Plays the Motion Tree with the specified name.                |
| **Stop**                | Stops the current animation playback.                        |

## Motion Trees

A Motion Tree lets a single animation "slot" blend between several AnimationClips based on an external value, similar in spirit to a 1D blend tree in Unity's Animator—think a walk cycle that should pick a different clip depending on the character's current speed. Rather than driving that value through the Animator's own parameters, `AsepriteAnimationBridge` asks your game code for it every frame via the `OnRequestMotionTreeValue` delegate.

To use Motion Trees:

1. Create an **AsepriteAnimationExtra** asset (**Assets > Create > Yontalane > Aseprite > Animation Extras**) and add one or more Motion Trees to it, each with an id and a list of animations (referenced either by AnimationClip or by name).
2. Assign the asset to the imported Aseprite file's `AsepriteAnimationBridge` component, in its **Extras** list.
3. Listen for `OnRequestMotionTreeValue` and populate the provided `KeyFloatPair`'s value (0 to 1) based on the tree's id.
4. Call `PlayMotionTree` (or `Play`/`TryPlay` with the tree's id) to start blending.

While a Motion Tree is playing, `AsepriteAnimationBridge` uses the requested value every frame to pick which clip in the tree to play and how far into it to seek, producing a smoothly blended result driven entirely by your own gameplay value.

## Aseprite Animation Extra

A ScriptableObject asset that holds the Motion Trees for one or more `AsepriteAnimationBridge` components.

### Properties

| Name             | Description                                             |
| ---------------- | -------------------------------------------------------- |
| **motionTrees**  | Array of Motion Trees associated with this asset.        |

## Motion Tree

### Properties

| Name       | Description                                        |
| ---------- | --------------------------------------------------- |
| **id**     | The identifier for this Motion Tree.                |
| **Count**  | The number of animations in this Motion Tree.       |

### Methods

| Name        | Description                                                  |
| ----------- | -------------------------------------------------------------- |
| **Get**     | Gets the AnimationClip at the specified index.                |
| **TryGet**  | Tries to get the AnimationClip at the specified index, returning whether it was found. |

## Sprite Object Info

On import, the Aseprite package records metadata about every Collision, Trigger, and Point layer, as well as the length of every animation, so your game code can query this information at runtime instead of hard-coding frame numbers or durations. This metadata is exposed via `AsepriteAnimationBridge`'s **SpriteObjectInfo** and **AnimationLengths** properties, and the `AsepriteExtensions` class provides helper methods for querying it.

For example, you can ask when, during a given animation, a particular collision layer is active—useful for enabling a hitbox only during specific frames of an attack animation.

### Properties

| Name              | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| **name**          | The name of the object (matches its source layer's name).    |
| **type**          | The type of the attached object (`Collider`, `Trigger`, or `Point`). |
| **animationInfo** | The list of per-animation activity data for this object.     |

## Sprite Object Animation Info

Contains per-animation data about a single sprite object—an entry from **Sprite Object Info**'s `animationInfo` list—including when it is active during that animation.

### Properties

| Name               | Description                                                  |
| ------------------ | ------------------------------------------------------------ |
| **animation**      | The animation's name.                                        |
| **length**         | The total frame count of the animation.                      |
| **duration**       | The length, in seconds, of the animation.                    |
| **framesOn**       | The list of frame indices at which the object is active.     |
| **timesOn**        | The list of times, in seconds, at which the object is active. |
| **FrameDuration**  | The length, in seconds, of a single frame.                    |

### Methods

| Name                    | Description                                                  |
| ----------------------- | ------------------------------------------------------------ |
| **GetKeyframePeriods**  | Populates a list with the time periods (in both frames and seconds) during which the object is active. |

## Activation Period

A start time and duration, counted both in seconds and frames. Returned by **Sprite Object Animation Info**'s `GetKeyframePeriods`.

### Properties

| Name           | Description                     |
| -------------- | -------------------------------- |
| **startTime**  | Start time, in seconds.          |
| **length**     | Duration, in seconds.            |
| **startFrame** | Start time, in frames.           |
| **frameCount** | Duration, in frames.             |
| **EndTime**    | End time, in seconds.            |
| **EndFrame**   | End time, in frames.             |

## Animation Length Info

The length of a single animation, in seconds. An entry in the list returned by `AsepriteAnimationBridge`'s **AnimationLengths** property.

### Properties

| Name       | Description                             |
| ---------- | ----------------------------------------- |
| **name**   | The animation's name.                     |
| **length** | The length, in seconds, of the animation. |

## Aseprite Extensions

Extension methods for querying lists of **Sprite Object Info** and **Animation Length Info**, as returned by `AsepriteAnimationBridge`'s **SpriteObjectInfo** and **AnimationLengths** properties.

### Methods

| Name                     | Description                                                  |
| ------------------------ | ------------------------------------------------------------ |
| **TryGetInfo**           | Tries to find a Sprite Object Info in a list by name.         |
| **GetInfo**              | Gets a Sprite Object Info from a list by name, or null if not found. |
| **TryGetLength**         | Tries to get the length of the animation with the given name from a list of Animation Length Info. |
| **TryGetAnimationInfo**  | Tries to find a Sprite Object Animation Info for a specific object and animation name. |
| **GetAnimationInfo**     | Gets a Sprite Object Animation Info for a specific object and animation name, or null if not found. |

## Settings

Yontalane's Aseprite settings are available under **Project Settings > Yontalane > Aseprite**. They control debug logging and the appearance of Scene view gizmos for colliders, triggers, and points.

### Properties

| Name              | Description                                     |
| ----------------- | ------------------------------------------------ |
| **debugSettings** | Debug logging settings. See Debug Settings below. |
| **gizmoInfo**     | Scene view gizmo appearance settings. See Gizmo Info below. |

## Debug Settings

### Properties

| Name       | Description                                     |
| ---------- | ------------------------------------------------ |
| **log**    | Enable or disable debug logging.                 |
| **filter** | Only log debug messages that contain this filter. |

## Gizmo Info

### Properties

| Name              | Description                          |
| ----------------- | -------------------------------------- |
| **colliderColor** | Color used to draw collider gizmos.    |
| **triggerColor**  | Color used to draw trigger gizmos.     |
| **pointColor**    | Color used to draw point gizmos.       |
| **pointRadius**   | Radius of the point gizmos.            |
