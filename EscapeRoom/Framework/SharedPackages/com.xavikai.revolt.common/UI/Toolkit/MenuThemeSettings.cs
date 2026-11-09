using UnityEngine;

namespace EscapeRoomRevolt.UI.Toolkit
{
    /// <summary>
    /// Optional re-skin for UIToolkitMenuController: panel color, accent border, title/button
    /// colors, background and button images, two fonts, a logo, the main-menu title and the credits text, all assignable from the Inspector without touching USS or
    /// code. Defaults match the shipped look exactly, so creating one and leaving it untouched
    /// changes nothing; assign it on UIToolkitMenuController's "_theme" field to activate it.
    /// Leaving that field empty (the default) keeps using the static EscapeRoomMenu.uss values.
    /// </summary>
    [CreateAssetMenu(fileName = "MenuThemeSettings", menuName = "Escape Room Framework/Menu Theme Settings")]
    public sealed class MenuThemeSettings : ScriptableObject
    {
        [Header("Colores")]
        [Tooltip("Fondo del panel del menú.")]
        public Color panelBackground = new Color32(32, 31, 25, 255);
        [Tooltip("Borde del panel, y color de acento reutilizado en tarjetas y campos de reasignación de teclas.")]
        public Color accent = new Color32(125, 108, 70, 255);
        [Tooltip("Color de texto del título de cada pantalla.")]
        public Color titleText = new Color32(223, 207, 158, 255);
        [Tooltip("Color de fondo de los botones de menú.")]
        public Color buttonBackground = new Color32(51, 48, 39, 255);
        [Tooltip("Color de fondo de los botones de menú al pasar el ratón por encima.")]
        public Color buttonBackgroundHover = new Color32(79, 73, 57, 255);
        [Tooltip("Color de texto de los botones de menú.")]
        public Color buttonText = new Color32(230, 225, 205, 255);

        [Header("Tipografía")]
        [Tooltip("Fuente del título de cada pantalla. Vacío = fuente por defecto de Unity.")]
        public Font titleFont;
        [Tooltip("Fuente del resto de textos (botones, ajustes, etiquetas). Vacío = fuente por defecto de Unity.")]
        public Font bodyFont;

        [Header("Imágenes")]
        [Tooltip("Imagen que llena toda la pantalla detrás del menú principal. Importa la imagen como Sprite (2D and UI). Vacío = fondo oscuro de la plantilla.")]
        public Sprite screenBackground;
        [Tooltip("Muestra también la imagen de fondo en el menú de pausa (taparía el juego). Desmarcado = solo en el menú principal.")]
        public bool screenBackgroundInPause;
        [Tooltip("Imagen del marco del menú (el panel donde están el título y los botones). Se estira al tamaño del marco; el color Panel Background se ve por debajo si la imagen tiene transparencia.")]
        public Sprite panelImage;
        [Tooltip("Píxeles de borde de la imagen del marco que no se estiran (9-slice). 0 = se estira toda la imagen.")]
        [Min(0)] public int panelImageSlice;
        [Tooltip("Imagen de fondo de los botones del menú, sin texto: el texto lo pone el juego y se traduce. Sustituye el color Button Background.")]
        public Sprite buttonImage;
        [Tooltip("Imagen del botón con el ratón encima. Vacío = se usa la misma imagen que Button Image.")]
        public Sprite buttonHoverImage;
        [Tooltip("Píxeles de borde de las imágenes de botón que no se estiran (9-slice). 0 = se estira toda la imagen.")]
        [Min(0)] public int buttonImageSlice;

        [Header("Marca")]
        [Tooltip("Se muestra encima del título en todas las pantallas del menú. Vacío = no se muestra nada.")]
        public Sprite logo;

        [Header("Textos propios")]
        [Tooltip("Título de la pantalla principal (por ejemplo, el nombre de tu juego). Vacío = título original de la plantilla. Si añades la misma cadena como clave en el catálogo de localización, también se traduce.")]
        public string mainMenuTitle = "";
        [Tooltip("Texto de la pantalla Créditos: autores, asignatura, licencias de terceros. Vacío = texto de ejemplo de la plantilla.")]
        [TextArea(3, 12)] public string creditsText = "";
    }
}
