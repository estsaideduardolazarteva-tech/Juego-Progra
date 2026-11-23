using System.Collections.Generic;
using UnityEngine;

// Dibuja textos con las letras y numeros pixel art (Assets/Resources/fuente). Solo A-Z, 0-9 y : . - !
public static class PixelFuente
{
    static readonly Dictionary<char, Texture2D> cache = new Dictionary<char, Texture2D>();
    const float AltoBase = 106f;   // alto de una letra en la imagen original

    static Texture2D Tex(char c)
    {
        c = char.ToUpperInvariant(c);
        switch (c)
        {
            case 'Á': c = 'A'; break;
            case 'É': c = 'E'; break;
            case 'Í': c = 'I'; break;
            case 'Ó': c = 'O'; break;
            case 'Ú': c = 'U'; break;
            case 'Ñ': c = 'N'; break;
        }
        Texture2D t;
        if (cache.TryGetValue(c, out t)) return t;

        string nombre = null;
        if (c >= 'A' && c <= 'Z') nombre = c.ToString();
        else if (c >= '0' && c <= '9') nombre = c.ToString();
        else if (c == ':') nombre = "dospuntos";
        else if (c == '.') nombre = "punto";
        else if (c == '-') nombre = "guion";
        else if (c == '!') nombre = "exclama";
        if (nombre != null) t = Resources.Load<Texture2D>("fuente/f_" + nombre);
        cache[c] = t;
        return t;
    }

    public static float Ancho(string texto, float alto)
    {
        float x = 0f;
        foreach (char c in texto)
        {
            var t = Tex(c);
            if (t == null) { x += alto * 0.45f; continue; }
            x += alto * t.width / AltoBase + alto * 0.04f;
        }
        return x;
    }

    public static void Dibujar(string texto, float x, float y, float alto)
    {
        foreach (char c in texto)
        {
            var t = Tex(c);
            if (t == null) { x += alto * 0.45f; continue; }
            float w = alto * t.width / AltoBase;
            float h = alto * t.height / AltoBase;
            GUI.DrawTexture(new Rect(x, y + alto - h, w, h), t, ScaleMode.StretchToFill, true);
            x += w + alto * 0.04f;
        }
    }

    public static void DibujarCentro(string texto, float xCentro, float y, float alto)
    {
        Dibujar(texto, xCentro - Ancho(texto, alto) / 2f, y, alto);
    }

    public static void DibujarDerecha(string texto, float xDerecha, float y, float alto)
    {
        Dibujar(texto, xDerecha - Ancho(texto, alto), y, alto);
    }
}
