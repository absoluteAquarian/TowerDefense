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

	private void Awake() {
		_controller = GetComponent<CharacterController>();
	}

	private void Update() {
		// Handle jump force
		_isGrounded = _controller.isGrounded;

		// Handle grounded interaction
		if (_isGrounded && _velocity.y < 0) {
			_velocity.y = 0;

			// Set the triggers in the animators
			foreach (Animator animator in GetComponentsInChildren<Animator>()) {
				animator.SetBoolSafely("jumping", false);
				if (!_oldGrounded && animator.GetFloatSafely("fallTime", defaultValue: 0) > 0)
					animator.SetBoolSafely("landing", true);
			}
		}

		// Get horizontal movement
		Vector3 move;
		if (!zeroVelocity) {
			move = canMoveHorizontally ? new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) : Vector3.zero;
			move = transform.TransformDirection(move);
			move *= _acceleration * Time.deltaTime;

			_velocity += move;
		} else {
			move = Vector3.zero;
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

		if (move.x != 0 || move.z != 0) {
			// Set the triggers in the animators
			foreach (Animator animator in GetComponentsInChildren<Animator>())
				animator.SetBoolSafely("hasHorizontalMotion", true);
		} else {
			// Set the triggers in the animators
			foreach (Animator animator in GetComponentsInChildren<Animator>())
				animator.SetBoolSafely("hasHorizontalMotion", false);
		}

		// Handle jump
		if (canJump && _isGrounded && Input.GetButtonDown("Jump")) {
			_velocity.y = _jumpStrength;

			// Set the triggers in the animators
			foreach (Animator animator in GetComponentsInChildren<Animator>())
				animator.SetBoolSafely("jumping", true);
		}

		_gravity = Physics.gravity * Time.deltaTime;
		if (!_isGrounded && Math.Sign(_velocity.y) == Math.Sign(_gravity.y) && Mathf.Abs(_velocity.y) >= Mathf.Abs(_gravity.y)) {
			// Set the triggers in the animators
			foreach (Animator animator in GetComponentsInChildren<Animator>())
				animator.IncrementFloat("fallTime", Time.deltaTime);
		} else {
			// Set the triggers in the animators
			foreach (Animator animator in GetComponentsInChildren<Animator>())
				animator.SetFloatSafely("fallTime", 0);
		}

		// Apply gravity
		_velocity += _gravity;

		if (_velocity.y > _maxFallVelocity) {
			_velocity.y = _maxFallVelocity;

			foreach (Animator animator in GetComponentsInChildren<Animator>())
				animator.SetBoolSafely("longFall", true);
		}

		// Move the player
		_collisionFlags = _controller.Move(_velocity * Time.deltaTime);

		_oldGrounded = _isGrounded;
	}
}
