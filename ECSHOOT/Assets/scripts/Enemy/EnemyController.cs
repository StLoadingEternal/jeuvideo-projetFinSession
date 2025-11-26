using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Déplacement horizontal")]
    public float moveSpeed; // vitesse horizontale
    public float maxX = 25f;      // limite droite
    public float minX = -25f;     // limite gauche

    [Header("Intelligence d'esquive")]
    public float dodgeDistance = 8f;
    public float dodgeSpeed = 10f;
    public float dodgeCooldown = 0.5f;
    private float lastDodgeTime = 0f;

    private int direction = 1;    // 1 = vers la droite, -1 = vers la gauche

    private GameManager gameManagerScript;

    public ParticleSystem hitEffectPrefab;

    private void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {

        MoveHorizontal();
       
    }

    void MoveHorizontal()
    {
        // Déplacement horizontal
        float newX = transform.position.x + moveSpeed * direction * Time.deltaTime;

        // Vérifier les limites et inverser la direction si nécessaire
        if (newX > maxX)
        {
            newX = maxX;
            direction = -1;
        }
        else if (newX < minX)
        {
            newX = minX;
            direction = 1;
        }

        // Appliquer la nouvelle position (sans toucher à Y ou Z)
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie que c'est une balle du joueur
        if (!other.CompareTag("fighterBullet"))
            return;

        // Détruit la balle
        Destroy(other.gameObject);

        // Jouer le hit effect en le détachant
        if (hitEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f); // détruit après effet
        }

        //// Jouer un son 
        //if (hitSound != null)
        //    AudioSource.PlayClipAtPoint(hitSound, transform.position);

        // Informer le GameManager
        gameManagerScript.OnEnemyDestroyed();

        // Détruire l’ennemi
        Destroy(gameObject);
    }


    
    
}
