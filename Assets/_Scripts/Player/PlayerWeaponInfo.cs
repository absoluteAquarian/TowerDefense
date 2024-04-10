using AbsoluteCommons.Attributes;
using AbsoluteCommons.Components;
using AbsoluteCommons.Utility;
using TowerDefense.CameraComponents;
using TowerDefense.Weapons;
using UnityEngine;

namespace TowerDefense.Player {
	[AddComponentMenu("Player/Player Weapon Info")]
	public class PlayerWeaponInfo : MonoBehaviour {
		public WeaponDatabase database;

		private WeaponType _previousWeapon;

		[Header("Weapon Properties")]
		[SerializeField] private WeaponType _currentWeapon;
		public WeaponType CurrentWeapon => _currentWeapon;

		[SerializeField, ReadOnly] private WeaponType _displayedWeapon;
		public WeaponType DisplayedWeapon {
			get => _displayedWeapon;
			private set => _displayedWeapon = value;
		}

		[SerializeField, ReadOnly] private bool _hasShootCooldown;
		[SerializeField, ReadOnly] private bool _triggerFinger;

		[SerializeField, ReadOnly] private TimersTracker _timers;

		[Header("Animation Properties")]
		[SerializeField, ReadOnly] private bool _playWeaponAnimation;
		[SerializeField, ReadOnly] private DeployState _deployState;
		[SerializeField, ReadOnly] private float _transitionTime;
		public float AnimationTransitionTime {
			get => _transitionTime;
			private set => _transitionTime = value;
		}

		[SerializeField, ReadOnly] private float _deployShowTime = 0f;
		[SerializeField, ReadOnly] private float _holsterHideTime = 1f;

		private Animator _firstPersonAnimator;
		private Animator _thirdPersonAnimator;

		private CameraFollow _camera;

		[Header("IK Properties")]
		[SerializeField, ReadOnly] private GameObject _weaponObject;
		[SerializeField, ReadOnly] private GameObject _firstPersonWeaponObject;
	//	[SerializeField, ReadOnly] private GameObject _leftHandIKTarget;
	//	[SerializeField, ReadOnly] private GameObject _firstPersonLeftHandIKTarget;
	//	[SerializeField, ReadOnly] private GameObject _rightHandIKTarget;
	//	[SerializeField, ReadOnly] private GameObject _firstPersonRightHandIKTarget;

		private void Awake() {
			// NOTE: the child paths may need to be changed if this script is used in a different project
			_firstPersonAnimator = gameObject.GetChild("Animator/Y Bot Arms").GetComponent<Animator>();
			_thirdPersonAnimator = gameObject.GetChild("Animator/Y Bot").GetComponent<Animator>();

			_camera = Camera.main.GetComponent<CameraFollow>();

			_timers = GetComponent<TimersTracker>();
		}

		private void Start() {
			DeployWeapon();
		}

		private void Update() {
			// Trigger weapon change animation if the weapon has changed
			if (_previousWeapon != _currentWeapon) {
				SetWeapon(_currentWeapon);
				_previousWeapon = _currentWeapon;
			}

			if (!_playWeaponAnimation) {
				// Ensure that the player can't get stuck in a Deploying or Holstering state
				if (_deployState == DeployState.Deploying) {
					_deployState = DeployState.Holstered;  // Force the weapon to deploy
					InitWeaponObject();
				} else if (_deployState == DeployState.Holstering)
					DestroyWeaponObject();
			}

			// Ensure that the weapon is visible when deployed and not visible when holstered
			if (_deployState == DeployState.Deployed) {
				if (!_weaponObject)
					InitWeaponObject();
			} else if (_deployState == DeployState.Holstered) {
				if (_weaponObject)
					DestroyWeaponObject();
			}

			if (Input.GetButtonDown("Deploy Weapon"))
				DeployWeapon();
			else if (Input.GetButtonDown("Holster Weapon"))
				HolsterWeapon();
			
			if (CanShootWeapon())
				ShootWeapon();
			else
				_triggerFinger = false;

			// Update the animator
			if (_firstPersonAnimator)
				_firstPersonAnimator.SetInteger("weaponState", (int)_deployState);

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.SetInteger("weaponState", (int)_deployState);
		}

		public void SetWeapon(WeaponType weapon) {
			_currentWeapon = weapon;

			Weapon info = database.GetWeaponInfo(_currentWeapon);
			if (info) {
				_deployShowTime = Mathf.Clamp(info.deployShowTime, 0f, 1f);
				_holsterHideTime = Mathf.Clamp(info.holsterHideTime, 0f, 1f);
			}

			// Only deploy if the game is running
			if (Application.isPlaying) {
				_deployState = DeployState.Holstered;  // Force the weapon to deploy
				DeployWeapon();
			}

			if (_firstPersonAnimator)
				_firstPersonAnimator.SetInteger("weaponID", (int)_currentWeapon);

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.SetInteger("weaponID", (int)_currentWeapon);
		}

