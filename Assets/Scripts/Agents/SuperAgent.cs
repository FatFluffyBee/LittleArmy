using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuperAgent : MonoBehaviour, ISelectable
{
    public enum FormationType {Random, Square, VShape}
    [SerializeField] private FormationType formationType;
    [SerializeField] private float spacing;
    [SerializeField] private List<Agent> agents; 
    
    [SerializeField] private Color unitColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unSelectedColor;

    private Transform destination;
    private Agent centerAgent;

    void Start(){
        foreach (Agent agent in agents){
            agent.Initialize(this, unitColor);
        }
        SetCenterAgent();
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.D)) {
            SetAgentsDestination(destination.position);
        }

        if(centerAgent == null)
            SetCenterAgent();

        if(agents.Count == 0)
            Destroy(gameObject);
    }
    public void SetAgentsDestination(Vector3 destination) {
        List<Vector3> formationPositions = GetFormationPos(destination, formationType, spacing);
        for(int i = 0; i < formationPositions.Count; i++ ) {
            agents[i].GiveMoveOrder(formationPositions[i]);
        }
    }

    public List<Vector3> GetFormationPos(Vector3 middlePos, FormationType formationType, float spacing)
    {
        List<Vector3> positions = new List<Vector3>();
        switch (formationType){
            case FormationType.Random:
                for(int i = 0; i < agents.Count; i++) {
                    positions.Add(middlePos + Random.insideUnitSphere * 2f * spacing);
                }
                break;
            case FormationType.Square:
                for(int i = 0; i < agents.Count; i++) {
                    positions.Add(middlePos + new Vector3(i % 3 - 1, 0, i / 3 - 1) * spacing);
                }
                break;
            case FormationType.VShape:
                for(int i = 0; i < agents.Count; i++) {
                    positions.Add(middlePos + new Vector3(i % 3 - 1, 0, i / 3 - 1) * spacing);
                }
                break;
        }
        return positions;
    }

    public void RemoveUnit(Agent agent) {
        if(agents.Contains(agent))
            agents.Remove(agent);
    }

    public void IsSelected(){
        foreach(Agent agent in agents){
            agent.SetSelectionFeedbackVisibility(true);
        }
    }

    public void IsDeselected(){
        foreach(Agent agent in agents){
            agent.SetSelectionFeedbackVisibility(false);
        }    
    }

    public AgentType GetAgentType(){
            return AgentType.Ally;
    }

    public ISelectable ReturnSelectedObject(){
        return this;
    }

    public void MouseEnterFeedback(){
        foreach(Agent agent in agents){
            agent.SetBaseColor(selectedColor);
        }
    }

    public void MouseExitFeedback(){
        foreach(Agent agent in agents){
            agent.SetBaseColor(unSelectedColor);
        }
    }

    public void SetCenterAgent() {
        if(centerAgent != null)
            centerAgent.ClearCenterAgent();
            
        centerAgent = agents[agents.Count / 2];
        centerAgent.SetCenterAgent();
    }
}
