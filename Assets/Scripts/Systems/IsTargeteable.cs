using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargettingType {First, Last, Closest, More, Strongest, Weakest}
public enum AgentType {Ally, Ennemi, Neutral}
public enum ObjectType {Agent, Structure}
public class IsTargeteable : MonoBehaviour
{
    public AgentType agentType;
}
