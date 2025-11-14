using UnityEngine;

public class BlasterEnemy : MonoBehaviour
{
    public float launchForce = 1000f;    // Force de lancement
    private float fireRate = 3f;        // Temps entre chaque tir
    private float nextFire = 0f;

    public Transform shootPoint;         // Point de tir
    public GameObject projectilePrefab;  // Projectile à lancer

    void Update()
    {
        if (Time.time >= nextFire)
        {
            Fire();
            nextFire = Time.time + fireRate;
        }
    }

    void Fire()
    {
        if (projectilePrefab == null || shootPoint == null) return;

        GameObject p = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        Rigidbody rb = p.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(shootPoint.forward * launchForce);
    }
}