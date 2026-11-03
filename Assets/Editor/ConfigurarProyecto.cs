#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Crea las 3 escenas y las agrega al Build Settings con un solo clic.
public static class ConfigurarProyecto
{
    [MenuItem("Hora de Salida/Crear escenas (hacer una sola vez)")]
    public static void CrearEscenas()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Directory.CreateDirectory("Assets/Scenes");
        string[] nombres = { "MenuPrincipal", "Niveles", "Nivel1" };
        var lista = new List<EditorBuildSettingsScene>();

        foreach (string n in nombres)
        {
            var escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            string ruta = "Assets/Scenes/" + n + ".unity";
            EditorSceneManager.SaveScene(escena, ruta);
            lista.Add(new EditorBuildSettingsScene(ruta, true));
        }

        EditorBuildSettings.scenes = lista.ToArray();
        EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity");
        EditorUtility.DisplayDialog("Listo",
            "Se crearon las escenas MenuPrincipal, Niveles y Nivel1.\n\nAhora presiona el boton Play (triangulo) con MenuPrincipal abierta.", "OK");
    }
}
#endif
