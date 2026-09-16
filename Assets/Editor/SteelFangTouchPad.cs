// Editor-only: builds the on-screen control pad into SampleScene.
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public static class SteelFangTouchPad
{
    const string RootName = "TouchControls";

    [MenuItem("SteelFang/Build Touch Pad")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

        var canvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .FirstOrDefault(c => c.transform.parent == null);
        if (canvas == null) { Debug.LogError("@@PAD no root Canvas"); EditorApplication.Exit(2); return; }

        // Re-running should replace the pad, not stack a second one on top.
        var existing = canvas.transform.Find(RootName);
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var font = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None)
            .Select(t => t.font).FirstOrDefault(f => f != null);

        var root = new GameObject(RootName, typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        var rootRect = (RectTransform)root.transform;
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
        rootRect.SetAsLastSibling();
        root.AddComponent<TouchControls>();

        // Wear the game's own UI rather than sitting on top of it: the menus are built from
        // Images/wooden_button with white TMP labels, so the pad uses the same sprite and the
        // same font instead of the plain translucent squares it started with.
        var buttonSprite = LoadButtonSprite();

        // The gameplay Canvas is Constant Pixel Size and the page pins the framebuffer to
        // 1920x1200, so these sizes are stable on every screen. They sit lower and smaller than
        // before, but only so far: a 390px-wide phone in portrait scales the canvas by 0.203, so
        // 195px lands near 40 CSS px. Shrinking much past this starts missing thumbs - held in
        // landscape, which is how a platformer is actually played, the same button is ~63px.
        Make(root, "Left",  new Vector2(0, 0), new Vector2( 130,   95), 195, "<",    "<Keyboard>/a",         font, buttonSprite);
        Make(root, "Right", new Vector2(0, 0), new Vector2( 345,   95), 195, ">",    "<Keyboard>/d",         font, buttonSprite);
        Make(root, "Jump",  new Vector2(1, 0), new Vector2(-130,   95), 215, "JUMP", "<Keyboard>/space",     font, buttonSprite);
        Make(root, "Dash",  new Vector2(1, 0), new Vector2(-375,  115), 180, "DASH", "<Keyboard>/leftShift", font, buttonSprite);
        Make(root, "Pause", new Vector2(1, 1), new Vector2( -90,  -90), 120, "II",   "<Keyboard>/escape",    font, buttonSprite);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("@@PAD built " + root.transform.childCount + " buttons");
        EditorApplication.Exit(0);
    }

    /// The menus reference wooden_button as a sub-sprite of a multi-sprite texture, so a plain
    /// LoadAssetAtPath&lt;Sprite&gt; comes back null - pick it out of the asset's contents instead.
    static Sprite LoadButtonSprite()
    {
        const string path = "Assets/Images/wooden_button.png";
        var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();

        if (sprite == null)
            Debug.LogWarning("@@PAD wooden_button sprite not found; falling back to a plain panel");

        return sprite;
    }

    static void Make(GameObject parent, string name, Vector2 anchor, Vector2 pos, float size,
                     string label, string controlPath, TMP_FontAsset font, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent.transform, false);

        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = new Vector2(pos.x - anchor.x * 0, pos.y);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(size, size);

        var image = go.GetComponent<Image>();
        // The menus tint this sprite pure white; the pad keeps that tint but pulls the alpha down
        // so the level still reads through a control resting over it.
        image.sprite = sprite;
        image.color = sprite != null ? new Color(1f, 1f, 1f, 0.82f) : new Color(1f, 1f, 1f, 0.22f);
        image.preserveAspect = false;
        image.raycastTarget = true;

        var button = go.AddComponent<OnScreenButton>();
        button.controlPath = controlPath;

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var textRect = (RectTransform)textGo.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;

        var text = textGo.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.text = label;
        text.fontSize = size * 0.34f;
        text.alignment = TextAlignmentOptions.Center;
        // Menu labels are pure white on this sprite - match them rather than inventing a tint.
        text.color = Color.white;
        text.raycastTarget = false;
    }
}
