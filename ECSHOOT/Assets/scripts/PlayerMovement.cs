using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float avancementForce = 30f;       // Force d'avancement
    public float monterDecendreVitesse = 20f; // Monter/descendre
    public float gaucheDroiteVitesse = 20f;   // Gauche/droite
    public float rollTorque = 15f;            // Rouler (banque latérale)

    public float boostForce = 150f;            // Force du boost quand clic droit

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Avancer toujours
        rb.AddForce(transform.forward * avancementForce);

        // Inputs de rotation
        float pitch = -Input.GetAxis("Vertical");   
        float yaw = Input.GetAxis("Horizontal");    
        float roll = 0f;

        if (Input.GetKey(KeyCode.Q)) roll = 1;
        if (Input.GetKey(KeyCode.E)) roll = -1;

        rb.AddTorque(transform.right * pitch * monterDecendreVitesse);  
        rb.AddTorque(transform.up * yaw * gaucheDroiteVitesse);        
        rb.AddTorque(transform.forward * roll * rollTorque);  

        // Boost quand clic droit maintenu
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(transform.forward * boostForce, ForceMode.Acceleration);
        }
    }
}