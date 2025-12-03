using UnityEngine;

public class BlasterEnemy : MonoBehaviour
{
    //public float launchForce = 1000f;    // Force de lancement
    //private float fireRate = 3f;        // Temps entre chaque tir
    //private float nextFire = 0f;
    //public Transform shootPoint;
    //public GameObject projectilePrefab;  // Projectile à lancer


    //private void Start()
    //{
        
    //}

    //void Update()
    //{
    //    //if (Time.time >= nextFire)
    //    //{
    //    //    Fire();
    //    //    nextFire = Time.time + fireRate;
    //    //}
    //}

    //void Fire()
    //{
    //    if (projectilePrefab == null) return;

    //    // Calculer la position de spawn dans le monde
    //    Vector3 worldPos = shootPoint.position;

    //    // Rotation initiale du projectile : -90° sur X
    //    Quaternion rotation = Quaternion.Euler(-90f, 0, 0);

    //    // Instancier le projectile
    //    GameObject p = Instantiate(projectilePrefab, worldPos, Quaternion.identity);

        
    //    Rigidbody rb = p.GetComponent<Rigidbody>();
    //    rb.AddForce(transform.forward * launchForce);
    //}
}