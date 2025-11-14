using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 20;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Gerer la vie ici 
    }
}
