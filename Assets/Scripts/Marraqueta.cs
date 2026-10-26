using UnityEngine;

// MINIJUEGO DE TRIVIA PACEÑA PARA GANAR MONEDAS EXTRAS
public class Marraqueta : MonoBehaviour
{
    bool abierto;
    string intento = "";
    string mensaje = "";
    int indiceActual = 0;

    GUIStyle estiloTexto;
    GUIStyle estiloBoton;
    GUIStyle estiloInput;

    // 10 preguntas y respuestas sobre La Paz
    string[] pistas = {
        "Pista: El pan crujiente y tradicional de La Paz",
        "Pista: El famoso sistema de transporte por cable en el cielo",
        "Pista: El mercado tradicional conocido por vender amuletos y ekikos",
        "Pista: La plaza principal ubicada en el kilómetro cero de la ciudad",
        "Pista: El imponente nevado blanco que cuida y vigila La Paz",
        "Pista: La calle colonial y empedrada más famosa del centro",
        "Pista: El bus municipal de transporte masivo de la ciudad",
        "Pista: El plato típico paceño con choclo, queso y habas cocidas",
        "Pista: El estadio de fútbol oficial ubicado en la zona de Miraflores",
        "Pista: El mirador turístico con una vista hermosa de la hoyada"
    };

    string[] respuestas = {
        "MARRAQUETA", "TELEFERICO", "MERCADO DE BRUJAS", "PLAZA MURILLO", "ILLIMANI", "JAEN", "PUMAKATARI", "QHUISPO", "HERNANDO SILES", "KILLIKILLI"
    };

    void Update()
    {
        // Se abre presionando la tecla P
        if (!abierto && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame)
        {
            abierto = true;
            intento = "";

            // Elige una pregunta al azar de las 10
            indiceActual = Random.Range(0, pistas.Length);
            mensaje = pistas[indiceActual];

            Time.timeScale = 0f; // Pausa el juego
        }
    }

    void OnDestroy() { Time.timeScale = 1f; }

    void Cerrar()
    {
        abierto = false;
        Time.timeScale = 1f; // Reanuda el juego
    }

    void VerificarRespuesta()
    {
        string respuestaUsuario = intento.Trim().ToUpperInvariant();
        string respuestaCorrecta = respuestas[indiceActual];

        if (respuestaUsuario == respuestaCorrecta)
        {
            mensaje = "¡Correcto! ¡Ganaste 20 monedas extras!";

            // Suma 20 monedas a las monedas totales del juego usando PlayerPrefs
            int monedasActuales = PlayerPrefs.GetInt("monedas", 0);
            PlayerPrefs.SetInt("monedas", monedasActuales + 20);
            PlayerPrefs.Save();
        }
        else
        {
            mensaje = "¡Fallaste! La respuesta era: " + respuestaCorrecta;
        }
        intento = "";
    }

    void OnGUI()
    {
        if (!abierto) return;

        float w = Screen.width;
        float h = Screen.height;
        int fs = Mathf.RoundToInt(h * 0.03f);

        if (estiloTexto == null)
        {
            estiloTexto = new GUIStyle(GUI.skin.label);
            estiloTexto.fontSize = fs;
            estiloTexto.fontStyle = FontStyle.Bold;
            estiloTexto.alignment = TextAnchor.MiddleCenter;
            estiloTexto.normal.textColor = Color.white;
            estiloTexto.wordWrap = true;
        }

        if (estiloInput == null)
        {
            estiloInput = new GUIStyle(GUI.skin.textField);
            estiloInput.fontSize = fs;
            estiloInput.alignment = TextAnchor.MiddleCenter;
        }

        if (estiloBoton == null)
        {
            estiloBoton = new GUIStyle(GUI.skin.button);
            estiloBoton.fontSize = fs;
        }

        // Fondo oscuro translúcido
        GUI.color = new Color(0f, 0f, 0f, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float px = w * 0.15f;
        float pw = w * 0.7f;
        float py = h * 0.2f;

        // Elementos visuales
        GUI.Label(new Rect(px, py, pw, h * 0.1f), "TRIVIA PACEÑA - ¡GANATE MONEDAS!", estiloTexto);
        GUI.Label(new Rect(px, py + h * 0.1f, pw, h * 0.12f), mensaje, estiloTexto);

        GUI.SetNextControlName("campoRespuesta");
        intento = GUI.TextField(new Rect(px + pw * 0.1f, py + h * 0.24f, pw * 0.8f, h * 0.08f), intento, 25, estiloInput);

        // Detectar tecla Enter para enviar respuesta rápido
        Event e = Event.current;
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            VerificarRespuesta();
            e.Use();
        }

        // Botones
        if (GUI.Button(new Rect(px + pw * 0.1f, py + h * 0.36f, pw * 0.38f, h * 0.09f), "Comprobar", estiloBoton))
        {
            VerificarRespuesta();
        }

        if (GUI.Button(new Rect(px + pw * 0.52f, py + h * 0.36f, pw * 0.38f, h * 0.09f), "Continuar", estiloBoton))
        {
            Cerrar();
        }
    }
}