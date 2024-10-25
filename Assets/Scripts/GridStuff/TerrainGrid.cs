using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class TerrainGrid : MonoBehaviour
{
    public bool isDebug = false;
    public static int GRID_SCALE = 10;
    public GameObject levelGeometry;
    public GameObject gridCasePrefab;
    private GridData[,] gridArray;
    private Vector3 gridOrigin;
    private Vector3 worldOffset;
    private Vector3 caseCenterOffset;
    private GridData lastHighlightedSquare;
    void Awake() {
        gridOrigin = transform.position;
        gridArray = SetupGridDataArray(levelGeometry);
    }

    void Update() {
        //DisplayGridDebug();
    }

    private GridData[,] SetupGridDataArray(GameObject levelGeometry){ //Initial setup of the gris taking the geometry data into parameter

        GridData[,] gridArray = GeometryToGrid.CreateGridFromGeometry(levelGeometry.transform, GRID_SCALE, out gridOrigin);
        worldOffset = CalculateGridOffset();
        caseCenterOffset = GetCenterCaseOffset();

        foreach(GridData e in gridArray){
            if(e!= null){
                Vector3 pos = new Vector3(e.Key.x * GRID_SCALE, e.Height + 0.1f, e.Key.y * GRID_SCALE) + caseCenterOffset + worldOffset;
                GameObject gridCase = Instantiate(gridCasePrefab, pos, gridCasePrefab.transform.rotation, transform);
                e.SetGridCase(gridCase.GetComponent<CaseGrid>());

                if(!isDebug) {
                    e.CaseGrid.SetTextVisibility(false);
                }
                else {

                }
            }
        }
        return gridArray;
    }

    public void DisplayGridDebug() //display the debug grid
    {
        for(int i = 0; i < gridArray.GetLength(0); i++)
            for(int j = 0; j < gridArray.GetLength(1); j++)
                if(gridArray[i, j] != null){
                    gridArray[i, j].CaseGrid.SetTextVisibility(true);
                    Vector3 worldPos = GetWorldPos(new Vector2Int(i, j));

                    Debug.DrawLine(worldPos, worldPos + Vector3.right * GRID_SCALE, Color.white);
                    Debug.DrawLine(worldPos, worldPos + Vector3.forward * GRID_SCALE, Color.white);
                }
                
        for(int i = 0; i < gridArray.GetLength(0); i++){
            Vector3 worldPos = GetWorldPos(new Vector2Int(i, gridArray.GetLength(1)));
            Debug.DrawLine(worldPos, worldPos + Vector3.right * GRID_SCALE, Color.white);
        }

         for(int j = 0; j < gridArray.GetLength(1); j++){
            Vector3 worldPos = GetWorldPos(new Vector2Int(gridArray.GetLength(0), j));
            Debug.DrawLine(worldPos, worldPos + Vector3.forward * GRID_SCALE, Color.white);
        }
    }
    
    private Vector3 CalculateGridOffset(){
        float offsetX = gridOrigin.x;
        float offsetY = gridOrigin.y;
        float offsetZ = gridOrigin.z;

        return new Vector3(offsetX, offsetY, offsetZ);
    }

    public Vector3 GetWorldPos(Vector2Int key){
        return new Vector3(key.x * GRID_SCALE + worldOffset.x, 0f, key.y * GRID_SCALE + worldOffset.z);
    }

    public Vector3 GetWorldPosCenter(Vector2Int key){
        return new Vector3(key.x * GRID_SCALE + worldOffset.x, gridArray[key.x, key.y].Height + worldOffset.y, key.y * GRID_SCALE + worldOffset.z) + caseCenterOffset;
    }

    public Vector3 GetCenterCaseOffset(){
        return new Vector3(1f, 0f, 1f) * GRID_SCALE / 2;
    }

    public Vector2Int GetKeyFromWorldPos(Vector3 worldPos){
        Vector2Int key = new Vector2Int((int)((worldPos.x - worldOffset.x) / GRID_SCALE), (int)((worldPos.z - worldOffset.z) /GRID_SCALE));

        if(key.x < 0 || key.x > gridArray.GetLength(0)-1 || key.y < 0 || key.y > gridArray.GetLength(1) -1)
            return - Vector2Int.one;

        return key;
    }

    public bool CheckIfInBound(Vector2Int key){
        return key.x >= 0 && key.x < gridArray.GetLength(0) && key.y >= 0 && key.y < gridArray.GetLength(1);
    }

    public bool CheckIfKeyPosIsAccessible(Vector2Int key) { //check if a given key is accessible, so it must be inbound, there must be a grid object in pos and it should be wlakable
         if(!CheckIfInBound(key)) return false;
         if(gridArray[key.x, key.y] == null) return false;
         else if(!gridArray[key.x, key.y].IsWalkable) return false;

         return true;
    }
    public void SetGridData(Vector2Int key, Agent agent) {
        gridArray[key.x, key.y].Agent = agent;
    }
    public void DisplayGridCase(bool choice) {
        foreach(GridData e in gridArray){
            if(e!= null){
                e.CaseGrid.SetRdVisibility(choice);
            }
        }
    }
}
    public class GridData {
        public Vector2Int Key {get;}
        public Agent Agent {get; set; }
        public CaseGrid CaseGrid {get; set; }
        public float Height {get; set;} = 0;
        public bool IsWalkable {get; set;} = false;
        public string Orientation {get; set;} // for slope and others, its binary representation 1010 or 0101 in this order : top / right / bottom / left  // 1 is true 0 is false

        public GridData(Vector2Int key, int height, bool isWalkable){
            Key = key;
            Height = height;
            IsWalkable = isWalkable;
            Orientation = "0000";
        }

        public void SetGridCase(CaseGrid caseGrid) {
            CaseGrid = caseGrid;
            caseGrid.SetText(Height.ToString());

            if(!IsWalkable){
                caseGrid.SetTextColor(Color.grey);
            }
            else {
                caseGrid.SetTextColor(Color.black);
            }
            caseGrid.SetTextVisibility(false);
            caseGrid.SetRdVisibility(false);
        }
    }
