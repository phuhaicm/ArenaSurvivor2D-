using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContainerRoot : HaiMonoBehaviour
{
    private static EnemyContainerRoot instance;
    public static EnemyContainerRoot Instance => instance;
    protected override void Awake()
    {
        if(instance != null && instance!= this)
        {

            Debug.LogError("Only one EnemyContainerRoot is allowed to exist.", gameObject);
            return;

        }
        instance = this;
        base.Awake();

    }

}
