using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Sergei Merkulov
// Patrol by Children State:
// - npc is in patrol state and if they will find player they will chase him and tell to the boss that they found player 
// - npc will go to chase state if player will be in chase dist or children found the player ( bool childrenFoundPlayer will be = true)
// - npc will go to "BackToSleepPoint" state if player will not detected for > 30 sec

public class PatrolByChildrenState_BOSS : FSMState
{

    const int CHASE_DIST = 50;

    const int WAYPOINT_DIST = 1; //distance range to waypoint dest position

    //Constructor
    public PatrolByChildrenState_BOSS()
    {        
        stateID = FSMStateID.PatrolByChildren;
    }


    public override void Reason(Transform player, Transform npc)
    {
        Boss_AIController enemy = npc.GetComponent<Boss_AIController>();

        //if health < = 0 - go to death state
        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);
            Debug.Log(enemy.name + " BOSS switched to DEAD from PatrolByChilren");
        }
            
        // The player has not been detected for > 30s, go to back to sleep point state
        if (enemy.PatrolTimer(30) && !enemy.childrenFoundPlayer)
        {
            //timer is done
            enemy.timerPatrolText.text = "0,0 s";
            enemy.PerformTransition(Transition.NotFoundForLongTime);
            Debug.Log(enemy.name + " switched to BACK TO SLEEP POINT from PatrolByChilren");
        }

        //if children found player or player is in chase range
        if(enemy.childrenFoundPlayer || IsInCurrentRange(npc,player.position, CHASE_DIST))
        {
            enemy.timerPatrolText.text = "0,0 s";
            enemy.PerformTransition(Transition.SeePlayer);
            Debug.Log(enemy.name + " switched to CHASE from PatrolByChilren");
        }

    }

    public override void Act(Transform player, Transform npc)
    {
        //do nothing... just wait
    }
}
