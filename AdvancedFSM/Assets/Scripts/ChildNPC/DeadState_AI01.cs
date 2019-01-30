using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Sergei Merkulov
// Dead State:
// -npc is doing nothing
//- npc switches to chase state if will be resurrected by boss(father)

public class DeadState_AI01 : FSMState
{
    bool cantBeResurrectedAnymore;

    //Constructor
    public DeadState_AI01()
    {        
        stateID = FSMStateID.Dead;
        curSpeed = 0.0f;
        curRotSpeed = 0.0f;
        cantBeResurrectedAnymore = false;
    }

  
    public override void Reason(Transform player, Transform npc)
    {
        
        AIController enemy = npc.GetComponent<AIController>();

       
        //if boss resurrected the npc, switch its state to Chase
        if(enemy.resurrected && !cantBeResurrectedAnymore)
        {
            npc.GetComponent<MeshRenderer>().material.color = Color.blue;
            cantBeResurrectedAnymore = true;
            enemy.PerformTransition(Transition.ResurrectedByBoss);
            Debug.Log(enemy.name + " switched to CHASE from dead state");
        }

    }
    

    public override void Act(Transform player, Transform npc)
    {
        npc.GetComponent<MeshRenderer>().material.color = Color.gray; // set color to gray because of death
    }

}
