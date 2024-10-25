using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LvlEditorCaseInfo : MonoBehaviour
{
    public enum CaseType {Cube, Slope}
    public CaseType caseType;
    
    public float height;
    public bool isWalkable;
    public bool isSelectable;

}
