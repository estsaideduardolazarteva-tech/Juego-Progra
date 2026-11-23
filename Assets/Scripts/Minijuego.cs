using System;
using UnityEngine;
using UnityEngine.UI;

// Retos opcionales que se abren con P y se juegan con el ratón.
public sealed class Minijuego
{
    const int Premio = 20;
    const int IntentosMaximos = 2;
    static readonly string[] titulos = { "Minibús Trivia", "Memoria del mercado", "Semáforo" };
    static readonly string[] puestos = { "Api", "Pan", "Queso" };
    static readonly string[,] trivia =
    {
        { "¿Qué pan paceño es crujiente y tiene un corte al medio?", "Sarna", "Marraqueta", "Hallulla" },
        { "¿Qué mercado es conocido por sus amuletos y ekekos?", "Mercado Lanza", "Mercado de Brujas", "Mercado Rodríguez" },
        { "¿Cómo se llama el bus municipal de La Paz?", "Pumakatari", "Sariri", "Chigita" },
        { "¿Qué montaña nevada se ve desde La Paz?", "Illimani", "Huayna Potosí", "Mururata" },
        { "¿Qué mirador paceño es conocido por su vista panorámica de la ciudad?", "Killi Killi", "Jallpha La Paz", "Sopocachi Alto" },
        { "¿Con qué se acompaña tradicionalmente un buen api?", "Con queso", "Con un pastel", "Con helado" }
    };
    static readonly int[] respuestas = { 1, 1, 0, 0, 0, 1 };

    enum Fase { Responder, Memorizar, EsperarVerde, Verde, Siguiente, Resultado }

    readonly int nivel;
    readonly Action<int> abonar;
    readonly Action agregarVida;
    readonly Canvas canvas;
    readonly Text titulo, mensaje;
    readonly Button[] botones = new Button[3];
    readonly int[] preguntas = { 0, 1, 2, 3, 4, 5 };
    readonly int[] secuencia = new int[3];
    int intentos, ronda, paso, ganadas;
    float limite;
    Fase fase;

    public bool Abierto => canvas.gameObject.activeSelf;
    public bool Disponible => intentos < IntentosMaximos;

    public Minijuego(int nivel, Transform padre, Action<int> abonar, Action agregarVida)
    {
        this.nivel = nivel;
        this.abonar = abonar;
        this.agregarVida = agregarVida;

        canvas = UIKit.CrearCanvas(titulos[nivel - 1]);
        canvas.transform.SetParent(padre, false);
        canvas.sortingOrder = 110;
        var cont = UIKit.Contenedor(canvas, 16f / 9f, UIKit.Hex("0B1B3A"));
        UIKit.Imagen(cont, "Tarjeta", UIKit.Hex("162C4D"), .13f, .12f, .87f, .88f);
        titulo = UIKit.Texto(cont, "", 64, UIKit.Hex("FFD66B"), TextAnchor.MiddleCenter, .18f, .73f, .82f, .85f);
        mensaje = UIKit.Texto(cont, "", 48, Color.white, TextAnchor.MiddleCenter, .19f, .52f, .81f, .72f);

        for (int i = 0; i < botones.Length; i++)
        {
            int opcion = i;
            float y = .42f - i * .10f;
            botones[i] = UIKit.BotonSolido(cont, "", UIKit.Hex("285786"), .24f, y, .76f, y + .085f,
                () => Elegir(opcion));
        }
        UIKit.BotonSolido(cont, "Volver al nivel", UIKit.Hex("435571"), .36f, .025f, .64f, .11f, Cerrar);
        canvas.gameObject.SetActive(false);
    }

