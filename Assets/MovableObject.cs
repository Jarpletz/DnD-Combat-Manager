using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    Vector3 screenPoint;
    Vector3 offset;
    float footOffsetDistance;

    public LayerMask groundMask;
    public Transform footOffsetPoint;

    private void Start()
    {
        footOffsetDistance = transform.position.y - footOffsetPoint.position.y;
    }

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

    }
    void OnMouseDrag()
    {
        transform.position = getNewPosition();
    }

    Vector3 getNewPosition()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        //get the new position
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        //for x and z, snap to grid
        newPosition.x = Mathf.Round(newPosition.x);
        newPosition.z = Mathf.Round(newPosition.z);
        //for y, snap to ground
        newPosition.y = getYPositionFromRaycast();

        return newPosition;
    }

    float getYPositionFromRaycast()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        Vector3 rayStart = new Vector3(transform.position.x, 1000f, transform.position.z);

        if (Physics.Raycast(rayStart, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, groundMask))
        {
            return hit.point.y + footOffsetDistance;
        }
        else
        {
            return transform.position.y;
        }
    }


}
