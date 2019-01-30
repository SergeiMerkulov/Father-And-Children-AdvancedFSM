using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Author: Sergei Merkulov
// "BackToSleepPoint" State:
// -npc moves to the sleep point and when it will reached npc goes to sleep state
// - npc will go to sleep state when the point will be reached
// - npc will go to chase if player will be in chase dist

public class BackToSleepPointState_AI01 : FSMState
{
    const int CHASE_DIST = 40; //chase range

    //Constructor
    public BackToSleepPointState_AI01(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.BackToSleepPoint;
        curSpeed = 3.0f;
        curRotSpeed = 2.0f;
        FindNextPoint();
    }


    public override void Reason(Transform player, Transform npc)
    {
        AIController enemy = npc.GetComponent<AIController>();

        //die if no health
        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);
            Debug.Log(enemy.name + " switched to DEAD from backtosleep");
        }
        
        //chase if a player into Chase range
        else if(IsInCurrentRange(npc,player.position, CHASE_DIST))
        {
            enemy.patrolTimer = 0;
            enemy.PerformTransition(Transition.SeePlayer);
            Debug.Log(enemy.name + " switched to CHASE from backtosleep");
        }

        //Reached the sleep point
        else if (enemy.backToSleepPoint)
        {            
            enemy.PerformTransition(Transition.ReachedSleepPoint);
            Debug.Log(enemy.name + " switched to Sleep from backtosleep");
        }

        else if (enemy.playerHasAttacked)
        {            
            enemy.PerformTransition(Transition.Attacked);
            Debug.Log(enemy.name + " switched to PATROL from backtosleep");
        }

    }

    public override void Act(Transform player, Transform npc)
    {
        AIController enemy = npc.GetComponent<AIController>();

        if (npc.position != destPos)
        {
            //set destpoint
            Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
            npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);

            if(!enemy.backToSleepPoint)
            {
                npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);
            }
        }

        if(IsInCurrentRange(npc,destPos,1))
        {
            Debug.Log("I Reached the sleep point");
            enemy.backToSleepPoint = true;
        }
    }
}
