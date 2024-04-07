using AbsoluteCommons.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace TowerDefense.Animations.Behaviors {
	public class GradualLayerWeightChangeBehavior : StateMachineBehaviour {
		public EasingMode easingMode = EasingMode.Linear;
		public float transitionTimeStart = 0f;
		public float transitionTimeEnd = 1f;
		public bool behaviorResetsWeight = true;

		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    
		//}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			if (stateInfo.normalizedTime < transitionTimeStart)
				animator.SetLayerWeight(layerIndex, behaviorResetsWeight ? 1f : 0f);
			else if (stateInfo.normalizedTime >= transitionTimeEnd)
				animator.SetLayerWeight(layerIndex, behaviorResetsWeight ? 0f : 1f);
			else {
				float duration = transitionTimeEnd - transitionTimeStart;
				float t = EasingExtensions.Ease(easingMode, (stateInfo.normalizedTime - transitionTimeStart) / duration);

				animator.SetLayerWeight(layerIndex, behaviorResetsWeight ? 1f - t : t);
			}

		//	Debug.Log("Layer " + layerIndex + " weight: " + animator.GetLayerWeight(layerIndex));
		}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			// Disable the layer
			animator.SetLayerWeight(layerIndex, behaviorResetsWeight ? 0f : 1f);
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
