using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed = 50f;  // vitesse du projectile
    private float lifeTime = 2f; // durée avant destruction automatique


    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }
}
