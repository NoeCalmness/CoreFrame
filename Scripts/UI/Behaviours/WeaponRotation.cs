using UnityEngine;

public class WeaponRotation : MonoBehaviour
{
    private float speed;
    private Vector3 target;
    
    public EquipType type { get; set; }

    void Start()
    {
        speed = GeneralConfigInfo.defaultConfig.rotationSpeed;
        target = type == EquipType.Cloth ? Vector3.up : GeneralConfigInfo.defaultConfig.aroundTarget;
    }

    void Update()
    {       
        if(type == EquipType.Cloth) transform.Rotate(target, speed * Time.deltaTime, Space.World);
        else transform.Rotate(target, speed*Time.deltaTime, Space.Self);
    }
}
