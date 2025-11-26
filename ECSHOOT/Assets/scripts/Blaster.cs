using UnityEngine;

public class Blaster : MonoBehaviour
{
    private float launchForce = 500f; // Force de lancement
    private float fireRate = 0.5f;
    private float nextFire = 0f;
    public Transform shootPoint;
    public GameObject projectilePrefab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;

            // Calculer la position de spawn dans le monde
            Vector3 worldPos = shootPoint.position;

            // Rotation initiale du projectile : -90° sur X
            Quaternion rotation = Quaternion.Euler(-90, transform.eulerAngles.y, -90);

            // Instancier le projectile
            GameObject p = Instantiate(projectilePrefab, worldPos, rotation);

            // Ajouter une force vers l’avant
            //Rigidbody rb = p.GetComponent<Rigidbody>();
            //rb.AddForce(transform.forward * launchForce);
        }
    }
}
