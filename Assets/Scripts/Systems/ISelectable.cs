using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectable
{
    GameObject gameObject { get ; } 
    abstract void IsSelected();
    abstract void IsDeselected();

    abstract AgentType GetAgentType();

    abstract ISelectable ReturnSelectedObject();
}
