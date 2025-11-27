using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Déplacement horizontal")]
    public float moveSpeed; // vitesse horizontale
    public float maxX = 25f;      // limite droite
    public float minX = -25f;     // limite gauche

    [Header("Déplacement zig zag")]
    public float zigzagFrequency = 5f;
    public float zigzagMagnitude = 5f;

    [Header("Déplacement wave")]
    public float waveSpeed = 2f;

    [Header("Déplacement stopGo")]
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

    [Header("Mouvement variés")]
    private MovementType movementType;

    private int direction = 1;    // direction de déplcement horizontal (1 = vers la droite, -1 = vers la gauche)

    //Références
    private GameManager gameManagerScript;//peut être static
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
        //Mouvement aléatoires 
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

        // Si le joueur ne détruit pas l'ennemi avant de le dépasser perd une vie
        if (transform.position.z < player.transform.position.z - 5f)
        {
            PlayerController playerControllerScript = player.GetComponent<PlayerController>();
            playerControllerScript.LoseLife(1);
            Destroy(gameObject); // détruire l'ennemi
        }

    }

    //Mouvement horizontal
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

    //S'arrête puis repart
    void StopAndGoMove()
    {
        stopTimer += Time.deltaTime;

        if (stopTimer > 2f && stopTimer < 3f)
            return; // il s'arrête

        if (stopTimer >= 3f)
            stopTimer = 0f;

        MoveHorizontal();
    }

    //Collision avec une balle du joueur perd de la vie (Chaque balle enlève 1 vie)
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("fighterBullet"))
            return;

        Destroy(other.gameObject);

        //Dégâts
        health.TakeDamage(1); 
    }
}



