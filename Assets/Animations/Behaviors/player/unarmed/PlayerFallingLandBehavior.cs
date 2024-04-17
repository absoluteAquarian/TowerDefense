using AbsoluteCommons.Utility;
using TowerDefense.Player;
using UnityEngine;

namespace TowerDefense.Animations.Behaviors.Player.Unarmed {
	public class PlayerFallingLandBehavior : StateMachineBehaviour {
		[SerializeField] private float jumpInterruptionTime;

		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			// Reset the fall time
			animator.SetFloatSafely("fallTime", 0f);
			animator.SetBoolSafely("longFall", false);

			// Reset the jumping flag
			animator.SetBoolSafely("jumping", false);

			var controller = animator.gameObject.GetComponentInParent<PlayerMovement>();
			if (controller != null)
				controller.canJump = false;
		}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			if (jumpInterruptionTime > 0f && stateInfo.normalizedTime >= jumpInterruptionTime) {
				var controller = animator.gameObject.GetComponentInParent<PlayerMovement>();
				if (controller != null)
					controller.canJump = true;
			}
		}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			// Reset the landing flag
			animator.SetBoolSafely("landing", false);

			var controller = animator.gameObject.GetComponentInParent<PlayerMovement>();
			if (controller != null)
				controller.canJump = true;
		}

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
