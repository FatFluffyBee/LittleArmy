using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

public class MouseInput : MonoBehaviour
{
    AgentType agentType;
    Camera cam;
    Vector2Int selectedObjectGridKey;
    public TerrainGrid terrainGrid;
    public bool debug = false;

    [Header("DEBUG ONLY")]
    [SerializeField] ISelectable selectedObject;

    bool cancelAllInput = false;

    void Start(){
        cam = Camera.main;
    }
    void Update(){ //HANDLE MORE EVENT WITH EVENT and interface
        Vector3 mousePos = Input.mousePosition;
        Ray camRay = cam.ScreenPointToRay(mousePos);
        
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 92.2f));
        
        if(debug)
        {
            Debug.DrawLine(cam.transform.position, cam.transform.position + camRay.direction * 1000);  //camera ray
        }
        //AGENT SELECTION
        if(Input.GetMouseButtonDown(0)){
            DeselectAgent();
            SelectTarget(camRay);
            if(selectedObject != null) 
                terrainGrid.DisplayGridCase(true);
            else
                terrainGrid.DisplayGridCase(false);
        }

        //AGENT ORDERING
        if(Input.GetMouseButtonUp(1) && !cancelAllInput){
            if(selectedObject != null)
                switch(agentType){
                    case AgentType.Ally :
                        InitiateAgentMoveOrder(selectedObject.gameObject.GetComponent<SuperAgent>(), camRay);
                    break;

                    case AgentType.Ennemi :
                    break;
                }   
            }

        //INPUT CANCELLATION
        if(!Input.GetMouseButton(0) && !Input.GetMouseButton(1)){
            cancelAllInput = false;
        }

        if(Input.GetMouseButton(0) && Input.GetMouseButton(1)) {
            cancelAllInput = true;
        }
    }

    void SelectTarget(Ray ray)
    {
        //Debug.Log("Trying to select an agent");              
        RaycastHit[] hits;
        
        hits = Physics.RaycastAll(ray);
        hits = hits.OrderBy((d) => d.distance).ToArray();

        foreach(RaycastHit hit in hits) {
            ISelectable iSelectable = hit.transform.GetComponent<ISelectable>();

            if(iSelectable != null){
                selectedObject = hit.transform.GetComponent<ISelectable>().ReturnSelectedObject(); //wanky but works for now
                selectedObjectGridKey = terrainGrid.GetKeyFromWorldPos(hit.transform.position);

                agentType = selectedObject.GetAgentType();

                selectedObject.IsSelected();
                break;
            }
        }
    }

    void DeselectAgent(){
        //Debug.Log("Deselecting agent"); 
        if(selectedObject != null) {
            selectedObject.IsDeselected();
            selectedObject = null;
        }
    }

    bool InitiateAgentMoveOrder(SuperAgent superAgent, Ray ray) { 
        if(Physics.Raycast(ray, out RaycastHit hit)) {
            Vector2Int keyFromHitPos = terrainGrid.GetKeyFromWorldPos(hit.point); 
            if(!terrainGrid.CheckIfKeyPosIsAccessible(keyFromHitPos)) return false; //out of bounds click

            Vector3 targetPos = terrainGrid.GetWorldPosCenter(keyFromHitPos);

            superAgent.SetAgentsDestination(targetPos);
            terrainGrid.SetGridData(selectedObjectGridKey, null);
            return true;
        }
        return false;
    }
}
