using UnityEngine;

public class CompassNeedle : MonoBehaviour
{
    public Transform player;     // Joueur (vaisseau)
    public Transform enemy;      // Ennemi à suivre
    public RectTransform needle; // Image UI de l’aiguille

    void Update()
    {
        if (player == null || enemy == null || needle == null) return;

        
        Vector3 toEnemy = enemy.position - player.position;
        toEnemy.y = 0; 

        
        Vector3 playerForward = player.forward;
        playerForward.y = 0;

  
        float angle = Vector3.SignedAngle(playerForward, toEnemy, Vector3.up);
        
        needle.localRotation = Quaternion.Euler(0, 0, -angle + 180f);
    }
}