using UnityEngine;

public enum TipoPickup { Moneda, Peligro, Meta }

// Objetos con los que el jugador choca sin frenarse: monedas, conos (peligro) y la meta.
public class Pickup : MonoBehaviour
{
    public TipoPickup tipo;

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
