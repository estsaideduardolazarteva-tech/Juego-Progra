using UnityEngine;

public enum TipoPickup { Moneda, Peligro, Meta }

// Objetos con los que el jugador choca sin frenarse: monedas, peligros (conos, pinchos...) y la meta.
public class Pickup : MonoBehaviour
{
    public TipoPickup tipo;
    public Sprite frente, dorso;   // solo monedas: giran mostrando las dos caras

    SpriteRenderer sr;
    float fase;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        if (tipo != TipoPickup.Moneda || sr == null) return;
        fase += Time.deltaTime * 3.2f;
        float c = Mathf.Cos(fase);
        transform.localScale = new Vector3(Mathf.Max(0.1f, Mathf.Abs(c)), 1f, 1f);
        if (frente != null && dorso != null) sr.sprite = c >= 0f ? frente : dorso;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.GetComponent<Jugador>() == null || Nivel1.Instancia == null) return;

        switch (tipo)
        {
            case TipoPickup.Moneda:
                Nivel1.Instancia.SumarMoneda();
                Destroy(gameObject);
                break;
            case TipoPickup.Peligro:
                Nivel1.Instancia.RecibirDanio(transform.position.x);
                break;
            case TipoPickup.Meta:
                Nivel1.Instancia.LlegarAMeta();
                break;
        }
    }
}
