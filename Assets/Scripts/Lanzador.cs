using UnityEngine;

// Enemigo lanzador de dinamita. Se queda quieto, apunta al jugador y le tira dinamita.
// Igual que los otros enemigos: si lo pisas desde arriba muere; si lo tocas de lado pierdes una vida.
public class Lanzador : MonoBehaviour
{
    public float alcance = 11f;

    Sprite[] poses;           // 0-4 = secuencia de lanzar, 5-6 = parado
    Sprite sVuelo, sSuelo;
    Sprite[] sExplosion;
    SpriteRenderer sr;
    BoxCollider2D col;
    Jugador jug;

    float espera, cooldown = 1f, t, anim;
    int fase;                 // 0 parado, 1 preparando, 2 lanzando
    bool lanzado, muerto;

    public void Iniciar(Sprite[] sPoses, Sprite vuelo, Sprite suelo, Sprite[] explosion, float tiempoEntre)
    {
        poses = sPoses; sVuelo = vuelo; sSuelo = suelo; sExplosion = explosion; espera = tiempoEntre;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (muerto || poses == null) return;
        if (Nivel.Instancia == null) return;
        if (jug == null) jug = Nivel.Instancia.JugadorActual;
        if (jug == null) return;

        float dx = jug.transform.position.x - transform.position.x;
        float dy = jug.transform.position.y - transform.position.y;
        sr.flipX = dx < 0f;   // los dibujos miran a la derecha
        cooldown -= Time.deltaTime;

        switch (fase)
        {
            case 0:
                anim += Time.deltaTime * 2.5f;
                sr.sprite = poses[5 + ((int)anim % 2)];
                if (cooldown <= 0f && Nivel.Instancia.Jugando && Mathf.Abs(dx) < alcance && Mathf.Abs(dy) < 7f)
                {
                    fase = 1; t = 0f;
                }
                break;

            case 1:   // prepara la dinamita (levanta el brazo)
                t += Time.deltaTime;
                sr.sprite = poses[t < 0.25f ? 0 : (t < 0.5f ? 1 : 2)];
                if (t >= 0.75f) { fase = 2; t = 0f; lanzado = false; }
                break;

            case 2:   // lanza
                t += Time.deltaTime;
                sr.sprite = t < 0.18f ? poses[3] : poses[4];
                if (!lanzado && t >= 0.1f) { lanzado = true; Lanzar(); }
                if (t >= 0.45f) { fase = 0; cooldown = espera; }
                break;
        }
    }

    void Lanzar()
    {
        if (jug == null) return;
        float lado = sr.flipX ? -1f : 1f;
        Vector2 origen = (Vector2)transform.position + new Vector2(lado * 0.6f, 1.5f);
        Vector2 destino = new Vector2(jug.transform.position.x, jug.transform.position.y + 0.6f);

        // trayectoria en arco que cae donde esta el jugador
        float dxx = destino.x - origen.x;
        float T = Mathf.Clamp(Mathf.Abs(dxx) / 7f, 0.7f, 1.4f);
        float g = -Physics2D.gravity.y * Dinamita.GravedadEscala;
        float vx = dxx / T;
        float vy = (destino.y - origen.y + 0.5f * g * T * T) / T;

        var go = new GameObject("Dinamita");
        go.transform.position = origen;
        var d = go.AddComponent<Dinamita>();
        d.Iniciar(sVuelo, sSuelo, sExplosion, jug, new Vector2(vx, vy), col);
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
            transform.localScale = new Vector3(transform.localScale.x, 0.3f, 1f);   // aplastado
            j.Rebotar();
            Destroy(gameObject, 0.6f);
        }
        else if (Nivel.Instancia != null)
        {
            Nivel.Instancia.RecibirDanio(transform.position.x);
        }
    }
}
