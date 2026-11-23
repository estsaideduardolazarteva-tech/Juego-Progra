using UnityEngine;

// Que niveles estan desbloqueados (se guarda en el equipo).
public static class Progreso
{
    const string Clave = "nivel_max";
    public const int TotalNiveles = 3;

    public static int Maximo { get { return PlayerPrefs.GetInt(Clave, 1); } }

    public static void Desbloquear(int nivel)
    {
        if (nivel > TotalNiveles) return;
        if (nivel > Maximo) { PlayerPrefs.SetInt(Clave, nivel); PlayerPrefs.Save(); }
    }
}
