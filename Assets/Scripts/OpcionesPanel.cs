using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Panel de CONFIGURACIONES (volumen general, musica y brillo). Se usa en el menu y en la pausa.
public static class OpcionesPanel
{
    public static GameObject Crear(Transform cont, UnityAction alCerrar, out Slider sliderMusica)
    {
        var oscuro = UIKit.Imagen(cont, "Opciones", new Color(0, 0, 0, 0.6f), 0, 0, 1, 1);
        var raiz = oscuro.gameObject;

        var borde = UIKit.Imagen(raiz.transform, "Borde", UIKit.Hex("2F4F73"), 0.20f, 0.12f, 0.80f, 0.88f);
        UIKit.Imagen(borde.transform, "Cuerpo", UIKit.Hex("F5A66A"), 0.015f, 0.02f, 0.985f, 0.98f);
        var titulo = UIKit.Imagen(borde.transform, "Titulo", UIKit.Hex("3F6A93"), 0.015f, 0.02f, 0.985f, 0.22f);
        UIKit.Texto(titulo.transform, "CONFIGURACIONES", 70, Color.white, TextAnchor.MiddleCenter, 0f, 0f, 1f, 1f);

        Fila(borde.transform, "VOLUMEN GENERAL", 0.30f, 0f, 1f, Ajustes.General, v => Ajustes.General = v);
        sliderMusica = Fila(borde.transform, "MÚSICA", 0.47f, 0f, 1f, Ajustes.Musica, v => Ajustes.Musica = v);
        Fila(borde.transform, "BRILLO", 0.64f, 0.25f, 1f, Ajustes.Brillo, v => Ajustes.Brillo = v);

        UIKit.BotonSolido(borde.transform, "VOLVER", UIKit.Hex("2F4F73"), 0.34f, 0.82f, 0.66f, 0.94f, alCerrar);

        raiz.SetActive(false);
        return raiz;
    }

    static Slider Fila(Transform padre, string etiqueta, float y, float min, float max, float valor, UnityAction<float> alCambiar)
    {
        UIKit.Texto(padre, etiqueta, 44, UIKit.Hex("1D2B4A"), TextAnchor.MiddleLeft, 0.07f, y, 0.48f, y + 0.12f);
        return UIKit.CrearSlider(padre, 0.52f, y + 0.02f, 0.93f, y + 0.10f, min, max, valor, alCambiar);
    }
}
