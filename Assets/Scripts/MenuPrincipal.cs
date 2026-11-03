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

        panelOpciones = OpcionesPanel.Crear(cont, CerrarOpciones, out sliderMusica);
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
}
