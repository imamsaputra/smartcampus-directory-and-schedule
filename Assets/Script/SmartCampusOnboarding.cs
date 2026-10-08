using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// SmartCampus - Welcome Screen (1 layar, dengan animasi masuk).
/// Tekan Play -> welcome screen otomatis muncul.
/// </summary>
public class SmartCampusOnboarding : MonoBehaviour
{
    [Header("Teks")]
    public string salam = "Selamat Datang di";
    public string judulDepan = "Smart";
    public string judulBelakang = "Campus";
    public string subjudul = "Direktori & jadwal kampus\ndalam satu aplikasi";
    public string[] fitur = { "Direktori", "Jadwal", "Info Kampus" };
    public string teksTombol = "Mulai Sekarang";
    public string footer = "Tugas Pemrograman Mobile";

    [Header("Opsional")]
    [Tooltip("Nama scene yang dibuka setelah menekan tombol. Kosongkan jika belum ada.")]
    public string nextSceneName = "";
    public Font customFont;
    public UnityEvent onFinished;

    // true = otomatis muncul saat Play (tanpa perlu menempel script)
    const bool AutoRun = true;
    static bool finished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { finished = false; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoStart()
    {
        if (!AutoRun || finished) return;
#if UNITY_2023_1_OR_NEWER
        var existing = FindFirstObjectByType<SmartCampusOnboarding>();
#else
        var existing = FindObjectOfType<SmartCampusOnboarding>();
#endif
        if (existing != null) return;
        new GameObject("SmartCampusOnboarding (Auto)").AddComponent<SmartCampusOnboarding>();
    }

    // ---------- Warna ----------
    static readonly Color Bg        = new Color32(0xF3, 0xF0, 0xFD, 255);
    static readonly Color Purple    = new Color32(0x8B, 0x6C, 0xF0, 255);
    static readonly Color PurpleLight = new Color32(0xCF, 0xC3, 0xF8, 255);
    static readonly Color LogoFill  = new Color32(0x6C, 0x4D, 0xE0, 255);
    static readonly Color Navy      = new Color32(0x2B, 0x21, 0x60, 255);
    static readonly Color Gray      = new Color32(0x7A, 0x75, 0x99, 255);
    static readonly Color Orange    = new Color32(0xFF, 0x9A, 0x3D, 255);
    static readonly Color Teal      = new Color32(0x2E, 0xC4, 0xB6, 255);

    // ---------- State ----------
    class Item { public RectTransform rt; public CanvasGroup cg; public Vector2 b; public float delay; }
    readonly List<Item> items = new List<Item>();
    Sprite roundSprite, circleSprite;
    Font font;
        float startTime;

    // =====================================================================
    void Start()
    {
        startTime = Time.unscaledTime;
        roundSprite = MakeSprite(128, 48);
        circleSprite = MakeSprite(128, 64);

        if (customFont != null) font = customFont;
        else
        {
#if UNITY_2022_2_OR_NEWER
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        }

        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        // Canvas
        var cgo = new GameObject("WelcomeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        cgo.transform.SetParent(transform, false);
        cgo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Background + dekorasi
        var bg = Box(cgo.transform, "Background", 0, 0, 10, 10, Bg, 0);
        bg.anchorMin = Vector2.zero; bg.anchorMax = Vector2.one;
        bg.offsetMin = Vector2.zero; bg.offsetMax = Vector2.zero;
        Circ(bg, "Deco1", -420, 800, 620, new Color(Purple.r, Purple.g, Purple.b, 0.09f));
        Circ(bg, "Deco2", 440, 660, 300, new Color(Teal.r, Teal.g, Teal.b, 0.12f));
        Circ(bg, "Deco3", 470, -700, 520, new Color(Orange.r, Orange.g, Orange.b, 0.10f));
        Circ(bg, "Deco4", -480, -420, 220, new Color(Purple.r, Purple.g, Purple.b, 0.08f));

        BuildLogo(bg);

        // Ucapan selamat datang
        var tSalam = Txt(bg, salam, 54, Gray, 0, 95, 1000, 80, false, false);
        Reveal(tSalam.rectTransform, 0.15f);

        var tTitle = Txt(bg, "<color=#8B6CF0>" + judulDepan + "</color>" + judulBelakang,
                         124, Navy, 0, -25, 1040, 160, true, true);
        Reveal(tTitle.rectTransform, 0.28f);

        var tSub = Txt(bg, subjudul, 42, Gray, 0, -160, 960, 130, false, false);
        Reveal(tSub.rectTransform, 0.40f);

        // Chip fitur
        var chips = Container(bg, "Chips", 0, -330);
        Reveal(chips, 0.52f);
        Color[] cc = { Purple, Orange, Teal };
        int n = fitur.Length;
        for (int i = 0; i < n; i++)
        {
            float x = (i - (n - 1) / 2f) * 290f;
            var pill = Box(chips, "Chip" + i, x, 0, 272, 92, Color.white, -1);
            AddShadow(pill.gameObject, 8, 0.15f);
            Circ(pill, "Dot", -96, 0, 34, cc[i % 3]);
            Txt(pill, fitur[i], 32, Navy, 24, 0, 200, 92, true, false);
        }

        // Tombol
        var btn = Box(bg, "StartButton", 0, -590, 780, 150, Purple, -1);
        AddShadow(btn.gameObject, 14, 0.40f);
        var img = btn.GetComponent<Image>();
        img.raycastTarget = true;
        var b = btn.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(Finish);
        var cb = b.colors;
        cb.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        cb.highlightedColor = new Color(0.94f, 0.94f, 0.94f, 1f);
        b.colors = cb;
        Txt(btn, teksTombol, 54, Color.white, 0, 0, 760, 150, true, false);

        // Footer
        var tFoot = Txt(bg, footer, 28, new Color(Gray.r, Gray.g, Gray.b, 0.85f), 0, -770, 900, 50, false, false);
        Reveal(tFoot.rectTransform, 0.80f);
    }

    // ---------------------------------------------------------------------
    //  LOGO: lencana bulat dengan topi toga (statis, tanpa animasi)
    // ---------------------------------------------------------------------
    void BuildLogo(Transform bg)
    {
        var logo = Container(bg, "Logo", 0, 420);

        // cincin dekoratif (diam)
        Circ(logo, "Ring", 0, 0, 500, new Color(Purple.r, Purple.g, Purple.b, 0.16f));
        Circ(logo, "RingCover", 0, 0, 478, Bg);
        Circ(logo, "Dot1", -220, -105, 28, Orange);
        Circ(logo, "Dot2", 200, 140, 20, Teal);
        Circ(logo, "Dot3", 60, -236, 16, Purple);

        // badan logo
        var main = Circ(logo, "Main", 0, 0, 380, Purple);
        AddShadow(main.gameObject, 16, 0.35f);
        Circ(logo, "Inner", 0, 0, 336, LogoFill);

        // ----- topi toga -----
        // bagian topi (di bawah papan)
        Box(logo, "CapBase", 0, -34, 150, 90, new Color32(0xE4, 0xDC, 0xFF, 255), 38);

        // ketebalan papan (belah ketupat gepeng)
        var edgeHolder = Container(logo, "BoardEdgeHolder", 0, 10);
        edgeHolder.localScale = new Vector3(1f, 0.5f, 1f);
        var edge = Box(edgeHolder, "BoardEdge", 0, 0, 205, 205, PurpleLight, 0);
        edge.localRotation = Quaternion.Euler(0, 0, 45);

        // papan atas
        var boardHolder = Container(logo, "BoardHolder", 0, 28);
        boardHolder.localScale = new Vector3(1f, 0.5f, 1f);
        var board = Box(boardHolder, "Board", 0, 0, 205, 205, Color.white, 0);
        board.localRotation = Quaternion.Euler(0, 0, 45);

        // kancing tengah
        Circ(logo, "Button", 0, 28, 24, Purple);

        // tali + rumbai
        Box(logo, "Cord", 64, 28, 128, 5, Orange, 2);
        Box(logo, "CordDown", 128, -4, 5, 64, Orange, 2);
        Box(logo, "Tassel", 128, -56, 20, 40, Orange, 9);

        // badge sinyal (smart)
        var badge = Circ(logo, "Badge", 138, 140, 104, Orange);
        AddShadow(badge.gameObject, 8, 0.30f);
        Box(badge, "S1", -22, -8, 12, 18, Color.white, 6);
        Box(badge, "S2", 0, -2, 12, 30, Color.white, 6);
        Box(badge, "S3", 22, 4, 12, 42, Color.white, 6);
    }

    // =====================================================================
    void Update()
    {
        float t = Time.unscaledTime - startTime;
        foreach (var it in items)
        {
            float k = Mathf.Clamp01((t - it.delay) / 0.7f);
            float e = 1f - Mathf.Pow(1f - k, 3f);
            it.cg.alpha = e;
            it.rt.anchoredPosition = it.b + new Vector2(0, -(1f - e) * 70f);
        }
    }

    void Finish()
    {
        finished = true;
        Debug.Log("[SmartCampus] Tombol Mulai ditekan");
        onFinished?.Invoke();
        if (!string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    // =====================================================================
    //  HELPER
    // =====================================================================
    void Reveal(RectTransform rt, float delay)
    {
        var cg = rt.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        items.Add(new Item { rt = rt, cg = cg, b = rt.anchoredPosition, delay = delay });
    }

    RectTransform Container(Transform p, string n, float x, float y)
    {
        var rt = new GameObject(n, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(p, false);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = Vector2.zero;
        return rt;
    }

    // corner: 0 = kotak biasa, -1 = pill (setengah sisi terpendek), >0 = radius tertentu
    RectTransform Box(Transform p, string n, float x, float y, float w, float h, Color c, float corner)
    {
        var go = new GameObject(n, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(p, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        var img = go.GetComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        if (corner != 0f)
        {
            float maxR = Mathf.Min(w, h) / 2f;
            float r = corner < 0f ? maxR : Mathf.Min(corner, maxR);
            img.sprite = roundSprite;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 48f / Mathf.Max(r, 1f);
        }
        return rt;
    }

    RectTransform Circ(Transform p, string n, float x, float y, float d, Color c)
    {
        var rt = Box(p, n, x, y, d, d, c, 0);
        rt.GetComponent<Image>().sprite = circleSprite;
        return rt;
    }

    Text Txt(Transform p, string s, int size, Color c, float x, float y, float w, float h, bool bold, bool rich)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var rt = (RectTransform)go.transform;
        rt.SetParent(p, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = s;
        t.fontSize = size;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.color = c;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.supportRichText = rich;
        t.raycastTarget = false;
        return t;
    }

    void AddShadow(GameObject go, float dist, float alpha)
    {
        var sh = go.AddComponent<Shadow>();
        sh.effectColor = new Color(0.30f, 0.20f, 0.65f, alpha);
        sh.effectDistance = new Vector2(0, -dist);
    }

    static Sprite MakeSprite(int size, int radius)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float fx = x + 0.5f, fy = y + 0.5f;
                float cx = Mathf.Clamp(fx, radius, size - radius);
                float cy = Mathf.Clamp(fy, radius, size - radius);
                float d = Mathf.Sqrt((fx - cx) * (fx - cx) + (fy - cy) * (fy - cy));
                px[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(radius - d + 0.5f) * 255f));
            }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                             SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}
