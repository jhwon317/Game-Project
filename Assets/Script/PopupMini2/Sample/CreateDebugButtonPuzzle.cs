#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using PopupMini;

public static class CreateDebugButtonPuzzle
{
    const string RootFolder = "Assets/PopupMini/Samples";
    const string PrefabPath = RootFolder + "/DebugButtonPuzzle.prefab";
    const string DefPath = RootFolder + "/DebugButtonPuzzleDef.asset";
    const string ArgsPath = RootFolder + "/DebugButtonArgs.asset";

    [MenuItem("Tools/PopupMini/Create Sample: DebugButtonPuzzle (Prefab + Definition)")]
    public static void CreateAll()
    {
        // 폴더 준비
        System.IO.Directory.CreateDirectory(RootFolder);

        // === 루트 GO ===
        var root = new GameObject("DebugButtonPuzzle");
        Undo.RegisterCreatedObjectUndo(root, "Create DebugButtonPuzzle");

        // 레이어(선택): MiniWorld 있으면 적용
        int miniLayer = LayerMask.NameToLayer("MiniWorld");
        if (miniLayer >= 0) SetLayerRecursive(root, miniLayer);

        // === 카메라 ===
        var camGO = new GameObject("PuzzleCam");
        camGO.transform.SetParent(root.transform, false);
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;      // 2D UI만 쓴다면 편함
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.nearClipPlane = 0.01f; cam.farClipPlane = 100f;
        cam.transform.localPosition = new Vector3(0, 0, -5f);
        cam.transform.localRotation = Quaternion.identity;

        // === World Space Canvas (프리팹 내부 UI) ===
        var canvasGO = new GameObject("Canvas", typeof(RectTransform));
        canvasGO.transform.SetParent(root.transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;

        var gr = canvasGO.AddComponent<GraphicRaycaster>();
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        // 캔버스 크기/위치(월드 1unit = 100px 가정)
        var crt = canvasGO.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(1920, 1080);
        crt.localScale = Vector3.one * 0.001f;   // 1920x1080 → 1.92x1.08 world
        crt.localPosition = Vector3.zero;

        // === 배경 패널 ===
        var panel = CreateUI<Image>("Panel", canvasGO.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1600, 900));
        panel.color = new Color(0.95f, 0.95f, 0.95f, 1f);

        // === 라벨 ===
        var label = CreateUI<Text>("Label", panel.transform, new Vector2(0.5f, 0.8f), Vector2.zero, new Vector2(800, 80));
        label.text = "Debug Button Puzzle";
        label.alignment = TextAnchor.MiddleCenter;
        label.fontSize = 48;
        label.color = Color.black;

        // === OK 버튼 ===
        var okBtn = CreateButton(panel.transform, "OK", new Vector2(0.5f, 0.25f), new Vector2(-150, 0));
        // === Cancel 버튼 ===
        var cancelBtn = CreateButton(panel.transform, "Cancel", new Vector2(0.5f, 0.25f), new Vector2(150, 0));

        // === 컨트롤러 ===
        var ctrl = root.AddComponent<PopupMini.Sample.SimplePuzzleController>();
        ctrl.OkButton = okBtn;
        ctrl.CancelButton = cancelBtn;

        // === (선택) 기본 Args TextAsset ===
        var args = new TextAsset("{\"sample\":\"hello\"}");
        AssetDatabase.CreateAsset(args, ArgsPath);

        var cfg = root.AddComponent<PuzzlePrefabConfig>();
        cfg.defaultArgsJson = args;  // 요청 Args가 비어있으면 이 값 사용

        // === Prefab 저장 ===
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out bool prefabSuccess);
        Object.DestroyImmediate(root);
        if (!prefabSuccess || prefab == null)
        {
            Debug.LogError("[CreateDebugButtonPuzzle] Prefab save failed");
            return;
        }

        // === Definition(SO) 생성 ===
        var def = AssetDatabase.LoadAssetAtPath<PuzzleDefinition>(DefPath);
        if (!def)
        {
            def = ScriptableObject.CreateInstance<PuzzleDefinition>();
            AssetDatabase.CreateAsset(def, DefPath);
        }

        def.Prefab = prefab;
        def.AspectMode = AspectMode.FitContain;
        def.Aspect = 16f / 9f;
        def.AntiAliasing = 1;
        def.FilterMode = FilterMode.Bilinear;
        def.BackgroundColor = new Color(0, 0, 0, 0);
        def.Modal = true;
        def.BackdropClosable = false;
        def.TimeoutSec = 0f;
        def.ShadowsOff = true;

        EditorUtility.SetDirty(def);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CreateDebugButtonPuzzle] Done.\nPrefab: {PrefabPath}\nDefinition: {DefPath}");
    }

    static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform c in go.transform) SetLayerRecursive(c.gameObject, layer);
    }

    static T CreateUI<T>(string name, Transform parent, Vector2 anchor, Vector2 anchoredPos, Vector2 size) where T : Graphic
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var g = go.AddComponent<T>();
        g.raycastTarget = true;
        return g;
    }

    static Button CreateButton(Transform parent, string text, Vector2 anchor, Vector2 anchoredPos)
    {
        // Button root
        var btnGO = new GameObject($"{text}Button", typeof(RectTransform));
        btnGO.transform.SetParent(parent, false);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(300, 110);

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.55f, 1f, 1f);

        var btn = btnGO.AddComponent<Button>();

        // Text child
        var txt = CreateUI<Text>("Text", btnGO.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 80));
        txt.text = text;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 36;
        txt.color = Color.white;

        // Highlight colors
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.25f, 0.65f, 1f, 1f);
        colors.pressedColor = new Color(0.15f, 0.45f, 0.9f, 1f);
        btn.colors = colors;

        return btn;
    }
}
#endif