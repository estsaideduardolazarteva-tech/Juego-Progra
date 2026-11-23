using System.Collections;
using UnityEngine;

// Dinamita lanzada: vuela en arco, cae al piso, explota despues de un momento (o si toca al jugador).
// La explosion quita una vida si el jugador esta cerca.
public class Dinamita : MonoBehaviour
{
    public const float GravedadEscala = 2f;
    const float Radio = 1.6f;      // alcance de la explosion
    const float Mecha = 1.0f;      // segundos desde que cae hasta que explota

    Sprite sSuelo;
    Sprite[] sExplosion;
    SpriteRenderer sr;
    Rigidbody2D rb;
    CircleCollider2D cc;
    Jugador jug;
    Collider2D jugCol;
    bool aterrizada, explotada;
    float fusible;

    public void Iniciar(Sprite vuelo, Sprite suelo, Sprite[] explosion, Jugador j, Vector2 velocidad, Collider2D ignorar)
    {
        sSuelo = suelo; sExplosion = explosion; jug = j;
        jugCol = j.GetComponent<Collider2D>();

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = vuelo;
        sr.sortingOrder = 7;

        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = GravedadEscala;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = velocidad;
#else
        rb.velocity = velocidad;
#endif
        rb.angularVelocity = velocidad.x >= 0f ? -360f : 360f;

        cc = gameObject.AddComponent<CircleCollider2D>();
        cc.radius = 0.25f;

        // no choca fisicamente con el jugador ni con quien la lanzo
        if (jugCol != null) Physics2D.IgnoreCollision(cc, jugCol);
        if (ignorar != null) Physics2D.IgnoreCollision(cc, ignorar);

        fusible = Mecha;
        Destroy(gameObject, 8f);
    }

    void Update()
    {
        if (explotada) return;

        if (!aterrizada)
        {
            // si pasa muy cerca del jugador, explota al instante
            if (jugCol != null && jugCol.bounds.SqrDistance(transform.position) < 0.35f * 0.35f) Explotar();
        }
        else
        {
            fusible -= Time.deltaTime;
            if (fusible <= 0f) Explotar();
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (aterrizada || explotada) return;
        aterrizada = true;
        sr.sprite = sSuelo;
        transform.rotation = Quaternion.identity;
        rb.angularVelocity = 0f;
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector2.zero;
#else
        rb.velocity = Vector2.zero;
#endif
        rb.simulated = false;
    }

    void Explotar()
    {
        explotada = true;
        sr.enabled = false;
        rb.simulated = false;
        cc.enabled = false;

        if (jugCol != null && Nivel.Instancia != null &&
            jugCol.bounds.SqrDistance(transform.position) <= Radio * Radio)
            Nivel.Instancia.RecibirDanio(transform.position.x);

        var fx = new GameObject("Explosion");
        fx.transform.position = transform.position;
        var fxSr = fx.AddComponent<SpriteRenderer>();
        fxSr.sortingOrder = 8;
        fx.AddComponent<EfectoExplosion>().Iniciar(sExplosion);
        Destroy(gameObject, 0.1f);
    }
}

// Animacion de la explosion: crece y luego se vuelve humo.
public class EfectoExplosion : MonoBehaviour
{
    Sprite[] sprites;
    SpriteRenderer sr;
    float t;

    public void Iniciar(Sprite[] s)
    {
        sprites = s;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprites[0];
        transform.localScale = Vector3.one * 0.5f;
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t < 0.28f)
        {
            sr.sprite = sprites[0];
            transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.1f, t / 0.28f);
        }
        else if (t < 0.6f)
        {
            sr.sprite = sprites[1];
            transform.localScale = Vector3.one;
            var c = sr.color; c.a = Mathf.Lerp(1f, 0f, (t - 0.28f) / 0.32f); sr.color = c;
        }
        else Destroy(gameObject);
    }
}
