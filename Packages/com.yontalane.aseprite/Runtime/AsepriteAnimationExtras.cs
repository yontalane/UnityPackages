using UnityEngine;
using UnityEngine.Events;

namespace Yontalane.Aseprite
{
    /// <summary>
    /// ScriptableObject that holds extra animation data for Aseprite animations, including motion trees and value handlers.
    /// </summary>
    [CreateAssetMenu(fileName ="Extras", menuName ="Yontalane/Aseprite/Animation Extras")]
    public class AsepriteAnimationExtras : ScriptableObject
    {
        #region Nested Types

        /// <summary>
        /// UnityEvent handler for KeyFloatPair events.
        /// </summary>
        [System.Serializable]
        public class KeyFloatHandler : UnityEvent<KeyFloatPair>
        { }

        #endregion

        #region Fields

        [Header("Motion Trees")]

        /// <summary>
        /// Array of motion trees associated with this asset.
        /// </summary>
        [Tooltip("Array of motion trees associated with this asset.")]
        public MotionTree[] motionTrees = new MotionTree[0];

        [Header("Data Requests")]

        /// <summary>
        /// Event invoked to request a motion tree value.
        /// </summary>
        [Tooltip("Event invoked to request a motion tree value.")]
        [SerializeField]
        private KeyFloatHandler m_onRequestMotionTreeValue = null;

        #endregion


        /// <summary>
        /// Event invoked to request a motion tree value.
        /// </summary>
        public KeyFloatHandler OnRequestMotionTreeValue => m_onRequestMotionTreeValue;


        #region Motion Tree Value Methods

        /// <summary>
        /// Gets the value associated with a motion tree by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the motion tree.</param>
        /// <returns>The float value associated with the motion tree.</returns>
        public float GetMotionTreeValue(string id)
        {
            KeyFloatPair result = new()
            {
                key = id,
                value = default,
            };

            m_onRequestMotionTreeValue?.Invoke(result);

            return result.value;
        }

        /// <summary>
        /// Gets the value associated with a given motion tree.
        /// </summary>
        /// <param name="motionTree">The motion tree to get the value for.</param>
        /// <returns>The float value associated with the motion tree.</returns>
        public float GetMotionTreeValue(MotionTree motionTree) => GetMotionTreeValue(motionTree.id);

        /// <summary>
        /// Attempts to get the animation name for the given motion tree based on its current parameter value.
        /// </summary>
        /// <param name="motionTree">The motion tree to evaluate.</param>
        /// <param name="animation">The resulting animation name, if found.</param>
        /// <returns>True if a valid animation was found; otherwise, false.</returns>
        public bool TryGetAnimation(MotionTree motionTree, out string animation)
        {
            // Check if the motion tree has any animations defined
            if (motionTree.animations == null || motionTree.animations.Length == 0)
            {
                animation = default;
                return false;
            }

            // Get the current parameter value for the motion tree
            float currentParameterSetting = GetMotionTreeValue(motionTree);

            // Evaluate the curve at the current parameter value
            float val = motionTree.curve.Evaluate(currentParameterSetting);

            // Normalize the evaluated value to a 0-1 range
            val = motionTree.curve.GetNormalizedValue(val);

            // Convert the normalized value to the nearest animation index
            int ind = Mathf.Clamp(Mathf.FloorToInt(val * motionTree.animations.Length), 0, motionTree.animations.Length - 1);

            // Check if the calculated index is within the valid range of animations
            if (ind < 0 || ind >= motionTree.animations.Length)
            {
                animation = default;
                return false;
            }

            // Assign the animation name at the calculated index and return true
            animation = motionTree.animations[ind] != null ? motionTree.animations[ind].name : string.Empty;
            return true;
        }

        /// <summary>
        /// Attempts to get the animation name for the motion tree with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the motion tree.</param>
        /// <param name="animation">The resulting animation name, if found.</param>
        /// <returns>True if a valid animation was found; otherwise, false.</returns>
        public bool TryGetAnimation(string id, out string animation)
        {
            if (!TryGetMotionTree(id, out MotionTree motionTree))
            {
                animation = default;
                return false;
            }

            return TryGetAnimation(motionTree, out animation);
        }

        /// <summary>
        /// Gets the animation name for the given motion tree based on its current parameter value.
        /// </summary>
        /// <param name="motionTree">The motion tree to evaluate.</param>
        /// <returns>The animation name, or default if not found.</returns>
        public string GetAnimation(MotionTree motionTree)
        {
            _ = TryGetAnimation(motionTree, out string animation);
            return animation;
        }

        /// <summary>
        /// Gets the animation name for the motion tree with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the motion tree.</param>
        /// <returns>The animation name, or default if not found.</returns>
        public string GetAnimation(string id)
        {
            _ = TryGetAnimation(id, out string animation);
            return animation;
        }

        #endregion

        #region Motion Tree Lookup Methods

        /// <summary>
        /// Checks if a motion tree with the specified identifier exists.
        /// </summary>
        /// <param name="id">The identifier to check for.</param>
        /// <returns>True if the motion tree exists; otherwise, false.</returns>
        public bool HasMotionTree(string id)
        {
            foreach (MotionTree motionTree in motionTrees)
            {
                if (motionTree.id == id)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to get a motion tree by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the motion tree.</param>
        /// <param name="motionTree">The found motion tree, if any.</param>
        /// <returns>True if the motion tree was found; otherwise, false.</returns>
        public bool TryGetMotionTree(string id, out MotionTree motionTree)
        {
            for (int i = 0; i < motionTrees.Length; i++)
            {
                if (motionTrees[i].id == id)
                {
                    motionTree = motionTrees[i];
                    return true;
                }
            }

            motionTree = default;
            return false;
        }

        /// <summary>
        /// Gets a motion tree by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the motion tree.</param>
        /// <returns>The found motion tree, or default if not found.</returns>
        public MotionTree GetMotionTree(string id)
        {
            _ = TryGetMotionTree(id, out MotionTree motionTree);
            return motionTree;
        }

        #endregion
    }
}
