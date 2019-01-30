using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Author: Sergei Merkulov
// Chase State:
// - npc is chasing the player
// - npc will go to attack state if player will be in range attack dist
// - npc will go to "Patrol by children" state if all children are alive and player is out of chase dist
// - npc will go to "Resurrection of Children" state if all children are killed and player is out of chase dist

public class Chase_BOSS : FSMState
{

    const int CHASE_DIST = 50;
    const int ATTACK_DIST = 30; // == to max range of attack


    //Constructor
    public Chase_BOSS(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Chase;
        curSpeed = 3.0f;
        curRotSpeed = 2.0f;   
    }


    public override void Reason(Transform player, Transform npc)
    {
        Boss_AIController enemy = npc.GetComponent<Boss_AIController>();

        //set destpoint
        destPos = player.position;

        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);
        }

        //If player is in attack range  - go to Attack State
        if (IsInCurrentRange(npc, destPos, ATTACK_DIST))
        {
            enemy.childrenFoundPlayer = false;
            Debug.Log("Switch to attack state");
            enemy.PerformTransition(Transition.ReachedPlayer);
            Debug.Log(enemy.name + " switched to ATTACK State from Chase_BOSS");
        }      
        //If player is out of chase range && all children are alive && player not found by children - go to Patrol by children State
        else if (!IsInCurrentRange(npc, destPos, CHASE_DIST) && !enemy.ChildrenAreKilled() && !enemy.childrenFoundPlayer)
        {
            for (int i = 0; i < enemy.children.Count; i++)
            {
                enemy.children[i].askedToPatrolByBoss = true;
                enemy.children[i].InitPatrolTimer();
            }
        
            //reset timer on the screen
            enemy.InitTimer();
            //go to next state
            enemy.PerformTransition(Transition.ChildrenAlive_LostPlayer);
            Debug.Log(enemy.name + " switched to PATROL by children State from Chase_BOSS");
        }
        //If player is out of chase range && all children are killed && didnt ressurect yet - go to Resurrection State
        else if (!IsInCurrentRange(npc, destPos, CHASE_DIST) && enemy.ChildrenAreKilled() && enemy.didntRessurectYet)
        {
            FindNextPoint();
            enemy.didntRessurectYet = false;
            enemy.PerformTransition(Transition.ChildrenKilled_LostPlayer);
            Debug.Log(enemy.name + " switched to RESURRECTION State from Chase_BOSS");
        }        
    }

    public override void Act(Transform player, Transform npc)
    {
        //Chasing
        //set destpoint
        destPos = player.position;
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);

    }
}
