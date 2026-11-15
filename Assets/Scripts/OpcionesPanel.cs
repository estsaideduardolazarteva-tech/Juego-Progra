using UnityEngine;
using UnityEngine.UI;

// Controla el panel de CONFIGURACIONES: refresca las barras al abrir, GUARDAR y CANCELAR.
public class OpcionesVista : MonoBehaviour
{
    public Slider general, musica, brillo;

    public bool Abierto { get { return gameObject.activeSelf; } }

    void OnEnable() { Refrescar(); }

    public void Refrescar()
    {
        if (general != null) general.SetValueWithoutNotify(Ajustes.General);
        if (musica != null) musica.SetValueWithoutNotify(Ajustes.Musica);
        if (brillo != null) brillo.SetValueWithoutNotify(Ajustes.Brillo);
    }

    public void Abrir() { gameObject.SetActive(true); }

    // GUARDAR CONFIGURACIONES: guarda y vuelve a la pantalla anterior
    public void Guardar()
    {
        Ajustes.Guardar();
        gameObject.SetActive(false);
    }

    // CANCELAR: descarta los cambios y vuelve a la pantalla anterior
    public void Cancelar()
    {
        Ajustes.Revertir();
        gameObject.SetActive(false);
    }
}

// Arma el panel con el dibujo ui_opciones_panel (marco azul, fondo naranja, barras pixel art).
public static class OpcionesPanel
{
    // Medidas sobre la imagen del panel (895 x 689)
    public const float Aspecto = 895f / 689f;

    public static OpcionesVista Crear(Transform cont)
    {
        var oscuro = UIKit.Imagen(cont, "Opciones", new Color(0, 0, 0, 0.6f), 0, 0, 1, 1);
        var raiz = oscuro.gameObject;
        var vista = raiz.AddComponent<OpcionesVista>();

        var holder = UIKit.Nuevo("Panel", raiz.transform);
        UIKit.Anclar(holder.GetComponent<RectTransform>(), 0.08f, 0.06f, 0.92f, 0.94f);
        var fit = holder.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fit.aspectRatio = Aspecto;

        var dibujo = holder.AddComponent<Image>();
        dibujo.sprite = UIKit.CargarSprite("ui_opciones_panel");

        // barras (una por fila)
        vista.general = UIKit.CrearSliderPixel(holder.transform, 0.644f, 0.238f, 0.918f, 0.284f, 0f, 1f, Ajustes.General, v => Ajustes.General = v);
        vista.musica  = UIKit.CrearSliderPixel(holder.transform, 0.644f, 0.340f, 0.918f, 0.386f, 0f, 1f, Ajustes.Musica,  v => Ajustes.Musica = v);
        vista.brillo  = UIKit.CrearSliderPixel(holder.transform, 0.644f, 0.449f, 0.918f, 0.495f, 0.25f, 1f, Ajustes.Brillo, v => Ajustes.Brillo = v);

        // botones dibujados en la imagen
        UIKit.BotonInvisible(holder.transform, "Guardar configuraciones", 0.142f, 0.624f, 0.857f, 0.763f, vista.Guardar);
        UIKit.BotonInvisible(holder.transform, "Cancelar", 0.142f, 0.798f, 0.857f, 0.938f, vista.Cancelar);

        raiz.SetActive(false);
        return vista;
    }
}
