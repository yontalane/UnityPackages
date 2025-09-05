using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.Aseprite
{
    /// <summary>
    /// A struct containing information about an animation lifecycle event.
    /// </summary>
    [System.Serializable]
    public struct AnimationLifecycleEvent
    {
        /// <summary>
        /// The name of the animation.
        /// </summary>
        public string animationName;

        /// <summary>
        /// Whether the animation is looping.
        /// </summary>
        public bool isLooping;
    }

    /// <summary>
    /// A struct containing information about an animation motion event.
    /// </summary>
    [System.Serializable]
    public struct AnimationMotionEvent
    {
        /// <summary>
        /// The animation frame at which the motion takes place.
        /// </summary>
        public int frameIndex;

        /// <summary>
        /// The root motion value.
        /// </summary>
        public Vector2 motion;
    }

    /// <summary>
    /// A bridge between Aseprite animations and Unity Animator.
    /// Facilitates interaction between Aseprite animations and Unity's Animator by handling root motion events, animation lifecycle events, and providing utility methods for animation control and querying.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Yontalane/Aseprite/Aseprite Animation Bridge")]
    public class AsepriteAnimationBridge : MonoBehaviour
    {
        #region Event Types

        /// <summary>
        /// UnityEvent that is invoked with a Vector2 representing root motion data from an Aseprite animation.
        /// </summary>
        [System.Serializable]
        public class OnMotionHandler : UnityEvent<AnimationMotionEvent> { }

        /// <summary>
        /// UnityEvent that is invoked upon animation lifecycle events.
        /// </summary>
        [System.Serializable]
        public class OnLifecycleHandler : UnityEvent<AnimationLifecycleEvent> { }

        #endregion

        #region Private Fields

        private Animator m_animator = null;
        private SpriteRenderer m_spriteRenderer = null;
        private bool m_playingMotionTree = false;
        private MotionTree m_currentMotionTree = default;

        #endregion

        #region Serialized Fields

        [Header("Events")]

        [Tooltip("Event invoked when root motion data is received from the Aseprite animation.")]
        [SerializeField]
        private OnMotionHandler m_onMotion = null;

        [Tooltip("Event invoked when the animation starts.")]
        [SerializeField]
        private OnLifecycleHandler m_onStart = null;

        [Tooltip("Event invoked when the animation completes.")]
        [SerializeField]
        private OnLifecycleHandler m_onComplete = null;

        [Header("Colliders and Points")]

        [Tooltip("Colliders defined in Aseprite.")]
        [SerializeField]
        private List<BoxCollider2D> m_colliders = new();

        [Tooltip("Trigger colliders defined in Aseprite.")]
        [SerializeField]
        private List<BoxCollider2D> m_triggers = new();

        [Tooltip("Points defined in Aseprite.")]
        [SerializeField]
        private List<Transform> m_points = new();

        [Header("Extras")]

        [Tooltip("Optional extra data for more complex animations.")]
        [SerializeField]
        private AsepriteAnimationExtras[] m_extras = new AsepriteAnimationExtras[0];

        #endregion

        #region Events

        /// <summary>
        /// Event invoked when root motion data is received from the Aseprite animation.
        /// </summary>
        public OnMotionHandler OnMotion => m_onMotion;

        /// <summary>
        /// Event invoked when the animation starts.
        /// </summary>
        public OnLifecycleHandler OnStart => m_onStart;

        /// <summary>
        /// Event invoked when the animation completes.
        /// </summary>
        public OnLifecycleHandler OnComplete => m_onComplete;

        #endregion

        #region Component Properties

        /// <summary>
        /// The Animator component attached to the GameObject.
        /// </summary>
        public Animator Animator
        {
            get
            {
                // If the Animator component is not assigned, get it from the GameObject
                if (m_animator == null)
                {
                    m_animator = GetComponent<Animator>();
                }

                // Return the Animator component
                return m_animator;
            }
        }

        /// <summary>
        /// Gets the SpriteRenderer.
        /// </summary>
        public SpriteRenderer SpriteRenderer
        {
            get
            {
                // Check if the SpriteRenderer reference is null and assign it if necessary
                if (m_spriteRenderer == null)
                {
                    m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }

                // Return the cached SpriteRenderer component
                return m_spriteRenderer;
            }
        }

        #endregion

        #region Sprite Properties

        /// <summary>
        /// The Sprite to render.
        /// </summary>
        public Sprite Sprite => SpriteRenderer.sprite;

        /// <summary>
        /// Rendering color for the Sprite graphic.
        /// </summary>
        public Color Color
        {
            get => SpriteRenderer.color;
            set => SpriteRenderer.color = value;
        }

        /// <summary>
        /// Renderer's order within a sorting layer.
        /// </summary>
        public int SortingOrder
        {
            get => SpriteRenderer.sortingOrder;
            set => SpriteRenderer.sortingOrder = value;
        }

        /// <summary>
        /// Flips the sprite on the X axis.
        /// </summary>
        public bool FlipX
        {
            get => SpriteRenderer.flipX;
            set => SpriteRenderer.flipX = value;
        }

        /// <summary>
        /// Flips the sprite on the Y axis.
        /// </summary>
        public bool FlipY
        {
            get => SpriteRenderer.flipY;
            set => SpriteRenderer.flipY = value;
        }

        /// <summary>
        /// Gets the name of the currently playing animation.
        /// </summary>
        public string CurrentAnimation { get; private set; } = string.Empty;

        /// <summary>
        /// Colliders defined in Aseprite.
        /// </summary>
        public List<BoxCollider2D> Colliders
        {
            get => m_colliders;
            set => m_colliders = value;
        }

        /// <summary>
        /// Trigger colliders defined in Aseprite.
        /// </summary>
        public List<BoxCollider2D> Triggers
        {
            get => m_triggers;
            set => m_triggers = value;
        }

        /// <summary>
        /// Points defined in Aseprite.
        /// </summary>
        public List<Transform> Points
        {
            get => m_points;
            set => m_points = value;
        }

        #endregion


        /// <summary>
        /// Updates the animation state each frame after all Update functions have been called.
        /// Handles playing the correct animation clip from the motion tree if applicable.
        /// </summary>
        private void LateUpdate()
        {
            // Check if the motion tree is currently playing; if not, exit the method.
            if (!m_playingMotionTree)
            {
                return;
            }

            // Attempt to retrieve the current motion tree and its associated animation clip; if unsuccessful, exit the method.
            if (!TryGetMotionTree(m_currentMotionTree.id, out MotionTree motionTree, out AnimationClip clip))
            {
                return;
            }

            // Play the animation clip using the Animator, setting the normalized time from the motion tree value.
            Animator.Play(clip.name, -1, m_extras.GetMotionTreeValue(motionTree));
        }

        #region Aseprite Animation Events

        /// <summary>
        /// Parses the root motion data from the Aseprite animation and invokes the OnMotion event with the parsed motion vector.
        /// </summary>
        /// <param name="position">The root motion data from the Aseprite animation.</param>
        public void OnAsepriteRootMotion(string position)
        {
            // Split the position string into an array of strings
            string[] pos = position.Split(',');
            int frameIndex = int.TryParse(pos[0].Trim(), out int outFrameIndex) ? outFrameIndex : -1;
            float x = float.TryParse(pos[1].Trim(), out float outX) ? outX : 0f;
            float y = float.TryParse(pos[2].Trim(), out float outY) ? outY : 0f;

            // Invoke the OnMotion event with the parsed motion vector
            OnMotion?.Invoke(new()
            {
                frameIndex = frameIndex,
                motion = new Vector2(x, y),
            });
        }

        /// <summary>
        /// Invoked when the animation starts.
        /// </summary>
        /// <param name="animationEvent">The AnimationEvent that triggered the start.</param>
        public void OnAsepriteAnimationStart(AnimationEvent animationEvent)
        {
            CurrentAnimation = animationEvent.stringParameter;
            OnStart?.Invoke(new()
            {
                animationName = animationEvent.stringParameter,
                isLooping = animationEvent.intParameter > 0,
            });
        }

        /// <summary>
        /// Invoked when the animation completes.
        /// </summary>
        /// <param name="animationEvent">The AnimationEvent that triggered the completion.</param>
        public void OnAsepriteAnimationComplete(AnimationEvent animationEvent)
        {
            OnComplete?.Invoke(new()
            {
                animationName = animationEvent.stringParameter,
                isLooping = animationEvent.intParameter > 0,
            });
        }

        #endregion

        #region Animation Clip Utilities

        /// <summary>
        /// Tries to get an animation clip with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to get.</param>
        /// <param name="animationClip">The animation clip if found, otherwise null.</param>
        /// <param name="includeMotionTrees">Whether to include motion trees in the search.</param>
        /// <returns>True if the animation clip was found, false otherwise.</returns>
        public bool TryGetAnimationClip(string animationName, bool includeMotionTrees, out AnimationClip animationClip)
        {
            // Check if the extras contain a motion tree for the given animation name.
            if (includeMotionTrees && m_extras.TryGetMotionTree(animationName, out MotionTree motionTree))
            {
                // If found, get the corresponding animation clip name from the motion tree and try to get the animation clip.
                string clipName = m_extras.GetAnimation(motionTree);
                return TryGetAnimationClip(clipName, out animationClip);
            }

            // Get the RuntimeAnimatorController from the Animator
            RuntimeAnimatorController controller = Animator.runtimeAnimatorController;

            // If the RuntimeAnimatorController is not assigned, return false
            if (controller == null)
            {
                animationClip = null;
                return false;
            }

            // Iterate through the animation clips in the RuntimeAnimatorController
            for (int i = 0; i < controller.animationClips.Length; i++)
            {
                // If the animation clip's name is not the same as the specified animation name, continue
                if (controller.animationClips[i].name != animationName)
                {
                    continue;
                }

                // Set the animation clip and return true
                animationClip = controller.animationClips[i];
                return true;
            }

            // If no animation clip's name is the same as the specified animation name, return false
            animationClip = null;
            return false;
        }

        /// <summary>
        /// Tries to get an animation clip or motion tree with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to get.</param>
        /// <param name="animationClip">The animation clip if found, otherwise null.</param>
        /// <returns>True if the animation clip was found, false otherwise.</returns>
        public bool TryGetAnimationClip(string animationName, out AnimationClip animationClip)
        {
            return TryGetAnimationClip(animationName, true, out animationClip);
        }

        /// <summary>
        /// Tries to get an animation clip with the specified name from within the motion trees.
        /// </summary>
        /// <param name="motionTreeName">The name of the animation to get.</param>
        /// <param name="motionTree">The motion tree if found.</param>
        /// <param name="animationClip">The animation clip if found, otherwise null.</param>
        /// <returns>True if the animation clip was found, false otherwise.</returns>
        public bool TryGetMotionTree(string motionTreeName, out MotionTree motionTree, out AnimationClip animationClip)
        {
            // Check if the extras contain a motion tree for the given animation name.
            if (m_extras.TryGetMotionTree(motionTreeName, out motionTree))
            {
                // If found, get the corresponding animation clip name from the motion tree and try to get the animation clip.
                string clipName = m_extras.GetAnimation(motionTree);
                return TryGetAnimationClip(clipName, out animationClip);
            }

            // If no animation clip's name is the same as the specified animation name, return false
            animationClip = null;
            return false;
        }

        /// <summary>
        /// Checks if the Animator has an animation with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to check for.</param>
        /// <param name="includeMotionTrees">Whether to include motion trees in the search.</param>
        /// <returns>True if the Animator has an animation with the specified name, false otherwise.</returns>
        public bool HasAnimation(string animationName, bool includeMotionTrees)
        {
            // Check if the extras contain a motion tree for the given animation name and if it has any animations defined
            if (includeMotionTrees && m_extras.TryGetMotionTree(animationName, out MotionTree motionTree) && motionTree.animations != null && motionTree.animations.Length > 0)
            {
                return true;
            }

            // If not found in motion trees, try to get the animation clip with the specified name
            return TryGetAnimationClip(animationName, out _);
        }

        /// <summary>
        /// Checks if the Animator has an animation with the specified name, or if there is a motion tree by that name.
        /// </summary>
        /// <param name="animationName">The name of the animation to check for.</param>
        /// <returns>True if the Animator has an animation with the specified name, false otherwise.</returns>
        public bool HasAnimation(string animationName) => HasAnimation(animationName, true);

        /// <summary>
        /// Checks if the <see cref="AsepriteAnimationBridge"/> has an <see cref="AsepriteAnimationExtras"/> asset with a <see cref="MotionTree"/> with the specified name.
        /// </summary>
        /// <param name="motionTreeName">The name of the motion tree to check for.</param>
        /// <returns>True if the motion tree with the specified name exists, false otherwise.</returns>
        public bool HasMotionTree(string motionTreeName)
        {
            // Check if the extras contain a motion tree for the given name and if it has any animations defined
            if (m_extras.TryGetMotionTree(motionTreeName, out MotionTree motionTree) && motionTree.animations != null && motionTree.animations.Length > 0)
            {
                return true;
            }

            // If not found, return false
            return false;
        }

        #endregion

        #region Animation Playback

        /// <summary>
        /// Stops the current animation playback and resets the playback state.
        /// </summary>
        public void Stop()
        {
            m_playingMotionTree = false;
            Animator.StopPlayback();
        }

        /// <summary>
        /// Tries to play an animation with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        /// <param name="startTime">The start time offset between zero and one.</param>
        /// <param name="restartLoop">Whether to restart the animation loop if the animation is looping and the current animation is the same as the specified animation name.</param>
        /// <param name="includeMotionTrees">Whether to try playing motion trees.</param>
        /// <returns>True if the animation was played, false otherwise.</returns>
        public bool TryPlay(string animationName, float startTime = 0f, bool restartLoop = false, bool includeMotionTrees = true)
        {
            if (includeMotionTrees && TryGetMotionTree(animationName, out MotionTree motionTree, out AnimationClip clip))
            {
                m_playingMotionTree = true;
                m_currentMotionTree = motionTree;
                Animator.Play(clip.name, -1, m_extras.GetMotionTreeValue(motionTree));
                return true;
            }

            // Try to get the animation clip with the specified name
            if (!TryGetAnimationClip(animationName, includeMotionTrees, out clip))
            {
                return false;
            }

            // If the animation is looping and the current animation is the same as the specified animation name, and restartLoop is false, return false
            if (!restartLoop && clip.isLooping && CurrentAnimation == animationName)
            {
                return false;
            }

            // Play the animation
            m_playingMotionTree = false;
            Animator.Play(clip.name, -1, startTime);
            return true;
        }

        /// <summary>
        /// Tries to play an animation with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        /// <param name="startTime">The start time offset between zero and one.</param>
        /// <param name="restartLoop">Whether to restart the animation loop if the animation is looping and the current animation is the same as the specified animation name.</param>
        /// <returns>True if the animation was played, false otherwise.</returns>
        public bool TryPlay(string animationName, float startTime = 0f, bool restartLoop = false) => TryPlay(animationName, startTime, restartLoop, true);

        /// <summary>
        /// Tries to play a motion tree with the specified name.
        /// </summary>
        /// <param name="motionTreeName">The name of the motion tree to play.</param>
        /// <returns>True if the motion tree was played, false otherwise.</returns>
        public bool TryPlayMotionTree(string motionTreeName)
        {
            // Try to get the animation clip with the specified name
            if (!TryGetMotionTree(motionTreeName, out MotionTree motionTree, out AnimationClip clip))
            {
                return false;
            }

            // If the animation is looping and the current animation is the same as the specified animation name, return false
            if (clip.isLooping && (CurrentAnimation == clip.name || CurrentAnimation == motionTree.id))
            {
                return false;
            }

            // Play the animation
            Animator.Play(clip.name, -1, m_extras.GetMotionTreeValue(motionTree));
            return true;
        }

        /// <summary>
        /// Plays an animation with the specified name.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        /// <param name="startTime">The start time offset between zero and one.</param>
        /// <param name="restartLoop">Whether to restart the animation loop if the animation is looping and the current animation is the same as the specified animation name.</param>
        public void Play(string animationName, float startTime = 0f, bool restartLoop = false) => _ = TryPlay(animationName, startTime, restartLoop);

        /// <summary>
        /// Plays a motion tree with the specified name.
        /// </summary>
        /// <param name="motionTreeName">The name of the motion tree to play.</param>
        public void PlayMotionTree(string motionTreeName) => _ = TryPlayMotionTree(motionTreeName);

        #endregion


#if UNITY_EDITOR

        #region Gizmos

        /// <summary>
        /// Draws gizmos in the editor to visualize colliders, triggers, and points associated with this object.
        /// </summary>
        private void OnDrawGizmos()
        {
            // Draw wireframe gizmos for each collider in m_colliders, if any exist.
            if (m_colliders != null)
            {
                Gizmos.color = AsepriteSettings.instance.gizmoInfo.colliderColor;

                foreach (BoxCollider2D item in m_colliders)
                {
                    DrawColliderGizmo(item);
                }
            }

            // Draw wireframe gizmos for each trigger in m_triggers, if any exist.
            if (m_triggers != null)
            {
                Gizmos.color = AsepriteSettings.instance.gizmoInfo.triggerColor;

                foreach (BoxCollider2D item in m_triggers)
                {
                    DrawColliderGizmo(item);
                }
            }

            // Draw wireframe sphere gizmos for each point in m_points, if any exist.
            if (m_points != null)
            {
                Gizmos.color = AsepriteSettings.instance.gizmoInfo.pointColor;
                float radius = AsepriteSettings.instance.gizmoInfo.pointRadius;

                foreach (Transform item in m_points)
                {
                    DrawPointGizmo(item, radius);
                }
            }
        }

        /// <summary>
        /// Draws a wireframe gizmo for the specified BoxCollider2D in the Scene view.
        /// </summary>
        /// <param name="collider">The BoxCollider2D to visualize.</param>
        private void DrawColliderGizmo(BoxCollider2D collider)
        {
            // Check if the collider is null or not active/enabled; if so, exit the method.
            if (collider == null || !collider.isActiveAndEnabled)
            {
                return;
            }

            // Calculate the world position of the collider's origin using its offset and the transform.
            Vector3 localOrigin = collider.offset;
            Vector3 worldOrigin = collider.transform.TransformPoint(localOrigin);

            // Get the local size of the collider and scale it by the transform's lossy scale to get the world size.
            Vector3 localSize = collider.size;
            Vector3 lossyScale = collider.transform.lossyScale;
            Vector3 worldSize = new()
            {
                x = Mathf.Max(Mathf.Abs(localSize.x * lossyScale.x), 0.05f),
                y = Mathf.Max(Mathf.Abs(localSize.y * lossyScale.y), 0.05f),
                z = Mathf.Max(Mathf.Abs(localSize.z * lossyScale.z), 0.05f),
            };

            // Draw a wireframe cube gizmo at the collider's world position and size.
            Gizmos.DrawWireCube(worldOrigin, worldSize);
        }

        /// <summary>
        /// Draws a wireframe sphere gizmo at the position of the specified Transform in the Scene view.
        /// </summary>
        /// <param name="transform">The Transform whose position to visualize.</param>
        private void DrawPointGizmo(Transform transform, float radius)
        {
            // Check if the transform is null or not active; if so, exit the method.
            if (transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

            // Draw a wireframe sphere gizmo at the transform's world position.
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        #endregion

#endif
    }
}
