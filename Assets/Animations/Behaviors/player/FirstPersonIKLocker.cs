using TowerDefense.Player;
using UnityEngine;

namespace TowerDefense.Animations.Behaviors.Player {
	public class FirstPersonIKLocker : StateMachineBehaviour {
		public bool lockOnEnter = true;
		public bool lockOnExit = false;

		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			FirstPersonModelRotation modelRotation = animator.GetComponent<FirstPersonModelRotation>();
			if (modelRotation)
				modelRotation.ForcedLock = lockOnEnter;
		}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			// Ensure that the lock holds
			FirstPersonModelRotation modelRotation = animator.GetComponent<FirstPersonModelRotation>();
			if (modelRotation)
				modelRotation.ForcedLock = lockOnEnter;
		}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			FirstPersonModelRotation modelRotation = animator.GetComponent<FirstPersonModelRotation>();
			if (modelRotation)
				modelRotation.ForcedLock = lockOnExit;
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