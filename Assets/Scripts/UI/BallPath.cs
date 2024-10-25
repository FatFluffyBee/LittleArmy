using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPath : MonoBehaviour
{
    private Renderer rd;

    public void Initialize(Color color) {
        rd = GetComponent<Renderer>();
        rd.material.color = color;
        rd.enabled = false;
    }
    public void SetColor(Color color) {
        rd.material.color = color;
        rd.enabled = true;
    }

    public void SetVisibility(bool choice) {
        rd.enabled = choice;
    }
}
