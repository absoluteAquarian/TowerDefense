using AbsoluteCommons.Utility;
using UnityEngine;

namespace TowerDefense.Animations.Behaviors.Player.Unarmed {
	public class PlayerUnarmedChooseIdleBehavior : StateMachineBehaviour {
		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    
		//}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			float idleTime = animator.GetFloat("idleTime");

			// If the player has been idle for more than 12 seconds, choose a new idle animation
			if (idleTime > 12) {
				int random = Random.Range(0, 3);
				animator.SetInteger("idleChoice", random);

				// Choice 0 is the default idle animation, which isn't a moving idle
				if (random != 0)
					animator.NetSetTriggerSafely("playMovingIdle");
			}
		}

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
