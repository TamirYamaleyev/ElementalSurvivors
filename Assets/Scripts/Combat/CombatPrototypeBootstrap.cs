using UnityEngine;
using UnityEngine.UI;

public class CombatPrototypeBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsurePrototypeCombatSetup()
    {
        if (Object.FindFirstObjectByType<CombatManager>() != null)
            return;

        BGMManager bgm = Object.FindFirstObjectByType<BGMManager>();
        if (bgm == null)
            bgm = new GameObject("BGMManager").AddComponent<BGMManager>();

        CombatUIManager uiManager = CreateRuntimeUI();
        CombatManager combatManager = new GameObject("CombatManager").AddComponent<CombatManager>();
        combatManager.ConfigureForRuntime(uiManager, bgm);
    }

    private static CombatUIManager CreateRuntimeUI()
    {
        // Always use a dedicated overlay canvas. Reusing an existing scene Canvas can fail
        // silently if that Canvas has zero scale or world-space sizing — combat UI then never shows.
        GameObject canvasGo = new GameObject("CombatCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRt = canvasGo.GetComponent<RectTransform>();
        canvasRt.anchorMin = Vector2.zero;
        canvasRt.anchorMax = Vector2.one;
        canvasRt.offsetMin = Vector2.zero;
        canvasRt.offsetMax = Vector2.zero;
        canvasRt.localScale = Vector3.one;

        GameObject root = CreatePanel("CombatRoot", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(1280, 720), Vector2.zero);
        GameObject playerPanel = CreatePanel("PlayerPanel", root.transform, new Vector2(0f, 1f), new Vector2(280, 120), new Vector2(140, -80));
        GameObject enemyPanel = CreatePanel("EnemyPanel", root.transform, new Vector2(1f, 1f), new Vector2(280, 120), new Vector2(-140, -80));
        CreatePanel("ArenaPanel", root.transform, new Vector2(0.5f, 0.5f), new Vector2(700, 220), new Vector2(0, 40));
        GameObject menuPanel = CreatePanel("CombatMenuPanel", root.transform, new Vector2(0.5f, 0f), new Vector2(980, 140), new Vector2(0, 90));

        CombatEntityHUD playerHud = playerPanel.AddComponent<CombatEntityHUD>();
        CombatEntityHUD enemyHud = enemyPanel.AddComponent<CombatEntityHUD>();

        Slider playerHp = CreateSlider("PlayerHP", playerPanel.transform, new Vector2(0, -20));
        Slider playerMp = CreateSlider("PlayerMP", playerPanel.transform, new Vector2(0, -50));
        Slider enemyHp = CreateSlider("EnemyHP", enemyPanel.transform, new Vector2(0, -20));

        AssignHUDSliders(playerHud, playerHp, playerMp);
        AssignHUDSliders(enemyHud, enemyHp, null);

        Button attack = CreateButton("Attack", menuPanel.transform, new Vector2(-300, 0));
        Button skill = CreateButton("Skills", menuPanel.transform, new Vector2(-100, 0));
        Button defend = CreateButton("Defend", menuPanel.transform, new Vector2(100, 0));
        Button run = CreateButton("Run", menuPanel.transform, new Vector2(300, 0));

        CombatLogView combatLog = BuildCombatLog(root.transform);

        CombatUIManager uiManager = root.AddComponent<CombatUIManager>();
        uiManager.ConfigureRuntime(root, playerHud, enemyHud, attack, skill, defend, run, combatLog);
        return uiManager;
    }

    private static CombatLogView BuildCombatLog(Transform root)
    {
        GameObject panel = new GameObject("CombatLogPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root, false);
        RectTransform prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0f, 0.5f);
        prt.anchorMax = new Vector2(0f, 0.5f);
        prt.pivot = new Vector2(0f, 0.5f);
        prt.anchoredPosition = new Vector2(20f, 32f);
        prt.sizeDelta = new Vector2(320f, 260f);
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        GameObject scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGo.transform.SetParent(panel.transform, false);
        StretchToParent(scrollGo.GetComponent<RectTransform>(), 6f);
        scrollGo.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.5f);
        ScrollRect scroll = scrollGo.GetComponent<ScrollRect>();

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollGo.transform, false);
        StretchToParent(viewport.GetComponent<RectTransform>(), 0f);
        Image vpImg = viewport.GetComponent<Image>();
        vpImg.color = new Color(0f, 0f, 0f, 0.12f);
        Mask mask = viewport.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        RectTransform crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0f, 120f);

        GameObject textGo = new GameObject("LogText", typeof(RectTransform), typeof(Text), typeof(ContentSizeFitter));
        textGo.transform.SetParent(content.transform, false);
        RectTransform trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 1f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = Vector2.zero;
        trt.sizeDelta = new Vector2(-10f, 0f);
        ContentSizeFitter fitter = textGo.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        Text txt = textGo.GetComponent<Text>();
        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtinFont != null)
            txt.font = builtinFont;
        txt.fontSize = 14;
        txt.color = Color.white;
        txt.alignment = TextAnchor.UpperLeft;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;

        scroll.content = crt;
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 22f;

        CombatLogView logView = panel.AddComponent<CombatLogView>();
        logView.Bind(txt, scroll, crt);
        return logView;
    }

    private static void StretchToParent(RectTransform rt, float padding)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
    }

    private static void AssignHUDSliders(CombatEntityHUD hud, Slider hp, Slider mp)
    {
        var hpField = typeof(CombatEntityHUD).GetField("hpSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var mpField = typeof(CombatEntityHUD).GetField("mpSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        hpField?.SetValue(hud, hp);
        mpField?.SetValue(hud, mp);
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 position)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = position;

        Image image = panel.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.55f);
        return panel;
    }

    private static Slider CreateSlider(string name, Transform parent, Vector2 anchoredPosition)
    {
        GameObject sliderGo = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(parent, false);

        RectTransform rt = sliderGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220, 18);
        rt.anchoredPosition = anchoredPosition;

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(sliderGo.transform, false);
        RectTransform bgRt = background.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        background.GetComponent<Image>().color = Color.black;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        RectTransform faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero;
        faRt.anchorMax = Vector2.one;
        faRt.offsetMin = new Vector2(4, 4);
        faRt.offsetMax = new Vector2(-4, -4);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = Color.green;

        Slider slider = sliderGo.GetComponent<Slider>();
        slider.fillRect = fillRt;
        slider.targetGraphic = fill.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;
        return slider;
    }

    private static Button CreateButton(string label, Transform parent, Vector2 anchoredPosition)
    {
        GameObject buttonGo = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(parent, false);

        RectTransform rt = buttonGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(170, 60);
        rt.anchoredPosition = anchoredPosition;

        Image image = buttonGo.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.85f);

        GameObject textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(buttonGo.transform, false);
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        Text text = textGo.GetComponent<Text>();
        text.text = label;
        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtinFont != null)
            text.font = builtinFont;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;

        return buttonGo.GetComponent<Button>();
    }
}
