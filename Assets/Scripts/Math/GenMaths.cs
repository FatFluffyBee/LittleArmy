using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenMaths 
{
    public static Vector3 WithoutY(Vector3 vector) {
        return new Vector3(vector.x, 0f, vector.z);
    }
}
