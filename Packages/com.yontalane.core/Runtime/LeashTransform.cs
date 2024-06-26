using UnityEngine;

namespace Yontalane
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Leash Transform")]
    public class LeashTransform : MonoBehaviour
    {
        #region Helper Datatypes
        public enum OffsetType
        {
            Auto = 0,
            Manual = 1
        }

        public enum Space
        {
            Local = 0,
            World = 1,
            Parent = 2
        }

        [System.Serializable]
        public class Config
        {
#if UNITY_EDITOR
#pragma warning disable 0414 // Disable "private field assigned but not used" warning.
            [SerializeField]
            private string m_name = "";
#endif

            public bool shouldLeash = default;
            public OffsetType offsetType = OffsetType.Auto;
            public Vector3 offset = default;
            public Space space = Space.World;

            [Min(0f)]
            public float slack = 0f;

            [Range(0f, 1f)]
            public float smoothTime = 0f;

            private Vector3 m_currentVelocity = Vector3.zero;

            public Config(bool shouldLeash) => this.shouldLeash = shouldLeash;

            public Config() : this(true) { }

            public Vector3 GetDestination(Vector3 currentValue, Vector3 targetValue)
            {
                Vector3 destination = GetDesiredDestination(currentValue, targetValue);
                return Mathf.Approximately(smoothTime, 0f)
                    ? destination
                    : Vector3.SmoothDamp(currentValue, destination, ref m_currentVelocity, smoothTime);
            }

            private Vector3 GetDesiredDestination(Vector3 currentValue, Vector3 targetValue)
            {
                Vector3 destination = targetValue + offset;
                if (Mathf.Approximately(slack, 0f))
                {
                    return destination;
                }
                else
                {
                    Vector3 direction = (destination - currentValue).normalized;
                    float distance = Mathf.Max(Vector3.Distance(currentValue, destination) - slack, 0f);
                    return currentValue + distance * direction;
                }
            }
        }

        public enum UpdateType
        {
            Update = 0,
            LateUpdate = 1
        }
        #endregion

        #region Members
        public Rigidbody Rigidbody { get; private set; } = null;
        private bool m_hasRigidbody = false;

        [SerializeField]
        private Transform m_target = null;
        public Transform Target
        {
            get => m_target;
            set => Initialize(value, m_positionConfig, m_rotationConfig, m_scaleConfig, m_updateType);
        }

        [SerializeField]
        private Config m_positionConfig = new Config(true);
        public Config PositionConfig => m_positionConfig;

        [SerializeField]
        private Config m_rotationConfig = new Config(false);
        public Config RotationConfig => m_rotationConfig;

        [SerializeField]
        private Config m_scaleConfig = new Config(false);
        public Config ScaleConfig => m_scaleConfig;

        [SerializeField]
        private UpdateType m_updateType = UpdateType.Update;

        [SerializeField]
        [Tooltip("Leash using Rigidbody, if one is present.")]
        private bool m_useRigidbody = false;
        public bool UseRigidbody
        {
            get => m_useRigidbody;
            set => m_useRigidbody = value;
        }

        [SerializeField]
        [Tooltip("Whether or not to bound positional leashing.")]
        private bool m_useBounds = false;
        public bool UseBounds
        {
            get => m_useBounds;
            set => m_useBounds = value;
        }

        [SerializeField]
        [Tooltip("Stop positional leashing if target is outside these world bounds.")]
        private Bounds m_bounds = new Bounds();
        public Bounds Bounds
        {
            get => m_bounds;
            set => m_bounds = value;
        }

        #endregion

        private void Start() => Initialize(m_target, m_positionConfig, m_rotationConfig, m_scaleConfig, m_updateType);

        #region Initialize
        public void Initialize(Transform target, Config positionConfig, Config rotationConfig, Config scaleConfig, UpdateType updateType)
        {
            m_target = target;
            m_positionConfig = positionConfig;
            m_rotationConfig = rotationConfig;
            m_scaleConfig = scaleConfig;
            m_updateType = updateType;

            Rigidbody = GetComponent<Rigidbody>();
            m_hasRigidbody = Rigidbody != null;

            if (Target == null) return;

            if (m_positionConfig.shouldLeash && m_positionConfig.offsetType == OffsetType.Auto)
            {
                if (m_positionConfig.space == Space.Local)
                {
                    m_positionConfig.offset = transform.localPosition - Target.localPosition;
                }
                if (m_positionConfig.space == Space.World)
                {
                    m_positionConfig.offset = transform.position - Target.position;
                }
                else if (m_positionConfig.space == Space.Parent)
                {
                    m_positionConfig.offset = Target.InverseTransformPoint(transform.position);
                }
            }

            if (m_rotationConfig.shouldLeash && m_rotationConfig.offsetType == OffsetType.Auto)
            {
                if (m_rotationConfig.space == Space.Local)
                {
                    m_rotationConfig.offset = transform.localEulerAngles - Target.localEulerAngles;
                }
                else if (m_rotationConfig.space == Space.World)
                {
                    m_rotationConfig.offset = transform.eulerAngles - Target.eulerAngles;
                }
                else if (m_rotationConfig.space == Space.Parent)
                {
                    m_rotationConfig.offset = Target.InverseTransformDirection(transform.eulerAngles);
                }
            }

            if (m_scaleConfig.shouldLeash && m_scaleConfig.offsetType == OffsetType.Auto)
            {
                m_scaleConfig.offset = transform.localScale - Target.localScale;
            }
        }

        public void Initialize(Transform target, Config positionConfig, Config rotationConfig, Config scaleConfig) => Initialize(target, positionConfig, rotationConfig, scaleConfig, UpdateType.Update);

        public void Initialize(Transform target, Config positionConfig, Config rotationConfig) => Initialize(target, positionConfig, rotationConfig, new Config(false), UpdateType.Update);

        public void Initialize(Transform target, Config positionConfig) => Initialize(target, positionConfig, new Config(false));

        public void Initialize(Transform target, float positionSmoothTime, float rotationSmoothTime)
        {
            Config positionConfig = new Config()
            {
                shouldLeash = true,
                offsetType = OffsetType.Auto,
                slack = 0f,
                space = Space.World,
                smoothTime = Mathf.Max(positionSmoothTime, 0f)
            };

            Config rotationConfig = new Config()
            {
                shouldLeash = true,
                offsetType = OffsetType.Auto,
                slack = 0f,
                space = Space.World,
                smoothTime = Mathf.Max(rotationSmoothTime, 0f)
            };

            Initialize(target, positionConfig, rotationConfig);
        }

        public void Initialize(Transform target, float positionSmoothTime)
        {
            Config positionConfig = new Config()
            {
                shouldLeash = true,
                offsetType = OffsetType.Auto,
                slack = 0f,
                space = Space.World,
                smoothTime = Mathf.Max(positionSmoothTime, 0f)
            };

            Config rotationConfig = new Config(false);

            Initialize(target, positionConfig, rotationConfig);
        }

        public void Initialize(Transform target) => Initialize(target, 0f);
        #endregion

        private void Update()
        {
            if (m_updateType == UpdateType.Update) Leash();
        }

        private void LateUpdate()
        {
            if (m_updateType == UpdateType.LateUpdate) Leash();
        }

        private void Leash()
        {
            if (Target == null) return;

            if (m_hasRigidbody && UseRigidbody && Rigidbody.detectCollisions == true)
            {
                if (m_positionConfig.shouldLeash)
                {
                    Vector3 p;
                    if (m_positionConfig.space == Space.Local)
                    {
                        if (m_useBounds)
                        {
                            p = ClampTargetPositionToBounds(Target.localPosition, m_bounds, false);
                        }
                        else
                        {
                            p = Target.localPosition;
                        }
                        Vector3 v = m_positionConfig.GetDestination(transform.localPosition, p);
                        Rigidbody.MovePosition(transform.TransformPoint(v));
                    }
                    else if (m_positionConfig.space == Space.World)
                    {
                        if (m_useBounds)
                        {
                            p = ClampTargetPositionToBounds(Target.position, m_bounds, true);
                        }
                        else
                        {
                            p = Target.position;
                        }
                        Rigidbody.MovePosition(m_positionConfig.GetDestination(transform.position, p));
                    }
                    else if (m_positionConfig.space == Space.Parent)
                    {
                        if (m_useBounds)
                        {
                            p = ClampTargetPositionToBounds(Target.TransformPoint(m_positionConfig.offset), m_bounds, true);
                        }
                        else
                        {
                            p = Target.TransformPoint(m_positionConfig.offset);
                        }
                        Rigidbody.MovePosition(m_positionConfig.GetDestination(transform.position, p));
                    }
                }
                if (m_rotationConfig.shouldLeash)
                {
                    if (m_rotationConfig.space == Space.Local)
                    {
                        Vector3 v = m_rotationConfig.GetDestination(transform.eulerAngles, Target.eulerAngles);
                        Rigidbody.MoveRotation(Quaternion.Euler(transform.TransformDirection(v)));
                    }
                    else if (m_positionConfig.space == Space.World)
                    {
                        Rigidbody.MoveRotation(Quaternion.Euler(m_rotationConfig.GetDestination(transform.eulerAngles, Target.eulerAngles)));
                    }
                    else if (m_positionConfig.space == Space.Parent)
                    {
                        Rigidbody.MoveRotation(Quaternion.Euler(m_rotationConfig.GetDestination(transform.eulerAngles, Target.TransformDirection(m_rotationConfig.offset))));
                    }
                }
            }
            else
            {
                if (m_positionConfig.shouldLeash)
                {
                    Vector3 p;
                    if (m_positionConfig.space == Space.Local)
                    {
                        if (m_useBounds)
                        {
                            p = ClampTargetPositionToBounds(Target.localPosition, m_bounds, false);
                        }
                        else
                        {
                            p = Target.localPosition;
                        }
                        transform.localPosition = m_positionConfig.GetDestination(transform.localPosition, p);
                    }
                    else if (m_positionConfig.space == Space.World)
                    {
                        if (m_useBounds)
                        {
                            p = ClampTargetPositionToBounds(Target.position, m_bounds, true);
                        }
                        else
                        {
                            p = Target.position;
                        }
                        transform.position = m_positionConfig.GetDestination(transform.position, p);
                    }
                    else if (m_positionConfig.space == Space.Parent)
                    {
                        if (m_useBounds)
                        {
                            p = ClampTargetPositionToBounds(Target.TransformPoint(m_positionConfig.offset), m_bounds, true);
                        }
                        else
                        {
                            p = Target.TransformPoint(m_positionConfig.offset);
                        }
                        transform.position = m_positionConfig.GetDestination(transform.position, p);
                    }
                }
                if (m_rotationConfig.shouldLeash)
                {
                    if (m_rotationConfig.space == Space.Local)
                    {
                        transform.localEulerAngles = m_rotationConfig.GetDestination(transform.localEulerAngles, Target.localEulerAngles);
                    }
                    else if (m_positionConfig.space == Space.World)
                    {
                        transform.eulerAngles = m_rotationConfig.GetDestination(transform.eulerAngles, Target.eulerAngles);
                    }
                    else if (m_positionConfig.space == Space.Parent)
                    {
                        transform.eulerAngles = m_rotationConfig.GetDestination(transform.eulerAngles, Target.TransformDirection(m_rotationConfig.offset));
                    }
                }
            }

            if (m_scaleConfig.shouldLeash)
            {
                transform.localScale = m_scaleConfig.GetDestination(transform.localScale, Target.localScale);
            }
        }

        private Vector3 ClampTargetPositionToBounds(Vector3 position, Bounds bounds, bool positionIsInWorldSpace)
        {
            Vector3 min, max;

            if (positionIsInWorldSpace)
            {
                min = bounds.min;
                max = bounds.max;
            }
            else
            {
                min = Target.InverseTransformPoint(bounds.min);
                max = Target.InverseTransformPoint(bounds.max);
            }

            return new()
            {
                x = Mathf.Clamp(position.x, min.x, max.x),
                y = Mathf.Clamp(position.y, min.y, max.y),
                z = Mathf.Clamp(position.z, min.z, max.z)
            };
        }
    }
}
