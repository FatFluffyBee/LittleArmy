using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentType {Ally, Ennemi, Neutral}
public enum ObjectType {Agent, Structure}
public class IsTargeteable : MonoBehaviour
{
    public AgentType agentType;
    public bool isRange;
}
