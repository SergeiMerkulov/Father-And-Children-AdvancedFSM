using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Sergei Merkulov
// "BackToSleepPoint" State:
// -npc moves to the sleep point and when it will reached npc goes to sleep state
// - npc will go to sleep state when the point will be reached
// - npc will go to chase if player will be in chase dist

public class BackToSleepState_BOSS : FSMState
{
    const int CHASE_DIST = 50; //chase range
 
    //Constructor
    public BackToSleepState_BOSS(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.BackToSleepPoint;
        curSpeed = 3.0f;
        curRotSpeed = 2.0f;   
        FindNextPoint();
    }


    public override void Reason(Transform player, Transform npc)
    {
        Boss_AIController enemy = npc.GetComponent<Boss_AIController>();

        //die if no health
        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);
            Debug.Log(enemy.name + " BOSS switched to DEAD");
        }

        //chase if a player into chase range
        else if (IsInCurrentRange(npc, player.position, CHASE_DIST))
        {
            enemy.PerformTransition(Transition.SeePlayer);
            Debug.Log(enemy.name + " switched to CHASE");
        }

        //Reached the sleep point
        else if (enemy.backToSleepPoint)
        {                           
            //switch to Sleep state
            Debug.Log("Daddy has reached the sleep poi... ZzZzZz");
            enemy.PerformTransition(Transition.ReachedSleepPoint);
            Debug.Log(enemy.name + " switched to SLEEP");

        }
    }

    public override void Act(Transform player, Transform npc)
    {
        Boss_AIController enemy = npc.GetComponent<Boss_AIController>();

        if (npc.position != destPos)
        {
            //set destpoint
            Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
            npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);

            if (!enemy.backToSleepPoint)
            {
                npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);
            }
        }

        //the sleep point has been reached
        if (IsInCurrentRange(npc, destPos, 1))
        {
            enemy.backToSleepPoint = true;
        }
    }
}

