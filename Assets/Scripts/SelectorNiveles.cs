using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla de niveles (imagen 2): VOLVER, JUGAR el nivel 1 y los niveles 2 y 3 bloqueados.
public class SelectorNiveles : MonoBehaviour
{
    Text aviso;
    float tAviso;

    void Start()
    {
        var cam = Camera.main;
        if (cam != null) { cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = UIKit.Hex("0B1B3A"); }

        var canvas = UIKit.CrearCanvas("Canvas");
        var cont = UIKit.Contenedor(canvas, 1536f / 1024f, UIKit.Hex("0B1B3A"));

        UIKit.ImagenSprite(cont, "Fondo", UIKit.CargarSprite("niveles"), 0, 0, 1, 1);

        UIKit.BotonInvisible(cont, "Volver",  0.023f, 0.039f, 0.180f, 0.105f, Volver);
        UIKit.BotonInvisible(cont, "Nivel 1", 0.059f, 0.327f, 0.326f, 0.777f, () => SceneManager.LoadScene("Nivel1"));

        // Niveles 2 y 3: se desbloquean al terminar el anterior (se cambia el boton BLOQUEADO por JUGAR)
        var botonJugar = UIKit.CargarSprite("ui_btn_jugar");
        if (Progreso.Maximo >= 2) UIKit.ImagenSprite(cont, "Jugar 2", botonJugar, 0.4264f, 0.7051f, 0.5938f, 0.7598f);
        if (Progreso.Maximo >= 3) UIKit.ImagenSprite(cont, "Jugar 3", botonJugar, 0.7389f, 0.7070f, 0.9063f, 0.7598f);

        UIKit.BotonInvisible(cont, "Nivel 2", 0.371f, 0.332f, 0.638f, 0.785f, () => IrANivel(2));
        UIKit.BotonInvisible(cont, "Nivel 3", 0.685f, 0.332f, 0.939f, 0.785f, () => IrANivel(3));

        aviso = UIKit.Texto(cont, "", 44, Color.white, TextAnchor.MiddleCenter, 0.25f, 0.80f, 0.75f, 0.88f);
        var sombra = aviso.gameObject.AddComponent<Shadow>();
        sombra.effectColor = Color.black;
    }

    void Update()
    {
        if (Entrada.EscapePresionado()) Volver();
        if (tAviso > 0f)
        {
            tAviso -= Time.deltaTime;
            if (tAviso <= 0f) aviso.text = "";
        }
    }

    void Volver() { SceneManager.LoadScene("MenuPrincipal"); }

    void IrANivel(int n)
    {
        if (Progreso.Maximo >= n) SceneManager.LoadScene("Nivel" + n);
        else Bloqueado();
    }

    void Bloqueado()
    {
        aviso.text = "Nivel bloqueado: completa el anterior";
        tAviso = 1.8f;
    }
}
