using System;
using Player;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("D�placement horizontal")]
    public float moveSpeed; // vitesse horizontale
    public float maxX = 25f;      // limite droite
    public float minX = -25f;     // limite gauche

    [Header("D�placement zig zag")]
    public float zigzagFrequency = 5f;
    public float zigzagMagnitude = 5f;

    [Header("D�placement wave")]
    public float waveSpeed = 2f;

    [Header("D�placement stopGo")]
    private float stopTimer;

    //Enum Mouvements
    public enum MovementType
    {
        Straight,
        ZigZag,
        Wave,
        StopAndGo,
        Drift,
    }

    private MovementType[] types = { MovementType.Straight, MovementType.ZigZag, MovementType.Wave, MovementType.StopAndGo};

    [Header("Mouvement vari�s")]
    private MovementType movementType;

    private int direction = 1;    // direction de d�plcement horizontal (1 = vers la droite, -1 = vers la gauche)

    //R�f�rences
    private GameManager gameManagerScript;//peut �tre static
    public GameObject player;
    private PlayerController playerControllerScript;
    EnemyHealth health;



    private void Start()
    {
        health = GetComponent<EnemyHealth>();
        playerControllerScript = player.GetComponent<PlayerController>();
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementType = types[UnityEngine.Random.Range(0, types.Length)];
    }

    void Update()
    {
        //Mouvement al�atoires 
        switch (movementType)
        {
            case MovementType.Straight:
                MoveHorizontal();
                break;

            case MovementType.ZigZag:
                ZigZagMove();
                break;

            case MovementType.Wave:
                WaveMove();
                break;

            case MovementType.StopAndGo:
                StopAndGoMove();
                break;

        }

        // Si le joueur ne d�truit pas l'ennemi avant de le d�passer perd une vie
        if (transform.position.z < player.transform.position.z - 5f)
        {
            PlayerController playerControllerScript = player.GetComponent<PlayerController>();
            playerControllerScript.LoseLife(1);
            Destroy(gameObject); // d�truire l'ennemi
        }

    }

    //Mouvement horizontal
    void MoveHorizontal()
    {
        // D�placement horizontal
        float newX = transform.position.x + moveSpeed * direction * Time.deltaTime;

        // V�rifier les limites et inverser la direction si n�cessaire
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

        // Appliquer la nouvelle position 
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    //Mouvement zig zag
    void ZigZagMove()
    {
        MoveHorizontal();
        transform.position += Vector3.up * Mathf.Sin(Time.time * zigzagFrequency) * zigzagMagnitude * Time.deltaTime;
    }

    //Mouvement de vague haut et bas 
    void WaveMove()
    {
        if (Time.timeScale == 0) return; // stop pendant la pause

        float newY = transform.position.y + Mathf.Sin(Time.time * waveSpeed) * 0.1f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    //S'arr�te puis repart
    void StopAndGoMove()
    {
        stopTimer += Time.deltaTime;

        if (stopTimer > 2f && stopTimer < 3f)
            return; // il s'arr�te

        if (stopTimer >= 3f)
            stopTimer = 0f;

        MoveHorizontal();
    }

    //Collision avec une balle du joueur perd de la vie (Chaque balle enl�ve 1 vie)
    private void OnTriggerEnter(Collider other)
    {

        if (!other.CompareTag("fighterBullet") & !other.CompareTag("Player"))
        return;

        if (other.gameObject.CompareTag("Player"))
        {
            PlayerShield playerShield = other.gameObject.GetComponent<PlayerShield>();
            if (playerShield.IsShieldActive() )
            {
                Debug.Log("Shield active (degats) ");
                Destroy(gameObject);
                playerShield.TakeShieldHit(1);
                
            }
            
        }
            
        //Degats De balles 
        if (other.CompareTag("fighterBullet"))
        {
            Destroy(other.gameObject);
            health.TakeDamage(1);

            if (health.IsDead)
            {
                DropPowerUpsSimple();
            }
        }
       
        //Dégats collision avec le joueur
        if (other.CompareTag("Player"))
        {
            health.TakeDamage(16); //Mort direct de l'ennemi s'il y'a collision avec le vaisseau
        }
    }
    
    
    
    
    [Header("Power-up Drops")] 
    public GameObject[] powerUpPrefabs;
    [Range(0, 100)] public float dropChance = 20f;
    
    void DropPowerUpsSimple()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            return;

        // Vérifier la chance de drop
        if ( UnityEngine.Random.Range(0f, 100f) > dropChance)
            return;

        // Nombre de drops
        int dropCount = 1;

        for (int i = 0; i < dropCount; i++)
        {
            // Choisir un power-up aléatoire
            GameObject powerUpPrefab = powerUpPrefabs[UnityEngine.Random.Range(0, powerUpPrefabs.Length)];
            
            if (powerUpPrefab != null)
            {
                Vector3 spawnPosition = transform.position + 
                                        new Vector3(
                                            UnityEngine.Random.Range(-1f, 1f),
                                            0.5f,
                                            UnityEngine.Random.Range(-1f, 1f)
                                        );

                Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
                Debug.Log("dropped power up");
            }
        }
    }
}



