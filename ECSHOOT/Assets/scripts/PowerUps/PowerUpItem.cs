namespace PowerUps
{
    using UnityEngine;

public enum PowerUpType
{
    FireRate,
    MultiShot,
    Speed,
    Shield
}

public class PowerUpItem : MonoBehaviour
{
    [Header("Type")]
    public PowerUpType type;
    
    [Header("Visuals")]
    public float rotationSpeed = 100f;
    public float floatAmplitude = 0.3f;
    public float floatFrequency = 2f;
    
    [Header("Effects")]
    public GameObject collectEffect;
    public AudioClip collectSound;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        SetVisuals();
    }
    
    void Update()
    {
        // Rotation
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        // Flottement
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void SetVisuals()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            switch (type)
            {
                case PowerUpType.FireRate:
                    renderer.material.color = Color.red;
                    break;
                case PowerUpType.MultiShot:
                    renderer.material.color = Color.blue;
                    break;
                case PowerUpType.Speed:
                    renderer.material.color = Color.green;
                    break;
                case PowerUpType.Shield:
                    renderer.material.color = Color.cyan;
                    break;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect(other.gameObject);
        }
    }
    
    void Collect(GameObject player)
    {
        PowerUpManager powerUpManager = player.GetComponent<PowerUpManager>();
        
        
        if (powerUpManager != null)
        {
            switch (type)
            {
                case PowerUpType.FireRate:
                    powerUpManager.CollectFireRatePowerUp();
                    break;
                    
                case PowerUpType.MultiShot:
                    powerUpManager.CollectMultiShotPowerUp();
                    break;
                    
                case PowerUpType.Speed:
                    powerUpManager.CollectSpeedPowerUp();
                    break;
                    
                case PowerUpType.Shield:
                    powerUpManager.CollectShieldPowerUp();
                    break;
            }
            Destroy(this.gameObject);
        }
        
        // Effets
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        
        // Détruire
        Destroy(gameObject);
    }
}
}