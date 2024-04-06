using AbsoluteCommons.Attributes;
using AbsoluteCommons.Utility;
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

		[Header("IK Properties")]
		[SerializeField, ReadOnly] private GameObject _weaponObject;
		[SerializeField, ReadOnly] private GameObject _leftHandIKTarget;
		[SerializeField, ReadOnly] private GameObject _rightHandIKTarget;

		private void Awake() {
			// NOTE: the child paths may need to be changed if this script is used in a different project
			_firstPersonAnimator = gameObject.GetChild("Animator/Y Bot Arms").GetComponent<Animator>();
			_thirdPersonAnimator = gameObject.GetChild("Animator/Y Bot").GetComponent<Animator>();
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

			if (Input.GetButtonDown("Deploy Weapon"))
				DeployWeapon();
			else if (Input.GetButtonDown("Holster Weapon"))
				HolsterWeapon();
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
					_firstPersonAnimator.SetTrigger("immediateDeployWeapon");

				if (_thirdPersonAnimator)
					_thirdPersonAnimator.SetTrigger("immediateDeployWeapon");

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
					_firstPersonAnimator.SetTrigger("immediateHolsterWeapon");

				if (_thirdPersonAnimator)
					_thirdPersonAnimator.SetTrigger("immediateHolsterWeapon");

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

		public void TickWeaponTransition(float normalizedTime) {
			if (_playWeaponAnimation) {
				float oldTime = _transitionTime;
				_transitionTime = normalizedTime;
				if (_transitionTime >= 1.0f) {
					_transitionTime = 1.0f;
					_playWeaponAnimation = false;

					switch (_deployState) {
						case DeployState.Deploying:
							_deployState = DeployState.Deployed;
							break;
						case DeployState.Holstering:
							_deployState = DeployState.Holstered;
							break;
					}
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
			
			if (_displayedWeapon == WeaponType.None)
				return;

			_weaponObject = database.InstantiateWeapon(_displayedWeapon);
			Weapon info = _weaponObject.GetComponent<Weapon>();
			if (info) {
				// Attach the weapon to the right hand bone
				Transform rightHand = _thirdPersonAnimator.GetBoneTransform(HumanBodyBones.RightHand);
				info.rightHandBone.transform.SetParent(rightHand, false);

				// The player is right-handed, so only the left hand needs to be set up for IK
				_leftHandIKTarget = info.leftHandBone;
			}

			if (_firstPersonAnimator)
				_firstPersonAnimator.SetBool("weaponDeployed", true);

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.SetBool("weaponDeployed", true);
		}

		private void DestroyWeaponObject() {
			_displayedWeapon = WeaponType.None;

			TypeExtensions.DestroyAndSetNull(ref _weaponObject);
			TypeExtensions.DestroyAndSetNull(ref _leftHandIKTarget);
			TypeExtensions.DestroyAndSetNull(ref _rightHandIKTarget);

			if (_firstPersonAnimator)
				_firstPersonAnimator.SetBool("weaponDeployed", false);

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.SetBool("weaponDeployed", false);
		}

		private void OnAnimatorIK(int layerIndex) {
			HandleIK(_firstPersonAnimator);
			HandleIK(_thirdPersonAnimator);
		}

		private void HandleIK(Animator animator) {
			if (animator) {
				// IK should only be active when weapon is fully deployed; the other states either have a transition animation or no weapon
				if (_deployState == DeployState.Deployed) {
					// Set the right hand target position and rotation, if one has been assigned
					if (_rightHandIKTarget) {
						animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
						animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
						animator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandIKTarget.transform.position);
						animator.SetIKRotation(AvatarIKGoal.RightHand, _rightHandIKTarget.transform.rotation);
					}

					// Set the left hand target position and rotation, if one has been assigned
					if (_leftHandIKTarget) {
						animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
						animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
						animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandIKTarget.transform.position);
						animator.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandIKTarget.transform.rotation);
					}
				} else {
					// Reset the right hand target position and rotation
					animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.0f);
					animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0.0f);

					// Reset the left hand target position and rotation
					animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.0f);
					animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0.0f);
				}
			}
		}
	}
}
