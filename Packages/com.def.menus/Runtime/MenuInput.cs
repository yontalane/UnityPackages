using UnityEngine;
using UnityEngine.InputSystem;

namespace DEF.Menus
{
    public sealed class MenuInputEvent
    {
        public Vector2Int move = new Vector2Int();
        public Vector2 scroll = new Vector2();
        public string buttonName = "";
    }

    [AddComponentMenu("DEF/Menus/Input")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    public sealed class MenuInput : MonoBehaviour
    {
        private const float DEAD_ZONE = 0.35f;
        private const float MOVE_DELAY = 0.25f;

        public delegate void InputEventHandler(MenuInputEvent e);
        public InputEventHandler OnInputEvent = null;

        [SerializeField]
        [Tooltip("If this is set to true, you can hold down a navigation button to continually select the next item in the Menu. Otherwise, you need to repeatedly press the navigation button.")]
        private bool m_allowHoldDownMove = false;

        private Vector2 m_move = new Vector2();
        private bool m_readyToMove = true;
        private bool m_moveIsDown = false;
        private float m_movePressTime = 0f;

        private Vector2 m_scroll = new Vector2();
        private bool m_scrollIsDown = false;

        public void OnMove(InputValue value)
        {
            Vector2 vector = value.Get<Vector2>();
            float magnitude = vector.magnitude;
            bool noInput = Mathf.Approximately(magnitude, 0f);

            if (m_readyToMove && !noInput)
            {
                m_move = value.Get<Vector2>();
                m_readyToMove = true;
                m_moveIsDown = true;
            }
            else if (noInput)
            {
                m_moveIsDown = false;
                m_readyToMove = true;
            }
        }

        public void OnScroll(InputValue value)
        {
            Vector2 vector = value.Get<Vector2>();
            float magnitude = vector.magnitude;
            bool noInput = Mathf.Approximately(magnitude, 0f);

            if (!noInput)
            {
                m_scroll = value.Get<Vector2>();
                m_scrollIsDown = true;
            }
            else
            {
                m_scrollIsDown = false;
            }
        }

        private void OnButton(string buttonName) => OnInputEvent?.Invoke(new MenuInputEvent() { buttonName = buttonName });

        public void OnAccept() => OnButton("Accept");
        public void OnCancel() => OnButton("Cancel");
        public void OnGamepadFaceButtonNorth() => OnButton("Face Button North");
        public void OnGamepadFaceButtonEast() => OnButton("Face Button East");
        public void OnGamepadFaceButtonSouth() => OnButton("Face Button South");
        public void OnGamepadFaceButtonWest() => OnButton("Face Button West");
        public void OnGamepadShoulderLeft() => OnButton("Shoulder Left");
        public void OnGamepadShoulderRight() => OnButton("Shoulder Right");
        public void OnGamepadStart() => OnButton("Start");
        public void OnGamepadSelect() => OnButton("Select");

        private void Update()
        {
            if (m_moveIsDown)
            {
                if (m_readyToMove && m_move.magnitude > DEAD_ZONE)
                {
                    OnInputEvent?.Invoke(new MenuInputEvent()
                    {
                        move = new Vector2Int()
                        {
                            x = m_move.x < -DEAD_ZONE ? -1 : m_move.x > DEAD_ZONE ? 1 : 0,
                            y = m_move.y < -DEAD_ZONE ? -1 : m_move.y > DEAD_ZONE ? 1 : 0
                        }
                    });
                    m_readyToMove = false;
                    m_movePressTime = Time.time;
                }
                else if (m_allowHoldDownMove && !m_readyToMove && (m_move.magnitude < DEAD_ZONE || Time.time - m_movePressTime > MOVE_DELAY))
                {
                    m_readyToMove = true;
                }
            }

            if (m_scrollIsDown)
            {
                OnInputEvent?.Invoke(new MenuInputEvent()
                {
                    scroll = m_scroll
                });
            }
        }
    }
}