using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Vitesse gauche droite 
    private float moveSpeed = 20f;

    //Vitesse d'avancement 
    //Rajouter effet accélération avec espace(public float boostForce = 150f; + Animation)
    private float forwardSpeed = 6f;

    private float inclinaison = 20f;

    // limites gauche/droite de déplacements
    private float maxX = 30f; 

    [Header("Vies")]
    public int maxLives = 3; // vies de base
    public int currentLives;

    //Références
    private Rigidbody rb;
    private GameManager gameManagerScript;

    void Start()
    {
        currentLives = maxLives;
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Avancer toujours en Z
        rb.linearVelocity = transform.forward * forwardSpeed + new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
    }

    void Update()
    {
        // Déplacement horizontal
        float horizontalInput = Input.GetAxis("Horizontal");

        Vector3 newPos = transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;

        //Déplacements limités
        if (newPos.x > maxX)
        {
            newPos.x = maxX;
        }
        else if (newPos.x < -maxX)
        {
            newPos.x = -maxX;
        }

        transform.position = newPos;

        // Inclinaison
        float rotateZ = -horizontalInput * inclinaison; 
        transform.localRotation = Quaternion.Euler(0, 0, rotateZ);
    }


    //Perte de vie
    public void LoseLife(int amount)
    {
        currentLives -= amount;
        
        // Mettre à jour l'UI des vies
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.UpdateLifeUI(currentLives);
            
            if (currentLives <= 0)
            {
                gameManager.GameOver();
            }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            LoseLife(3); // perdre 3 vies
            Destroy(collision.gameObject); // détruire l'ennemi
            //Effet
        }

        if (collision.gameObject.CompareTag("enemyBullet"))
        {
            // À voir 
        }
    }
}