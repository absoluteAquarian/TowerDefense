using TowerDefense.CameraComponents;
using TowerDefense.Player;
using UnityEngine;

namespace TowerDefense.Animations.Behaviors.Player {
	public class PlayerModelRotationLockBehavior : StateMachineBehaviour {
		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			Camera.main.GetComponent<CameraFollowTargetTransformInterceptor>().Lock();
			ThirdPersonModelRotation modelRotation = animator.GetComponent<ThirdPersonModelRotation>();
			if (modelRotation)
				modelRotation.ForcedLock = true;
		}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    
		//}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			Camera.main.GetComponent<CameraFollowTargetTransformInterceptor>().Unlock(lerping: true);
			ThirdPersonModelRotation modelRotation = animator.GetComponent<ThirdPersonModelRotation>();
			if (modelRotation)
				modelRotation.ForcedLock = false;
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
