using UnityEngine;
using UnityEngine.InputSystem;

namespace Yontalane.Demos.LayoutTilemap
{
    /// <summary>
    /// Controls player movement and rotation based on input in the Layout Tilemap demo.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    [AddComponentMenu("Yontalane/Demos/Player")]
    public class Player : MonoBehaviour
    {
        private const float TURN_SPEED_SCALE = 100f;

        private Vector2 m_input = new();

        /// <summary>
        /// The movement speed of the player character (units per second).
        /// </summary>
        [Tooltip("The movement speed of the player character (units per second).")]
        public float moveSpeed = 2.5f;

        /// <summary>
        /// The turning speed of the player character (multiplier for rotation responsiveness).
        /// </summary>
        [Tooltip("The turning speed of the player character (multiplier for rotation responsiveness).")]
        public float turnSpeed = 5f;

        /// <summary>
        /// Handles player movement and rotation each frame based on current input.
        /// </summary>
        private void Update()
        {
            // Rotate the player around the Y axis based on horizontal input (m_input.x)
            transform.Rotate(Vector3.up, m_input.x * TURN_SPEED_SCALE * turnSpeed * Time.deltaTime);
            // Move the player forward/backward based on vertical input (m_input.y)
            transform.Translate(m_input.y * moveSpeed * Time.deltaTime * Vector3.forward, Space.Self);
        }

        /// <summary>
        /// Called by the input system to update the player's movement input vector.
        /// </summary>
        /// <param name="inputValue">The input value containing the movement vector.</param>
        public void OnMove(InputValue inputValue)
        {
            m_input = inputValue.Get<Vector2>();
        }
    }
}