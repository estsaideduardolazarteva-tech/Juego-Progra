using UnityEngine;
using UnityEngine.SceneManagement;

// Cuando se carga una escena, mira su NOMBRE y arma el contenido.
// Asi las escenas pueden estar vacias: MenuPrincipal, Niveles y Nivel1.
public static class Arranque
{
    static Scene ultimaEscena;
    static bool hayUltima;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Registrar()
    {
        SceneManager.sceneLoaded -= AlCargar;
        SceneManager.sceneLoaded += AlCargar;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void PrimeraEscena()
    {
        Construir(SceneManager.GetActiveScene());
    }

    static void AlCargar(Scene escena, LoadSceneMode modo)
    {
        Construir(escena);
    }

    static void Construir(Scene escena)
    {
        if (hayUltima && escena == ultimaEscena) return;
        ultimaEscena = escena;
        hayUltima = true;
        Time.timeScale = 1f;

        switch (escena.name)
        {
            case "MenuPrincipal": new GameObject("MenuPrincipal").AddComponent<MenuPrincipal>(); break;
            case "Niveles": new GameObject("SelectorNiveles").AddComponent<SelectorNiveles>(); break;
            case "Nivel1": new GameObject("Nivel1").AddComponent<Nivel1>(); break;
        }
    }
}