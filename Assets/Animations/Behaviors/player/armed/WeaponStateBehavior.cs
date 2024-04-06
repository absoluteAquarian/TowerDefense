using TowerDefense.Player;
using TowerDefense.Weapons;
using UnityEngine;

namespace TowerDefense.Animations.Behaviors.Player.Armed {
	public class WeaponStateBehavior : StateMachineBehaviour {
		public ForcedDeployState enterState;
		public ForcedDeployState exitState;

		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			PlayerWeaponInfo info = animator.gameObject.GetComponentInParent<PlayerWeaponInfo>();

			if (info) {
				switch (enterState) {
					case ForcedDeployState.Holster:
						info.HolsterWeapon(immediate: true);
						break;
					case ForcedDeployState.Deploy:
						info.DeployWeapon(immediate: true);
						break;
				}
			}
		}

		// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
		//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		//{
		//    
		//}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			PlayerWeaponInfo info = animator.gameObject.GetComponentInParent<PlayerWeaponInfo>();

			if (info) {
				switch (exitState) {
					case ForcedDeployState.Holster:
						info.HolsterWeapon(immediate: true);
						break;
					case ForcedDeployState.Deploy:
						info.DeployWeapon(immediate: true);
						break;
				}
			}
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
