using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Suivre le joueur")]
    public Vector3 offset;

    void Start()
    {
        if (player != null)
            offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        if (player == null) return;
            
        SuivreJoueur();
    }

    void SuivreJoueur()
    {
        transform.position = player.transform.position + offset;
    }
}
