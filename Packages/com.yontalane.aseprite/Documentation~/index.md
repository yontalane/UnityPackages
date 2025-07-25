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

| Name           | Description                                                  |
| -------------- | ------------------------------------------------------------ |
| **OnMotion**   | Event invoked when root motion data is received from the Aseprite animation. |
| **OnStart**    | Event invoked when an animation starts.                      |
| **OnComplete** | Event invoked when an animation completes.                   |

### Properties

| Name                 | Description                                        |
| -------------------- | -------------------------------------------------- |
| **Animator**         | The Animator component attached to the GameObject. |
| **CurrentAnimation** | Gets the name of the currently playing animation.  |

### Methods

| Name                    | Description                                                  |
| ----------------------- | ------------------------------------------------------------ |
| **TryGetAnimationClip** | Tries to get an animation clip with the specified name.      |
| **HasAnimation**        | Checks if the Animator has an animation with the specified name. |
| **TryPlay**             | Tries to play an animation with the specified name.          |
| **Play**                | Plays an animation with the specified name.                  |
