using AbsoluteCommons.Utility;
using System;
using UnityEngine;

[AddComponentMenu("Player/Player Movement")]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
	[SerializeField, ReadOnly] private Vector3 _velocity;
	[SerializeField, ReadOnly] private float _velocityMagnitude;
	[SerializeField, ReadOnly] private bool _isGrounded;
	private bool _oldGrounded;
	[SerializeField, ReadOnly] private CollisionFlags _collisionFlags;
	[SerializeField, ReadOnly] private Vector3 _gravity;
	private Vector3 _oldGravity;
	private bool _hasOldGravity;
	[SerializeField] private float _jumpStrength = 6.8f;
	[SerializeField] private float _maxVelocity = 5.5f;
	[SerializeField] private float _sprintingMaxVelocity = 11f;
	[SerializeField] private float _maxFallVelocity = 25f;
	[SerializeField] private float _acceleration = 115f;
	[SerializeField] private float _sprintingAcceleration = 250f;
	[SerializeField] private float _friction = 13f;
	[SerializeField] private float _airFriction = 7f;

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

		// Handle sprinting
		bool sprinting = Input.GetButton("Sprint");
		_thirdPersonAnimator.SetBool("sprinting", sprinting);

		// Get a rotation to be applied to directions
		Vector3 euler = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
		Quaternion movementRotation = Quaternion.Euler(euler);

		// Get horizontal movement
		if (!zeroVelocity) {
			if (canMoveHorizontally) {
				Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
				move = movementRotation * move;
				move *= (sprinting ? _sprintingAcceleration : _acceleration) * Time.deltaTime;

				_velocity += move;
			}
		} else {
			_velocity.x = 0;
			_velocity.z = 0;
			zeroVelocity = false;
		}

		// Apply friction
		float frictionForce = (_isGrounded ? _friction : _airFriction) * Time.deltaTime;
		Vector2 friction = frictionForce * _velocity.GetXZ();
		_velocity -= friction.ToXZ();

		float min = Mathf.Max(frictionForce, 0.1f);
		if (Mathf.Abs(_velocity.x) < min)
			_velocity.x = 0;
		if (Mathf.Abs(_velocity.z) < min)
			_velocity.z = 0;

		Vector2 xzVelocity = _velocity.GetXZ();
		VectorMath.RestrictMagnitude(ref xzVelocity, sprinting ? _sprintingMaxVelocity : _maxVelocity);
		_velocity.x = xzVelocity.x;
		_velocity.z = xzVelocity.y;

		Vector2 horizontalVelocity = _velocity.GetXZ();
		_velocityMagnitude = horizontalVelocity.magnitude;

		_thirdPersonAnimator.SetBool("hasHorizontalMovement", _velocityMagnitude > 0.1f);

		Vector2 unitHorizontal = horizontalVelocity.normalized;

		_thirdPersonAnimator.SetFloat("forwardVelocity", Vector2.Dot(unitHorizontal, (movementRotation * Vector3.forward).GetXZ()));
		_thirdPersonAnimator.SetFloat("strafeVelocity", Vector2.Dot(unitHorizontal, (movementRotation * Vector3.right).GetXZ()));

		// Handle jump
		if (canJump && _isGrounded && Input.GetButtonDown("Jump")) {
			_velocity.y = _jumpStrength;

			// Set the triggers in the animator
			_thirdPersonAnimator.SetBool("jumping", true);
		}

		_gravity = Physics.gravity * Time.deltaTime;

		// Player is "falling" if the dot product of the velocity and gravity is positive (vectors are in the same direction)
		Vector3 projection = Vector3.Project(_velocity, _gravity.normalized);
		if (!_isGrounded && Vector3.Dot(projection, _gravity) > 0 && (!_hasOldGravity || projection.sqrMagnitude > _oldGravity.sqrMagnitude)) {
			// Set the triggers in the animator
			_thirdPersonAnimator.IncrementFloat("fallTime", Time.deltaTime);
		} else {
			// Set the triggers in the animator
			_thirdPersonAnimator.SetFloat("fallTime", 0);
		}

		// Apply gravity
		_velocity += _gravity;
		_oldGravity = _gravity;
		_hasOldGravity = true;

		if (_velocity.y < -_maxFallVelocity) {
			_velocity.y = -_maxFallVelocity;

			// Set the triggers in the animator
			_thirdPersonAnimator.SetBool("longFall", true);
		}

		// Move the player
		_collisionFlags = _controller.Move(_velocity * Time.deltaTime);

		_oldGrounded = _isGrounded;
	}
}
