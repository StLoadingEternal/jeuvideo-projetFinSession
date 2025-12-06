using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlimUI.ModernMenu{
    /// <summary>
    /// Classe pour les éléments UI thématisés avec des fonctionnalités spécifiques pour les images et textes.
    /// Hérite de ThemedUI pour bénéficier des fonctionnalités de thème de base.
    /// </summary>
    [System.Serializable]
    public class ThemedUIElement : ThemedUI {
        
        [Header("Parameters")]
        // Variables pour le style de contour (déclaré mais non utilisé dans cette version)
        Color outline;
        
        // Référence au composant Image de l'objet
        Image image;
        
        // Référence au GameObject courant
        GameObject message;
        
        // Enumération des styles de contour disponibles (pour extension future)
        public enum OutlineStyle {solidThin, solidThick, dottedThin, dottedThick};
        
        // Indicateurs pour spécifier le type d'élément UI
        [Tooltip("Cocher si cet objet a un composant Image à styliser")]
        public bool hasImage = false;
        
        [Tooltip("Cocher si cet objet contient du texte TextMeshPro à styliser")]
        public bool isText = false;

        /// <summary>
        /// Applique le thème à l'élément UI en fonction de ses paramètres.
        /// Redéfinit la méthode OnSkinUI de la classe parente.
        /// </summary>
        protected override void OnSkinUI(){
            // Appelle d'abord la méthode de la classe parente (pour toute initialisation de base)
            base.OnSkinUI();

            // Si l'objet a un composant Image, applique la couleur du thème
            if(hasImage){
                // Récupère ou ajoute le composant Image
                image = GetComponent<Image>();
                // Applique la couleur actuelle du thème contrôlé
                image.color = themeController.currentColor;
            }

            // Stocke une référence au GameObject courant
            message = gameObject;

            // Si l'objet contient du texte, applique la couleur de texte du thème
            if(isText){
                // Récupère le composant TextMeshPro et applique la couleur de texte du thème
                message.GetComponent<TextMeshPro>().color = themeController.textColor;
            }
        }
    }
}