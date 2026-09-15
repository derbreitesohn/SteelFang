using UnityEngine;

/// <summary>
/// Shows the on-screen control pad only on devices that actually need it.
/// The game binds everything to the keyboard, so on a phone there was no way
/// to move, jump, dash or pause at all. The buttons under this object are
/// OnScreenButtons that feed a virtual keyboard, so the existing bindings and
/// the direct Keyboard.current reads in PlayerMovementController both work
/// without changing any of the gameplay code.
/// </summary>
public class TouchControls : MonoBehaviour
{
    [Tooltip("Keep the pad visible everywhere. Handy for testing in the editor.")]
    [SerializeField] bool alwaysShow;

    void Awake()
    {
        SetVisible(alwaysShow || WantsTouchControls());
    }

    void SetVisible(bool visible)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(visible);
    }

    static bool WantsTouchControls()
    {
        // ?touch=1 / ?touch=0 on the page URL forces the pad on or off, which is the
        // only way to see either state from a desktop browser.
        var url = Application.absoluteURL;
        if (!string.IsNullOrEmpty(url))
        {
            if (url.Contains("touch=1")) return true;
            if (url.Contains("touch=0")) return false;
        }

        // Screen.width/height is no help: the canvas is letterboxed to a fixed
        // 1920x1200 framebuffer, so it reads 1.6 on every device. Touchscreen.current
        // is no help either - null on a phone until the first touch lands, which is
        // the very touch that needs a button, and non-null in desktop Chrome, which
        // put the pad in front of players who have a keyboard. The WebGL player sets
        // both of these from the browser's user agent instead.
        return Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;
    }
}
