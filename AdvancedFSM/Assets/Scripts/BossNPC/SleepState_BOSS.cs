using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Sergei Merkulov
// Sleep State:
// - npc is doing nothing.
// - npc will go to attack state if player will be in melee attack range
// - npc will go to chase state if all children are killed and player in chase range


public class SleepState_BOSS : FSMState
{
    const int MELEE_DIST = 6;
    const int CHASE_DIST = 50;


    //Constructor
    public SleepState_BOSS()
    {       
        stateID = FSMStateID.Sleep;
        curSpeed = 3.0f;
        curRotSpeed = 2.0f;         
    }


    public override void Reason(Transform player, Transform npc)
    {
        Boss_AIController enemy = npc.GetComponent<Boss_AIController>();

        //if health = 0 - go to Death State
        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);
            Debug.Log(enemy.name + " switched to DEAD from sleep boss");
        }

        //If player is in melee attack range - go to Attack State  OR  player has attacked
        if (IsInCurrentRange(npc, player.position, MELEE_DIST) || enemy.playerHasAttacked)
        {
            TurnOffSleepAnimation(enemy);
            enemy.PerformTransition(Transition.Attacked);
            Debug.Log(enemy.name + " switched to ATTACK from sleep boss");
        }

        //If player is in chase range && all children are killed - go to Chase State
        if (IsInCurrentRange(npc, player.position, CHASE_DIST) && enemy.ChildrenAreKilled())
        {
            TurnOffSleepAnimation(enemy);
            enemy.PerformTransition(Transition.ChildrenKilled_SeePlayer);
            Debug.Log(enemy.name + " switched to CHASE from sleep boss");
        }        

    }

    public override void Act(Transform player, Transform npc)
    {
        Boss_AIController enemy = npc.GetComponent<Boss_AIController>();

        if (enemy.backToSleepPoint) // reset health in case if npc was in "Back to sleep point" stage and reached sleep point.
        {
            enemy.SetHealth(100); //reset npc's health
            enemy.backToSleepPoint = false;
            enemy.playerHasAttacked = false;
            //turn on SleepingMarks animation
            enemy.sleepingAnimation.SetActive(true);
        }
    }


    void TurnOffSleepAnimation(Boss_AIController enemy)
    {
        //turn off SleepingMarks animation
        enemy.sleepingAnimation.SetActive(false);
        enemy.isSleeping = false;
    }
}
