using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Marraqueta : MonoBehaviour
{
    public static Marraqueta Instancia;

    [Header("Referencias de UI del Panel")]
    public GameObject panelTrivia;
    public TextMeshProUGUI textoPregunta;
    public Button[] botonesOpciones;
    public TextMeshProUGUI textoFeedback;
    public RectTransform minibusIcono; // El icono del minibús

    [Header("Posiciones de la Ruta (Eje X en la barra)")]
    public float[] posicionesParadasX; // Arrastra o define aquí las posiciones (ej: -100, 0, 100)

    [Header("Configuración de Límite")]
    public int limiteUsosPorNivel = 2; // Máximo 2 trivias por nivel
    private int triviasJugadas = 0;

    private bool abierto;
    private int preguntaActualIndex = 0;
    private int puntosGanados = 0;
    private const int totalParadasPorRuta = 3; // Mantenemos exactamente 3 preguntas por cada viaje

    // 🚎 BANCO AMPLIADO DE PREGUNTAS PACEÑAS
    private string[] bancoPreguntas = {
        "¿Qué gritas cuando el minibús va lleno y quieres bajar a media cuadra?",
        "¿Cuál es la ley climática inquebrantable a las 2 de la tarde en La Paz?",
        "¿Con qué se acompaña tradicionalmente un buen api con pastel en la mañana?",
        "¿Cómo se llama el pan redondo y crujiente tradicional de la hoyada?",
        "¿Qué famoso mercado tradicional es conocido por vender amuletos y ekekos?",
        "¿Cuál es el nombre de la imponente montaña blanca que vigila la ciudad?",
        "¿Cómo se llama el bus municipal de transporte masivo de la ciudad?",
        "¿Cuál es el mirador turístico famoso por sus coloridos techos y vista panorámica?"
    };

    private string[][] bancoOpciones = {
        new string[] { "¡Con su vuelto, chofer!", "¡Baja por favor!", "¡Esquina, m'hijo!" },
        new string[] { "Frío polar todo el día", "Sol rajante y granizada a las 4", "Neblina silenciosa" },
        new string[] { "Con refresco de durazno", "Con charla y apapudo", "Solo con agua" },
        new string[] { "Marraqueta", "Sarna", "Hallulla" },
        new string[] { "Mercado Lanza", "Mercado de Brujas", "Mercado Rodríguez" },
        new string[] { "Illimani", "Huayna Potosí", "Mururata" },
        new string[] { "Pumakatari", "Sariri", "Chikita" },
        new string[] { "Killi Killi", "Jallpha La Paz", "Sopocachi Alto" }
    };

    private int[] bancoRespuestas = { 2, 1, 1, 0, 1, 0, 0, 0 }; // Índices de la respuesta correcta de cada una

    // Lista para guardar las 3 preguntas seleccionadas al azar en este viaje
    private List<int> preguntasSeleccionadasRuta = new List<int>();

    void Awake()
    {
        Instancia = this;
        if (panelTrivia != null) panelTrivia.SetActive(false);
    }

    void Update()
    {
        // Se abre con la P, solo si no está abierta y si aún le quedan intentos disponibles (máx 2)
        if (!abierto && UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (triviasJugadas < limiteUsosPorNivel)
            {
                AbrirTrivia();
            }
            else
            {
                Debug.Log("¡Ya completaste el límite de 2 Minibús Trivias en este nivel, m'hijo!");
            }
        }
    }

    public void AbrirTrivia()
    {
        triviasJugadas++; // Aumentamos un intento usado
        abierto = true;
        preguntaActualIndex = 0;
        puntosGanados = 0;

        // Seleccionar 3 preguntas al azar del banco grande sin repetir en la misma partida
        SeleccionarPreguntasAleatorias();

        // Resetear la posición inicial del minibús en la primera parada
        if (minibusIcono != null)
        {
            Vector2 pos = minibusIcono.anchoredPosition;
            pos.x = (posicionesParadasX != null && posicionesParadasX.Length > 0) ? posicionesParadasX[0] : -100f;
            minibusIcono.anchoredPosition = pos;
        }

        if (panelTrivia != null) panelTrivia.SetActive(true);
        Time.timeScale = 0f;
        MostrarPregunta();
    }

    void SeleccionarPreguntasAleatorias()
    {
        preguntasSeleccionadasRuta.Clear();
        List<int> indicesDisponibles = new List<int>();
        for (int i = 0; i < bancoPreguntas.Length; i++) indicesDisponibles.Add(i);

        // Elegir exactamente 3 preguntas al azar
        for (int i = 0; i < totalParadasPorRuta; i++)
        {
            if (indicesDisponibles.Count > 0)
            {
                int randomIndex = Random.Range(0, indicesDisponibles.Count);
                preguntasSeleccionadasRuta.Add(indicesDisponibles[randomIndex]);
                indicesDisponibles.RemoveAt(randomIndex); // Evitar repetirse en el mismo viaje
            }
        }
    }

    void MostrarPregunta()
    {
        if (preguntaActualIndex < preguntasSeleccionadasRuta.Count)
        {
            int indexReal = preguntasSeleccionadasRuta[preguntaActualIndex];

            if (textoPregunta != null)
                textoPregunta.text = "🚎 Parada " + (preguntaActualIndex + 1) + " de 3:\n\n" + bancoPreguntas[indexReal];

            if (textoFeedback != null)
                textoFeedback.text = "";

            for (int i = 0; i < botonesOpciones.Length; i++)
            {
                int btnIndex = i;
                botonesOpciones[i].interactable = true;

                var textoBtn = botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>();
                if (textoBtn != null && i < bancoOpciones[indexReal].Length)
                    textoBtn.text = bancoOpciones[indexReal][i];

                botonesOpciones[i].onClick.RemoveAllListeners();
                botonesOpciones[i].onClick.AddListener(() => Responder(btnIndex, indexReal));
            }
        }
        else
        {
            CerrarTrivia();
        }
    }

    void Responder(int seleccion, int indexReal)
    {
        foreach (var btn in botonesOpciones) btn.interactable = false;

        if (seleccion == bancoRespuestas[indexReal])
        {
            puntosGanados += 20;
            if (textoFeedback != null)
                textoFeedback.text = "¡SAPE! ¡El minibús avanza a la siguiente parada!";

            // 🚎 HACER QUE EL MINIBÚS AVANCE EN LA BARRA
            if (minibusIcono != null)
            {
                Vector2 posActual = minibusIcono.anchoredPosition;

                if (posicionesParadasX != null && posicionesParadasX.Length > preguntaActualIndex)
                {
                    posActual.x = posicionesParadasX[preguntaActualIndex];
                }
                else
                {
                    posActual.x += 60f; // Avance automático de respaldo
                }

                minibusIcono.anchoredPosition = posActual;
            }

            // Sumar monedas
            int monedasActuales = PlayerPrefs.GetInt("monedas", 0);
            PlayerPrefs.SetInt("monedas", monedasActuales + 20);
            PlayerPrefs.Save();

            if (Nivel1.Instancia != null)
            {
                for (int i = 0; i < 20; i++) Nivel1.Instancia.SumarMoneda();
            }
        }
        else
        {
            if (textoFeedback != null)
                textoFeedback.text = "¡TRANCON! Te quedaste atascado.";
        }

        StartCoroutine(EsperaYPasarSiguiente());
    }

    IEnumerator EsperaYPasarSiguiente()
    {
        yield return new WaitForSecondsRealtime(1.2f);

        preguntaActualIndex++;

        if (preguntaActualIndex < preguntasSeleccionadasRuta.Count)
        {
            MostrarPregunta();
        }
        else
        {
            CerrarTrivia();
        }
    }

    void CerrarTrivia()
    {
        abierto = false;
        if (panelTrivia != null) panelTrivia.SetActive(false);
        Time.timeScale = 1f;
    }
}