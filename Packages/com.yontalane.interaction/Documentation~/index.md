# Interaction

Yontalane's Interaction package is a small system for proximity-based interaction in 3D games—opening doors, pulling levers, picking up objects, talking to NPCs, and the like. An `Interactor` (typically attached to the player) detects nearby `Interactable` objects by raycasting; each `Interactable` exposes one or more `InteractionBase` behaviors, keyed by an interaction type string, that define what actually happens when an interaction is triggered.

To use this package:

* Add an **Interactor** component to whichever object should be able to interact with the world (typically the player).
* Add an **Interactable** component to any object that can be interacted with, and assign its root, renderer, animator, and audio source references.
* Add one or more components derived from **InteractionBase**—such as the included **InteractionSimple** or **InteractionCarry**—as children of the interactable's root. Each defines an `interactionType` string (e.g. `"Action"`, `"Carry"`).
* When you want to attempt an interaction (e.g. on a button press), call the Interactor's `TryDoInteraction` method, passing the interaction type you're looking for and an `InteractionInfo` struct describing the interactor.

```c#
private void Update()
{
    if (!Input.GetButtonDown("Interact"))
    {
        return;
    }

    InteractionInfo info = new InteractionInfo()
    {
        interactor = myInteractor,
        rootTransform = transform,
        animator = myAnimator
    };

    myInteractor.TryDoInteraction("Action", info);
}
```

Some interactions need mid-animation feedback—for example, a lever-pull animation that should only open a door once the pull finishes. Call the Interactor's `DoEvent` method (commonly wired up as an Animation Event) to signal this to whichever interaction is currently active.

## Interaction Info

A block of data describing the interactor, passed to an Interactable when an interaction is attempted.

### Properties

| Name | Description |
| --- | --- |
| **interactor** | The interactor involved in the interaction. |
| **rootTransform** | The interactor's root transform. Required if the interaction needs to move the interactor. |
| **animator** | An animator attached to the interactor. Required if the interaction needs to play an animation. |
| **data** | A generic data object to be used for interaction special cases. |

## Interactor

Handles detection of nearby Interactable objects, via a spread of raycasts projected from one or more heights in front of the interactor, and lets you attempt interactions with whichever is currently detected.

### Properties

| Name | Description |
| --- | --- |
| **IsLocked** | A locked interactor will not gain or lose a current interactable. |

### Methods

| Name | Description |
| --- | --- |
| **CheckNow** | Checks whether an interactable is in proximity, updating the current interactable and firing enter/exit events as needed. Called automatically every `LateUpdate`. |
| **TryDoInteraction** | If an interactable is in proximity and responds to the given interaction type, initiates an interaction on it. |
| **DoEvent** | Forwards an event (e.g. from an Animation Event) to the current interactable's active interaction. |
| **TryGetInteractable** | Gets the current interactable, if one is in proximity or locked. |

### Delegates

| Name | Description |
| --- | --- |
| **InteractorEventHandler** | Signature for the `OnInteractableEnter` and `OnInteractableExit` events. |

### Events

| Name | Description |
| --- | --- |
| **OnInteractableEnter** | Invoked when the interactor detects an interactable. |
| **OnInteractableExit** | Invoked when the interactor stops detecting an interactable. |

## Interactable

Represents an object in the scene that can be interacted with by an Interactor. Manages references to its root, renderer, animator, and audio source; dispatches interaction requests to its InteractionBase behaviors; and handles highlighting.

### Properties

| Name | Description |
| --- | --- |
| **Root** | This interactable's root GameObject. |
| **AudioSource** | The audio source attached to this interactable. |
| **Animator** | The animator attached to this interactable. |
| **Renderer** | The renderer attached to this interactable. |
| **IsHighlightVisible** | Whether this interactable is currently allowed to display a highlight. |
| **IsHighlighted** | Is this interactable currently highlighted? |
| **IsInteracting** | Is any interaction on this interactable currently active? |

### Methods

| Name | Description |
| --- | --- |
| **TryInteraction** | Attempts to perform an interaction of the given type. Fails if a different interaction is already active. |
| **DoEvent** | Forwards an event to this interactable's currently active interaction, if any. |
| **SetHighlightOn** | Highlights this interactable, if `IsHighlightVisible` is true. |
| **SetHighlightOff** | Unhighlights this interactable. |
| **IgnoreCollision** | Ignores (or stops ignoring) collision between this interactable's colliders and those beneath a target root. |
| **TryGetInteraction** | Finds an interaction of the given type attached to this interactable. |
| **TryGetActiveInteraction** | Finds this interactable's currently active interaction, if any. |

### Delegates

| Name | Description |
| --- | --- |
| **HighlightHandler** | Signature for the `OnHighlight` event. |

### Events

| Name | Description |
| --- | --- |
| **OnHighlight** | Static event invoked whenever any interactable's highlighted state changes. |

## Interaction Base

Abstract base class for interaction behaviors. Inherit from this class to define custom interactions that an Interactor can trigger on an Interactable.

### Properties

| Name | Description |
| --- | --- |
| **Interactable** | The Interactable this interaction is attached to. Assigned automatically. |
| **InteractionType** | This interaction's type string. An Interactor can only interact with one type at a time. |
| **IsInteracting** | Is the interaction currently happening? |

### Methods

| Name | Description |
| --- | --- |
| **Interact** | Abstract. Initiates the interaction. |
| **DoEvent** | Virtual. Some interactions need feedback partway through—for example, an Animation Event. |

### Protected Helper Methods

These are available to subclasses for common interaction bookkeeping.

| Name | Description |
| --- | --- |
| **IsInteractableAndInfoValid** | Checks that this interaction is attached to a valid Interactable and that the provided InteractionInfo is properly populated. |
| **TryKillRigidbodyMovement** | Zeroes out a GameObject's Rigidbody velocity, if it has one. |
| **TrySetRigidbodyKinematic** | Toggles a GameObject's Rigidbody kinematic state, if it has one. |

## Interaction Simple

A ready-to-use InteractionBase that teleports the interactor to a fixed position and orientation relative to the interactable, triggers animator triggers on both parties, and optionally plays an audio clip via `DoEvent`. Useful for things like sitting down, pulling a lever, or examining an object in place.

### Properties

| Name | Description |
| --- | --- |
| **InteractPosition** | The world-space position the interactor is teleported to when interacting. |
| **InteractDirection** | The world-space direction the interactor faces when interacting. |

### Methods

| Name | Description |
| --- | --- |
| **Interact** | Toggles between the "not interacting" and "interacting" states, teleporting and orienting the interactor and firing the configured animator triggers. |
| **DoEvent** | Plays the configured audio clip through an AudioSource on this GameObject, if any. |

## Interaction Carry

A ready-to-use InteractionBase that lets the interactor pick up and carry the interactable, matching its position and rotation to a carry root every frame while carried.

### Methods

| Name | Description |
| --- | --- |
| **Interact** | Begins picking up the interactable, or, if already carrying, begins putting it down. |
| **DoEvent** | Call at the start and end of the pick-up/put-down animations to transition between phases. |
