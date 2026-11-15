using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// NIVEL 1 - Avenida Camacho. Arma todo el nivel por código y controla vidas, monedas, tiempo,
// pausa, derrota, victoria y la Minibús Trivia generada dinámicamente.
public class Nivel1 : MonoBehaviour
{
    public static Nivel1 Instancia;

    // ---- Ajustes que puedes cambiar ----
    const float Largo = 108f;        // largo del nivel
    const float SueloY = -3.5f;      // altura del piso (coincide con la vereda del fondo)
    const float TiempoTotal = 90f;   // segundos para llegar al minibus
    const int VidasIniciales = 3;
    const float BusX = 104.5f;       // centro del minibus (la meta)
    // -------------------------------------

    enum Estado { Jugando, Ganado, Perdido }
    Estado estado = Estado.Jugando;

    int vidas = VidasIniciales;
    int monedas;
    float tiempo = TiempoTotal;
    float invulnerable;
    float tFin;
    float tAyuda = 7f;
    string mensaje = "";
    bool pausado;
    bool pantallaFin;

    Jugador jugador;
    CamaraSigue camSigue;
    SpriteRenderer busSr;
    Sprite busCerrado;
    Sprite sprMonedaFrente, sprMonedaDorso;
    Sprite cuadro;
    Texture2D texCorazon, texCorazonVacio, texMoneda;
    GameObject pantallaPausa;
    OpcionesVista opciones;
    readonly float[] checkpoints = { 2f, 26f, 52.5f, 80f };
    float puntoRespawn = 2f;

    // ================= VARIABLES DE LA MARRAQUETA TRIVIA =================
    private GameObject panelTrivia;
    private TextMeshProUGUI textoPregunta;
    private Button[] botonesOpciones = new Button[3];
    private TextMeshProUGUI textoFeedback;
    private RectTransform minibusIcono;

    private float[] posicionesParadasX = { -150f, 0f, 150f };
    private int limiteUsosPorNivel = 2; // Máximo 2 trivias por nivel
    private int triviasJugadas = 0;

    private bool triviaAbierta;
    private int preguntaActualIndex = 0;
    private const int totalParadasPorRuta = 3;

    private string[] bancoPreguntas = {
        "¿Qué gritas cuando el minibús va lleno y quieres bajar a media cuadra?",
        "¿Cuál es la ley climática inquebrantable a las 2 de la tarde en La Paz?",
        "¿Con qué se acompaña tradicionalmente un buen api con pastel en la mañana?",
        "¿Cómo se llama el pan redondo y crujiente tradicional de la hoyada?",
        "¿Qué famoso mercado tradicional es conocido por vender amuletos y ekekos?",
        "¿Cuál es el nombre de la imponente montaña blanca que vigila la ciudad?",
        "¿Cómo se llama el bus municipal de transporte masivo de la ciudad?",
        "¿Cuál es el mirador turístico famoso por sus coloridos techos y vista panorámica?"
    };

    private string[][] bancoOpciones = {
        new string[] { "¡Con su vuelto, chofer!", "¡Baja por favor!", "¡Esquina, m'hijo!" },
        new string[] { "Frío polar todo el día", "Sol rajante y granizada a las 4", "Neblina silenciosa" },
        new string[] { "Con refresco de durazno", "Con charla y apapudo", "Solo con agua" },
        new string[] { "Marraqueta", "Sarna", "Hallulla" },
        new string[] { "Mercado Lanza", "Mercado de Brujas", "Mercado Rodríguez" },
        new string[] { "Illimani", "Huayna Potosí", "Mururata" },
        new string[] { "Pumakatari", "Sariri", "Chikita" },
        new string[] { "Killi Killi", "Jallpha La Paz", "Sopocachi Alto" }
    };

    private int[] bancoRespuestas = { 2, 1, 1, 0, 1, 0, 0, 0 };
    private List<int> preguntasSeleccionadasRuta = new List<int>();
    // ====================================================================

    GUIStyle estilo;

    public bool Invulnerable { get { return invulnerable > 0f; } }
    public bool Pausado { get { return pausado; } }

    void Awake() { Instancia = this; }

