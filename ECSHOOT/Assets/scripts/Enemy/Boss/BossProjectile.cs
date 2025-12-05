using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public int damage = 2;
    public float lifetime = 5f;
    
    private Vector3 direction;
    private float speed;
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    public void Initialize(Vector3 shootDirection, float projectileSpeed)
    {
        direction = shootDirection.normalized;
        speed = projectileSpeed;
    }
    
    void FixedUpdate()
    {
        transform.position += direction * speed * Time.fixedDeltaTime;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.transform.root == transform.root)
        {
            foreach (Collider bossCollider in other.gameObject.GetComponents<Collider>())
            {
                // Ignorer la collision entre le boss et le projectile
                Physics.IgnoreCollision(bossCollider, transform.GetComponent<Collider>(), true);
            }
            return;
        }
       
        
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.LoseLife(damage);
            }
            Destroy(gameObject);
            
        }
        else if (other.CompareTag("fighterBullet"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
        
        Destroy(gameObject);
    }
    
    
}