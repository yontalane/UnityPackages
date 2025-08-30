using UnityEngine;
using UnityEngine.InputSystem;

namespace Yontalane.UIElements
{
    /// <summary>
    /// Represents a navigation override, specifying a custom input action and target for UI navigation.
    /// </summary>
    [System.Serializable]
    public class NavigationOverride
    {
        [Tooltip("The direction of this navigation.")]
        public Directions direction;

        [Tooltip("The UI element that will get focus upon doing this navigation.")]
        public string target;
    }
}
