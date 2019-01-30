using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Author: Sergei Merkulov
// Attack State:
// -npc rotates to player and attack it by calling func HitPlayer()
//	call others npcs if health < 50 and some of npcs is sleeping
//	suicide if health < 10 
// - npc will go to chase state if player will be out of attack range
// - npc will go to death state if health <=0

public class AttackState_AI01 : FSMState
{
    const int ATTACK_DIST = 15;
    bool suicideUsed;
    bool suicideIsCasting;
    private float nextFire;
    private float exlodingSize;
    private float increasingSpeed;

    //Constructor
    public AttackState_AI01()
    {      
        stateID = FSMStateID.Attack;
        curSpeed = 3.5f;
        curRotSpeed = 3.0f;          
        suicideUsed = false;
        suicideIsCasting = false;        
        exlodingSize = 4;
        increasingSpeed = 0.8f;
    }


    public override void Reason(Transform player, Transform npc)
    {
        AIController enemy = npc.GetComponent<AIController>();
        destPos = player.position;

        if (enemy.GetHealth() <= 0)
        {      
            enemy.PerformTransition(Transition.NoHealth);
            return;
        }

        if (!IsInCurrentRange(npc, destPos, ATTACK_DIST) && !suicideIsCasting)
        {           
            enemy.PerformTransition(Transition.OutOfAttackRange);
        }       

    }

    public override void Act(Transform player, Transform npc)
    {
        AIController enemy = npc.GetComponent<AIController>();

        //set destpoint
        destPos = player.position;
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //shoot   
        if(Time.time > nextFire)
        {
            nextFire = Time.time + enemy.fireRate;           
            enemy.HitPLayer();
        }
                      

        //------------------------------
        //CALL
        //------------------------------
                   
        //if enemy's health <= 50% health and if some of nearby enemies are still sleeping, call them
        if (enemy.GetHealth() <= 50)
        {
            foreach (AIController g in enemy.father.children)
            {
                if (g != enemy && g.called == false && g.isSleeping)
                {                    
                    g.isSleeping = false;
                    g.called = true;
                    g.posOfCalledAlly = npc;
                }               
            }            
        }


        //------------------------------
        //SUICIDE
        //------------------------------
        if (enemy.GetHealth() <= 10 && !suicideUsed)
        {
                    
            Debug.Log(enemy.name + "Casting Suicide");
            suicideIsCasting = true;
            //play suicide sound

            if(suicideIsCasting && npc.localScale.x < exlodingSize)
            {
                npc.localScale += npc.localScale * Time.deltaTime * increasingSpeed;
            }
            else
            {
                enemy.PlayParticles();
                enemy.aSourse.PlayOneShot(enemy.explode, 1.0f);
                suicideUsed = true;
                enemy.SetHealth(0);
                Debug.Log(enemy.name +  "has Suicided");
            }            
        }
    }
    
}
