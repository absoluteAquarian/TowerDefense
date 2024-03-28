using AbsoluteCommons.Utility;
using System;
using UnityEngine;

[AddComponentMenu("Player/Player Movement")]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
	[SerializeField, ReadOnly] private Vector3 _velocity;
	[SerializeField, ReadOnly] private bool _isGrounded;
	private bool _oldGrounded;
	[SerializeField, ReadOnly] private CollisionFlags _collisionFlags;
	[SerializeField, ReadOnly] private Vector3 _gravity;
	[SerializeField] private float _jumpStrength = 6.8f;
	[SerializeField] private float _maxVelocity = 4f;
	[SerializeField] private float _maxFallVelocity = 25f;
	[SerializeField] private float _acceleration = 42f;
	[SerializeField] private float _friction = 2.75f;

	[Header("Controls")]
	public bool canJump = true;
	public bool canMoveHorizontally = true;
	public bool zeroVelocity = false;

	private CharacterController _controller;
	private Animator _firstPersonAnimator;
	private Animator _thirdPersonAnimator;

	private void Awake() {
		_controller = GetComponent<CharacterController>();

		// NOTE: the child paths may need to be changed if this script is used in a different project
		_firstPersonAnimator = transform.gameObject.GetChild("Animator/Y Bot Arms").GetComponent<Animator>();
		_thirdPersonAnimator = transform.gameObject.GetChild("Animator/Y Bot").GetComponent<Animator>();
	}

	private void Update() {
		// Handle jump force
		_isGrounded = _controller.isGrounded;

		// Handle grounded interaction
		if (_isGrounded && _velocity.y < 0) {
			_velocity.y = 0;

			// Set the triggers in the animator
			_thirdPersonAnimator.SetBoolSafely("jumping", false);
			if (!_oldGrounded && _thirdPersonAnimator.GetFloat("fallTime") > 0)
				_thirdPersonAnimator.SetBoolSafely("landing", true);
		}

		// Get horizontal movement
		if (!zeroVelocity) {
			Vector3 move = canMoveHorizontally ? new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) : Vector3.zero;
			move = transform.TransformDirection(move);
			move *= _acceleration * Time.deltaTime;

			_velocity += move;
		} else {
			_velocity.x = 0;
			_velocity.z = 0;
			zeroVelocity = false;
		}

		// Apply friction
		if (_isGrounded) {
			Vector3 friction = _friction * Time.deltaTime * new Vector3(_velocity.x, 0, _velocity.z);
			_velocity -= friction;

			if (Mathf.Abs(_velocity.x) < 0.1f)
				_velocity.x = 0;
			if (Mathf.Abs(_velocity.z) < 0.1f)
				_velocity.z = 0;
		}

		Vector3 xzVelocity = new Vector3(_velocity.x, 0, _velocity.z);
		VectorMath.RestrictMagnitude(ref xzVelocity, _maxVelocity);
		_velocity.x = xzVelocity.x;
		_velocity.z = xzVelocity.z;

		// Handle jump
		if (canJump && _isGrounded && Input.GetButtonDown("Jump")) {
			_velocity.y = _jumpStrength;

			// Set the triggers in the animator
			_thirdPersonAnimator.SetBool("jumping", true);
		}

		_gravity = Physics.gravity * Time.deltaTime;

		// Player is "falling" if the dot product of the velocity and gravity is positive (vectors are in the same direction)
		if (!_isGrounded && Vector3.Dot(_velocity, _gravity) > 0) {
			// Set the triggers in the animator
			_thirdPersonAnimator.IncrementFloat("fallTime", Time.deltaTime);
		} else {
			// Set the triggers in the animator
			_thirdPersonAnimator.SetFloat("fallTime", 0);
		}

		// Apply gravity
		_velocity += _gravity;

		if (_velocity.y > _maxFallVelocity) {
			_velocity.y = _maxFallVelocity;

			// Set the triggers in the animator
			_thirdPersonAnimator.SetBool("longFall", true);
		}

		// Move the player
		_collisionFlags = _controller.Move(_velocity * Time.deltaTime);

		_oldGrounded = _isGrounded;
	}
}
