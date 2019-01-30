using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Sergei Merkulov
// Patrol State:
// - npc randomly finds a point and then move to it
// - npc will go to chase state if player will be in chase range
// - npc will go to "BackToSleepPoint" state if player will not detected for > 30 sec


public class PatrolState_AI01 : FSMState
{
    const int CHASE_DIST = 40;
    const int WAYPOINT_DIST = 1; //distance range to waypoint dest position


    //Constructor
    public PatrolState_AI01(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Patrol;
        curSpeed = 3.0f;
        curRotSpeed = 2.0f;
    }
   
    public override void Reason(Transform player, Transform npc)
    {
        AIController enemy = npc.GetComponent<AIController>();
        
        //if health < = 0 - go to death state
        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);
            Debug.Log(enemy.name + " switched to DEAD from Patrol");
        }
        
        //if player in chase range - go to chase state and tell to boss that u see player
        else if (IsInCurrentRange(npc, player.position, CHASE_DIST))
        {
            destPos = player.position;         
            enemy.father.childrenFoundPlayer = true;
            enemy.PerformTransition(Transition.SeePlayer);
            Debug.Log(enemy.name + " switched to CHASE from Patrol");

        }       

        // The player has not been detected for > 30s, go to back to sleep point state
        if (enemy.PatrolTimer(30))
        {
            //timer is done
            enemy.patrolTimerText.text = "0,0 s";
            enemy.PerformTransition(Transition.NotFoundForLongTime);
            Debug.Log(enemy.name + " switched to BACK TO SLEEP POINT from Patrol");
        }
    
    }

    public override void Act(Transform player, Transform npc)
    {        
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        //if reached previous point, find new and keep patroling 
        if (IsInCurrentRange(npc, destPos, WAYPOINT_DIST))
        {
            FindNextPoint();
        }
    }

}
