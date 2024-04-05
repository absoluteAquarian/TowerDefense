using AbsoluteCommons.Utility;
using UnityEngine;

[AddComponentMenu("Player/Player Weapon Info")]
public class PlayerWeaponInfo : MonoBehaviour {
	public enum WeaponType {
		None,
		Pistol
	}

	public enum DeployState {
		Holstered,
		Holstering,
		Deploying,
		Deployed
	}

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

	[SerializeField] private float _deployShowTime = 0.2f;
	[SerializeField] private float _holsterHideTime = 0.8f;

	private Animator _animator;

	[Header("IK Properties")]
	[SerializeField, ReadOnly] private GameObject _weaponObject;
	[SerializeField, ReadOnly] private GameObject _leftHandIKTarget;
	[SerializeField, ReadOnly] private GameObject _rightHandIKTarget;

	private void OnValidate() {
		_deployShowTime = Mathf.Clamp(_deployShowTime, 0f, 1f);
		_holsterHideTime = Mathf.Clamp(_holsterHideTime, 0f, 1f);

		// Trigger weapon change animation if the weapon has changed in the inspector
		if (_previousWeapon != _currentWeapon) {
			SetWeapon(_currentWeapon);
			_previousWeapon = _currentWeapon;
		}
	}

	private void Awake() {
		// NOTE: the child paths may need to be changed if this script is used in a different project
		_animator = gameObject.GetChild("Animator/Y Bot").GetComponent<Animator>();
	}

	public void SetWeapon(WeaponType weapon) {
		_currentWeapon = weapon;
		DeployWeapon();
	}

	public void DeployWeapon() {
		if (_displayedWeapon == _currentWeapon)
			return;

		_deployState = DeployState.Deploying;
		_displayedWeapon = _currentWeapon;
		_transitionTime = 0.0f;
		_playWeaponAnimation = true;
	}

	public void HolsterWeapon() {
		if (_displayedWeapon == WeaponType.None)
			return;

		_deployState = DeployState.Holstering;
		_transitionTime = 0.0f;
		_playWeaponAnimation = true;
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
		if (_displayedWeapon != _currentWeapon) {
			DestroyWeaponObject();
			_displayedWeapon = _currentWeapon;
			
			if (_displayedWeapon == WeaponType.None)
				return;

			// TODO: spawn weapon object, set IK targets
		}
	}

	private void DestroyWeaponObject() {
		if (_displayedWeapon != WeaponType.None) {
			_displayedWeapon = WeaponType.None;

			TypeExtensions.DestroyAndSetNull(ref _weaponObject);
			TypeExtensions.DestroyAndSetNull(ref _leftHandIKTarget);
			TypeExtensions.DestroyAndSetNull(ref _rightHandIKTarget);
		}
	}

	private void OnAnimatorIK(int layerIndex) {
		if (_animator) {
			// IK should only be active when weapon is fully deployed; the other states either have a transition animation or no weapon
			if (_deployState == DeployState.Deployed) {
				// Set the right hand target position and rotation, if one has been assigned
				if (_rightHandIKTarget) {
					_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
					_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
					_animator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandIKTarget.transform.position);
					_animator.SetIKRotation(AvatarIKGoal.RightHand, _rightHandIKTarget.transform.rotation);
				}

				// Set the left hand target position and rotation, if one has been assigned
				if (_leftHandIKTarget) {
					_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
					_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
					_animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandIKTarget.transform.position);
					_animator.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandIKTarget.transform.rotation);
				}
			} else {
				// Reset the right hand target position and rotation
				_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.0f);
				_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0.0f);

				// Reset the left hand target position and rotation
				_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.0f);
				_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0.0f);
			}
		}
	}
}
