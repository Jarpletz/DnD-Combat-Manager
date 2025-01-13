using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [Serializable]
    public class EntityCondition
    {
        public string name;
        public Color color;
    }

    public static GameSettings Instance;

    public float distanceScaleMultipler;
    public float snapOffsetDistance;
    public EntityCondition[] conditions;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
