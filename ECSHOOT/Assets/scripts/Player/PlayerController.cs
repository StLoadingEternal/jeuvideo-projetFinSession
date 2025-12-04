using System.Collections;
using Player;
using PowerUps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Vitesse gauche droite 
    private float initialSpeed = 20f;
    private float moveSpeed;

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
    

    //Animation Accélération
    public float boostSpeed = 150f;
    public ParticleSystem boost_L;
    public ParticleSystem boost_R;

    //Hit Ennemi
    public Material shipMaterial;
    private float flashDuration = 0.15f;

    
    [Header("Shield System")]
    public ShieldShaderController shieldController;

    

    void Start()
    {
        moveSpeed = initialSpeed;
        currentLives = maxLives;
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody>();

        if (shieldController == null)
            shieldController = GetComponentInChildren<ShieldShaderController>();

    }

    void FixedUpdate()
    {
        // Avancer toujours en Z
        rb.linearVelocity = transform.forward * forwardSpeed + new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
    }

    void Update()
    {
        

        if (gameManagerScript.isGameOver)
            return;

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

        //acceleration
        Acceleration();

    }


    public void Acceleration()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            boost_L.Play();
            boost_R.Play();
            moveSpeed = 40f;
        }
        else
        {
            boost_L.Stop();
            boost_R.Stop();
            moveSpeed = initialSpeed;

        }
    }


    //Perte de vie
    public void LoseLife(int amount)
    {
        currentLives -= amount;

      
        if (gameManagerScript != null)
        {
            gameManagerScript.UpdateLifeUI(currentLives);

            if (currentLives <= 0)
            {
                gameManagerScript.GameOver();
            }
        }
    }

 
    private void OnTriggerEnter(Collider other)
    {
        
        // COLLECTER POWER-UP SHIELD
        if (other.CompareTag("PowerUp") && other.GetComponent<PowerUpItem>()?.type == PowerUpType.Shield)
        {
            if (shieldController != null)
            {
                shieldController.ActivateShield();
            }
            //Destroy(other.gameObject); L'enemi gère déjà sa destruction avec animation
            return;
        }
        
        // PRENDRE UN HIT
        if (other.CompareTag("Enemy"))
        {
            //Shader de hit
            StartCoroutine(HitFlash());

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            
            // Vérifier le shield d'abord
            if (shieldController != null && shieldController.IsShieldActive())
            {
                // Appliquer un hit au shield
                shieldController.TakeShieldHit(0.2f,hitPoint );
                
                // Si le shield est encore actif, arrêter ici
                if (shieldController.GetShieldHealth() > 0)
                {
                    //Destroy(other.gameObject);L'enemi gère déjà sa destruction avec animation
                    return;
                }
            }
            
            // Pas de shield ou shield cassé();
            LoseLife(1);
            //Destroy(other.gameObject); L'enemi gère déjà sa destruction avec animation
        }
    }

    private IEnumerator HitFlash()
    {
       // Active le flash
        shipMaterial.SetFloat("_FlashAmount", 1f);

        yield return new WaitForSeconds(flashDuration);

        // Retour normal
        shipMaterial.SetFloat("_FlashAmount", 0f);
    }

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    public float ForwardSpeed
    {
        get => forwardSpeed;
        set => forwardSpeed = value;
    }
}