using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// NIVELES 1, 2 y 3. Arma todo el nivel por codigo y controla vidas, monedas, tiempo,
// pausa, derrota y victoria.
public class Nivel : MonoBehaviour
{
    public static Nivel Instancia;

    public int numero = 1;          // 1, 2 o 3 (lo pone Arranque segun el nombre de la escena)

    // ---- Datos de cada nivel (se llenan en Configurar) ----
    float Largo = 108f;        // largo del nivel
    const float SueloY = -3.5f;      // altura del piso (coincide con la vereda del fondo)
    float TiempoTotal = 90f;   // segundos para llegar al minibus
    const int VidasIniciales = 3;
    float BusX = 104.5f;       // centro del minibus (la meta)
    float[,] tramosNivel = { { -6f, 22f }, { 25f, 48f }, { 51.5f, 75f }, { 79f, 108f } };
    float tiempoLanzador = 3f;
    // -------------------------------------

    enum Estado { Jugando, Ganado, Perdido }
    Estado estado = Estado.Jugando;

    int vidas = VidasIniciales;
    int monedas;
    float tiempo;
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
    float[] checkpoints = { 2f, 26f, 52.5f, 80f };
    float puntoRespawn = 2f;

    GUIStyle estilo;

    public bool Invulnerable { get { return invulnerable > 0f; } }
    public bool Pausado { get { return pausado; } }
    public bool Jugando { get { return estado == Estado.Jugando; } }

    void Awake() { Instancia = this; }

    void Start()
    {
        Configurar();
        tiempo = TiempoTotal;

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
    }

    // ================= CONSTRUCCION DEL NIVEL =================

    void CrearFondo()
    {
        string normal = numero == 1 ? "fondo_nivel1" : numero == 2 ? "fondo_nivel2" : "fondo_nivel3";
        string fin = numero == 2 ? "fondo_nivel2_fin" : numero == 3 ? "fondo_nivel3_fin" : null;

        var sprite = UIKit.CargarSprite(normal, 10f);   // el fondo mide 10 unidades de alto
        float ancho = sprite.bounds.size.x;

        if (fin == null)
        {
            int cantidad = Mathf.CeilToInt((Largo + 30f) / ancho) + 1;
            for (int i = 0; i < cantidad; i++) PonerFondo(sprite, -8f + ancho * (i + 0.5f), -10);
            return;
        }

        // Fondo normal que se repite y, al final del nivel, el fondo especial (plaza / ciudad)
        var spFin = UIKit.CargarSprite(fin, 10f);
        float anchoFin = spFin.bounds.size.x;
        float derecha = Largo + 7f;
        PonerFondo(spFin, derecha - anchoFin / 2f, -9);
        float borde = derecha - anchoFin;
        while (borde > -14f)
        {
            borde -= ancho;
            PonerFondo(sprite, borde + ancho / 2f, -10);
        }
    }

    void PonerFondo(Sprite sprite, float x, int orden)
    {
        var g = new GameObject("Fondo");
        g.transform.SetParent(transform);
        g.transform.position = new Vector3(x, 0f, 0f);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = orden;
    }

    void CrearSuelo()
    {
        // tramos de piso: {desde, hasta}. Entre tramo y tramo hay un hoyo.
        float[,] tramos = tramosNivel;
        var spHoyo = UIKit.CargarSprite("obs_hoyo", 1f);
        float anchoHoyo = spHoyo.bounds.size.x;

        for (int i = 0; i < tramos.GetLength(0); i++)
        {
            float a = tramos[i, 0], b = tramos[i, 1];
            Caja("Piso " + i, new Vector2((a + b) / 2f, SueloY - 2f), new Vector2(b - a, 4f));

            if (i + 1 < tramos.GetLength(0))
            {
                float h0 = b, h1 = tramos[i + 1, 0];
                // dibujo del hoyo (imagen estirada)
                var g = new GameObject("Hoyo " + i);
                g.transform.SetParent(transform);
                g.transform.position = new Vector2((h0 + h1) / 2f, -4.4f);
                g.transform.localScale = new Vector3((h1 - h0) / anchoHoyo, 2.3f, 1f);
                var sr = g.AddComponent<SpriteRenderer>();
                sr.sprite = spHoyo;
                sr.sortingOrder = 1;
            }
        }
        // paredes invisibles en los extremos
        Caja("Pared izquierda", new Vector2(-6.5f, 0f), new Vector2(1f, 30f));
        Caja("Pared derecha", new Vector2(Largo + 4.5f, 0f), new Vector2(1f, 30f));
    }

