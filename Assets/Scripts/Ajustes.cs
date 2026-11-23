using UnityEngine;
using UnityEngine.UI;

// Volumen general, musica y brillo.
// Los cambios se ven/oyen al instante (vista previa) pero SOLO se guardan al apretar GUARDAR.
// CANCELAR vuelve a los valores guardados.
public static class Ajustes
{
    const string KGeneral = "vol_general";
    const string KMusica = "vol_musica";
    const string KBrillo = "brillo";

    static float general = 1f, musica = 1f, brillo = 1f;
    static Image velo;          // capa negra que simula el brillo
    static AudioSource fuenteMusica, efectos;

    public static float General { get { return general; } set { general = value; Aplicar(); } }
    public static float Musica { get { return musica; } set { musica = value; Aplicar(); } }
    public static float Brillo { get { return brillo; } set { brillo = value; Aplicar(); } }

    public static void Guardar()
    {
        PlayerPrefs.SetFloat(KGeneral, general);
        PlayerPrefs.SetFloat(KMusica, musica);
        PlayerPrefs.SetFloat(KBrillo, brillo);
        PlayerPrefs.Save();
    }

    // Vuelve a lo ultimo que se guardo
    public static void Revertir()
    {
        general = PlayerPrefs.GetFloat(KGeneral, 1f);
        musica = PlayerPrefs.GetFloat(KMusica, 1f);
        brillo = PlayerPrefs.GetFloat(KBrillo, 1f);
        Aplicar();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Iniciar()
    {
        var go = new GameObject("[Ajustes]");
        Object.DontDestroyOnLoad(go);

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;

        var hijo = new GameObject("Brillo", typeof(RectTransform));
        hijo.transform.SetParent(go.transform, false);
        velo = hijo.AddComponent<Image>();
        velo.raycastTarget = false;
        var rt = velo.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        fuenteMusica = go.AddComponent<AudioSource>();
        fuenteMusica.loop = true;
        fuenteMusica.playOnAwake = false;
        efectos = go.AddComponent<AudioSource>();
        efectos.playOnAwake = false;

        Revertir();   // carga lo guardado y lo aplica
    }

    public static AudioClip CargarSonido(string nombre)
    {
        return Resources.Load<AudioClip>("Sounds/" + nombre);
    }

    public static void CambiarMusica(string nombre)
    {
        fuenteMusica.Stop();
        fuenteMusica.clip = nombre == null ? null : CargarSonido(nombre);
        if (fuenteMusica.clip != null) fuenteMusica.Play();
    }

    public static void DetenerEfectos()
    {
        efectos.Stop();
    }

    public static void Sonar(AudioClip clip, float volumen = 1f)
    {
        if (clip != null) efectos.PlayOneShot(clip, volumen);
    }

    public static void Aplicar()
    {
        AudioListener.volume = general;
        if (fuenteMusica != null) fuenteMusica.volume = musica * 0.4f;
        if (velo != null) velo.color = new Color(0f, 0f, 0f, 1f - brillo);
    }
}
