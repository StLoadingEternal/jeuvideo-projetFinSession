using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    private float moveSpeed = 20f;// Gauche/droite

    private float forwardSpeed = 40f;//Vitesse d'avancement 

    private float inclinaison = 10f;

    private float maxX = 25f; // limites gauche/droite

    //public float boostForce = 150f;

    public GameObject hitEffect;

    private Rigidbody rb;

    void Start()
    {
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
}