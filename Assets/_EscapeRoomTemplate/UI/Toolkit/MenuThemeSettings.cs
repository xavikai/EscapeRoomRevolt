using UnityEngine;

namespace EscapeRoomRevolt.UI.Toolkit
{
    /// <summary>
    /// Optional re-skin for UIToolkitMenuController: panel color, accent border, title/button
    /// colors, two fonts and a logo, all assignable from the Inspector without touching USS or
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

        [Header("Marca")]
        [Tooltip("Se muestra encima del título en todas las pantallas del menú. Vacío = no se muestra nada.")]
        public Sprite logo;
    }
}
