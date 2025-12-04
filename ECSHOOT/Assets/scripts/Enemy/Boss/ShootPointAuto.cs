using UnityEngine;

[RequireComponent(typeof(BossController))]
public class AutoCreateShootPoints : MonoBehaviour
{
    [Header("Configuration")]
    public int numberOfPoints = 2;
    public float forwardOffset = 2f;
    public float horizontalSpread = 1.5f;
    public float verticalOffset = 0.5f;
    
    [Header("Options")]
    public bool createOnStart = true;
    
    private BossController bossController;
    
    void Start()
    {
        bossController = GetComponent<BossController>();
        
        if (bossController == null) return;
        
        if (createOnStart)
        {
            CreateShootPoints();
        }
    }
    
    [ContextMenu("Créer Shoot Points")]
    public void CreateShootPoints()
    {
        if (bossController == null)
        {
            bossController = GetComponent<BossController>();
            if (bossController == null) return;
        }
        
        bossController.shootPoints = new Transform[numberOfPoints];
        
        for (int i = 0; i < numberOfPoints; i++)
        {
            GameObject shootPoint = new GameObject($"ShootPoint_{i}");
            shootPoint.transform.SetParent(transform);
            shootPoint.transform.localPosition = Vector3.zero;
            shootPoint.transform.localRotation = Quaternion.identity;
            
            float xPos = 0f;
            float yPos = verticalOffset;
            float zPos = forwardOffset;
            
            if (numberOfPoints > 1 && horizontalSpread > 0)
            {
                float t = (float)i / (numberOfPoints - 1);
                xPos = Mathf.Lerp(-horizontalSpread, horizontalSpread, t);
            }
            
            shootPoint.transform.localPosition = new Vector3(xPos, yPos, zPos);
            bossController.shootPoints[i] = shootPoint.transform;
        }
    }
}