using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Sergei Merkulov
// Attack State:
// - npc is attacking player by range or melee attack depends on attack dist between player and npc
// - npc will go to "Resurrection of Children" state if all children are killed and player is out of attack dist
// - npc will go to "Patrol by children" state if all children are alive and player is out of attack dist

public class AttackState_BOSS : FSMState
{  
    const int RANGE_DIST = 30;

    //Constructor
    public AttackState_BOSS()
    {
        stateID = FSMStateID.Attack;
        curSpeed = 8.0f;
        curRotSpeed = 6.0f;
    }


    public override void Reason(Transform player, Transform npc)
    {
        Boss_AIController enemy = npc.GetComponent<Boss_AIController>();
        destPos = player.position;

        //if health = 0 - go to Death State
        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);
            return;
        }

        //If player is out of attack range && all children are killed && didnt ressurect yet - switch to Resurrection State
        else if (!IsInCurrentRange(npc, destPos, RANGE_DIST) && enemy.ChildrenAreKilled() && enemy.didntRessurectYet)
        {
            enemy.didntRessurectYet = false;
            enemy.fist.SetActive(false); // deactivate fist
            enemy.PerformTransition(Transition.ChildrenKilled_OutOfAttackRange);
            Debug.Log(enemy.name + " switched to Resurrection State from AttackState_BOSS");            
        }

        //If player is out of attack range  - switch to Chase State
        else if (!IsInCurrentRange(npc, destPos, RANGE_DIST))
        {
            enemy.fist.SetActive(false); // deactivate fist
            enemy.PerformTransition(Transition.OutOfAttackRange);
            Debug.Log(enemy.name + " switched to CHASE State from AttackState_BOSS");
        }
        
    }

    public override void Act(Transform player, Transform npc)
    {
        //set destpoint
        destPos = player.position;
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        //do attack
        Boss_AIController enemy = npc.GetComponent<Boss_AIController>();

        if(!enemy.fist.activeInHierarchy)
        {
            enemy.ActivateMeleeWeapon();
        }
                        
    }
}
