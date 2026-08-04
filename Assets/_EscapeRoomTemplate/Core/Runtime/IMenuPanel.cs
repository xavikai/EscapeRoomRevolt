namespace EscapeRoomRevolt.Core
{
    /// <summary>
    /// Marker implemented by whichever UI controller renders the pause/main menu screens.
    /// Lets generic VR panel setup (VRUIToolkitPresenter) classify a UIDocument as menu vs
    /// gameplay without a concrete reference to the UI controller type.
    /// </summary>
    public interface IMenuPanel { }
}
