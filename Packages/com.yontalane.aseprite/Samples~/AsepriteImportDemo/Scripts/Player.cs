using UnityEngine;
using UnityEngine.InputSystem;

namespace Yontalane.Demos.Aseprite
{
    /// <summary>
    /// Controls the player character, handling movement, animation state, and input events.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayerInput))]
    [AddComponentMenu("Yontalane/Demos/Aseprite/Player")]
    public class Player : MonoBehaviour
    {
        /// <summary>
        /// The animator component controlling the player's animations.
        /// </summary>
        public Animator Animator { get; private set; } = null;

        /// <summary>
        /// Whether the player is ready to move and perform actions.
        /// </summary>
        public bool Ready { get; private set; } = true;

        /// <summary>
        /// Initializes the player's components and sets up animation events.
        /// </summary>
        private void Start()
        {
            // Get the animator component
            Animator = GetComponent<Animator>();

            // Loop through all the animations
            for (int i = 0; i < Animator.runtimeAnimatorController.animationClips.Length; i++)
            {
                // Get the current animation clip
                AnimationClip clip = Animator.runtimeAnimatorController.animationClips[i];

                // Only modify non-looping animations
                if (clip.isLooping)
                {
                    continue;
                }

                // Add an event to the animation clip to call OnAnimationComplete when the animation completes
                clip.AddEvent(new()
                {
                    functionName = nameof(OnAnimationComplete),
                    time = clip.length,
                });

                // Set the animation clip to the modified animation clip
                Animator.runtimeAnimatorController.animationClips[i] = clip;
            }
        }

        /// <summary>
        /// Handles the player's movement based on the Aseprite motion input.
        /// </summary>
        /// <param name="value">The movement direction and speed.</param>
        public void OnAsepriteMotion(Vector2 value)
        {
            // Move the player based on the Aseprite motion input (x-axis only)
            transform.Translate(value.x, 0f, 0f);
        }

        /// <summary>
        /// Called when the current animation completes.
        /// </summary>
        public void OnAnimationComplete()
        {
            // Play the idle animation and set the player to ready
            Animator.Play("Idle");
            Ready = true;
        }

        /// <summary>
        /// Handles player movement input.
        /// </summary>
        /// <param name="inputValue">The input value containing the movement direction.</param>
        public void OnMove(InputValue inputValue)
        {
            // Check if the player is ready before processing the movement input
            if (!Ready)
            {
                return;
            }

            // Get the horizontal movement direction
            float x = inputValue.Get<Vector2>().x;

            // Check if the player is idle
            if (Mathf.Approximately(x, 0f))
            {
                Animator.Play("Idle");
            }
            // Check if the player is advancing
            else if (x > 0f)
            {
                Animator.Play("Advance");
            }
            // Check if the player is retreating
            else
            {
                Animator.Play("Retreat");
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
            Animator.Play("Lunge");
            Ready = false;
        }
    }
}