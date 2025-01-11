using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float camSpeed;
    public float zoomSpeed;
    public float rotateSpeed;
    public Vector2 xClamps;
    public Vector2 zClamps;
    public Vector2 zoomClamps;


    Camera mainCamera;
    Vector3 lastMousePos;
    Vector3 mouseDelta
    {
        get
        {
            return Input.mousePosition - lastMousePos;
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        lastMousePos = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {

        //Movememt (WASD)
        float xAxisValue = Input.GetAxis("Horizontal") * camSpeed  * Time.deltaTime;
        float zAxisValue = Input.GetAxis("Vertical") * camSpeed * Time.deltaTime;


        //Zoom (Scrollwheel) 
        float zoomDirection =Input.GetAxis("Mouse ScrollWheel");
        if (zoomDirection > 0.0)
        {
            zoomDirection = 1;
        }
        else if (zoomDirection < 0.0)
        {
            zoomDirection = -1;
        }
        float yAxisValue =  -zoomDirection * zoomSpeed * Time.deltaTime;


        //Rotate (Right Mouse Button)
        float rotateValue = 0.0f;
        if (Input.GetMouseButton(1))
        {
            float rotateDirection = mouseDelta.x;

            rotateValue = rotateDirection * rotateSpeed * Time.deltaTime;

        }

        //clamp x
        if(transform.position.x <= xClamps.x && xAxisValue < 0.0)
        {
            xAxisValue = 0;
        }
        if (transform.position.x >= xClamps.y && xAxisValue > 0.0)
        {
            xAxisValue = 0;
        }

        //clamp y
        if (transform.position.y <= zoomClamps.x && yAxisValue < 0.0)
        {
            yAxisValue = 0;
        }
        if (transform.position.y >= zoomClamps.y && yAxisValue > 0.0)
        {
            yAxisValue = 0;
        }

        //clamp z
        if (transform.position.z <= xClamps.x && zAxisValue < 0.0)
        {
            zAxisValue = 0;
        }
        if (transform.position.z >= xClamps.y && zAxisValue > 0.0)
        {
            zAxisValue = 0;
        }


        transform.Translate(new Vector3(xAxisValue, yAxisValue, zAxisValue));

        transform.Rotate(new Vector3(0.0f, rotateValue, 0.0f));

        lastMousePos = Input.mousePosition;
    }
}
