using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CaseGrid : MonoBehaviour
{
    [SerializeField] private Renderer caseRd;
    [SerializeField] private TextMeshPro textItem;
    [SerializeField] private Color baseColor;
    [SerializeField] private Color hoverColor;

    void Start() {
        caseRd.material.color = baseColor;
    }

    public void SetGridCasePos(Vector3 newPos){
        transform.position = newPos;
    }

    public void SetRdVisibility(bool visible) {
        caseRd.enabled = visible;
    }

    public void SetBaseColor() {
        caseRd.material.color = baseColor;
    }

    public void SetHoverColor() {
        caseRd.material.color = hoverColor;
    }

    public void SetText(string text) {
        textItem.text = text;
    }

    public void SetTextColor(Color color) {
        textItem.color = color;
    }
    public void SetTextVisibility(bool value){
        textItem.enabled = value;
    }

    void OnMouseEnter() {
        caseRd.material.color = hoverColor;
    }

    void OnMouseExit() {
        caseRd.material.color = baseColor;
    }

}
