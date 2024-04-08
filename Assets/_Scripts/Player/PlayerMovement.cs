using AbsoluteCommons.Attributes;
using AbsoluteCommons.PhysicsMath;
using AbsoluteCommons.Utility;
using System;
using UnityEngine;

namespace TowerDefense.Player {
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
			_isGrounded = _controller.isGrounded;

			// Handle grounded interaction
			if (_isGrounded && _velocity.y < 0) {
				_velocity.y = 0;

				// Set the triggers in the animator
				_thirdPersonAnimator.SetBoolSafely("jumping", false);
				if (!_oldGrounded && _thirdPersonAnimator.GetBool("falling"))
					_thirdPersonAnimator.SetBoolSafely("landing", true);
			}

			// Handle sprinting
			bool sprinting = Input.GetButton("Sprint");
			_thirdPersonAnimator.SetBool("sprinting", sprinting);

			// Get a rotation to be applied to directions
			Vector3 euler = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
			Vector3 forward = Quaternion.Euler(euler) * Vector3.forward;
			Quaternion movementRotation = ArbitraryGravity.GetRotation(_gravity, forward);

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
			Vector3 planar = _velocity.PerpendicularTo(_gravity);

			_velocity -= frictionForce * planar;

			_velocity = _velocity
				.WithPerpendicularDeadZone(_gravity, 0.1f)
				.WithPerpendicularSpeedCap(_gravity, sprinting ? _sprintingMaxVelocity : _maxVelocity);

			Vector3 horizontalVelocity = _velocity.PerpendicularTo(_gravity);
			_velocityMagnitude = horizontalVelocity.magnitude;

			_thirdPersonAnimator.SetBool("hasHorizontalMovement", _velocityMagnitude > 0.1f);

			Vector3 unitHorizontal = horizontalVelocity.normalized;

			_thirdPersonAnimator.SetFloat("forwardVelocity", Vector3.Dot(unitHorizontal, movementRotation * Vector3.forward));
			_thirdPersonAnimator.SetFloat("strafeVelocity", Vector3.Dot(unitHorizontal, movementRotation * Vector3.right));

			bool canTaunt = _isGrounded && !_thirdPersonAnimator.GetBool("landing");

			// Handle jump
			if (canJump && _isGrounded && Input.GetButtonDown("Jump")) {
				_velocity += _jumpStrength * ArbitraryGravity.Up(_gravity);

				// Set the triggers in the animator
				_thirdPersonAnimator.SetBool("jumping", true);

				canTaunt = false;
			}

			_gravity = Physics.gravity * Time.deltaTime;

			// Player is "falling" if the dot product of the velocity and gravity is positive (vectors are in the same direction)
			Vector3 projection = _velocity.ParallelTo(_gravity);
			if (!_isGrounded && _velocity.IsFallingToward(_gravity) && (!_hasOldGravity || projection.sqrMagnitude > _oldGravity.sqrMagnitude)) {
				// Set the triggers in the animator
				_thirdPersonAnimator.SetBool("falling", true);

				canTaunt = false;
			} else {
				// Set the triggers in the animator
				_thirdPersonAnimator.SetBool("falling", false);
			}

			_firstPersonAnimator.SetBool("canTaunt", canTaunt);
			_thirdPersonAnimator.SetBool("canTaunt", canTaunt);

			// Apply gravity
			_velocity += _gravity;
			_oldGravity = _gravity;
			_hasOldGravity = true;

			if (ArbitraryGravity.RestrictTerminalVelocity(ref _velocity, _gravity, _maxFallVelocity)) {
				// Set the triggers in the animator
				_thirdPersonAnimator.SetBool("longFall", true);
			}

			// Move the player
			_collisionFlags = _controller.Move(_velocity * Time.deltaTime);

			_oldGrounded = _isGrounded;
		}
	}
}