    void Start()
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, Color.white);
        t.Apply();
        cuadro = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        sprMonedaFrente = UIKit.CargarSprite("moneda_frente", 0.6f);
        sprMonedaDorso = UIKit.CargarSprite("moneda_dorso", 0.6f);
        texMoneda = Resources.Load<Texture2D>("moneda_frente");
        texCorazon = CrearCorazon(new Color(0.92f, 0.16f, 0.2f));
        texCorazonVacio = CrearCorazon(new Color(0.35f, 0.35f, 0.4f));

        CrearFondo();
        CrearSuelo();
        CrearObstaculos();
        CrearMonedas();
        CrearEnemigos();
        CrearMeta();
        CrearJugador();
        CrearCamara();

        // Creamos la UI de la trivia por código para no tocar nada en el editor
        CrearPanelTriviaPorCodigo();
    }

    // ================= CONSTRUCCION DEL NIVEL =================

    void CrearFondo()
    {
        var sprite = UIKit.CargarSprite("fondo_nivel1", 10f);
        float ancho = sprite.bounds.size.x;
        int cantidad = Mathf.CeilToInt((Largo + 30f) / ancho) + 1;
        for (int i = 0; i < cantidad; i++)
        {
            var g = new GameObject("Fondo " + i);
            g.transform.SetParent(transform);
            g.transform.position = new Vector3(-8f + ancho * (i + 0.5f), 0f, 0f);
            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
        }
    }

    void CrearSuelo()
    {
        float[,] tramos = { { -6f, 22f }, { 25f, 48f }, { 51.5f, 75f }, { 79f, Largo } };
        var spHoyo = UIKit.CargarSprite("obs_hoyo", 1f);
        float anchoHoyo = spHoyo.bounds.size.x;

        for (int i = 0; i < tramos.GetLength(0); i++)
        {
            float a = tramos[i, 0], b = tramos[i, 1];
            Caja("Piso " + i, new Vector2((a + b) / 2f, SueloY - 2f), new Vector2(b - a, 4f));

            if (i + 1 < tramos.GetLength(0))
            {
                float h0 = b, h1 = tramos[i + 1, 0];
                var g = new GameObject("Hoyo " + i);
                g.transform.SetParent(transform);
                g.transform.position = new Vector2((h0 + h1) / 2f, -4.4f);
                g.transform.localScale = new Vector3((h1 - h0) / anchoHoyo, 2.3f, 1f);
                var sr = g.AddComponent<SpriteRenderer>();
                sr.sprite = spHoyo;
                sr.sortingOrder = 1;
            }
        }
        Caja("Pared izquierda", new Vector2(-6.5f, 0f), new Vector2(1f, 30f));
        Caja("Pared derecha", new Vector2(Largo + 4.5f, 0f), new Vector2(1f, 30f));
    }

    void CrearObstaculos()
    {
        Decorado("obs_bandera", 0.6f, 2.6f);
        Obstaculo("obs_graffiti", 10f, 1.6f, 1.0f, 0.95f, false);
        Obstaculo("obs_riel", 29.5f, 1.9f, 0.69f, 0.92f, false);
        Obstaculo("obs_caja", 45f, 1.1f, 1.0f, 0.95f, false);
        Obstaculo("obs_maceta", 58f, 1.7f, 0.62f, 0.9f, false);
        Obstaculo("obs_graffiti", 62.5f, 2.0f, 1.0f, 0.95f, false);
        Obstaculo("obs_barrera_roja", 67.5f, 1.0f, 0.95f, 0.9f, false);
        Obstaculo("obs_caja", 90.5f, 1.1f, 1.0f, 0.95f, false);
        Obstaculo("obs_barril", 92.6f, 1.1f, 1.0f, 0.9f, false);

        Obstaculo("obs_cono", 14f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_cono", 41.5f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_pinchos", 55f, 0.7f, 0.8f, 0.8f, true);
        Obstaculo("obs_rodillo", 86f, 0.9f, 0.8f, 0.8f, true);
        Obstaculo("obs_cono", 96f, 0.9f, 0.85f, 0.6f, true);

        Plataforma(35.3f, 5f, 3.0f);
        Plataforma(73.3f, 5f, 3.0f);
    }

    void CrearMonedas()
    {
        Linea(4f, 7f, 4, 1.4f);
        Moneda(10f, 2.9f);
        Moneda(13f, 2.1f); Moneda(14f, 2.7f); Moneda(15f, 2.1f);
        Moneda(22.8f, 2.4f); Moneda(23.5f, 2.9f); Moneda(24.2f, 2.4f);
        Moneda(29.5f, 2.7f);
        Linea(33.3f, 37.3f, 5, 3.9f);
        Moneda(40.5f, 2.1f); Moneda(41.5f, 2.7f); Moneda(42.5f, 2.1f);
        Moneda(45f, 2.5f);
        Moneda(49f, 2.4f); Moneda(49.75f, 2.9f); Moneda(50.5f, 2.4f);
        Moneda(54f, 2.1f); Moneda(55f, 2.7f); Moneda(56f, 2.1f);
        Moneda(58f, 2.4f);
        Moneda(62.5f, 3.4f);
        Moneda(67.5f, 2.4f);
        Linea(71.3f, 75.3f, 5, 3.9f);
        Moneda(76.3f, 2.4f); Moneda(77f, 2.9f); Moneda(77.7f, 2.4f);
        Moneda(85f, 2.1f); Moneda(86f, 2.8f); Moneda(87f, 2.1f);
        Moneda(90.5f, 2.5f); Moneda(92.6f, 2.5f);
        Moneda(95f, 2.1f); Moneda(96f, 2.7f); Moneda(97f, 2.1f);
    }

    void CrearEnemigos()
    {
        NuevoEnemigo(16f, 21f);
        NuevoEnemigo(33f, 40f);
        NuevoEnemigo(70f, 74.5f);
        NuevoEnemigo(94f, 99.5f);
    }

    void CrearMeta()
    {
        float ancho = 6.4f;
        float alto = ancho * 430f / 900f;
        var sprAbierto = UIKit.CargarSprite("minibus_abierto", alto, false, true);
        busCerrado = UIKit.CargarSprite("minibus_cerrado", alto, false, true);

        var bus = new GameObject("Minibus");
        bus.transform.SetParent(transform);
        bus.transform.position = new Vector2(BusX, SueloY - 0.05f);
        busSr = bus.AddComponent<SpriteRenderer>();
        busSr.sprite = sprAbierto;
        busSr.sortingOrder = 3;

        var g = new GameObject("Meta");
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(BusX - 1.6f, SueloY + 2.5f);
        var bc = g.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(2.2f, 5f);
        g.AddComponent<Pickup>().tipo = TipoPickup.Meta;
    }

    void CrearJugador()
    {
        var idle = UIKit.CargarFrames("player_idle_", 4, 2.5f);
        var run = UIKit.CargarFrames("player_run_", 8, 2.5f);
        var jump = UIKit.CargarFrames("player_jump_", 8, 2.5f);

        var g = new GameObject("Jugador");
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(2f, SueloY + 0.05f);

        var sr = g.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 6;

        var rb = g.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3.5f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var bc = g.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(0.65f, 2.1f);
        bc.offset = new Vector2(0f, 1.05f);
        var sinFriccion = new PhysicsMaterial2D("SinFriccion");
        sinFriccion.friction = 0f;
        sinFriccion.bounciness = 0f;
        bc.sharedMaterial = sinFriccion;
        rb.sharedMaterial = sinFriccion;

        jugador = g.AddComponent<Jugador>();
        jugador.Iniciar(idle, run, jump);
    }

    void CrearCamara()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var g = new GameObject("Main Camera");
            g.tag = "MainCamera";
            cam = g.AddComponent<Camera>();
        }
        if (cam.GetComponent<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();

        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = UIKit.Hex("3B8FE0");

        camSigue = cam.GetComponent<CamaraSigue>();
        if (camSigue == null) camSigue = cam.gameObject.AddComponent<CamaraSigue>();
        camSigue.objetivo = jugador.transform;
        camSigue.minX = -6f;
        camSigue.maxX = Largo + 4f;

        float mitad = cam.orthographicSize * cam.aspect;
        cam.transform.position = new Vector3(-6f + mitad, 0f, -10f);
    }

    // ================= CREACION DE LA TRIVIA POR CODIGO =================
    void CrearPanelTriviaPorCodigo()
    {
        // Canvas independiente para la Trivia con orden alto para que tape todo
        var canvasObj = UIKit.CrearCanvas("CanvasTriviaDinamico");
        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.sortingOrder = 200;
        canvasObj.transform.SetParent(transform, false);

        // Panel de fondo semi-transparente oscuro
        panelTrivia = new GameObject("PanelFondoTrivia");
        panelTrivia.transform.SetParent(canvasObj.transform, false);
        var imgFondo = panelTrivia.AddComponent<Image>();
        imgFondo.color = new Color(0f, 0f, 0f, 0.85f);
        var rtFondo = panelTrivia.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.sizeDelta = Vector2.zero;

        // Texto de Pregunta
        var objPregunta = new GameObject("TextoPregunta");
        objPregunta.transform.SetParent(panelTrivia.transform, false);
        textoPregunta = objPregunta.AddComponent<TextMeshProUGUI>();
        textoPregunta.fontSize = 32;
        textoPregunta.alignment = TextAlignmentOptions.Center;
        textoPregunta.color = Color.white;
        var rtPregunta = objPregunta.GetComponent<RectTransform>();
        rtPregunta.anchorMin = new Vector2(0.1f, 0.6f);
        rtPregunta.anchorMax = new Vector2(0.9f, 0.85f);
        rtPregunta.sizeDelta = Vector2.zero;

        // Botones de Opciones (3 botones)
        float posY = 0.45f;
        for (int i = 0; i < 3; i++)
        {
            var btnObj = new GameObject("BotonOpcion_" + i);
            btnObj.transform.SetParent(panelTrivia.transform, false);
            var imgBtn = btnObj.AddComponent<Image>();
            imgBtn.color = new Color(0.2f, 0.4f, 0.8f);
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = imgBtn;

            var rtBtn = btnObj.GetComponent<RectTransform>();
            rtBtn.anchorMin = new Vector2(0.25f, posY - 0.1f);
            rtBtn.anchorMax = new Vector2(0.75f, posY);
            rtBtn.sizeDelta = Vector2.zero;

            // Texto dentro del botón
            var txtBtnObj = new GameObject("TextoBtn");
            txtBtnObj.transform.SetParent(btnObj.transform, false);
            var txtBtn = txtBtnObj.AddComponent<TextMeshProUGUI>();
            txtBtn.fontSize = 24;
            txtBtn.alignment = TextAlignmentOptions.Center;
            txtBtn.color = Color.white;
            var rtTxt = txtBtnObj.GetComponent<RectTransform>();
            rtTxt.anchorMin = Vector2.zero;
            rtTxt.anchorMax = Vector2.one;
            rtTxt.sizeDelta = Vector2.zero;

            botonesOpciones[i] = btn;
            posY -= 0.13f;
        }

        // Texto de Feedback (Acierto o Trancón)
        var objFeedback = new GameObject("TextoFeedback");
        objFeedback.transform.SetParent(panelTrivia.transform, false);
        textoFeedback = objFeedback.AddComponent<TextMeshProUGUI>();
        textoFeedback.fontSize = 28;
        textoFeedback.alignment = TextAlignmentOptions.Center;
        textoFeedback.color = Color.yellow;
        var rtFeedback = objFeedback.GetComponent<RectTransform>();
        rtFeedback.anchorMin = new Vector2(0.1f, 0.05f);
        rtFeedback.anchorMax = new Vector2(0.9f, 0.18f);
        rtFeedback.sizeDelta = Vector2.zero;

        // Barra e Icono del Minibús
        var barraObj = new GameObject("BarraRuta");
        barraObj.transform.SetParent(panelTrivia.transform, false);
        var imgBarra = barraObj.AddComponent<Image>();
        imgBarra.color = new Color(0.3f, 0.3f, 0.3f);
        var rtBarra = barraObj.GetComponent<RectTransform>();
        rtBarra.anchorMin = new Vector2(0.35f, 0.88f);
        rtBarra.anchorMax = new Vector2(0.65f, 0.92f);
        rtBarra.sizeDelta = Vector2.zero;

        var iconoObj = new GameObject("MinibusIcono");
        iconoObj.transform.SetParent(barraObj.transform, false);
        var imgIcono = iconoObj.AddComponent<Image>();
        imgIcono.color = Color.yellow; // Representa el minibús
        minibusIcono = iconoObj.GetComponent<RectTransform>();
        minibusIcono.anchorMin = new Vector2(0.5f, 0.5f);
        minibusIcono.anchorMax = new Vector2(0.5f, 0.5f);
        minibusIcono.sizeDelta = new Vector2(30, 30);
        minibusIcono.anchoredPosition = new Vector2(posicionesParadasX[0], 0);

        // Apagar el panel al inicio
        panelTrivia.SetActive(false);
    }
    // ====================================================================

    void Caja(string nombre, Vector2 centro, Vector2 tam)
    {
        var g = new GameObject(nombre);
        g.transform.SetParent(transform);
        g.transform.position = centro;
        var bc = g.AddComponent<BoxCollider2D>();
        bc.size = tam;
    }

    void Obstaculo(string imagen, float x, float alto, float fraccion, float anchoCol, bool peligro)
    {
        var sp = UIKit.CargarSprite(imagen, alto, false, true);
        var g = new GameObject(imagen);
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(x, SueloY);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.sortingOrder = 3;

        Vector2 tam = sp.bounds.size;
        var bc = g.AddComponent<BoxCollider2D>();
        float h = tam.y * fraccion;
        bc.size = new Vector2(tam.x * anchoCol, h);
        bc.offset = new Vector2(0f, h / 2f);
        if (peligro)
        {
            bc.isTrigger = true;
            g.AddComponent<Pickup>().tipo = TipoPickup.Peligro;
        }
    }

    void Decorado(string imagen, float x, float alto)
    {
        var g = new GameObject(imagen);
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(x, SueloY);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = UIKit.CargarSprite(imagen, alto, false, true);
        sr.sortingOrder = 2;
    }

    void Plataforma(float x, float ancho, float altoArriba)
    {
        float grosor = 0.55f;
        float arriba = SueloY + altoArriba;
        var g = new GameObject("Plataforma");
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(x, arriba - grosor / 2f);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = UIKit.CargarSprite("obs_losa", grosor);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(ancho, grosor);
        sr.sortingOrder = 3;
        var bc = g.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(ancho, grosor);
    }

    void Moneda(float x, float altura)
    {
        var g = new GameObject("Moneda");
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(x, SueloY + altura);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = sprMonedaFrente;
        sr.sortingOrder = 4;
        var cc = g.AddComponent<CircleCollider2D>();
        cc.radius = 0.4f;
        cc.isTrigger = true;
        var p = g.AddComponent<Pickup>();
        p.tipo = TipoPickup.Moneda;
        p.frente = sprMonedaFrente;
        p.dorso = sprMonedaDorso;
    }

    void Linea(float x0, float x1, int cantidad, float altura)
    {
        for (int i = 0; i < cantidad; i++)
            Moneda(Mathf.Lerp(x0, x1, cantidad == 1 ? 0f : i / (float)(cantidad - 1)), altura);
    }

    void NuevoEnemigo(float izq, float der)
    {
        var caminar = UIKit.CargarFrames("enemigo_", 4, 1.4f);
        var aplastado = UIKit.CargarSprite("enemigo_aplastado", 1.4f, false, true);

        var g = new GameObject("Enemigo");
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(izq, SueloY);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = caminar[0];
        sr.sortingOrder = 5;

        var rb = g.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var bc = g.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(0.9f, 1.25f);
        bc.offset = new Vector2(0f, 0.65f);

        g.AddComponent<Enemigo>().Iniciar(caminar, aplastado, izq, der);
    }

    void Update()
    {
        // Abrir Trivia con la tecla P
        if (!pausado && UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame && estado == Estado.Jugando)
        {
            if (!triviaAbierta && triviasJugadas < limiteUsosPorNivel)
            {
                AbrirTrivia();
            }
            else if (!triviaAbierta)
            {
                Debug.Log("¡Ya completaste el límite de 2 Minibús Trivias en este nivel, m'hijo!");
            }
        }

        if (Entrada.EscapePresionado() && estado == Estado.Jugando)
        {
            if (pausado && opciones != null && opciones.Abierto) opciones.Cancelar();
            else AlternarPausa();
        }
        if (pausado || triviaAbierta) return;

        if (tAyuda > 0f) tAyuda -= Time.deltaTime;

        if (estado == Estado.Perdido)
        {
            tFin -= Time.deltaTime;
            if (tFin <= 0f && !pantallaFin) MostrarPerdiste();
            return;
        }
        if (estado == Estado.Ganado) return;

        tiempo -= Time.deltaTime;
        if (invulnerable > 0f) invulnerable -= Time.deltaTime;

        if (tiempo <= 0f) { tiempo = 0f; Perder("¡Llegaste tarde!"); return; }

        float x = jugador.transform.position.x;
        foreach (float cp in checkpoints) if (x >= cp) puntoRespawn = cp;

        if (jugador.transform.position.y < -8f) Caida();
    }

    public void AbrirTrivia()
    {
        triviasJugadas++;
        triviaAbierta = true;
        preguntaActualIndex = 0;

        SeleccionarPreguntasAleatorias();

        if (minibusIcono != null)
        {
            Vector2 pos = minibusIcono.anchoredPosition;
            pos.x = (posicionesParadasX != null && posicionesParadasX.Length > 0) ? posicionesParadasX[0] : -100f;
            minibusIcono.anchoredPosition = pos;
        }

        if (panelTrivia != null) panelTrivia.SetActive(true);
        Time.timeScale = 0f;
        MostrarPregunta();
    }

    void SeleccionarPreguntasAleatorias()
    {
        preguntasSeleccionadasRuta.Clear();
        List<int> indicesDisponibles = new List<int>();
        for (int i = 0; i < bancoPreguntas.Length; i++) indicesDisponibles.Add(i);

        for (int i = 0; i < totalParadasPorRuta; i++)
        {
            if (indicesDisponibles.Count > 0)
            {
                int randomIndex = Random.Range(0, indicesDisponibles.Count);
                preguntasSeleccionadasRuta.Add(indicesDisponibles[randomIndex]);
                indicesDisponibles.RemoveAt(randomIndex);
            }
        }
    }

    void MostrarPregunta()
    {
        if (preguntaActualIndex < preguntasSeleccionadasRuta.Count)
        {
            int indexReal = preguntasSeleccionadasRuta[preguntaActualIndex];

            if (textoPregunta != null)
                textoPregunta.text = "🚎 Parada " + (preguntaActualIndex + 1) + " de 3:\n\n" + bancoPreguntas[indexReal];

            if (textoFeedback != null)
                textoFeedback.text = "";

            for (int i = 0; i < botonesOpciones.Length; i++)
            {
                int btnIndex = i;
                botonesOpciones[i].interactable = true;

                var textoBtn = botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>();
                if (textoBtn != null && i < bancoOpciones[indexReal].Length)
                    textoBtn.text = bancoOpciones[indexReal][i];

                botonesOpciones[i].onClick.RemoveAllListeners();
                botonesOpciones[i].onClick.AddListener(() => Responder(btnIndex, indexReal));
            }
        }
        else
        {
            CerrarTrivia();
        }
    }

    void Responder(int seleccion, int indexReal)
    {
        foreach (var btn in botonesOpciones) btn.interactable = false;

        if (seleccion == bancoRespuestas[indexReal])
        {
            if (textoFeedback != null)
                textoFeedback.text = "¡SAPE! ¡El minibús avanza a la siguiente parada!";

            if (minibusIcono != null)
            {
                Vector2 posActual = minibusIcono.anchoredPosition;
                if (posicionesParadasX != null && posicionesParadasX.Length > preguntaActualIndex)
                {
                    posActual.x = posicionesParadasX[preguntaActualIndex];
                }
                else
                {
                    posActual.x += 60f;
                }
                minibusIcono.anchoredPosition = posActual;
            }

            monedas += 20;
            PlayerPrefs.SetInt("monedas", PlayerPrefs.GetInt("monedas", 0) + 20);
            PlayerPrefs.Save();
        }
        else
        {
            if (textoFeedback != null)
                textoFeedback.text = "¡TRANCON! Te quedaste atascado.";
        }

        StartCoroutine(EsperaYPasarSiguiente());
    }

    IEnumerator EsperaYPasarSiguiente()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        preguntaActualIndex++;

        if (preguntaActualIndex < preguntasSeleccionadasRuta.Count)
        {
            MostrarPregunta();
        }
        else
        {
            CerrarTrivia();
        }
    }

    void CerrarTrivia()
    {
        triviaAbierta = false;
        if (panelTrivia != null) panelTrivia.SetActive(false);
        Time.timeScale = 1f;
    }

    void Caida()
    {
        vidas--;
        invulnerable = 1.5f;
        if (vidas <= 0) { Perder("¡Te caíste!"); return; }
        jugador.Reaparecer(new Vector2(puntoRespawn, SueloY + 2f));
    }

    public void RecibirDanio(float xOrigen)
    {
        if (estado != Estado.Jugando || invulnerable > 0f) return;
        vidas--;
        invulnerable = 1.5f;
        if (vidas <= 0) { Perder("¡Te quedaste sin vidas!"); return; }
        float dir = jugador.transform.position.x >= xOrigen ? 1f : -1f;
        jugador.Empujar(dir);
    }

    public void SumarMoneda() { monedas++; }

    void Perder(string texto)
    {
        if (estado != Estado.Jugando) return;
        estado = Estado.Perdido;
        mensaje = texto;
        tFin = 1.3f;
        jugador.Congelar();
    }

    public void LlegarAMeta()
    {
        if (estado != Estado.Jugando) return;
        estado = Estado.Ganado;
        StartCoroutine(SecuenciaMeta());
    }

    IEnumerator SecuenciaMeta()
    {
        jugador.IrA(BusX);
        float limite = 5f;
        while (!jugador.LlegoAuto && limite > 0f) { limite -= Time.deltaTime; yield return null; }

        jugador.Ocultar();
        yield return new WaitForSeconds(0.5f);
        busSr.sprite = busCerrado;
        yield return new WaitForSeconds(0.6f);

        camSigue.objetivo = busSr.transform;
        float vel = 0f, t = 0f;
        while (t < 2.8f)
        {
            t += Time.deltaTime;
            vel += 7f * Time.deltaTime;
            busSr.transform.Translate(vel * Time.deltaTime, 0f, 0f);
            yield return null;
        }
        MostrarVictoria();
    }

    Transform NuevaPantalla(string nombre, string imagen, out GameObject raiz)
    {
        var canvas = UIKit.CrearCanvas(nombre);
        canvas.sortingOrder = 100;
        canvas.transform.SetParent(transform, false);
        raiz = canvas.gameObject;
        var cont = UIKit.Contenedor(canvas, 1672f / 941f, UIKit.Hex("0B1B3A"));
        UIKit.ImagenSprite(cont, "Fondo", UIKit.CargarSprite(imagen), 0, 0, 1, 1);
        return cont;
    }

    void MostrarPerdiste()
    {
        pantallaFin = true;
        GameObject raiz;
        var cont = NuevaPantalla("PantallaPerdiste", "pantalla_perdiste", out raiz);
        UIKit.BotonInvisible(cont, "Volver a intentar", 0.299f, 0.723f, 0.700f, 0.824f, Reiniciar);
        UIKit.BotonInvisible(cont, "Menu principal", 0.299f, 0.847f, 0.700f, 0.956f, IrAlMenu);
    }

    void MostrarVictoria()
    {
        pantallaFin = true;
        GameObject raiz;
        var cont = NuevaPantalla("PantallaVictoria", "pantalla_victoria", out raiz);
        UIKit.BotonInvisible(cont, "Siguiente nivel", 0.299f, 0.661f, 0.703f, 0.761f, ProximoNivel);
        UIKit.BotonInvisible(cont, "Volver a intentar", 0.299f, 0.774f, 0.703f, 0.874f, Reiniciar);
        UIKit.BotonInvisible(cont, "Menu principal", 0.299f, 0.888f, 0.703f, 0.986f, IrAlMenu);

        int seg = Mathf.CeilToInt(TiempoTotal - tiempo);
        var t = UIKit.Texto(cont, "Monedas: " + monedas + "     Tiempo: " + (seg / 60) + ":" + (seg % 60).ToString("00"),
                            44, Color.white, TextAnchor.MiddleLeft, 0.02f, 0.03f, 0.40f, 0.10f);
        t.gameObject.AddComponent<Shadow>().effectColor = Color.black;
    }

    void AlternarPausa()
    {
        pausado = !pausado;
        Time.timeScale = pausado ? 0f : 1f;
        if (pausado && pantallaPausa == null) CrearPausa();
        if (pantallaPausa != null) pantallaPausa.SetActive(pausado);
        if (!pausado && opciones != null && opciones.Abierto) opciones.Cancelar();
    }

    void CrearPausa()
    {
        var cont = NuevaPantalla("PantallaPausa", "pantalla_pausa", out pantallaPausa);
        UIKit.BotonInvisible(cont, "Reiniciar nivel", 0.287f, 0.402f, 0.710f, 0.528f, Reiniciar);
        UIKit.BotonInvisible(cont, "Salir al menu", 0.287f, 0.547f, 0.710f, 0.672f, IrAlMenu);
        UIKit.BotonInvisible(cont, "Opciones", 0.287f, 0.691f, 0.710f, 0.810f, () => opciones.Abrir());

        var ayuda = UIKit.Texto(cont, "ESC para continuar", 40, Color.white, TextAnchor.MiddleCenter, 0.3f, 0.91f, 0.7f, 0.98f);
        ayuda.gameObject.AddComponent<Shadow>().effectColor = Color.black;

        opciones = OpcionesPanel.Crear(cont);
    }

    void Reiniciar() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    void IrAlMenu() { SceneManager.LoadScene("MenuPrincipal"); }

    void ProximoNivel()
    {
        if (Application.CanStreamedLevelBeLoaded("Nivel2")) SceneManager.LoadScene("Nivel2");
        else SceneManager.LoadScene("Niveles");
    }

    Texture2D CrearCorazon(Color relleno)
    {
        string[] mapa =
        {
            ".11...11.",
            "1111.1111",
            "111111111",
            "111111111",
            ".1111111.",
            "..11111..",
            "...111...",
            "....1...."
        };
        int w = mapa[0].Length + 2, h = mapa.Length + 2;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var vacio = new Color(0, 0, 0, 0);
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) tex.SetPixel(x, y, vacio);

        bool Lleno(int x, int y)
        {
            int fila = mapa.Length - 1 - (y - 1);
            int col = x - 1;
            return fila >= 0 && fila < mapa.Length && col >= 0 && col < mapa[0].Length && mapa[fila][col] == '1';
        }
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (Lleno(x, y)) tex.SetPixel(x, y, relleno);
                else if (Lleno(x + 1, y) || Lleno(x - 1, y) || Lleno(x, y + 1) || Lleno(x, y - 1))
                    tex.SetPixel(x, y, new Color(0.1f, 0.05f, 0.12f, 1f));
            }
        tex.Apply();
        return tex;
    }

    void OnGUI()
    {
        if (pausado || pantallaFin || triviaAbierta) return;

        if (estilo == null)
        {
            estilo = new GUIStyle(GUI.skin.label);
            estilo.fontStyle = FontStyle.Bold;
        }
        float w = Screen.width, h = Screen.height;
        float icono = h * 0.06f;
        estilo.fontSize = Mathf.RoundToInt(h * 0.045f);

        for (int i = 0; i < VidasIniciales; i++)
        {
            var r = new Rect(20f + i * icono * 1.2f, 14f, icono, icono * 0.9f);
            GUI.DrawTexture(r, i < vidas ? texCorazon : texCorazonVacio, ScaleMode.ScaleToFit, true);
        }
        if (texMoneda != null)
            GUI.DrawTexture(new Rect(20f, 14f + icono * 1.15f, icono, icono), texMoneda, ScaleMode.ScaleToFit, true);
        Dibujar(new Rect(20f + icono * 1.2f, 14f + icono * 1.15f, 300f, icono), "x " + monedas, TextAnchor.MiddleLeft);

        int seg = Mathf.CeilToInt(tiempo);
        Dibujar(new Rect(w / 2f - 20f, 10f, w / 2f, h * 0.08f), "TIEMPO: " + (seg / 60) + ":" + (seg % 60).ToString("00"), TextAnchor.UpperRight);

        if (tAyuda > 0f && estado == Estado.Jugando)
            Dibujar(new Rect(0f, h - h * 0.1f, w, h * 0.08f), "A / D o flechas: mover     ESPACIO: saltar     ESC: pausa     P: Minibús Trivia", TextAnchor.MiddleCenter);

        if (estado == Estado.Perdido)
        {
            estilo.fontSize = Mathf.RoundToInt(h * 0.09f);
            Dibujar(new Rect(0f, h * 0.25f, w, h * 0.3f), mensaje, TextAnchor.MiddleCenter);
        }
    }

    void Dibujar(Rect r, string texto, TextAnchor alineacion)
    {
        estilo.alignment = alineacion;
        estilo.normal.textColor = Color.black;
        GUI.Label(new Rect(r.x + 3, r.y + 3, r.width, r.height), texto, estilo);
        estilo.normal.textColor = Color.white;
        GUI.Label(r, texto, estilo);
    }
}
