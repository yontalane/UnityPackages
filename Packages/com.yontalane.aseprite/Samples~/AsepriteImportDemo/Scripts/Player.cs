using UnityEngine;
using UnityEngine.InputSystem;
using Yontalane.Aseprite;

namespace Yontalane.Demos.Aseprite
{
    /// <summary>
    /// The player controller. This is a simple example of how to use the AsepriteAnimationBridge component.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AsepriteAnimationBridge))]
    [RequireComponent(typeof(PlayerInput))]
    [AddComponentMenu("Yontalane/Demos/Aseprite/Player")]
    public class Player : MonoBehaviour
    {
        /// <summary>
        /// The AsepriteAnimationBridge component.
        /// </summary>
        public AsepriteAnimationBridge AnimationBridge { get; private set; } = null;

        /// <summary>
        /// Whether the player is ready to move and perform actions. This is used to prevent the player from moving before the animation is complete.
        /// </summary>
        public bool Ready { get; private set; } = true;

        /// <summary>
        /// Initializes the player's components.
        /// </summary>
        private void Start()
        {
            // Get the AsepriteAnimationBridge component
            AnimationBridge = GetComponent<AsepriteAnimationBridge>();
        }

        /// <summary>
        /// Handles the player's movement based on the Aseprite motion input. This is used to move the player based on the Aseprite motion input.
        /// </summary>
        /// <param name="value">The movement direction and speed.</param>
        public void OnAsepriteMotion(AnimationMotionEvent motionEvent)
        {
            // Move the player based on the Aseprite motion input (x-axis only)
            transform.Translate(motionEvent.motion.x, 0f, 0f);
        }

        /// <summary>
        /// Called when an animation starts playing. Logs trigger collider activation frames for the current animation.
        /// </summary>
        public void OnAnimationStart(AnimationLifecycleEvent lifecycleEvent)
        {
            // Create a log message indicating which animation is starting
            string log = $"Starting animation \"{lifecycleEvent.animationName}\"";

            // Iterate through all trigger colliders associated with the animation bridge
            foreach (Collider2D trigger in AnimationBridge.Triggers)
            {
                // Check if there is animation info for this trigger and the current animation
                if (AnimationBridge.SpriteObjectInfo.TryGetAnimationInfo(trigger.name, lifecycleEvent.animationName, out SpriteObjectAnimationInfo info))
                {
                    // Append animation and object name to the log, and prepare to list frames
                    log += $"\n Trigger: {trigger.name} Frames:";

                    // List all frames where this object is active in the animation
                    foreach (int frameOn in info.framesOn)
                    {
                        log += $" {frameOn}";
                    }
                }
            }

            // Output the constructed log message to the Unity console
            Debug.Log(log);
        }

        /// <summary>
        /// Called when the current animation completes. This is used to set the player to ready and play the idle animation.
        /// </summary>
        public void OnAnimationComplete(AnimationLifecycleEvent lifecycleEvent)
        {
            // If the animation is looping, do nothing
            if (lifecycleEvent.isLooping)
            {
                return;
            }

            // Play the idle animation and set the player to ready. This is used to set the player to ready and play the idle animation.
            AnimationBridge.Play("Idle");
            Ready = true;
        }

        /// <summary>
        /// Handles player movement input. This is used to move the player based on the input.
        /// </summary>
        /// <param name="inputValue">The input value containing the movement direction.</param>
        public void OnMove(InputValue inputValue)
        {
            // Check if the player is ready before processing the movement input. This is used to prevent the player from moving before the animation is complete.
            if (!Ready)
            {
                return;
            }

            // Get the horizontal movement direction. This is used to move the player based on the input.
            float x = inputValue.Get<Vector2>().x;

            // Check if the player is idle
            if (Mathf.Approximately(x, 0f))
            {
                AnimationBridge.Play("Idle");
            }
            // Check if the player is advancing
            else if (x > 0f)
            {
                AnimationBridge.Play("Advance");
            }
            // Check if the player is retreating
            else
            {
                AnimationBridge.Play("Retreat");
            }
        }

        /// <summary>
        /// Handles the player's attack input.
        /// </summary>
        /// <param name="inputValue">The input value containing the attack state.</param>
        public void OnFire(InputValue inputValue)
        {
            // Check if the player is ready before processing the attack input
            if (!Ready)
            {
                return;
            }

            // Check if the attack input is pressed
            if (!inputValue.isPressed)
            {
                return;
            }

            // Play the attack animation and set the player to not ready
            AnimationBridge.Play("Lunge");
            Ready = false;
        }
    }
}