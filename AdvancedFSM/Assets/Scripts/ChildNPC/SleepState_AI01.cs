using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Sergei Merkulov
// Sleep State:
// -npc is doing nothing.
// - npc will go to attack state if player will be in attack range
// - npc will go to "Run to calling ally" state if some of npcs has health < 50

public class SleepState_AI01 : FSMState
{
    const int ATTACK_DIST = 15;
    const int CHASE_DIST = 40;

    
    //Constructor
    public SleepState_AI01()
    {       
        stateID = FSMStateID.Sleep;
        curSpeed = 3.0f;
        curRotSpeed = 2.0f;  
    }


    public override void Reason(Transform player, Transform npc)
    {
        AIController enemy = npc.GetComponent<AIController>(); 

        //switch to Death State
        if (enemy.GetHealth() <= 0)
        {
            enemy.PerformTransition(Transition.NoHealth);
        }

        
        else if (IsInCurrentRange(npc, player.position, ATTACK_DIST))
        {
            TurnOffSleepAnimation(enemy);
            enemy.PerformTransition(Transition.SeePlayer);
            Debug.Log(enemy.name + " switched to ATTACK from sleep ");
        }

        else if (enemy.called)
        {
            TurnOffSleepAnimation(enemy);
            enemy.PerformTransition(Transition.Called);
            Debug.Log(enemy.name + " has been called by ally, so switched to RUN to ALLY from sleep");
        }

        else if (enemy.playerHasAttacked)
        {
            TurnOffSleepAnimation(enemy);
            enemy.PerformTransition(Transition.Attacked);
            Debug.Log(enemy.name + " switched to CHASE from sleep");
        }

        else if(enemy.askedToPatrolByBoss)
        {
            enemy.askedToPatrolByBoss = false;
            TurnOffSleepAnimation(enemy);            
            enemy.PerformTransition(Transition.AskedToPatrolByBoss);
            Debug.Log(enemy.name + " switched to PATROL cause daddy asked from sleep");
        }

    }

    public override void Act(Transform player, Transform npc)
    {       
        AIController enemy = npc.GetComponent<AIController>();

        if(enemy.backToSleepPoint) // reset health in case if npc was in "Back to sleep point" stage and reached sleep point.
        {
            enemy.SetHealth(100); //reset npc's health
            enemy.isSleeping = true;
            enemy.called = false;
            enemy.backToSleepPoint = false;
            enemy.playerHasAttacked = false;
            enemy.sleepingAnimation.SetActive(true);
        }                
    }

    void TurnOffSleepAnimation(AIController enemy)
    {
        //turn off SleepingMarks animation
        enemy.sleepingAnimation.SetActive(false);
        enemy.isSleeping = false;
    }
   
}
