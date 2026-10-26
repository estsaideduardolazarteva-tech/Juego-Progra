using UnityEngine;
using UnityEngine.UI;

// Guarda volumen general, musica y brillo. Se crea solo y vive en todas las escenas.
public static class Ajustes
{
    const string KGeneral = "vol_general";
    const string KMusica = "vol_musica";
    const string KBrillo = "brillo";

    static Image velo;          // capa negra que simula el brillo
    static AudioSource musica;  // reproduce Resources/musica (opcional)

    public static float General
    {
        get { return PlayerPrefs.GetFloat(KGeneral, 1f); }
        set { PlayerPrefs.SetFloat(KGeneral, value); Aplicar(); }
    }

    public static float Musica
    {
        get { return PlayerPrefs.GetFloat(KMusica, 1f); }
        set { PlayerPrefs.SetFloat(KMusica, value); Aplicar(); }
    }

    public static float Brillo
    {
        get { return PlayerPrefs.GetFloat(KBrillo, 1f); }
        set { PlayerPrefs.SetFloat(KBrillo, value); Aplicar(); }
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

        musica = go.AddComponent<AudioSource>();
        musica.loop = true;
        musica.playOnAwake = false;
        var clip = Resources.Load<AudioClip>("musica");
        if (clip != null) { musica.clip = clip; musica.Play(); }

        Aplicar();
    }

    public static void Aplicar()
    {
        AudioListener.volume = General;
        if (musica != null) musica.volume = Musica;
        if (velo != null) velo.color = new Color(0f, 0f, 0f, 1f - Brillo);
    }
}
