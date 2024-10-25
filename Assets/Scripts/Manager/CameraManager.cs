using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform cameraPivot;
    public Transform cam;
    private Vector3 centerIsland;

    [Header("Zoom")]
    public float minZoom;
    public float maxZoom;
    public float startZoom;
    public float scrollIntensity;
    public float currentZoom;

    [Header("Rotation")]
    public float rotationSpeed;
    public float minRotation;
    public float maxRotation;
    public float rotationFallOff;
    float rotationRemaining;
    Vector3 lastMousePosition;

    [Header ("Camera Offset")]
    public float panningSpeed;
    public Vector3 baseCameraOffset;
    public Vector2 cameraOffset;
    public Vector2 xBoundCamera;
    public Vector2 yBoundCamera;
    public float timePanningReset;
    float timeMaxPanningResetTimer = 0;

    public float cameraLerpSpeed;
    void Start(){
        currentZoom = startZoom;
    }

    void Update(){

        Vector2 mouseDelta = new Vector2(lastMousePosition.x - Input.mousePosition.x, (lastMousePosition.y - Input.mousePosition.y));

        //ROTATION
        if(Input.GetMouseButton(1)){
            cameraPivot.RotateAround(cameraPivot.transform.position, Vector3.up, - mouseDelta.x * rotationSpeed * Time.deltaTime);
            rotationRemaining = 0;
        }
        else if(Input.GetMouseButtonUp(1)){
            rotationRemaining = -(lastMousePosition.x - Input.mousePosition.x);
            if(rotationRemaining > 0) {
                if(rotationRemaining < minRotation) rotationRemaining = 0;
                if(rotationRemaining > maxRotation) rotationRemaining = maxRotation;
            }
            else {
                if(rotationRemaining > -minRotation) rotationRemaining = 0;
                if(rotationRemaining < -maxRotation) rotationRemaining = -maxRotation;
            }
        }
        if(rotationRemaining != 0){
            cameraPivot.RotateAround(cameraPivot.transform.position, Vector3.up, rotationRemaining * rotationSpeed * Time.deltaTime);
            rotationRemaining *= 1 - Time.deltaTime / rotationFallOff;
        }

        //PANNING
        if(Input.GetMouseButton(2))
        {
            cameraOffset += mouseDelta * Time.deltaTime * panningSpeed;
            cameraOffset.x = Mathf.Clamp(cameraOffset.x, xBoundCamera.x, xBoundCamera.y);
            cameraOffset.y = Mathf.Clamp(cameraOffset.y, yBoundCamera.x, yBoundCamera.y);
        }

        //ZOOM
        float zoomChange = -  Input.mouseScrollDelta.y * scrollIntensity; //cause why not apparently, 
        currentZoom = Mathf.Clamp(currentZoom + zoomChange, minZoom, maxZoom);

         //PANNING + ZOOM RESET 
        if(Input.GetMouseButtonDown(2)) 
            timeMaxPanningResetTimer = timePanningReset + Time.time;
        
        if(Input.GetMouseButtonUp(2) && timeMaxPanningResetTimer > Time.time) {
            cameraOffset = Vector2.zero;
            currentZoom = startZoom;
        }

        

        //FINALCALCUL
        Vector3 finalOffset = new Vector3(cameraOffset.x, cameraOffset.y, currentZoom) + baseCameraOffset;
        Vector3 finalCameraPos = cameraPivot.position + finalOffset.x * cam.right + finalOffset.y * cam.up - finalOffset.z * cam.forward;

        cam.position = Vector3.Lerp(cam.position, finalCameraPos, Time.deltaTime * cameraLerpSpeed);

        


        lastMousePosition = Input.mousePosition;
    }

    private void SetTargetRotation(){

    }

    private void SetTargetZoom()
    {

    }

    public void SetCameraTarget()
    {

    }


}
