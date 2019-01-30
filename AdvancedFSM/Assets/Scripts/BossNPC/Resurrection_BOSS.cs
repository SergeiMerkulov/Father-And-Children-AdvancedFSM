using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Author: Sergei Merkulov
// Resurrection State:
// - npc is casting ressurection spell during 3s and all children(small npc) will go to chase state
// - npc will go to "Patrol by children" state if all children are alive and player is out of chase dist
// - npc will go to attack state if player will be in range attack dist and all children are alive

public class Resurrection_BOSS : FSMState
{
    const int CHASE_DIST = 50;
    const int RANGE_DIST = 30;

    bool resurrected;

    //Constructor
    public Resurrection_BOSS()
    {
        stateID = FSMStateID.Resurrection;       
        resurrected = false;
    }


    public override void Reason(Transform player, Transform npc)
    {
        Boss_AIController enemy = npc.GetComponent<Boss_AIController>();

        //if health less ot = 0 - go to death state
        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);

        }
        
        // All children are alive &&  still no player - go to Patrol by children state
        else if (!IsInCurrentRange(npc, player.position, RANGE_DIST) && !enemy.ChildrenAreKilled())
        {
            for (int i = 0; i < enemy.children.Count; i++)
            {
                enemy.children[i].askedToPatrolByBoss = true;
            }
            enemy.PerformTransition(Transition.ChildrenAlive_LostPlayer);
            Debug.Log(enemy.name + " switched to PATROL by children from Resurrection_BOSS");
        }

        //All children are alive  &&  See Player - go to Attack state
        else if (IsInCurrentRange(npc, player.position, RANGE_DIST) && !enemy.ChildrenAreKilled())
        {     
            enemy.PerformTransition(Transition.ChildrenAlive_SeePlayer);
            Debug.Log(enemy.name + " switched to ATTACK from Resurrection_BOSS");
        }

    }

    public override void Act(Transform player, Transform npc)
    {        
        if (!resurrected)
        {
            Boss_AIController enemy = npc.GetComponent<Boss_AIController>();

            //display resurrection info board on the sreen
            enemy.ressurectionBoard.SetActive(true);

            //if casting resurrection spell is finished, resurrect each  dead child.
            if (enemy.PatrolTimer(3))
            {
                foreach (AIController g in enemy.children)
                {
                    //if child was not resurrected  previosly, reset health and all his flags
                    if (!g.resurrected)
                    {
                        Debug.Log("Resurrection of children");
                        g.SetHealth(100);
                        g.called = false;
                        g.isSleeping = false;                        
                        g.askedToPatrolByBoss = false;
                        g.resurrected = true;
                        g.transform.localScale = new Vector3(2, 2, 2);
                    }
                }

                //hide resurrection info board on the sreen
                enemy.ressurectionBoard.SetActive(false);
                resurrected = true;
            }
        }
    }


}
