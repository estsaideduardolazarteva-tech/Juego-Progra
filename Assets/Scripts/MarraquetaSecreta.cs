using UnityEngine;

// CÓDIGO DE LA MARRAQUETA SECRETA
public class MarraquetaSecreta : MonoBehaviour
{
    bool abierto;
    string codigo = "";
    string aviso = "";
    GUIStyle estilo;

    void Update()
    {
        // Se abre con la tecla P
        if (!abierto && Input.GetKeyDown(KeyCode.P))
        {
            abierto = true;
            codigo = "";
            aviso = "";
            Time.timeScale = 0f; // Pausa el juego
        }
    }

    void OnDestroy() { Time.timeScale = 1f; }

    void Cerrar()
    {
        abierto = false;
        Time.timeScale = 1f; // Reanuda el juego
    }

    void Probar()
    {
        string c = codigo.Trim().ToUpperInvariant();
        if (c == "MARRAQUETA")
        {
            // Guarda el truco para que te dé velocidad al empezar
            PlayerPrefs.SetInt("marraqueta_gratis", 1);
            PlayerPrefs.Save();
            aviso = "¡Código correcto! Marraqueta activada";
        }
        else
        {
            aviso = "Código incorrecto";
        }
        codigo = "";
    }

    void OnGUI()
    {
        if (!abierto) return;
        float w = Screen.width, h = Screen.height;
        int fs = Mathf.RoundToInt(h * 0.04f);

        if (estilo == null)
        {
            estilo = new GUIStyle(GUI.skin.label);
            estilo.fontSize = fs;
            estilo.fontStyle = FontStyle.Bold;
            estilo.alignment = TextAnchor.MiddleCenter;
            estilo.normal.textColor = Color.white;
        }

        var campo = new GUIStyle(GUI.skin.textField);
        campo.fontSize = fs;
        campo.alignment = TextAnchor.MiddleCenter;

        var boton = new GUIStyle(GUI.skin.button);
        boton.fontSize = fs;

        // Fondo oscuro de pausa
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float px = w * 0.25f, pw = w * 0.5f, py = h * 0.18f;
        GUI.Label(new Rect(px, py, pw, h * 0.1f), "MENÚ SECRETO", estilo);
        GUI.Label(new Rect(px, py + h * 0.12f, pw, h * 0.08f), "Ingresa el código:", estilo);

        GUI.SetNextControlName("codigo");
        codigo = GUI.TextField(new Rect(px + pw * 0.1f, py + h * 0.21f, pw * 0.8f, h * 0.08f), codigo, 20, campo);

        var e = Event.current;
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            Probar();
            e.Use();
        }

        if (GUI.Button(new Rect(px + pw * 0.1f, py + h * 0.31f, pw * 0.38f, h * 0.08f), "Probar", boton)) Probar();
        if (GUI.Button(new Rect(px + pw * 0.52f, py + h * 0.31f, pw * 0.38f, h * 0.08f), "Continuar", boton)) Cerrar();

        if (aviso != "") GUI.Label(new Rect(px, py + h * 0.42f, pw, h * 0.08f), aviso, estilo);
    }
}
}