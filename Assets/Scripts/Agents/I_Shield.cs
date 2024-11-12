using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Shield
{
    public Transform TargetShield { get; set; }

    public Shield GetShield();
}