    void CrearObstaculos()
    {
        if (numero == 1) Obstaculos1(); else if (numero == 2) Diseno2(); else Diseno3();
    }

    void Obstaculos1()
    {
        // Obstaculos SOLIDOS (los puedes saltar o subirte encima)
        // Obstaculo(imagen, x, alto total, fraccion solida, ancho de colision, peligro)
        Decorado("obs_bandera", 0.6f, 2.6f);

        Obstaculo("obs_graffiti", 10f, 1.6f, 1.0f, 0.95f, false);
        Obstaculo("obs_riel", 29.5f, 1.9f, 0.69f, 0.92f, false);
        Obstaculo("obs_caja", 45f, 1.1f, 1.0f, 0.95f, false);
        Obstaculo("obs_maceta", 58f, 1.7f, 0.62f, 0.9f, false);
        Obstaculo("obs_graffiti", 62.5f, 2.0f, 1.0f, 0.95f, false);
        Obstaculo("obs_barrera_roja", 67.5f, 1.0f, 0.95f, 0.9f, false);
        Obstaculo("obs_caja", 90.5f, 1.1f, 1.0f, 0.95f, false);
        Obstaculo("obs_barril", 92.6f, 1.1f, 1.0f, 0.9f, false);

        // Obstaculos PELIGROSOS (te quitan una vida)
        Obstaculo("obs_cono", 14f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_cono", 41.5f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_pinchos", 55f, 0.7f, 0.8f, 0.8f, true);
        Obstaculo("obs_rodillo", 86f, 0.9f, 0.8f, 0.8f, true);
        Obstaculo("obs_cono", 96f, 0.9f, 0.85f, 0.6f, true);

        // Plataformas altas: (x centro, ancho, altura de la parte de arriba)
        Plataforma(35.3f, 5f, 3.0f);
        Plataforma(73.3f, 5f, 3.0f);
    }

    void CrearMonedas()
    {
        if (numero == 1) Monedas1();
    }

