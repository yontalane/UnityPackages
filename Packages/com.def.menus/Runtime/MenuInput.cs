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

    [AddComponentMenu("DEF/Menus/Menu Input")]
    [DisallowMultipleComponent]
    public sealed class MenuInput : MonoBehaviour
    {
        private const float DEAD_ZONE = 0.35f;
        private const float MOVE_DELAY = 0.25f;

        public delegate void InputEventHandler(MenuInputEvent e);
        public InputEventHandler OnInputEvent = null;

        [Header("Input")]

        [SerializeField]
        [Tooltip("An input config asset. If you don't use this, you'll need to receive input in a different way, such as by attaching a PlayerInput to this GameObject.")]
        private InputActionAsset m_actions = null;

        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionMoveMap = "Move";
        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionScrollMap = "Scroll";

        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionSubmitMap = "Submit";
        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionCancelMap = "Cancel";

        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionGamepadFaceButtonNorthMap = "Gamepad Face Button North";
        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionGamepadFaceButtonEastMap = "Gamepad Face Button East";
        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionGamepadFaceButtonSouthMap = "Gamepad Face Button South";
        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionGamepadFaceButtonWestMap = "Gamepad Face Button West";

        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionGamepadShoulderLeftMap = "Gamepad Shoulder Left";
        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionGamepadShoulderRightMap = "Gamepad Shoulder Right";

        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionGamepadStartMap = "Gamepad Start";
        [SerializeField]
        [Tooltip("Leave blank to ignore this action.")]
        private string m_actionGamepadSelectMap = "Gamepad Select";

        [Header("Navigation")]

        [SerializeField]
        [Tooltip("If this is set to true, you can hold down a navigation button to continually select the next item in the Menu. Otherwise, you need to repeatedly press the navigation button.")]
        private bool m_allowHoldDownMove = false;

        private Vector2 m_move = new Vector2();
        private bool m_readyToMove = true;
        private bool m_moveIsDown = false;
        private float m_movePressTime = 0f;

        private Vector2 m_scroll = new Vector2();
        private bool m_scrollIsDown = false;

        #region InputActionAsset Setup

        private void Awake()
        {
            if (m_actions == null) return;

            if (!string.IsNullOrEmpty(m_actionMoveMap))
            {
                m_actions[m_actionMoveMap].started += OnInput_MoveStart;
                m_actions[m_actionMoveMap].canceled += OnInput_MoveStop;
            }
            if (!string.IsNullOrEmpty(m_actionScrollMap))
            {
                m_actions[m_actionScrollMap].started += OnInput_ScrollStart;
                m_actions[m_actionScrollMap].canceled += OnInput_ScrollStop;
            }

            if (!string.IsNullOrEmpty(m_actionSubmitMap)) m_actions[m_actionSubmitMap].performed += OnInputPerformed_Submit;
            if (!string.IsNullOrEmpty(m_actionCancelMap)) m_actions[m_actionCancelMap].performed += OnInputPerformed_Cancel;

            if (!string.IsNullOrEmpty(m_actionGamepadFaceButtonNorthMap)) m_actions[m_actionGamepadFaceButtonNorthMap].performed += OnInputPerformed_GamepadFaceButtonNorth;
            if (!string.IsNullOrEmpty(m_actionGamepadFaceButtonEastMap)) m_actions[m_actionGamepadFaceButtonEastMap].performed += OnInputPerformed_GamepadFaceButtonEast;
            if (!string.IsNullOrEmpty(m_actionGamepadFaceButtonSouthMap)) m_actions[m_actionGamepadFaceButtonSouthMap].performed += OnInputPerformed_GamepadFaceButtonSouth;
            if (!string.IsNullOrEmpty(m_actionGamepadFaceButtonWestMap)) m_actions[m_actionGamepadFaceButtonWestMap].performed += OnInputPerformed_GamepadFaceButtonWest;

            if (!string.IsNullOrEmpty(m_actionGamepadShoulderLeftMap)) m_actions[m_actionGamepadShoulderLeftMap].performed += OnInputPerformed_GamepadShoulderLeft;
            if (!string.IsNullOrEmpty(m_actionGamepadShoulderRightMap)) m_actions[m_actionGamepadShoulderRightMap].performed += OnInputPerformed_GamepadShoulderRight;

            if (!string.IsNullOrEmpty(m_actionGamepadStartMap)) m_actions[m_actionGamepadStartMap].performed += OnInputPerformed_GamepadStart;
            if (!string.IsNullOrEmpty(m_actionGamepadSelectMap)) m_actions[m_actionGamepadSelectMap].performed += OnInputPerformed_GamepadSelect;
        }

        private void OnInput_MoveStart(InputAction.CallbackContext _) => OnMove(m_actions[m_actionMoveMap].ReadValue<Vector2>());
        private void OnInput_MoveStop(InputAction.CallbackContext _) => OnMove(Vector2.zero);

        private void OnInput_ScrollStart(InputAction.CallbackContext _) => OnScroll(m_actions[m_actionScrollMap].ReadValue<Vector2>());
        private void OnInput_ScrollStop(InputAction.CallbackContext _) => OnScroll(Vector2.zero);

        private void OnInputPerformed_Submit(InputAction.CallbackContext _) => OnSubmit();
        private void OnInputPerformed_Cancel(InputAction.CallbackContext _) => OnCancel();

        private void OnInputPerformed_GamepadFaceButtonNorth(InputAction.CallbackContext _) => OnGamepadFaceButtonNorth();
        private void OnInputPerformed_GamepadFaceButtonEast(InputAction.CallbackContext _) => OnGamepadFaceButtonEast();
        private void OnInputPerformed_GamepadFaceButtonSouth(InputAction.CallbackContext _) => OnGamepadFaceButtonSouth();
        private void OnInputPerformed_GamepadFaceButtonWest(InputAction.CallbackContext _) => OnGamepadFaceButtonWest();

        private void OnInputPerformed_GamepadShoulderLeft(InputAction.CallbackContext _) => OnGamepadShoulderLeft();
        private void OnInputPerformed_GamepadShoulderRight(InputAction.CallbackContext _) => OnGamepadShoulderRight();

        private void OnInputPerformed_GamepadStart(InputAction.CallbackContext _) => OnGamepadStart();
        private void OnInputPerformed_GamepadSelect(InputAction.CallbackContext _) => OnGamepadSelect();

        private void OnEnable()
        {
            if (m_actions != null) m_actions.Enable();
        }

        private void OnDisable()
        {
            if (m_actions != null) m_actions.Disable();
        }

        #endregion

        #region On Direction

        private void OnMove(Vector2 vector)
        {
            float magnitude = vector.magnitude;
            bool noInput = Mathf.Approximately(magnitude, 0f);

            if (m_readyToMove && !noInput)
            {
                m_move = vector;
                m_readyToMove = true;
                m_moveIsDown = true;
            }
            else if (noInput)
            {
                m_moveIsDown = false;
                m_readyToMove = true;
            }
        }

        public void OnMove(InputValue value) => OnMove(value.Get<Vector2>());

        private void OnScroll(Vector2 vector)
        {
            float magnitude = vector.magnitude;
            bool noInput = Mathf.Approximately(magnitude, 0f);

            if (!noInput)
            {
                m_scroll = vector;
                m_scrollIsDown = true;
            }
            else
            {
                m_scrollIsDown = false;
            }
        }

        public void OnScroll(InputValue value) => OnScroll(value.Get<Vector2>());

        #endregion

        #region On Button

        private void OnButton(string buttonName) => OnInputEvent?.Invoke(new MenuInputEvent() { buttonName = buttonName });

        public void OnAccept() => OnButton("Accept");
        public void OnSubmit() => OnButton("Accept");
        public void OnConfirm() => OnButton("Accept");
        public void OnCancel() => OnButton("Cancel");
        public void OnGamepadFaceButtonNorth() => OnButton("Face Button North");
        public void OnGamepadFaceButtonEast() => OnButton("Face Button East");
        public void OnGamepadFaceButtonSouth() => OnButton("Face Button South");
        public void OnGamepadFaceButtonWest() => OnButton("Face Button West");
        public void OnGamepadShoulderLeft() => OnButton("Shoulder Left");
        public void OnGamepadShoulderRight() => OnButton("Shoulder Right");
        public void OnGamepadStart() => OnButton("Start");
        public void OnGamepadSelect() => OnButton("Select");

        #endregion

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