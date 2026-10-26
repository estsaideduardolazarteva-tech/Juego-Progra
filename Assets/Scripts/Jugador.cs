using UnityEngine;

// Movimiento tipo Mario: correr, saltar (mas alto si mantienes), animacion de caminar con 2 sprites.
public class Jugador : MonoBehaviour
{
    public float velocidad = 6.5f;
    public float fuerzaSalto = 13f;

    Rigidbody2D rb;
    BoxCollider2D col;
    SpriteRenderer sr;
    Sprite parado, paso1, paso2;

    float inputX, saltoBuffer, coyote, animT, bloqueo;
    bool enSuelo, congelado;

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

    public void Iniciar(Sprite sParado, Sprite sPaso1, Sprite sPaso2)
    {
        parado = sParado; paso1 = sPaso1; paso2 = sPaso2;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr.sprite = parado;

        if (PlayerPrefs.GetInt("marraqueta_gratis", 0) == 1)
        {
            velocidad = 10f; // Sube la velocidad normal (que era 6.5f) a 10f para ir super rápido
            Debug.Log("¡Efecto Marraqueta Activado! Velocidad aumentada.");
        }
    }

    void Update()
    {
        if (rb == null) return;

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
            AudioClip clipSalto = Resources.Load<AudioClip>("Audio/salto"); // Asegúrate de que tu archivo de audio en la carpeta Resources/Audio se llame "salto"
            if (clipSalto != null)
            {
                AudioSource.PlayClipAtPoint(clipSalto, transform.position, 0.7f);
            }
        }
        if (Entrada.SaltoSoltado() && v.y > 0f) v.y *= 0.5f; // salto corto si sueltas rapido
        Vel = v;

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

        if (!enSuelo) sr.sprite = paso1;
        else if (Mathf.Abs(inputX) > 0.1f)
        {
            animT += Time.deltaTime;
            sr.sprite = ((int)(animT / 0.16f) % 2 == 0) ? paso1 : paso2;
        }
        else { sr.sprite = parado; animT = 0f; }

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
        inputX = 0f;
        sr.enabled = true;
        sr.sprite = parado;
    }
}