    void Monedas1()
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
        if (numero == 1) Enemigos1();
    }

    void Enemigos1()
    {
        NuevoEnemigo(16f, 21f);
        NuevoEnemigo(33f, 40f);
        NuevoEnemigo(70f, 74.5f);
        NuevoEnemigo(94f, 99.5f);
    }

    void CrearMeta()
    {
        // Minibus: al llegar, el personaje se sube y el minibus se va.
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

        // zona que activa la meta (justo antes de la puerta)
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
        bc.offset = new Vector2(0f, 1.05f);   // el sprite tiene el punto de anclaje en los pies
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

    // ================= PIEZAS =================

    // Colision invisible
    void Caja(string nombre, Vector2 centro, Vector2 tam)
    {
        var g = new GameObject(nombre);
        g.transform.SetParent(transform);
        g.transform.position = centro;
        var bc = g.AddComponent<BoxCollider2D>();
        bc.size = tam;
    }

    // Objeto con dibujo apoyado en el piso. 'fraccion' = que parte de abajo es solida (el resto es adorno).
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

    // Solo adorno, sin colision
    void Decorado(string imagen, float x, float alto)
    {
        var g = new GameObject(imagen);
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(x, SueloY);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = UIKit.CargarSprite(imagen, alto, false, true);
        sr.sortingOrder = 2;
    }

    // Losa de piedra flotante (la textura se repite a lo ancho)
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

    void NuevoEnemigo(float izq, float der, float vel = 2f)
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

        var en = g.AddComponent<Enemigo>();
        en.velocidad = vel;
        en.Iniciar(caminar, aplastado, izq, der);
    }

    // ================= DATOS DE CADA NIVEL =================

    void Configurar()
    {
        if (numero == 2)
        {
            Largo = 170f;
                        TiempoTotal = 120f;
                        BusX = Largo - 3.5f;
                        tiempoLanzador = 3f;
                        tramosNivel = new float[,]
                        {
                            { -6f, 30f },
                            { 33.5f, 64f },
                            { 68f, 100f },
                            { 103.5f, 136f },
                            { 140f, 170f }
                        };
                        checkpoints = new float[] { 2f, 34.5f, 69f, 104.5f, 141f };
                        puntoRespawn = 2f;
        }
        else if (numero == 3)
        {
            Largo = 230f;
                        TiempoTotal = 140f;
                        BusX = Largo - 3.5f;
                        tiempoLanzador = 2.2f;
                        tramosNivel = new float[,]
                        {
                            { -6f, 28f },
                            { 32f, 60f },
                            { 64f, 92f },
                            { 96.2f, 124f },
                            { 128f, 160f },
                            { 164.2f, 196f },
                            { 200f, 230f }
                        };
                        checkpoints = new float[] { 2f, 33f, 65f, 97.2f, 129f, 165.2f, 201f };
                        puntoRespawn = 2f;
        }
        // el nivel 1 usa los valores por defecto de arriba
    }

    // NIVEL 2 (atardecer): largo 170, mas enemigos y lanzadores de dinamita
    void Diseno2()
    {
        // ---- obstaculos, enemigos y lanzadores ----
        Decorado("obs_bandera", 0.6f, 2.6f);
        Obstaculo("obs_cono", 10f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_cemento", 15f, 1f, 1f, 0.95f, false);
        NuevoEnemigo(18.5f, 24f, 2f);
        NuevoLanzador(27.5f, 0f, 3f);
        Obstaculo("obs_riel", 37.5f, 1.9f, 0.69f, 0.92f, false);
        Plataforma(43.3f, 5f, 3f);
        NuevoEnemigo(45f, 51f, 2f);
        Obstaculo("obs_pinchos", 54f, 0.7f, 0.8f, 0.8f, true);
        Obstaculo("obs_maceta", 58f, 1.7f, 0.62f, 0.9f, false);
        Obstaculo("obs_graffiti", 61.5f, 1.6f, 1f, 0.95f, false);
        NuevoLanzador(61.5f, 1.6f, 3f);
        Obstaculo("obs_cono", 72f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_barrera_roja", 76.5f, 1f, 0.95f, 0.9f, false);
        NuevoEnemigo(79.5f, 84.5f, 2f);
        Obstaculo("obs_rodillo", 88f, 0.9f, 0.8f, 0.8f, true);
        Obstaculo("obs_caja", 92f, 1.1f, 1f, 0.95f, false);
        NuevoLanzador(95.5f, 0f, 3f);
        Obstaculo("obs_cono", 98f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_caja", 107f, 1.1f, 1f, 0.95f, false);
        Obstaculo("obs_graffiti", 110.5f, 2f, 1f, 0.95f, false);
        Obstaculo("obs_riel", 116f, 1.9f, 0.69f, 0.92f, false);
        Plataforma(122f, 5f, 3f);
        NuevoEnemigo(125f, 130.5f, 2f);
        Obstaculo("obs_pinchos", 132.5f, 0.7f, 0.8f, 0.8f, true);
        NuevoLanzador(134.5f, 0f, 3f);
        Obstaculo("obs_cono", 143f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_maceta", 147f, 1.7f, 0.62f, 0.9f, false);
        NuevoEnemigo(150f, 156f, 2f);
        Obstaculo("obs_cemento", 159.5f, 1f, 1f, 0.95f, false);
        NuevoLanzador(159.5f, 1f, 3f);
        Obstaculo("obs_cono", 162f, 0.9f, 0.85f, 0.6f, true);
        // ---- monedas ----
        Moneda(9f, 2.1f);
        Moneda(10f, 2.7f);
        Moneda(11f, 2.1f);
        Moneda(15f, 2f);
        Moneda(37.5f, 2.31f);
        Moneda(41.3f, 3.9f);
        Moneda(42.3f, 3.9f);
        Moneda(43.3f, 3.9f);
        Moneda(44.3f, 3.9f);
        Moneda(45.3f, 3.9f);
        Moneda(53f, 2.1f);
        Moneda(54f, 2.7f);
        Moneda(55f, 2.1f);
        Moneda(58f, 2.05f);
        Moneda(61.5f, 2.6f);
        Moneda(71f, 2.1f);
        Moneda(72f, 2.7f);
        Moneda(73f, 2.1f);
        Moneda(76.5f, 1.95f);
        Moneda(87f, 2.1f);
        Moneda(88f, 2.7f);
        Moneda(89f, 2.1f);
        Moneda(92f, 2.1f);
        Moneda(97f, 2.1f);
        Moneda(98f, 2.7f);
        Moneda(99f, 2.1f);
        Moneda(107f, 2.1f);
        Moneda(110.5f, 3f);
        Moneda(116f, 2.31f);
        Moneda(120f, 3.9f);
        Moneda(121f, 3.9f);
        Moneda(122f, 3.9f);
        Moneda(123f, 3.9f);
        Moneda(124f, 3.9f);
        Moneda(131.5f, 2.1f);
        Moneda(132.5f, 2.7f);
        Moneda(133.5f, 2.1f);
        Moneda(142f, 2.1f);
        Moneda(143f, 2.7f);
        Moneda(144f, 2.1f);
        Moneda(147f, 2.05f);
        Moneda(159.5f, 2f);
        Moneda(161f, 2.1f);
        Moneda(162f, 2.7f);
        Moneda(163f, 2.1f);
        Moneda(30.95f, 2.3f);
        Moneda(31.75f, 2.9f);
        Moneda(32.55f, 2.3f);
        Moneda(65.2f, 2.3f);
        Moneda(66f, 2.9f);
        Moneda(66.8f, 2.3f);
        Moneda(100.95f, 2.3f);
        Moneda(101.75f, 2.9f);
        Moneda(102.55f, 2.3f);
        Moneda(137.2f, 2.3f);
        Moneda(138f, 2.9f);
        Moneda(138.8f, 2.3f);
        Moneda(4f, 1.4f);
        Moneda(5f, 1.4f);
        Moneda(6f, 1.4f);
        Moneda(7f, 1.4f);
    }

    // NIVEL 3 (noche): largo 230, mas lanzadores, rodillos y pinchos
    void Diseno3()
    {
        // ---- obstaculos, enemigos y lanzadores ----
        Decorado("obs_bandera", 0.6f, 2.6f);
        Obstaculo("obs_cono", 9f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_caja", 13f, 1.1f, 1f, 0.95f, false);
        NuevoEnemigo(16f, 21f, 2.6f);
        Obstaculo("obs_cono", 24f, 0.9f, 0.85f, 0.6f, true);
        NuevoLanzador(26.5f, 0f, 2.2f);
        Obstaculo("obs_cemento", 36f, 1f, 1f, 0.95f, false);
        Obstaculo("obs_rodillo", 41f, 0.9f, 0.8f, 0.8f, true);
        NuevoEnemigo(44.5f, 50f, 2.6f);
        Obstaculo("obs_pinchos", 52.5f, 0.7f, 0.8f, 0.8f, true);
        Obstaculo("obs_graffiti", 57f, 1.6f, 1f, 0.95f, false);
        NuevoLanzador(57f, 1.6f, 2.2f);
        Obstaculo("obs_riel", 68f, 1.9f, 0.69f, 0.92f, false);
        Plataforma(73.8f, 5f, 3f);
        NuevoLanzador(74f, 3f, 2.2f);
        NuevoEnemigo(72f, 78f, 2.6f);
        Obstaculo("obs_cono", 80f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_caja", 84f, 1.1f, 1f, 0.95f, false);
        Obstaculo("obs_barril", 85.3f, 1.1f, 1f, 0.9f, false);
        Obstaculo("obs_cono", 99.5f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_bolardo", 103f, 1.1f, 1f, 0.8f, false);
        Obstaculo("obs_pinchos", 106f, 0.7f, 0.8f, 0.8f, true);
        NuevoEnemigo(109f, 115f, 2.6f);
        Obstaculo("obs_graffiti", 118f, 2f, 1f, 0.95f, false);
        NuevoLanzador(118f, 2f, 2.2f);
        Obstaculo("obs_cemento", 121.5f, 1f, 1f, 0.95f, false);
        Obstaculo("obs_cono", 131f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_riel", 135f, 1.9f, 0.69f, 0.92f, false);
        Plataforma(140.8f, 5f, 3f);
        NuevoLanzador(141f, 3f, 2.2f);
        NuevoEnemigo(139f, 146f, 2.6f);
        Obstaculo("obs_rodillo", 148f, 0.9f, 0.8f, 0.8f, true);
        Obstaculo("obs_caja", 155f, 1.1f, 1f, 0.95f, false);
        Obstaculo("obs_cono", 157.5f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_cemento", 168f, 1f, 1f, 0.95f, false);
        Obstaculo("obs_maceta", 172f, 1.7f, 0.62f, 0.9f, false);
        Obstaculo("obs_graffiti", 176f, 2f, 1f, 0.95f, false);
        NuevoLanzador(176f, 2f, 2.2f);
        NuevoEnemigo(180f, 186f, 2.6f);
        Obstaculo("obs_pinchos", 188.5f, 0.7f, 0.8f, 0.8f, true);
        NuevoLanzador(191.5f, 0f, 2.2f);
        Obstaculo("obs_cono", 194f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_cono", 203f, 0.9f, 0.85f, 0.6f, true);
        Obstaculo("obs_riel", 207f, 1.9f, 0.69f, 0.92f, false);
        Plataforma(212.8f, 5f, 3f);
        NuevoLanzador(213f, 3f, 2.2f);
        NuevoEnemigo(211f, 216.5f, 2.6f);
        Obstaculo("obs_rodillo", 219f, 0.9f, 0.8f, 0.8f, true);
        // ---- monedas ----
        Moneda(8f, 2.1f);
        Moneda(9f, 2.7f);
        Moneda(10f, 2.1f);
        Moneda(13f, 2.1f);
        Moneda(23f, 2.1f);
        Moneda(24f, 2.7f);
        Moneda(25f, 2.1f);
        Moneda(36f, 2f);
        Moneda(40f, 2.1f);
        Moneda(41f, 2.7f);
        Moneda(42f, 2.1f);
        Moneda(51.5f, 2.1f);
        Moneda(52.5f, 2.7f);
        Moneda(53.5f, 2.1f);
        Moneda(57f, 2.6f);
        Moneda(68f, 2.31f);
        Moneda(71.8f, 3.9f);
        Moneda(72.8f, 3.9f);
        Moneda(73.8f, 3.9f);
        Moneda(74.8f, 3.9f);
        Moneda(75.8f, 3.9f);
        Moneda(79f, 2.1f);
        Moneda(80f, 2.7f);
        Moneda(81f, 2.1f);
        Moneda(84f, 2.1f);
        Moneda(85.3f, 2.1f);
        Moneda(98.5f, 2.1f);
        Moneda(99.5f, 2.7f);
        Moneda(100.5f, 2.1f);
        Moneda(103f, 2.1f);
        Moneda(105f, 2.1f);
        Moneda(106f, 2.7f);
        Moneda(107f, 2.1f);
        Moneda(118f, 3f);
        Moneda(121.5f, 2f);
        Moneda(130f, 2.1f);
        Moneda(131f, 2.7f);
        Moneda(132f, 2.1f);
        Moneda(135f, 2.31f);
        Moneda(138.8f, 3.9f);
        Moneda(139.8f, 3.9f);
        Moneda(140.8f, 3.9f);
        Moneda(141.8f, 3.9f);
        Moneda(142.8f, 3.9f);
        Moneda(147f, 2.1f);
        Moneda(148f, 2.7f);
        Moneda(149f, 2.1f);
        Moneda(155f, 2.1f);
        Moneda(156.5f, 2.1f);
        Moneda(157.5f, 2.7f);
        Moneda(158.5f, 2.1f);
        Moneda(168f, 2f);
        Moneda(172f, 2.05f);
        Moneda(176f, 3f);
        Moneda(187.5f, 2.1f);
        Moneda(188.5f, 2.7f);
        Moneda(189.5f, 2.1f);
        Moneda(193f, 2.1f);
        Moneda(194f, 2.7f);
        Moneda(195f, 2.1f);
        Moneda(202f, 2.1f);
        Moneda(203f, 2.7f);
        Moneda(204f, 2.1f);
        Moneda(207f, 2.31f);
        Moneda(210.8f, 3.9f);
        Moneda(211.8f, 3.9f);
        Moneda(212.8f, 3.9f);
        Moneda(213.8f, 3.9f);
        Moneda(214.8f, 3.9f);
        Moneda(218f, 2.1f);
        Moneda(219f, 2.7f);
        Moneda(220f, 2.1f);
        Moneda(29.2f, 2.3f);
        Moneda(30f, 2.9f);
        Moneda(30.8f, 2.3f);
        Moneda(61.2f, 2.3f);
        Moneda(62f, 2.9f);
        Moneda(62.8f, 2.3f);
        Moneda(93.3f, 2.3f);
        Moneda(94.1f, 2.9f);
        Moneda(94.9f, 2.3f);
        Moneda(125.2f, 2.3f);
        Moneda(126f, 2.9f);
        Moneda(126.8f, 2.3f);
        Moneda(161.3f, 2.3f);
        Moneda(162.1f, 2.9f);
        Moneda(162.9f, 2.3f);
        Moneda(197.2f, 2.3f);
        Moneda(198f, 2.9f);
        Moneda(198.8f, 2.3f);
        Moneda(4f, 1.4f);
        Moneda(5f, 1.4f);
        Moneda(6f, 1.4f);
        Moneda(7f, 1.4f);
    }

    // Lanzador de dinamita (se puede pisar como los demas enemigos)
    void NuevoLanzador(float x, float baseY, float espera)
    {
        var poses = UIKit.CargarFrames("lanzador_", 7, 1.6f);
        var vuelo = UIKit.CargarSprite("dinamita_vuelo", 0.7f);
        var suelo = UIKit.CargarSprite("dinamita_suelo", 0.35f);
        var explosion = new Sprite[] { UIKit.CargarSprite("explosion_0", 2.0f), UIKit.CargarSprite("explosion_1", 1.6f) };

        var g = new GameObject("Lanzador");
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(x, SueloY + baseY);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = poses[5];
        sr.sortingOrder = 5;

        var rb = g.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var bc = g.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(0.9f, 1.25f);
        bc.offset = new Vector2(0f, 0.65f);

        g.AddComponent<Lanzador>().Iniciar(poses, vuelo, suelo, explosion, espera);
    }

    public Jugador JugadorActual { get { return jugador; } }

    // ================= REGLAS DEL JUEGO =================

    void Update()
    {
        // ESC: pausa (o cierra las opciones si estan abiertas)
        if (Entrada.EscapePresionado() && estado == Estado.Jugando)
        {
            if (pausado && opciones != null && opciones.Abierto) opciones.Cancelar();
            else AlternarPausa();
        }
        if (pausado) return;

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

        if (tiempo <= 0f) { tiempo = 0f; Perder("TIEMPO AGOTADO!"); return; }

        float x = jugador.transform.position.x;
        foreach (float cp in checkpoints) if (x >= cp) puntoRespawn = cp;

        if (jugador.transform.position.y < -8f) Caida();
    }

    void Caida()
    {
        vidas--;
        invulnerable = 1.5f;
        if (vidas <= 0) { Perder("TE CAISTE!"); return; }
        jugador.Reaparecer(new Vector2(puntoRespawn, SueloY + 2f));
    }

    public void RecibirDanio(float xOrigen)
    {
        if (estado != Estado.Jugando || invulnerable > 0f) return;
        vidas--;
        invulnerable = 1.5f;
        if (vidas <= 0) { Perder("SIN VIDAS!"); return; }
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
        Progreso.Desbloquear(numero + 1);
        StartCoroutine(SecuenciaMeta());
    }

    // El personaje camina a la puerta, se sube y el minibus se va.
    IEnumerator SecuenciaMeta()
    {
        jugador.IrA(BusX);
        float limite = 5f;
        while (!jugador.LlegoAuto && limite > 0f) { limite -= Time.deltaTime; yield return null; }

        jugador.Ocultar();                         // ya subio
        yield return new WaitForSeconds(0.5f);
        busSr.sprite = busCerrado;                 // cierra la puerta
        yield return new WaitForSeconds(0.6f);

        camSigue.objetivo = busSr.transform;       // la camara sigue al minibus
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

    // ================= PANTALLAS (pausa, derrota, victoria) =================

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
        // Nivel 1 -> 2 -> 3. Despues del 3 vuelve a la pantalla de niveles.
        string siguiente = "Nivel" + (numero + 1);
        if (numero < 3 && Application.CanStreamedLevelBeLoaded(siguiente)) SceneManager.LoadScene(siguiente);
        else SceneManager.LoadScene("Niveles");
    }

    // ================= TEXTOS E ICONOS EN PANTALLA =================

    // Corazon dibujado pixel por pixel (con borde oscuro)
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
        if (pausado || pantallaFin) return;

        float w = Screen.width, h = Screen.height;
        float icono = h * 0.06f;

        // vidas (corazones)
        for (int i = 0; i < VidasIniciales; i++)
        {
            var r = new Rect(20f + i * icono * 1.2f, 14f, icono, icono * 0.9f);
            GUI.DrawTexture(r, i < vidas ? texCorazon : texCorazonVacio, ScaleMode.ScaleToFit, true);
        }
        // monedas
        float yMon = 14f + icono * 1.15f;
        if (texMoneda != null)
            GUI.DrawTexture(new Rect(20f, yMon, icono, icono), texMoneda, ScaleMode.ScaleToFit, true);
        PixelFuente.Dibujar("X " + monedas, 20f + icono * 1.2f, yMon + icono * 0.05f, icono * 0.9f);

        // numero de nivel (arriba al centro)
        PixelFuente.DibujarCentro("NIVEL " + numero, w / 2f, 14f, h * 0.06f);

        // tiempo (arriba a la derecha): titulo + digitos
        int seg = Mathf.CeilToInt(tiempo);
        string reloj = (seg / 60) + ":" + (seg % 60).ToString("00");
        var titulo = Resources.Load<Texture2D>("fuente/f_titulo");
        float hTit = h * 0.07f;
        if (titulo != null)
        {
            float wTit = hTit * titulo.width / titulo.height;
            GUI.DrawTexture(new Rect(w - wTit - 24f, 10f, wTit, hTit), titulo, ScaleMode.StretchToFill, true);
        }
        PixelFuente.DibujarDerecha(reloj, w - 24f, 10f + hTit * 1.1f, h * 0.075f);

        if (tAyuda > 0f && estado == Estado.Jugando)
            PixelFuente.DibujarCentro("A D MOVER    ESPACIO SALTAR    ESC PAUSA", w / 2f, h - h * 0.09f, h * 0.045f);

        if (estado == Estado.Perdido)
            PixelFuente.DibujarCentro(mensaje, w / 2f, h * 0.3f, h * 0.11f);
    }
}
