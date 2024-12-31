using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NametagPosition : MonoBehaviour
{
    Transform target;

    [SerializeField] float scaleFactor = 0.1f; // Adjust this to control scaling sensitivity
    [SerializeField] float minScale = 0.001f;
    [SerializeField] float maxScale = 0.01f;

    private void Start()
    {
        target = Camera.main.transform;
    }
    void Update()
    {

        float distance = Vector3.Distance(transform.position, target.position);
        float scale = Mathf.Clamp(distance * scaleFactor, minScale, maxScale);

        // Apply the scale to the nametag
        transform.localScale = Vector3.one * scale;

        // Ensure the nametag always faces the camera
        transform.LookAt(target);
        transform.Rotate(0, 180, 0); // Correct the rotation if needed
    }
}