		public void DeployWeapon(bool immediate = false) {
			if (_deployState != DeployState.Holstered && _currentWeapon != WeaponType.None)
				return;

			if (_currentWeapon == WeaponType.None) {
				HolsterWeapon();
				return;
			}

			if (immediate) {
				_deployState = DeployState.Deployed;
				_transitionTime = 1.0f;
				_playWeaponAnimation = false;
				InitWeaponObject();
				
				if (_firstPersonAnimator)
					_firstPersonAnimator.ForceTrigger("immediateDeployWeapon");

				if (_thirdPersonAnimator)
					_thirdPersonAnimator.ForceTrigger("immediateDeployWeapon");

				return;
			}

			DestroyWeaponObject();

			_deployState = DeployState.Deploying;
			_displayedWeapon = WeaponType.None;
			_transitionTime = 0.0f;
			_playWeaponAnimation = true;

			if (_firstPersonAnimator)
				_firstPersonAnimator.SetTrigger("deployWeapon");

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.SetTrigger("deployWeapon");
		}

		public void HolsterWeapon(bool immediate = false) {
			if (_deployState != DeployState.Deployed && _currentWeapon != WeaponType.None)
				return;

			if (_currentWeapon == WeaponType.None)
				return;

			if (immediate) {
				_deployState = DeployState.Holstered;
				_transitionTime = 1.0f;
				_playWeaponAnimation = false;
				DestroyWeaponObject();

				if (_firstPersonAnimator)
					_firstPersonAnimator.ForceTrigger("immediateHolsterWeapon");

				if (_thirdPersonAnimator)
					_thirdPersonAnimator.ForceTrigger("immediateHolsterWeapon");

				return;
			}

			_deployState = DeployState.Holstering;
			_transitionTime = 0.0f;
			_playWeaponAnimation = true;

			if (_firstPersonAnimator)
				_firstPersonAnimator.SetTrigger("holsterWeapon");

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.SetTrigger("holsterWeapon");
		}

		private bool CanShootWeapon() {
			if (_hasShootCooldown || _deployState != DeployState.Deployed || _currentWeapon == WeaponType.None)
				return false;

			Weapon info = database.GetWeaponInfo(_currentWeapon);

			if (!info.autoFire)
				return Input.GetButtonDown("Fire");

			return Input.GetButton("Fire");
		}

		private void ShootWeapon() {
			_hasShootCooldown = true;

			Weapon info = database.GetWeaponInfo(_currentWeapon);

			// TODO: spawn projectile
			
			if (!info.autoFire || !_triggerFinger) {
				Timer timer = Timer.CreateCountdown(ResetShootTriggers, info.shootTime, repeating: info.autoFire);
				timer.Start();

				_timers.AddTimer(timer);
			}

			if (_firstPersonAnimator)
				_firstPersonAnimator.SetTrigger("shoot");

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.SetTrigger("shoot");

			_triggerFinger = true;
		}

		private void ResetShootTriggers() {
			_hasShootCooldown = false;

			if (_firstPersonAnimator)
				_firstPersonAnimator.ResetTrigger("shoot");

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.ResetTrigger("shoot");

			// If the weapon autofires, check the input and shoot again
			// Checking in Update may be too late
			Weapon info = database.GetWeaponInfo(_currentWeapon);

			if (info.autoFire && CanShootWeapon()) {
				_triggerFinger = true;
				ShootWeapon();
			}
		}

		public void TickWeaponTransition(float normalizedTime) {
			if (_playWeaponAnimation) {
				float oldTime = _transitionTime;
				_transitionTime = normalizedTime;
				if (_transitionTime >= 1.0f) {
					_transitionTime = 1.0f;
					_playWeaponAnimation = false;
				}

				CheckWeaponVisibility(oldTime, _transitionTime);
			}
		}

		private void CheckWeaponVisibility(float lastTick, float currentTick) {
			switch (_deployState) {
				case DeployState.Deploying:
					if (lastTick < _deployShowTime && currentTick >= _deployShowTime) {
						// Make the weapon visible
						InitWeaponObject();
					}
					break;
				case DeployState.Holstering:
					if (lastTick < _holsterHideTime && currentTick >= _holsterHideTime) {
						// Hide the weapon
						DestroyWeaponObject();
					}
					break;
			}
		}

