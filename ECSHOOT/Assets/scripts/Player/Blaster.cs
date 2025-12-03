using UnityEngine;
using System.Collections;

public class Blaster : MonoBehaviour
{
    public Transform shootPoint;
    public GameObject projectilePrefab;

    private float fireRate = 0.5f;
    private float nextFire = 0f;

    private bool isMultiShot = false;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;

            if (isMultiShot)
            {
                // Tirer 2 projectiles dans la même direction
                Shoot();
                Shoot(0.4f);
            }
            else
            {
                Shoot();
            }
        }
    }

    private void Shoot(float additionalPos = 0)
    {
        Vector3 worldPos = shootPoint.position + new Vector3(additionalPos, 0, 0);

        // Bonne orientation du projectile
        Quaternion rotation = Quaternion.Euler(-90, transform.eulerAngles.y, -90);

        // Instancier le projectile
        GameObject p = Instantiate(projectilePrefab, worldPos, rotation);
        
        
        Rigidbody rb = p.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shootPoint.forward * 20f; // vitesse constante
                
           
        }
    }

    public void ActivateMultiShot(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(MultiShotCoroutine(duration));
    }

    private IEnumerator MultiShotCoroutine(float duration)
    {
        isMultiShot = true;
        yield return new WaitForSeconds(duration);
        isMultiShot = false;
    }
}
