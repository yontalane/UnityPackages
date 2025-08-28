namespace Yontalane.DialogUGUI
{
    /// <summary>
    /// Specifies the method used to control a button's active state.
    /// </summary>
    [System.Serializable]
    internal enum ButtonActiveMethod
    {
        /// <summary>
        /// The button's interactable property is used to control its interactable state.
        /// </summary>
        Interactable = 0,

        /// <summary>
        /// The button's GameObject active state is used to control its interactable state.
        /// </summary>
        Active = 10,
    }
}
