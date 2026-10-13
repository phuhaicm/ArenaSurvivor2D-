using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeXPOrb : HaiMonoBehaviour
{
    [SerializeField] private int experienceAmount = 1;

    public int ExperienceAmount => experienceAmount;

    protected override void ResetValues()
    {
        base.ResetValues();
        experienceAmount = 1;
    }
}
