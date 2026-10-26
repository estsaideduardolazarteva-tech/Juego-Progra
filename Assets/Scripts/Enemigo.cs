using UnityEngine;

// Enemigo que patrulla. Si lo pisas desde arriba muere; si lo tocas de lado pierdes una vida.
public class Enemigo : MonoBehaviour
{
    public float izquierda, derecha, velocidad = 2f;

    Rigidbody2D rb;
    BoxCollider2D col;
    int dir = 1;
    bool muerto;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        if (muerto) return;
        Vector2 p = rb.position;
        p.x += dir * velocidad * Time.fixedDeltaTime;
        if (p.x > derecha) { p.x = derecha; dir = -1; }
        if (p.x < izquierda) { p.x = izquierda; dir = 1; }
        rb.MovePosition(p);
    }

    void OnCollisionEnter2D(Collision2D c) { Tocar(c.collider); }
    void OnCollisionStay2D(Collision2D c) { Tocar(c.collider); }

    void Tocar(Collider2D otro)
    {
        if (muerto) return;
        var j = otro.GetComponent<Jugador>();
        if (j == null) return;

        float pies = otro.bounds.min.y;
        float techo = col.bounds.max.y;
        if (pies > techo - 0.3f && j.VelY <= 0.5f)
        {
            muerto = true;
            col.enabled = false;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 0.3f, 1f);
            j.Rebotar();
            Destroy(gameObject, 0.25f);
        }
        else if (Nivel1.Instancia != null)
        {
            Nivel1.Instancia.RecibirDanio(transform.position.x);
        }
    }
}
