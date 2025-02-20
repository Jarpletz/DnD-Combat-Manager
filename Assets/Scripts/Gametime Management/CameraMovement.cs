using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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

        //cancel zoom if mouse is over a UI element
        if (HelperFunctions.IsPointerOverUIElement())
        {
            zoomDirection = 0;
        }

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

        transform.Translate(new Vector3(xAxisValue, yAxisValue, zAxisValue));

        transform.Rotate(new Vector3(0.0f, rotateValue, 0.0f));


        //clamp x
        float xClamp = Mathf.Clamp(transform.position.x, xClamps.x, xClamps.y);
        //clamp y
        float yClamp = Mathf.Clamp(transform.position.y, zoomClamps.x, zoomClamps.y);
        //clamp z
        float zClamp = Mathf.Clamp(transform.position.z, zClamps.x, zClamps.y);
        //clamp position
        transform.position = new Vector3(xClamp, yClamp, zClamp);


        lastMousePos = Input.mousePosition;
    }
}
