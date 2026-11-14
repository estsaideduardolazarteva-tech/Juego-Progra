using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

// Herramientas para armar la interfaz por codigo.
// Todas las posiciones son proporciones (0 a 1) medidas desde la esquina superior izquierda.
public static class UIKit
{
    public static Color Hex(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString("#" + hex, out c);
        return c;
    }

    public static Font Fuente()
    {
#if UNITY_2022_2_OR_NEWER
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
    }

    // Carga una imagen de Assets/Resources y la convierte en Sprite.
    // unidadesAlto > 0 : la imagen mide esa cantidad de unidades de alto en el mundo.
    // pie = true : el punto de anclaje queda abajo al centro (para que la posicion sea donde pisa).
    public static Sprite CargarSprite(string nombre, float unidadesAlto = 0f, bool pixel = false, bool pie = false)
    {
        var tex = Resources.Load<Texture2D>(nombre);
        if (tex == null)
        {
            Debug.LogError("No encuentro la imagen '" + nombre + "' en Assets/Resources");
            return null;
        }
        tex.filterMode = pixel ? FilterMode.Point : FilterMode.Bilinear;
        float ppu = unidadesAlto > 0f ? tex.height / unidadesAlto : 100f;
        var pivote = pie ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivote, ppu, 0, SpriteMeshType.FullRect);
    }

    public static Sprite CargarSpriteBorde(string nombre, Vector4 borde)
    {
        var tex = Resources.Load<Texture2D>(nombre);
        if (tex == null) { Debug.LogError("No encuentro la imagen '" + nombre + "' en Assets/Resources"); return null; }
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, borde);
    }

    public static void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    // Barra deslizante con dibujos pixel art (fondo, relleno gris y mando naranja)
    public static Slider CrearSliderPixel(Transform padre, float x0, float y0, float x1, float y1,
                                          float min, float max, float valor, UnityAction<float> alCambiar)
    {
        var g = Nuevo("Barra", padre);
        Anclar(g.GetComponent<RectTransform>(), x0, y0, x1, y1);

        var fondo = Nuevo("Fondo", g.transform);
        Estirar(fondo.GetComponent<RectTransform>());
        var fondoImg = fondo.AddComponent<Image>();
        fondoImg.sprite = CargarSpriteBorde("ui_slider_fondo", new Vector4(8, 0, 8, 0));
        fondoImg.type = Image.Type.Sliced;

        var area = Nuevo("Area relleno", g.transform);
        Estirar(area.GetComponent<RectTransform>());
        var rel = Nuevo("Relleno", area.transform);
        var relImg = rel.AddComponent<Image>();
        relImg.sprite = CargarSpriteBorde("ui_slider_relleno", new Vector4(8, 0, 8, 0));
        relImg.type = Image.Type.Sliced;
        relImg.raycastTarget = false;
        var relRt = rel.GetComponent<RectTransform>();
        relRt.anchorMin = Vector2.zero; relRt.anchorMax = new Vector2(0.5f, 1f);
        relRt.offsetMin = Vector2.zero; relRt.offsetMax = Vector2.zero;

        var zona = Nuevo("Area mando", g.transform);
        Estirar(zona.GetComponent<RectTransform>());
        var mando = Nuevo("Mando", zona.transform);
        var manImg = mando.AddComponent<Image>();
        manImg.sprite = CargarSprite("ui_slider_mando", 0f, true);
        var manRt = mando.GetComponent<RectTransform>();
        manRt.anchorMin = new Vector2(0.5f, -0.3f);
        manRt.anchorMax = new Vector2(0.5f, 1.3f);
        manRt.offsetMin = Vector2.zero; manRt.offsetMax = Vector2.zero;
        var ar = mando.AddComponent<AspectRatioFitter>();
        ar.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        ar.aspectRatio = 36f / 52f;

        var s = g.AddComponent<Slider>();
        s.targetGraphic = fondoImg;
        s.transition = Selectable.Transition.None;
        var nav = s.navigation; nav.mode = Navigation.Mode.None; s.navigation = nav;
        s.fillRect = relRt;
        s.handleRect = manRt;
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = min; s.maxValue = max;
        s.SetValueWithoutNotify(valor);
        s.onValueChanged.AddListener(alCambiar);
        return s;
    }

    public static Sprite[] CargarFrames(string prefijo, int cantidad, float unidadesAlto, bool pie = true)
    {
        var lista = new Sprite[cantidad];
        for (int i = 0; i < cantidad; i++) lista[i] = CargarSprite(prefijo + i, unidadesAlto, false, pie);
        return lista;
    }

    public static GameObject Nuevo(string nombre, Transform padre)
    {
        var g = new GameObject(nombre, typeof(RectTransform));
        g.transform.SetParent(padre, false);
        return g;
    }

    public static void Anclar(RectTransform rt, float x0, float y0, float x1, float y1)
    {
        rt.anchorMin = new Vector2(x0, 1f - y1);
        rt.anchorMax = new Vector2(x1, 1f - y0);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static Canvas CrearCanvas(string nombre)
    {
        var go = new GameObject(nombre);
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();

        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }
        return c;
    }

    // Rectangulo que mantiene la proporcion de la imagen de fondo (con barras a los lados si hace falta).
    public static RectTransform Contenedor(Canvas canvas, float aspecto, Color barras)
    {
        Imagen(canvas.transform, "Barras", barras, 0, 0, 1, 1);
        var g = Nuevo("Contenedor", canvas.transform);
        var rt = g.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var f = g.AddComponent<AspectRatioFitter>();
        f.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        f.aspectRatio = aspecto;
        return rt;
    }

    public static Image Imagen(Transform padre, string nombre, Color color, float x0, float y0, float x1, float y1)
    {
        var g = Nuevo(nombre, padre);
        var im = g.AddComponent<Image>();
        im.color = color;
        Anclar(g.GetComponent<RectTransform>(), x0, y0, x1, y1);
        return im;
    }

    public static Image ImagenSprite(Transform padre, string nombre, Sprite sprite, float x0, float y0, float x1, float y1)
    {
        var im = Imagen(padre, nombre, Color.white, x0, y0, x1, y1);
        im.sprite = sprite;
        im.raycastTarget = false;
        return im;
    }

    public static Text Texto(Transform padre, string texto, int tamMax, Color color, TextAnchor alineacion,
                             float x0, float y0, float x1, float y1)
    {
        var g = Nuevo("Texto", padre);
        var t = g.AddComponent<Text>();
        t.font = Fuente();
        t.text = texto;
        t.color = color;
        t.alignment = alineacion;
        t.fontStyle = FontStyle.Bold;
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = 8;
        t.resizeTextMaxSize = tamMax;
        t.raycastTarget = false;
        Anclar(g.GetComponent<RectTransform>(), x0, y0, x1, y1);
        return t;
    }

    // Boton transparente que se pone ENCIMA de un boton dibujado en la imagen.
    public static Button BotonInvisible(Transform padre, string nombre, float x0, float y0, float x1, float y1, UnityAction accion)
    {
        var im = Imagen(padre, nombre, Color.white, x0, y0, x1, y1);
        var b = im.gameObject.AddComponent<Button>();
        b.targetGraphic = im;
        var cb = b.colors;
        cb.normalColor = new Color(1, 1, 1, 0f);
        cb.highlightedColor = new Color(1, 1, 1, 0.18f);
        cb.pressedColor = new Color(1, 1, 1, 0.35f);
        cb.selectedColor = new Color(1, 1, 1, 0f);
        cb.disabledColor = new Color(1, 1, 1, 0f);
        cb.fadeDuration = 0.08f;
        b.colors = cb;
        b.onClick.AddListener(accion);
        return b;
    }

    public static Button BotonSolido(Transform padre, string texto, Color color, float x0, float y0, float x1, float y1, UnityAction accion)
    {
        var im = Imagen(padre, "Boton " + texto, color, x0, y0, x1, y1);
        var b = im.gameObject.AddComponent<Button>();
        b.targetGraphic = im;
        b.onClick.AddListener(accion);
        Texto(im.transform, texto, 48, Color.white, TextAnchor.MiddleCenter, 0.05f, 0.1f, 0.95f, 0.9f);
        return b;
    }

    public static Slider CrearSlider(Transform padre, float x0, float y0, float x1, float y1,
                                     float min, float max, float valor, UnityAction<float> alCambiar)
    {
        var g = DefaultControls.CreateSlider(new DefaultControls.Resources());
        g.transform.SetParent(padre, false);
        Anclar(g.GetComponent<RectTransform>(), x0, y0, x1, y1);
        var s = g.GetComponent<Slider>();
        s.minValue = min; s.maxValue = max;
        s.SetValueWithoutNotify(valor);
        s.onValueChanged.AddListener(alCambiar);

        foreach (var im in g.GetComponentsInChildren<Image>())
        {
            if (im.gameObject.name == "Background") im.color = Hex("35405E");
            else if (im.gameObject.name == "Fill") im.color = Hex("FFD166");
            else if (im.gameObject.name == "Handle") im.color = Color.white;
        }
        if (s.handleRect != null) s.handleRect.sizeDelta = new Vector2(30f, 0f);
        return s;
    }
}
