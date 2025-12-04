using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Comportement")]
    public float moveSpeed = 5f;
    public float maxX = 25f;
    public float minX = -25f;
    private int direction = 1;

    [Header("Tir")]
    public float fireRate = 2f;
    private float nextFireTime = 0f;
    public Transform[] shootPoints;
    public GameObject projectilePrefab;
    public float projectileSpeed = 25f;

    [Header("Évitement")]
    public float avoidanceForce = 3f;
    public float detectionRadius = 8f;
    public LayerMask bossLayerMask;
    
    private Transform player;
    private Rigidbody rb;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody>();
        
        // Créer un Layer pour les bosses si nécessaire
        gameObject.layer = LayerMask.NameToLayer("Boss");
        bossLayerMask = LayerMask.GetMask("Boss");
    }

    void Update()
    {
        if (player == null) return;

        MoveWithAvoidance();
        
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void MoveWithAvoidance()
    {
        // Déplacement de base
        float baseMovement = moveSpeed * direction * Time.deltaTime;
        float newX = transform.position.x + baseMovement;
        
        // Force d'évitement
        Vector3 avoidance = CalculateAvoidanceForce();
        
        // Appliquer l'évitement
        newX += avoidance.x * Time.deltaTime;
        
        // Limites
        if (newX > maxX)
        {
            newX = maxX;
            direction = -1;
        }
        else if (newX < minX)
        {
            newX = minX;
            direction = 1;
        }
        
        // Garder Y et Z constants
        Vector3 newPosition = new Vector3(newX, transform.position.y, transform.position.z);
        transform.position = newPosition;
        
        // OU utiliser Rigidbody pour un mouvement plus naturel
        if (rb != null)
        {
            Vector3 velocity = new Vector3(baseMovement + avoidance.x, 0, 0);
            rb.linearVelocity = velocity;
        }
    }
    
    Vector3 CalculateAvoidanceForce()
    {
        Vector3 avoidanceForceVector = Vector3.zero;
        
        // Trouver les autres bosses proches
        Collider[] nearbyBosses = Physics.OverlapSphere(transform.position, detectionRadius, bossLayerMask);
        
        foreach (Collider bossCollider in nearbyBosses)
        {
            if (bossCollider.gameObject == gameObject) continue; // Ignorer soi-même
            
            Vector3 toOtherBoss = transform.position - bossCollider.transform.position;
            float distance = toOtherBoss.magnitude;
            
            if (distance < detectionRadius && distance > 0)
            {
                // Plus le boss est proche, plus la force d'évitement est forte
                float forceStrength = avoidanceForce * (1f - (distance / detectionRadius));
                avoidanceForceVector += toOtherBoss.normalized * forceStrength;
            }
        }
        
        return avoidanceForceVector;
    }

    void Shoot()
    {
        if (projectilePrefab == null || shootPoints == null || shootPoints.Length == 0)
            return;

        foreach (Transform shootPoint in shootPoints)
        {
            if (shootPoint == null) continue;
            
            GameObject projectile = Instantiate(projectilePrefab, 
                shootPoint.position, 
                Quaternion.identity);
            
            if (player != null)
            {
                projectile.transform.LookAt(player.position);
            }
            
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 directionToPlayer = (player.position - shootPoint.position).normalized;
                rb.linearVelocity = directionToPlayer * projectileSpeed;
            }
        }
    }
    
    
}