		private void InitWeaponObject() {
			DestroyWeaponObject();
			_displayedWeapon = _currentWeapon;
			_deployState = DeployState.Deployed;
			
			if (_displayedWeapon == WeaponType.None)
				return;

			_weaponObject = database.InstantiateWeapon(_displayedWeapon);
			Weapon info = _weaponObject.GetComponent<Weapon>();
			if (info) {
				// Attach the weapon to the right hand bone
				Transform rightHand = _thirdPersonAnimator.GetBoneTransform(HumanBodyBones.RightHand);
				info.rightHandBone.transform.SetParent(rightHand, false);

				// The player is right-handed, so only the left hand needs to be set up for IK
			//	_leftHandIKTarget = info.leftHandBone;
			}

			_firstPersonWeaponObject = database.InstantiateWeapon(_displayedWeapon);
			info = _firstPersonWeaponObject.GetComponent<Weapon>();
			if (info) {
				// Attach the weapon to the right hand bone
				Transform rightHand = _firstPersonAnimator.GetBoneTransform(HumanBodyBones.RightHand);
				info.rightHandBone.transform.SetParent(rightHand, false);

				// The player is right-handed, so only the left hand needs to be set up for IK
			//	_firstPersonLeftHandIKTarget = info.leftHandBone;
			}

			if (_firstPersonAnimator)  {
				_firstPersonAnimator.SetBool("weaponDeployed", true);
				_firstPersonAnimator.SetFloat("shootSpeed", info.shootAnimationMultiplier);
			}

			if (_thirdPersonAnimator) {
				_thirdPersonAnimator.SetBool("weaponDeployed", true);
				_thirdPersonAnimator.SetFloat("shootSpeed", info.shootAnimationMultiplier);
			}
		}

		private void DestroyWeaponObject() {
			_displayedWeapon = WeaponType.None;
			_deployState = DeployState.Holstered;

			TypeExtensions.DestroyAndSetNull(ref _weaponObject);
			TypeExtensions.DestroyAndSetNull(ref _firstPersonWeaponObject);
		//	TypeExtensions.DestroyAndSetNull(ref _leftHandIKTarget);
		//	TypeExtensions.DestroyAndSetNull(ref _firstPersonLeftHandIKTarget);
		//	TypeExtensions.DestroyAndSetNull(ref _rightHandIKTarget);
		//	TypeExtensions.DestroyAndSetNull(ref _firstPersonRightHandIKTarget);

			if (_firstPersonAnimator)
				_firstPersonAnimator.SetBool("weaponDeployed", false);

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.SetBool("weaponDeployed", false);
		}

		// TODO: none of this works.  too lazy to fix it right now though

		public void HandleFirstPersonIK(Animator animator) {
		//	HandleIK(animator, _firstPersonLeftHandIKTarget, null);
		}

		public void HandleThirdPersonIK(Animator animator) {
		//	HandleIK(animator, _leftHandIKTarget, null);
		}

		private void HandleIK(Animator animator, GameObject leftHandTarget, GameObject rightHandTarget) {
			if (animator) {
				// IK should only be active when weapon is fully deployed; the other states either have a transition animation or no weapon
				if (_deployState == DeployState.Deployed) {
					// Set the right hand target position and rotation, if one has been assigned
					if (rightHandTarget)
						HandleIK_SetParameters(animator, true, rightHandTarget);
					else
						ClearRightHandIK(animator);

					// Set the left hand target position and rotation, if one has been assigned
					if (leftHandTarget)
						HandleIK_SetParameters(animator, false, leftHandTarget);
					else
						ClearLeftHandIK(animator);
				} else {
					ClearRightHandIK(animator);
					ClearLeftHandIK(animator);
				}
			}
		}

		private void HandleIK_SetParameters(Animator animator, bool rightHand, GameObject target) {
			AvatarIKGoal goal = rightHand ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;
			AvatarIKHint hint = rightHand ? AvatarIKHint.RightElbow : AvatarIKHint.LeftElbow;

			animator.SetIKPositionWeight(goal, 1.0f);
			animator.SetIKRotationWeight(goal, 1.0f);
			animator.SetIKPosition(goal, target.transform.position);

			Quaternion rotation = target.transform.rotation;
			// Adjust the rotation so that the hand is in the correct orientation (it is offset by a 90 degree rotation in two axes)
			// TODO: right hand doesn't use IK, will this work?
			rotation *= rightHand ? Quaternion.Euler(0, 90, 90) : Quaternion.Euler(0, -90, 90);

			animator.SetIKRotation(goal, rotation);

			// Set the hint position
			animator.SetIKHintPositionWeight(hint, 1.0f);
			animator.SetIKHintPosition(hint, target.transform.position);
		}

		public void ClearIK(Animator animator) {
			if (animator) {
				ClearRightHandIK(animator);
				ClearLeftHandIK(animator);
			}
		}

		private void ClearRightHandIK(Animator animator) {
			// Reset the right hand target position and rotation
			animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.0f);
			animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0.0f);
			animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0.0f);
		}

		private void ClearLeftHandIK(Animator animator) {
			// Reset the left hand target position and rotation
			animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.0f);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0.0f);
			animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0.0f);
		}
	}
}
