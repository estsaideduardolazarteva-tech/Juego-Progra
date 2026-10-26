using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla de inicio: imagen de fondo + botones invisibles encima + panel de opciones.
public class MenuPrincipal : MonoBehaviour
{
    GameObject panelOpciones;
    Slider sliderMusica;

    void Start()
    {
        var cam = Camera.main;
        if (cam != null) { cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = UIKit.Hex("0B1B3A"); }

        var canvas = UIKit.CrearCanvas("Canvas");
        var cont = UIKit.Contenedor(canvas, 1536f / 1024f, UIKit.Hex("0B1B3A"));

        UIKit.ImagenSprite(cont, "Fondo", UIKit.CargarSprite("menu_principal"), 0, 0, 1, 1);

        // Botones (posiciones medidas sobre la imagen 1536 x 1024)
        UIKit.BotonInvisible(cont, "Jugar",    0.380f, 0.508f, 0.620f, 0.613f, Jugar);
        UIKit.BotonInvisible(cont, "Niveles",  0.384f, 0.635f, 0.615f, 0.723f, AbrirNiveles);
        UIKit.BotonInvisible(cont, "Opciones", 0.384f, 0.744f, 0.615f, 0.832f, AbrirOpciones);
        UIKit.BotonInvisible(cont, "Nivel 1 (tarjeta)", 0.706f, 0.469f, 0.990f, 0.576f, Jugar);
        UIKit.BotonInvisible(cont, "Musica", 0.762f, 0.887f, 0.811f, 0.957f, AlternarMusica);

        CrearPanelOpciones(cont);
    }

    void Update()
    {
        if (Entrada.EscapePresionado() && panelOpciones != null && panelOpciones.activeSelf)
            CerrarOpciones();
    }

    public void Jugar() { SceneManager.LoadScene("Nivel1"); }
    public void AbrirNiveles() { SceneManager.LoadScene("Niveles"); }
    public void AbrirOpciones() { panelOpciones.SetActive(true); }
    public void CerrarOpciones() { panelOpciones.SetActive(false); }

    void AlternarMusica()
    {
        Ajustes.Musica = Ajustes.Musica > 0f ? 0f : 1f;
        if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(Ajustes.Musica);
    }

    void CrearPanelOpciones(Transform cont)
    {
        var oscuro = UIKit.Imagen(cont, "Opciones", new Color(0, 0, 0, 0.6f), 0, 0, 1, 1);
        panelOpciones = oscuro.gameObject;

        var borde = UIKit.Imagen(panelOpciones.transform, "Borde", UIKit.Hex("2F4F73"), 0.20f, 0.12f, 0.80f, 0.88f);
        UIKit.Imagen(borde.transform, "Cuerpo", UIKit.Hex("F5A66A"), 0.015f, 0.02f, 0.985f, 0.98f);
        var titulo = UIKit.Imagen(borde.transform, "Titulo", UIKit.Hex("3F6A93"), 0.015f, 0.02f, 0.985f, 0.22f);
        UIKit.Texto(titulo.transform, "CONFIGURACIONES", 70, Color.white, TextAnchor.MiddleCenter, 0f, 0f, 1f, 1f);

        Fila(borde.transform, "VOLUMEN GENERAL", 0.30f, 0f, 1f, Ajustes.General, v => Ajustes.General = v);
        sliderMusica = Fila(borde.transform, "MÚSICA", 0.47f, 0f, 1f, Ajustes.Musica, v => Ajustes.Musica = v);
        Fila(borde.transform, "BRILLO", 0.64f, 0.25f, 1f, Ajustes.Brillo, v => Ajustes.Brillo = v);

        UIKit.BotonSolido(borde.transform, "VOLVER", UIKit.Hex("2F4F73"), 0.34f, 0.82f, 0.66f, 0.94f, CerrarOpciones);

        panelOpciones.SetActive(false);
    }

    Slider Fila(Transform padre, string etiqueta, float y, float min, float max, float valor, UnityEngine.Events.UnityAction<float> alCambiar)
    {
        UIKit.Texto(padre, etiqueta, 44, UIKit.Hex("1D2B4A"), TextAnchor.MiddleLeft, 0.07f, y, 0.48f, y + 0.12f);
        return UIKit.CrearSlider(padre, 0.52f, y + 0.02f, 0.93f, y + 0.10f, min, max, valor, alCambiar);
    }
}
