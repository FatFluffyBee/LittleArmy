using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine 
{
    private List<State> allStates = new List<State>();
    private State currentState;
    private State previousState;

    public StateMachine() {

    }

    public void AddState(State state) {

    }

    public void ChangeState() {

    }


}

public class State
{
    public string name;
    public delegate void StateChanged();
    public event StateChanged OnStateEnter;
    public event StateChanged OnStateStay;
    public event StateChanged OnStateExit;
}
