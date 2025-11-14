using System.Collections.Generic;
using UnityEngine;

public class skyboxSetter : MonoBehaviour
{
    [SerializeField] private List<Material> skyboxMaterials;
    Skybox skybox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        skybox = GetComponent<Skybox>();
    }

    // Update is called once per frame
    void OnEnable()
    {
        ChangeSky(0);
    }

    void ChangeSky(int value)
    {
        if (skybox != null && value >= 0 && value <= skyboxMaterials.Count)
        {
            skybox.material = skyboxMaterials[value];
        }
    }
}
