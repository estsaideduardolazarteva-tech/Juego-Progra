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
        UIKit.BotonInvisible(cont, "Nivel 2", 0.371f, 0.332f, 0.638f, 0.785f, Bloqueado);
        UIKit.BotonInvisible(cont, "Nivel 3", 0.685f, 0.332f, 0.939f, 0.785f, Bloqueado);

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

    void Bloqueado()
    {
        aviso.text = "Nivel bloqueado: completa el anterior";
        tAviso = 1.8f;
    }
}
