using UnityEngine;

namespace Yontalane.Aseprite
{
    /// <summary>
    /// <see cref="ScriptableObject"/> that holds extra animation data for Aseprite animations, including <see cref="MotionTree"/> objects and value handlers.
    /// </summary>
    [CreateAssetMenu(fileName ="Extras", menuName ="Yontalane/Aseprite/Animation Extras")]
    public class AsepriteAnimationExtra : ScriptableObject
    {
        /// <summary>
        /// Array of <see cref="MotionTree"/> objects associated with this asset.
        /// </summary>
        [Tooltip("Array of motion trees associated with this asset.")]
        public MotionTree[] motionTrees = new MotionTree[0];
    }
}
