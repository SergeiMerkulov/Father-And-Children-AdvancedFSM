using UnityEngine;
using System.Collections;

//Author: Sergei Merkulov
// Dead State for BOSS:
// -npc is doing nothing


public class DeadState : FSMState
{

	//Constructor
    public DeadState()
	{		
		stateID = FSMStateID.Dead;
	}


	public override void Reason( Transform player, Transform npc)
	{    
    }
    

	public override void Act( Transform player, Transform npc)
	{
        npc.GetComponent<MeshRenderer>().material.color = Color.gray; // set color to gray because of death
    }

}
