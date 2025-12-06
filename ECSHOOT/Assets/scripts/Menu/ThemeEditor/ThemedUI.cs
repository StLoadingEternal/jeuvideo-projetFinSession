using UnityEngine;

namespace SlimUI.ModernMenu{
    /// <summary>
    /// Classe de base pour les éléments UI thématisés. 
    /// Permet d'appliquer automatiquement un thème visuel aux composants UI.
    /// </summary>
    [ExecuteInEditMode()] // S'exécute aussi dans l'éditeur pour prévisualiser les changements
    [System.Serializable]
    public class ThemedUI : MonoBehaviour {

        /// <summary>
        /// Référence au contrôleur de thème qui contient les paramètres visuels (couleurs, styles, etc.)
        /// </summary>
        public ThemedUIData themeController;

        /// <summary>
        /// Méthode virtuelle à redéfinir dans les classes filles pour appliquer le thème aux composants UI.
        /// Cette méthode est appelée lors de l'initialisation et des mises à jour.
        /// </summary>
        protected virtual void OnSkinUI(){
            // À implémenter dans les classes dérivées
        }

        /// <summary>
        /// Méthode appelée au démarrage de l'objet (Awake).
        /// Applique le thème une première fois.
        /// </summary>
        public virtual void Awake(){
            OnSkinUI(); // Applique le thème dès le démarrage
        }

        /// <summary>
        /// Méthode appelée à chaque frame (Update).
        /// Permet de mettre à jour le thème en temps réel dans l'éditeur.
        /// </summary>
        public virtual void Update(){
            // En mode éditeur, permet de voir les changements de thème en temps réel
#if UNITY_EDITOR
            OnSkinUI(); // Met à jour continuellement dans l'éditeur
#endif
        }
    }
}