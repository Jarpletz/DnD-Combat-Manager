using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    Entity entity;
    MeshRenderer meshRenderer;

    Color playerColor;

    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponent<Entity>();
        meshRenderer = GetComponent<MeshRenderer>();
        playerColor = meshRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        updatePlayerColor();
    }

    void updatePlayerColor()
    {
        Color entityColor = entity.GetEntityColor();
        if (entityColor != playerColor)
        {
            playerColor = entityColor;
            meshRenderer.material.color = playerColor;
        }
    }
}
