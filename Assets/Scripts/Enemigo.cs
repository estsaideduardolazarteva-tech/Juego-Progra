using UnityEngine;

// Monstrito que camina de izquierda a derecha. Si lo pisas desde arriba se aplasta; si lo tocas de lado pierdes una vida.
public class Enemigo : MonoBehaviour
{
    public float velocidad = 2f;

    float izquierda, derecha, anim;
    Sprite[] caminar;
    Sprite aplastado;
    SpriteRenderer sr;
    Rigidbody2D rb;
    BoxCollider2D col;
    int dir = 1;
    bool muerto;

    public void Iniciar(Sprite[] sCaminar, Sprite sAplastado, float izq, float der)
    {
        caminar = sCaminar; aplastado = sAplastado; izquierda = izq; derecha = der;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (muerto || caminar == null) return;
        anim += Time.deltaTime * 6f;
        sr.sprite = caminar[(int)anim % caminar.Length];
        sr.flipX = dir < 0;   // los dibujos miran a la derecha
    }

    void FixedUpdate()
    {
        if (muerto || rb == null) return;
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
            sr.sprite = aplastado;   // dibujo del monstrito aplastado
            j.Rebotar();
            Destroy(gameObject, 0.6f);
        }
        else if (Nivel1.Instancia != null)
        {
            Nivel1.Instancia.RecibirDanio(transform.position.x);
        }
    }
}
