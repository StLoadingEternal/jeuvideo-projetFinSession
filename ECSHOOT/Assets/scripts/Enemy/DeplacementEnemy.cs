using System;
using UnityEngine;

public class DeplacementEnemy : MonoBehaviour
{
    public Transform player;
    public float orbitDistance = 100f;
    public float orbitSpeed = 1f;       // vitesse rad/s
    public float heightOffset = 0f;
    public float rotationSpeed = 2f;    // vitesse rotation vers le joueur

    private float angle = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        // Position initiale orbitale
        Vector3 offset = new Vector3(Mathf.Cos(angle) * orbitDistance, heightOffset, Mathf.Sin(angle) * orbitDistance);
        rb.position = player.position + offset;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // Calcul de la position orbitale ( tourne autour du joueur)
        angle += orbitSpeed * Time.fixedDeltaTime;
        float x = Mathf.Cos(angle) * orbitDistance;
        float z = Mathf.Sin(angle) * orbitDistance;
        Vector3 targetPos = player.position + new Vector3(x, heightOffset, z);
        
        rb.MovePosition(targetPos);

        // Rotation lisse vers le joueur
        Vector3 lookDir = player.position - rb.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }
    }
}