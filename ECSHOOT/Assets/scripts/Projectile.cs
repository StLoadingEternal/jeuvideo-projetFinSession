using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifeTime = 1f;
    

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Gerer la vie ici 
    }
}
