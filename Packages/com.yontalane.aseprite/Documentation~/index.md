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

(An AsepriteAnimationBridge component is added to the imported Aseprite file, and it's this component, specifically, that broadcasts `OnMotion`.)

## Bridge

An AsepriteAnimationBridge component is added to the imported Aseprite file.

The Bridge broadcasts the following events:

* OnStart: with string parameter animationName and bool parameter isLooping
* OnComplete: with string parameter animationName and bool parameter isLooping
* OnMotion: with Vector2 parameter motion

The Bridge's other features include a Play() method, allowing you to play an animation by name, and a CurrentAnimation property, allowing you to get the name of the currently playing animation.

A complete list of methods and properties:

* string CurrentAnimation: A read-only string denoting the name of the active animation.
* bool TryGetAnimationClip(string animationName, out AnimationClip animationClip): Returns the animation clip with the given name if it exists.
* bool HasClip(string animationName): Returns true if the animator contains an animation with the given name.
* bool TryPlay(string animationName): Plays the animation if it exists. Returns false if the animation doesn't exist.
* void Play(string animationName): Plays the animation if it exists.