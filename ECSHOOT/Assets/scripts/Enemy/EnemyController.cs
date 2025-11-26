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


    public enum MovementType
    {
        Straight,
        ZigZag,
        Wave,
        StopAndGo,
        Drift,
    }

    MovementType[] types = { MovementType.Straight, MovementType.ZigZag, MovementType.Wave, MovementType.StopAndGo};

    [Header("Intelligence d'esquive")]
    public MovementType movementType;



    private int direction = 1;    // 1 = vers la droite, -1 = vers la gauche

    private GameManager gameManagerScript;//peut être static

    public GameObject player;
    private PlayerController playerControllerScript;



    private void Start()
    {
        playerControllerScript = player.GetComponent<PlayerController>();
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementType = types[UnityEngine.Random.Range(0, types.Length)];
    }

    void Update()
    {

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

        if (transform.position.z < player.transform.position.z - 5f)
        {
            PlayerController playerControllerScript = player.GetComponent<PlayerController>();
            playerControllerScript.LoseLife(1);

            Destroy(gameObject); // détruire l'ennemi
        }

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

    void ZigZagMove()
    {
        MoveHorizontal();
        transform.position += Vector3.up * Mathf.Sin(Time.time * zigzagFrequency) * zigzagMagnitude * Time.deltaTime;
    }

    void WaveMove()
    {
        
        float newY = transform.position.y + Mathf.Sin(Time.time * waveSpeed) * 0.1f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void StopAndGoMove()
    {
        stopTimer += Time.deltaTime;

        if (stopTimer > 2f && stopTimer < 3f)
            return; // il s'arrête

        if (stopTimer >= 3f)
            stopTimer = 0f;

        MoveHorizontal();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("fighterBullet"))
            return;

        Destroy(other.gameObject);

        EnemyHealth eh = GetComponent<EnemyHealth>();
        if (eh != null)
            eh.TakeDamage(1); // ENFIN on utilise la vie !

    }

}



