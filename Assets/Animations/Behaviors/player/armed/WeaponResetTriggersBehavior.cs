using AbsoluteCommons.Utility;
using UnityEngine;

namespace TowerDefense.Animations.Behaviors.Player.Armed {
	public class WeaponResetTriggersBehavior : StateMachineBehaviour {
		public bool resetDeployTrigger;
		public bool resetDeployImmediateTrigger;
		public bool resetHolsterTrigger;
		public bool resetHolsterImmediateTrigger;

		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			if (resetDeployTrigger)
				animator.NetResetTriggerSafely("deployWeapon");

			if (resetDeployImmediateTrigger)
				animator.NetResetTriggerSafely("immediateDeployWeapon");

			if (resetHolsterTrigger)
				animator.NetResetTriggerSafely("holsterWeapon");

			if (resetHolsterImmediateTrigger)
				animator.NetResetTriggerSafely("immediateHolsterWeapon");
		}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    
		//}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    
		//}

		// OnStateMove is called right after Animator.OnAnimatorMove()
		//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    // Implement code that processes and affects root motion
		//}

		// OnStateIK is called right after Animator.OnAnimatorIK()
		//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    // Implement code that sets up animation IK (inverse kinematics)
		//}
	}
}
