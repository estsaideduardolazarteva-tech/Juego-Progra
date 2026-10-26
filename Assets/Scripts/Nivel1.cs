using UnityEngine;
using UnityEngine.SceneManagement;

// NIVEL 1 - Avenida Camacho. Arma todo el nivel por codigo y controla vidas, monedas, tiempo y fin de nivel.
public class Nivel1 : MonoBehaviour
{
    public static Nivel1 Instancia;

    // ---- Ajustes que puedes cambiar ----
    const float Largo = 108f;        // largo del nivel
    const float SueloY = -3.5f;      // altura del piso (coincide con la vereda del fondo)
    const float TiempoTotal = 90f;   // segundos para llegar
    const int VidasIniciales = 3;
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

    Jugador jugador;
    Sprite cuadro;
    float puntoRespawn = 2f;
    readonly float[] checkpoints = { 2f, 26.5f, 52.5f, 80f };

    GUIStyle estilo;

    public bool Invulnerable { get { return invulnerable > 0f; } }

    void Awake() { Instancia = this; }

    void Start()
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, Color.white);
        t.Apply();
        cuadro = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

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
        var sprite = UIKit.CargarSprite("fondo_nivel1", 10f);   // el fondo mide 10 unidades de alto
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
        // tramos de piso: {desde, hasta}. Entre tramo y tramo hay un hoyo.
        float[,] tramos = { { -6f, 22f }, { 25f, 48f }, { 51.5f, 75f }, { 79f, Largo } };
        for (int i = 0; i < tramos.GetLength(0); i++)
        {
            float a = tramos[i, 0], b = tramos[i, 1];
            Caja("Piso " + i, new Vector2((a + b) / 2f, SueloY - 2f), new Vector2(b - a, 4f));

            if (i + 1 < tramos.GetLength(0))
            {
                float h0 = b, h1 = tramos[i + 1, 0];
                // dibujo oscuro del hoyo (alcantarilla) sobre la vereda
                Bloque("Hoyo " + i, new Vector2((h0 + h1) / 2f, -4.4f), new Vector2(h1 - h0, 2.3f),
                       UIKit.Hex("14101F"), 1, false);
            }
        }
        // paredes invisibles en los extremos
        Caja("Pared izquierda", new Vector2(-6.5f, 0f), new Vector2(1f, 30f));
        Caja("Pared derecha", new Vector2(Largo + 4.5f, 0f), new Vector2(1f, 30f));
    }

    void CrearObstaculos()
    {
        // bloques bajos / escalones: {x centro, ancho, alto}
        float[,] bloques = { { 10f, 2.4f, 1.2f }, { 28f, 2f, 1.2f }, { 44f, 2f, 1.2f }, { 58f, 2f, 1.2f },
                             { 62f, 2f, 2.4f }, { 66f, 2f, 1.2f }, { 90f, 2f, 1.2f } };
        for (int i = 0; i < bloques.GetLength(0); i++)
            Banca(bloques[i, 0], bloques[i, 1], bloques[i, 2]);

        // plataformas altas: {x centro, ancho, altura de la parte de arriba}
        Plataforma(33.5f, 5f, 3.4f);
        Plataforma(71.5f, 5f, 3.4f);

        // conos (te quitan una vida)
        float[] conos = { 14f, 38f, 55f, 85f, 99f };
        foreach (float x in conos) Cono(x);
    }

    void CrearMonedas()
    {
        Linea(4f, 7f, 4, 1.4f);
        Moneda(10f, 2.7f);
        Moneda(13f, 2.1f); Moneda(14f, 2.7f); Moneda(15f, 2.1f);
        Moneda(22.8f, 2.4f); Moneda(23.5f, 2.9f); Moneda(24.2f, 2.4f);
        Linea(31f, 36f, 5, 4.3f);
        Moneda(37f, 2.1f); Moneda(38f, 2.7f); Moneda(39f, 2.1f);
        Moneda(49f, 2.4f); Moneda(49.75f, 2.9f); Moneda(50.5f, 2.4f);
        Moneda(54f, 2.1f); Moneda(55f, 2.7f); Moneda(56f, 2.1f);
        Moneda(62f, 3.8f);
        Linea(69.5f, 73.5f, 5, 4.3f);
        Moneda(76.3f, 2.4f); Moneda(77f, 2.9f); Moneda(77.7f, 2.4f);
        Moneda(84f, 2.1f); Moneda(85f, 2.7f); Moneda(86f, 2.1f);
        Moneda(90f, 2.7f);
        Moneda(98f, 2.1f); Moneda(99f, 2.7f); Moneda(100f, 2.1f);
    }

    void CrearEnemigos()
    {
        NuevoEnemigo(16f, 21f);
        NuevoEnemigo(31f, 41f);
        NuevoEnemigo(68f, 74f);
        NuevoEnemigo(93f, 101f);
    }

    void CrearMeta()
    {
        float x = Largo - 4f;
        Bloque("Poste", new Vector2(x, SueloY + 2f), new Vector2(0.18f, 4f), UIKit.Hex("D9D9D9"), 3, false);
        Bloque("Bandera", new Vector2(x + 0.8f, SueloY + 3.4f), new Vector2(1.5f, 0.9f), UIKit.Hex("2ECC71"), 3, false);
        var g = new GameObject("Meta");
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(x, SueloY + 3f);
        var bc = g.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(1.2f, 7f);
        g.AddComponent<Pickup>().tipo = TipoPickup.Meta;
    }

    void CrearJugador()
    {
        var idle = UIKit.CargarSprite("player_idle", 2.2f, true);
        var p1 = UIKit.CargarSprite("player_walk1", 2.2f, true);
        var p2 = UIKit.CargarSprite("player_walk2", 2.2f, true);

        var g = new GameObject("Jugador");
        g.transform.SetParent(transform);
        g.transform.position = new Vector2(2f, SueloY + 1.4f);

        var sr = g.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 6;

        var rb = g.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3.5f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var bc = g.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(0.65f, 2.1f);
        bc.offset = new Vector2(0f, -0.05f);
        var sinFriccion = new PhysicsMaterial2D("SinFriccion");
        sinFriccion.friction = 0f;
        sinFriccion.bounciness = 0f;
        bc.sharedMaterial = sinFriccion;
        rb.sharedMaterial = sinFriccion;

        jugador = g.AddComponent<Jugador>();
        jugador.Iniciar(idle, p1, p2);
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

        var sigue = cam.GetComponent<CamaraSigue>();
        if (sigue == null) sigue = cam.gameObject.AddComponent<CamaraSigue>();
        sigue.objetivo = jugador.transform;
        sigue.minX = -6f;
        sigue.maxX = Largo + 4f;

        float mitad = cam.orthographicSize * cam.aspect;
        cam.transform.position = new Vector3(-6f + mitad, 0f, -10f);
    }

    // ================= PIEZAS =================

    // Rectangulo de color (usa el cuadrito blanco de 1x1 escalado)
    GameObject Bloque(string nombre, Vector2 centro, Vector2 tam, Color color, int orden, bool conColision)
    {
        var g = new GameObject(nombre);
        g.transform.SetParent(transform);
        g.transform.position = centro;
        g.transform.localScale = new Vector3(tam.x, tam.y, 1f);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = cuadro;
        sr.color = color;
        sr.sortingOrder = orden;
        if (conColision)
        {
            var bc = g.AddComponent<BoxCollider2D>();
            bc.size = Vector2.one;
        }
        return g;
    }

    // Colision invisible
    void Caja(string nombre, Vector2 centro, Vector2 tam)
    {
        var g = new GameObject(nombre);
        g.transform.SetParent(transform);
        g.transform.position = centro;
        var bc = g.AddComponent<BoxCollider2D>();
        bc.size = tam;
    }

    void Banca(float x, float ancho, float alto)
    {
        Bloque("Banca", new Vector2(x, SueloY + alto / 2f), new Vector2(ancho, alto), UIKit.Hex("8B5A2B"), 2, true);
        Bloque("Banca borde", new Vector2(x, SueloY + alto - 0.1f), new Vector2(ancho, 0.2f), UIKit.Hex("C98A4B"), 3, false);
    }

    void Plataforma(float x, float ancho, float altoArriba)
    {
        float grosor = 0.4f;
        float arriba = SueloY + altoArriba;
        Bloque("Plataforma", new Vector2(x, arriba - grosor / 2f), new Vector2(ancho, grosor), UIKit.Hex("8B5A2B"), 2, true);
        Bloque("Plataforma borde", new Vector2(x, arriba - 0.08f), new Vector2(ancho, 0.16f), UIKit.Hex("C98A4B"), 3, false);
    }

    void Cono(float x)
    {
        var g = Bloque("Cono", new Vector2(x, SueloY + 0.45f), new Vector2(0.75f, 0.9f), UIKit.Hex("FF7A1A"), 4, false);
        var bc = g.AddComponent<BoxCollider2D>();
        bc.size = Vector2.one;
        bc.isTrigger = true;
        g.AddComponent<Pickup>().tipo = TipoPickup.Peligro;
        Bloque("Cono franja", new Vector2(x, SueloY + 0.5f), new Vector2(0.55f, 0.15f), Color.white, 5, false);
    }

    void Moneda(float x, float altura)
    {
        var g = Bloque("Moneda", new Vector2(x, SueloY + altura), new Vector2(0.45f, 0.45f), UIKit.Hex("FFD400"), 4, false);
        g.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
        var cc = g.AddComponent<CircleCollider2D>();
        cc.radius = 0.8f;
        cc.isTrigger = true;
        g.AddComponent<Pickup>().tipo = TipoPickup.Moneda;
    }

    void Linea(float x0, float x1, int cantidad, float altura)
    {
        for (int i = 0; i < cantidad; i++)
            Moneda(Mathf.Lerp(x0, x1, cantidad == 1 ? 0f : i / (float)(cantidad - 1)), altura);
    }

    void NuevoEnemigo(float izq, float der)
    {
        var g = Bloque("Enemigo", new Vector2(izq, SueloY + 0.45f), new Vector2(1.1f, 0.9f), UIKit.Hex("C0392B"), 5, true);
        var rb = g.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var e = g.AddComponent<Enemigo>();
        e.izquierda = izq;
        e.derecha = der;
        // ojitos
        var ojo1 = Bloque("Ojo", new Vector2(izq - 0.2f, SueloY + 0.6f), new Vector2(0.22f, 0.22f), Color.white, 6, false);
        var ojo2 = Bloque("Ojo", new Vector2(izq + 0.2f, SueloY + 0.6f), new Vector2(0.22f, 0.22f), Color.white, 6, false);
        ojo1.transform.SetParent(g.transform, true);
        ojo2.transform.SetParent(g.transform, true);
    }

    // ================= REGLAS DEL JUEGO =================

    void Update()
    {
        if (Entrada.EscapePresionado()) { SceneManager.LoadScene("Niveles"); return; }

        if (tAyuda > 0f) tAyuda -= Time.deltaTime;

        if (estado != Estado.Jugando)
        {
            tFin -= Time.deltaTime;
            if (tFin <= 0f)
            {
                if (estado == Estado.Ganado) SceneManager.LoadScene("Niveles");
                else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        tiempo -= Time.deltaTime;
        if (invulnerable > 0f) invulnerable -= Time.deltaTime;

        if (tiempo <= 0f) { tiempo = 0f; Perder("¡Llegaste tarde!"); return; }

        float x = jugador.transform.position.x;
        foreach (float cp in checkpoints) if (x >= cp) puntoRespawn = cp;

        if (jugador.transform.position.y < -8f) Caida();
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

    public void LlegarAMeta()
    {
        if (estado != Estado.Jugando) return;
        estado = Estado.Ganado;
        mensaje = "¡LLEGASTE A TIEMPO!\nMonedas: " + monedas;
        tFin = 4f;
        jugador.Congelar();
    }

    void Perder(string texto)
    {
        if (estado != Estado.Jugando) return;
        estado = Estado.Perdido;
        mensaje = texto + "\nReiniciando...";
        tFin = 2.5f;
        jugador.Congelar();
    }

    // ================= TEXTOS EN PANTALLA =================

    void OnGUI()
    {
        if (estilo == null)
        {
            estilo = new GUIStyle(GUI.skin.label);
            estilo.fontStyle = FontStyle.Bold;
        }
        estilo.fontSize = Mathf.RoundToInt(Screen.height * 0.045f);
        float w = Screen.width, h = Screen.height;

        Dibujar(new Rect(20, 10, w / 2f, h * 0.08f), "VIDAS: " + vidas + "     MONEDAS: " + monedas, TextAnchor.UpperLeft);
        int seg = Mathf.CeilToInt(tiempo);
        Dibujar(new Rect(w / 2f - 20, 10, w / 2f, h * 0.08f), "TIEMPO: " + (seg / 60) + ":" + (seg % 60).ToString("00"), TextAnchor.UpperRight);

        if (tAyuda > 0f && estado == Estado.Jugando)
            Dibujar(new Rect(0, h - h * 0.1f, w, h * 0.08f), "A / D o flechas: mover     ESPACIO: saltar     ESC: salir", TextAnchor.MiddleCenter);

        if (estado != Estado.Jugando)
        {
            estilo.fontSize = Mathf.RoundToInt(Screen.height * 0.09f);
            Dibujar(new Rect(0, h * 0.25f, w, h * 0.4f), mensaje, TextAnchor.MiddleCenter);
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
