using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Author: Sergei Merkulov
// Chase State:
// -npc moves and rotates to player until npc will reach attack dist
// - npc will go to attack state if player will be in attack range
// - npc will go to patrol state if it lost the player ( out of chase range)


public class ChaseState_AI01 : FSMState
{
    const int CHASE_DIST = 40;
    const int PATROL_DIST = 60;
    const int ATTACK_DIST = 15;

    //Constructor
    public ChaseState_AI01(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Chase;
        curSpeed = 5.0f;
        curRotSpeed = 3.0f;
    }


    public override void Reason(Transform player, Transform npc)
    {
        AIController enemy = npc.GetComponent<AIController>();

        //if health < or = 0 -go to death state
        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);
        }

        //in attack arrange - go to attack state
        else if (IsInCurrentRange(npc, destPos, ATTACK_DIST))
        {            
            Debug.Log(enemy.name + " switched to ATTACK from CHASE");
            enemy.PerformTransition(Transition.ReachedPlayer);
        }
     
        //if in patrol dist - go to Patrol state
        else if (!IsInCurrentRange(npc, destPos, CHASE_DIST))
        {
            FindNextPoint();
            enemy.father.childrenFoundPlayer = false; // say that we lost player

            // reset timer
            enemy.InitPatrolTimer();
            //go to Patrol state
            Debug.Log(enemy.name + " switched to PATROL from CHASE");
            enemy.PerformTransition(Transition.LostPlayer);
        }

        //------------------------------
        //CALL friends
        //------------------------------

        //if enemy's health <= 50% health and if some of nearby enemies are still sleeping, call them
        if (enemy.GetHealth() <= 50)
        {
            foreach (AIController g in enemy.father.children)
            {
                if (g != enemy && g.called == false && g.isSleeping)
                {
                    Debug.Log("Casting Calling");
                    g.isSleeping = false;
                    g.called = true;
                    g.posOfCalledAlly = npc;
                }
            }
        }
    }

    public override void Act(Transform player, Transform npc)
    {
        //set destpoint
        destPos = player.position;
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
        npc.rotation = Quaternion.Slerp (npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

}
