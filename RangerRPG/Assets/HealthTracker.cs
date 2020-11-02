using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthTracker : MonoBehaviour
{

    public Slider HealthbarObject;
    public Character Target;

    // Update is called once per frame
    void LateUpdate()
    {
        HealthbarObject.maxValue = Target.maxHP;
        HealthbarObject.value = Target.HP;
    }
}
