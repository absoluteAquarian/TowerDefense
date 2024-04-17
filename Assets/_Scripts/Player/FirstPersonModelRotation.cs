using AbsoluteCommons.Utility;
using TowerDefense.CameraComponents;
using TowerDefense.Weapons;
using UnityEngine;

namespace TowerDefense.Player {
	[AddComponentMenu("Player/First Person Model Rotation")]
	public class FirstPersonModelRotation : MonoBehaviour {
		private PlayerWeaponInfo _weaponInfo;
		private CameraFollow _camera;

		private Animator _firstPersonAnimator;

		private Vector3 modelOffset;

		private bool _forcedLock;
		public bool ForcedLock {
			get => _forcedLock;
			set => _forcedLock = value;
		}

		private void Awake() {
			_weaponInfo = GetComponentInParent<PlayerWeaponInfo>();

			_firstPersonAnimator = GetComponent<Animator>();

			_camera = Camera.main.GetComponent<CameraFollow>();
		}

		private void Start() {
			modelOffset = transform.localPosition;
		}

		private void Update() {
			// Get the view rotation from the main camera
			FirstPersonView view = Camera.main.GetComponent<FirstPersonView>();
			CameraFollow follow = Camera.main.GetComponent<CameraFollow>();
			Quaternion rotation = Quaternion.Euler(view.ViewRotation);

			// Weapon info can further adjust the model offset and rotation to make the model look better
			Weapon weapon = _weaponInfo ? _weaponInfo.GetWeaponObject() : null;
			Vector3 modelOffset = this.modelOffset;
			if (weapon) {
				modelOffset += weapon.firstPersonModelOffset;
				rotation *= Quaternion.Euler(weapon.firstPersonModelRotation);
			}

			Vector3 positionBase = transform.parent.position + rotation * modelOffset;
			Vector3 pivot = follow.GetFirstPersonTarget() + rotation * modelOffset;

			// Set the position and rotation of the model
			transform.position = positionBase;
			transform.SetRotationWithPivot(pivot, rotation);
		}

		private void OnAnimatorIK(int layerIndex) {
			if (ForcedLock) {
				// Reset the IK
				ClearIK();
				return;
			}

		//	_weaponInfo.HandleFirstPersonIK(_firstPersonAnimator);

			// Rotate the arms towards the crosshair
			_firstPersonAnimator.SetLookAtPosition(_camera.GetFirstPersonTarget() + _camera.transform.forward * 100f);
			_firstPersonAnimator.SetLookAtWeight(weight: 1,
				bodyWeight: 0.6f,
				headWeight: 0.8f,
				eyesWeight: 0f,
				clampWeight: 0.5f);
		}

		private void ClearIK() {
			_firstPersonAnimator.SetLookAtWeight(0);
			_firstPersonAnimator.SetLookAtPosition(Vector3.zero);

			_weaponInfo.ClearIK(_firstPersonAnimator);
		}
	}
}