    public void Abrir()
    {
        if (!Disponible || Abierto) return;
        intentos++;
        ronda = 0;
        paso = 0;
        ganadas = 0;
        canvas.gameObject.SetActive(true);
        Time.timeScale = 0f;

        if (nivel == 1)
        {
            for (int i = 0; i < preguntas.Length; i++)
            {
                int otro = UnityEngine.Random.Range(i, preguntas.Length);
                (preguntas[i], preguntas[otro]) = (preguntas[otro], preguntas[i]);
            }
            MostrarPregunta();
        }
        else if (nivel == 2)
        {
            for (int i = 0; i < secuencia.Length; i++)
                secuencia[i] = UnityEngine.Random.Range(0, puestos.Length);
            titulo.text = titulos[1] + " · Premio: 1 vida";
            mensaje.text = "Memoriza el orden\n" + string.Join("  →  ", Array.ConvertAll(secuencia, i => puestos[i]));
            MostrarBotones();
            Esperar(Fase.Memorizar, 3.5f);
        }
        else
        {
            titulo.text = titulos[2];
            mensaje.text = "Espera a que cambie a verde\ny pulsa el botón en menos de 0,9 segundos.";
            MostrarBotones("ROJO");
            Esperar(Fase.EsperarVerde, UnityEngine.Random.Range(2.5f, 4.5f));
        }
    }

    public void Actualizar()
    {
        if (Entrada.EscapePresionado())
        {
            Cerrar();
            return;
        }
        if (Time.unscaledTime < limite) return;

        switch (fase)
        {
            case Fase.Memorizar:
                fase = Fase.Responder;
                mensaje.text = "Repite el orden de los puestos.\nPaso 1 de 3";
                MostrarBotones(puestos);
                break;
            case Fase.EsperarVerde:
                mensaje.text = "¡Ahora!";
                MostrarBotones("VERDE");
                Esperar(Fase.Verde, .9f);
                break;
            case Fase.Verde:
                Terminar("Se acabó el tiempo.");
                break;
            case Fase.Siguiente:
                if (ronda < 3) MostrarPregunta();
                else Terminar("Terminaste la ruta.");
                break;
        }
    }

    void MostrarPregunta()
    {
        int pregunta = preguntas[ronda];
        titulo.text = "Minibús Trivia · Parada " + (ronda + 1) + " de 3";
        mensaje.text = trivia[pregunta, 0];
        fase = Fase.Responder;
        MostrarBotones(trivia[pregunta, 1], trivia[pregunta, 2], trivia[pregunta, 3]);
    }

    void Elegir(int opcion)
    {
        if (!Abierto) return;
        switch (fase)
        {
            case Fase.Responder:
                if (nivel == 1)
                {
                    bool correcta = opcion == respuestas[preguntas[ronda]];
                    if (correcta) DarPremio();
                    ronda++;
                    mensaje.text = correcta ? "¡Correcto! +" + Premio + " monedas" : "La respuesta no era esa.";
                    MostrarBotones();
                    Esperar(Fase.Siguiente, 1.1f);
                }
                else if (opcion != secuencia[paso]) Terminar("Ese puesto no seguía.");
                else if (++paso == secuencia.Length)
                {
                    agregarVida();
                    ganadas = 1;
                    Terminar("¡Orden correcto!");
                }
                else mensaje.text = "Repite el orden de los puestos.\nPaso " + (paso + 1) + " de 3";
                break;
            case Fase.EsperarVerde:
                Terminar("Pulsaste antes de que cambiara a verde.");
                break;
            case Fase.Verde:
                if (Time.unscaledTime >= limite) Terminar("Se acabó el tiempo.");
                else
                {
                    DarPremio();
                    Terminar("¡A tiempo!");
                }
                break;
        }
    }

    void MostrarBotones(params string[] etiquetas)
    {
        for (int i = 0; i < botones.Length; i++)
        {
            botones[i].gameObject.SetActive(i < etiquetas.Length);
            if (i >= etiquetas.Length) continue;
            botones[i].GetComponentInChildren<Text>().text = etiquetas[i];
            botones[i].image.color = etiquetas[i] == "ROJO" ? UIKit.Hex("9A3039") :
                                     etiquetas[i] == "VERDE" ? UIKit.Hex("237447") : UIKit.Hex("285786");
        }
    }

    void DarPremio()
    {
        ganadas += Premio;
        abonar(Premio);
    }

    void Terminar(string texto)
    {
        fase = Fase.Resultado;
        string premio = nivel == 2 ? "Vidas extra: " : "Monedas ganadas: ";
        mensaje.text = texto + "\n" + premio + ganadas;
        MostrarBotones();
    }

    void Esperar(Fase siguiente, float segundos)
    {
        fase = siguiente;
        limite = Time.unscaledTime + segundos;
    }

    void Cerrar()
    {
        canvas.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
