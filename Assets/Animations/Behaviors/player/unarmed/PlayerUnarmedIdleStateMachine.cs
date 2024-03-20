using AbsoluteCommons.Utility;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerUnarmedIdleStateMachine : StateMachineBehaviour {
	[DoNotSerialize] private static readonly string[] _idleStateNames = new string[] { "idle_breathing", "idle_look", "idle_tap_foot" };

	[SerializeField, ReadOnly] private float turningFactor;
	[SerializeField] private float turningSpeed = 8f;

	// OnStateEnter is called before OnStateEnter is called on any state inside this state machine
	//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    
	//}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		// Get the CharacterController component of the actor
		CharacterController controller = animator.gameObject.GetComponentInParent<CharacterController>();

		// Set "hasHorizontalMovement" to true if the actor is moving horizontally
		animator.SetBool("hasHorizontalMotion", controller.velocity.x != 0f);

		// Set "hasVerticalMovement" to true if the actor is moving vertically
		animator.SetBool("hasVerticalMotion", controller.velocity.y != 0f);

		// If the actor is falling, increase "fallTime" by the time since the last frame
		// Otherwise, reset "fallTime" to 0
		if (controller.velocity.y < 0)
			animator.IncrementFloat("fallTime", Time.deltaTime);
		else
			animator.SetFloat("fallTime", 0);

		// If the actor is in any of the idle states, increase "idleTime"
		// Otherwise, reset "idleTime" to 0
		if (Array.FindIndex(_idleStateNames, stateInfo.IsName) >= 0)
			animator.IncrementFloat("idleTime", Time.deltaTime);
		else
			animator.SetFloat("idleTime", 0);

		// Get the FirstPersonView component from the main camera and set the "turnDirection" parameter of the animator
		FirstPersonView firstPersonView = Camera.main.GetComponent<FirstPersonView>();

		int direction = firstPersonView.RotationDirection.Horizontal;
		int factorSign = Math.Sign(turningFactor);
		if (direction == 0)
			direction = -factorSign;

		if (direction != 0) {
			float step = Time.deltaTime * turningSpeed;

			// If the factor would step over 0, then set it to 0
			if (turningFactor != 0 && Math.Sign(turningFactor + step) != factorSign)
				turningFactor = 0;
			else
				turningFactor += step * direction;

			turningFactor = Mathf.Clamp(turningFactor, -1, 1);
		}

		animator.SetInteger("turnDirection", Math.Sign(turningFactor));
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
