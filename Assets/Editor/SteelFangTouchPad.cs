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

        // The gameplay Canvas is Constant Pixel Size and the page pins the
        // framebuffer to 1920x1200, so these sizes are stable on every screen.
        // 230px of 1920 is about 12% of the width, which lands near 47 CSS px
        // on a 390px-wide phone - just over the 44px comfortable tap target.
        Make(root, "Left",  new Vector2(0, 0), new Vector2( 200,  200), 230, "<",     "<Keyboard>/a",         font);
        Make(root, "Right", new Vector2(0, 0), new Vector2( 470,  200), 230, ">",     "<Keyboard>/d",         font);
        Make(root, "Jump",  new Vector2(1, 0), new Vector2(-200,  200), 260, "JUMP",  "<Keyboard>/space",     font);
        Make(root, "Dash",  new Vector2(1, 0), new Vector2(-470,  250), 200, "DASH",  "<Keyboard>/leftShift", font);
        Make(root, "Pause", new Vector2(1, 1), new Vector2(-120, -120), 140, "II",    "<Keyboard>/escape",    font);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("@@PAD built " + root.transform.childCount + " buttons");
        EditorApplication.Exit(0);
    }

    static void Make(GameObject parent, string name, Vector2 anchor, Vector2 pos, float size,
                     string label, string controlPath, TMP_FontAsset font)
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
        image.color = new Color(1f, 1f, 1f, 0.22f);
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
        text.color = new Color(1f, 1f, 1f, 0.85f);
        text.raycastTarget = false;
    }
}
