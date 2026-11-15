using UnityEngine;

// Movimiento tipo Mario: correr, saltar (mas alto si mantienes) y animaciones de parado, correr y saltar.
public class Jugador : MonoBehaviour
{
    public float velocidad = 6.5f;
    public float fuerzaSalto = 13f;

    Rigidbody2D rb;
    BoxCollider2D col;
    SpriteRenderer sr;
    Sprite[] parado, correr, saltar;

    float inputX, saltoBuffer, coyote, animT, bloqueo;
    bool enSuelo, congelado, oculto, auto, llego;
    float destino;

    Vector2 Vel
    {
        get
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }
        set
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = value;
#else
            rb.velocity = value;
#endif
        }
    }

    public float VelY { get { return Vel.y; } }
    public bool LlegoAuto { get { return llego; } }

    public void Iniciar(Sprite[] sParado, Sprite[] sCorrer, Sprite[] sSaltar)
    {
        parado = sParado; correr = sCorrer; saltar = sSaltar;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr.sprite = parado[0];
    }

    void Update()
    {
        if (rb == null) return;
        if (Nivel1.Instancia != null && Nivel1.Instancia.Pausado) return;
        if (oculto) return;

        if (auto) { AutoCaminar(); return; }

        if (congelado)
        {
            Vel = new Vector2(0f, Vel.y);
            return;
        }

        inputX = Entrada.Horizontal();
        if (Entrada.SaltoPresionado()) saltoBuffer = 0.12f;
        saltoBuffer -= Time.deltaTime;
        coyote -= Time.deltaTime;
        bloqueo -= Time.deltaTime;

        enSuelo = ComprobarSuelo();
        if (enSuelo && Vel.y <= 0.1f) coyote = 0.1f;

        var v = Vel;
        if (bloqueo <= 0f) v.x = inputX * velocidad;

        if (saltoBuffer > 0f && coyote > 0f)
        {
            v.y = fuerzaSalto;
            saltoBuffer = 0f;
            coyote = 0f;
        }
        if (Entrada.SaltoSoltado() && v.y > 0f) v.y *= 0.5f; // salto corto si sueltas rapido
        Vel = v;

        Animar();
    }

    // Al llegar a la meta el personaje camina solo hasta la puerta del minibus.
    void AutoCaminar()
    {
        enSuelo = ComprobarSuelo();
        float dx = destino - transform.position.x;
        if (Mathf.Abs(dx) < 0.12f)
        {
            Vel = new Vector2(0f, Vel.y);
            inputX = 0f;
            auto = false;
            congelado = true;
            llego = true;
            sr.sprite = parado[0];
            return;
        }
        inputX = Mathf.Sign(dx);
        Vel = new Vector2(inputX * velocidad * 0.7f, Vel.y);
        Animar();
    }

    bool ComprobarSuelo()
    {
        Bounds b = col.bounds;
        Vector2 centro = new Vector2(b.center.x, b.min.y - 0.05f);
        Vector2 tam = new Vector2(b.size.x * 0.9f, 0.1f);
        var golpes = Physics2D.OverlapBoxAll(centro, tam, 0f);
        foreach (var g in golpes)
            if (g != col && !g.isTrigger) return true;
        return false;
    }

    void Animar()
    {
        if (inputX > 0.1f) sr.flipX = false;
        else if (inputX < -0.1f) sr.flipX = true;

        if (!enSuelo)
        {
            // fotograma del salto segun si sube o baja
            float t = Mathf.InverseLerp(fuerzaSalto, -fuerzaSalto, Vel.y);
            int i = Mathf.Clamp(Mathf.FloorToInt(t * saltar.Length), 0, saltar.Length - 1);
            sr.sprite = saltar[i];
        }
        else if (Mathf.Abs(inputX) > 0.1f)
        {
            animT += Time.deltaTime * 12f;
            sr.sprite = correr[(int)animT % correr.Length];
        }
        else
        {
            animT += Time.deltaTime * 5f;
            sr.sprite = parado[(int)animT % parado.Length];
        }

        // parpadeo cuando esta invulnerable
        bool parpadeo = Nivel1.Instancia != null && Nivel1.Instancia.Invulnerable && ((int)(Time.time * 15f) % 2 == 0);
        sr.enabled = !parpadeo;
    }

    public void Empujar(float direccion)
    {
        bloqueo = 0.25f;
        Vel = new Vector2(direccion * 5f, 7f);
    }

    public void Rebotar()
    {
        Vel = new Vector2(Vel.x, 10f);
    }

    public void Reaparecer(Vector2 pos)
    {
        transform.position = pos;
        Vel = Vector2.zero;
    }

    public void Congelar()
    {
        congelado = true;
        auto = false;
        inputX = 0f;
        sr.enabled = true;
        sr.sprite = parado[0];
    }

    public void IrA(float x)
    {
        destino = x;
        auto = true;
        llego = false;
        congelado = false;
        sr.enabled = true;
    }

    public void Ocultar()
    {
        oculto = true;
        sr.enabled = false;
        Vel = Vector2.zero;
        rb.simulated = false;
    }
}
