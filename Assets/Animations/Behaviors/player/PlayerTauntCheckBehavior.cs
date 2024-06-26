﻿using AbsoluteCommons.Utility;
using TowerDefense.Player;
using Unity.Netcode;
using UnityEngine;

namespace TowerDefense.Animations.Behaviors.Player {
	public class PlayerTauntCheckBehavior : StateMachineBehaviour {
		// OnStateEnter is called before OnStateEnter is called on any state inside this state machine
		//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    
		//}

		// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			// If the taunt button is pressed, set the trigger
			NetworkObject netSelf = animator.GetComponentInParent<NetworkObject>();
			if ((!netSelf || netSelf.IsOwner) && ClientInput.IsTriggered("Taunt"))
				animator.NetSetTriggerSafely("taunting");
		}

		// OnStateExit is called before OnStateExit is called on any state inside this state machine
		//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    
		//}

		// OnStateMove is called before OnStateMove is called on any state inside this state machine
		//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    
		//}

		// OnStateIK is called before OnStateIK is called on any state inside this state machine
		//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    
		//}

		// OnStateMachineEnter is called when entering a state machine via its Entry Node
		//override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
		//{
		//    
		//}

		// OnStateMachineExit is called when exiting a state machine via its Exit Node
		//override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
		//{
		//    
		//}
	}
}
