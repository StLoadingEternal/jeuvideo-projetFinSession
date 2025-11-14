using UnityEngine;

public class Blaster : MonoBehaviour
{
    public float launchForce = 1000f;    // Force de lancement
    public float fireRate = 0.2f;
    private float nextFire = 0f;
    public Transform shootPoint; // ch
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

            GameObject p = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            Rigidbody rb = p.GetComponent<Rigidbody>();
            rb.AddForce(shootPoint.forward * launchForce);
        }
    }
}
