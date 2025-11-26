using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Suivre le joueur")]
    public Vector3 offset;

    [Header("Camera Shake")]
    public float shakeIntensity = 0.05f;
    public float shakeDuration = 0.15f;
    private float shakeTimer = 0f;


   

    void Start()
    {
        if (player != null)
            offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        if (player == null) return;
            
        SuivreJoueur();
        //ApplyShake();
    }

    void SuivreJoueur()
    {
        transform.position = player.transform.position + offset;
    }

    void ApplyShake()
    {
        if (shakeTimer > 0)
        {
            transform.localPosition += Random.insideUnitSphere * shakeIntensity;
            shakeTimer -= Time.deltaTime;
        }
    }

    public void Shake()
    {
        shakeTimer = shakeDuration;
    }
}
