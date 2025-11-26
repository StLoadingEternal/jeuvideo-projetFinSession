using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    private float moveSpeed = 20f;// Gauche/droite

    private float forwardSpeed = 6f;//Vitesse d'avancement //Rajouter effet accélération avec espace//public float boostForce = 150f;

    private float inclinaison = 20f;

    private float maxX = 30f; // limites gauche/droite

    [Header("Vies")]
    public int maxLives = 3; // vies de base
    private int currentLives;

    

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
        // Inputs horizontaux
        float horizontalInput = Input.GetAxis("Horizontal");

        Vector3 newPos = transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;

       
        // Limite sur X avec if
        if (newPos.x > maxX)
        {
            newPos.x = maxX;
        }
        else if (newPos.x < -maxX)
        {
            newPos.x = -maxX;
        }

        // Appliquer seulement si besoin
        transform.position = newPos;

        float rotateZ = -horizontalInput * inclinaison; // Inclinaison
        transform.localRotation = Quaternion.Euler(0, 0, rotateZ);
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
             // mort instantanée
        }

        if (other.CompareTag("enemyBullet"))
        {
            //
            Destroy(other.gameObject);
        }
    }

    public void LoseLife(int perte)
    {
        currentLives -= perte;
        if (currentLives < 0) currentLives = 0;

        gameManagerScript.UpdateLifeUI(currentLives);

        if (currentLives <= 0)
            gameManagerScript.GameOver();
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            LoseLife(3); // perdre 3 vies
            Destroy(collision.gameObject); // détruire l'ennemi
        }
    }
}