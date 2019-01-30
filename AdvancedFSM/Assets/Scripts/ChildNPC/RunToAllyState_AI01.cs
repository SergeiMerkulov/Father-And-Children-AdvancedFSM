using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Sergei Merkulov
// RunToCallingAlly State:
// -npc moves and rotates to player until npc will reach chase dist then go to chase state
// - npc will go to attack state if player will be in attack range
// - npc will go to patrol state if it lost the player ( out of chase range)


public class RunToAllyState_AI01 : FSMState
{
    const int CHASE_DIST = 40;
    const int DISTANCE_BEFORE_CALLED_ALLY = 5; // if we at this range and still not in caase range , so let's go to patrol
    bool hasReachedAlly;

    //Constructor
    public RunToAllyState_AI01()
    {        
        stateID = FSMStateID.RunToCallingAlly;
        curSpeed = 8.0f;
        curRotSpeed = 5.0f;
        hasReachedAlly = false;
    }


    public override void Reason(Transform player, Transform npc)
    {
        AIController enemy = npc.GetComponent<AIController>();

        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);
        }

        else if (IsInCurrentRange(npc, player.position, CHASE_DIST))
        {
            enemy.PerformTransition(Transition.SeePlayer);
            Debug.Log(enemy.name + "switched to CHASE from RunToAlly");
        }
      
        else if (hasReachedAlly && !IsInCurrentRange(npc, player.position, CHASE_DIST))
        {
            enemy.PerformTransition(Transition.ReachedAllyPos);
            Debug.Log(enemy.name + "switched to PATROL from RunToAlly");
        }

    }

    public override void Act(Transform posOfCallingAlly, Transform npc)
    {      
        //if npc has not reached pos of called ally
        if (!IsInCurrentRange(npc, posOfCallingAlly.position, DISTANCE_BEFORE_CALLED_ALLY))
        {            
            //set destpoint
            destPos = posOfCallingAlly.position;
            Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
            npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);
            npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);
        }
        else
        {
            hasReachedAlly = true;
        }
    }
}

