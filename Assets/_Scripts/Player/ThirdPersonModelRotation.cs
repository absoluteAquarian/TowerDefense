﻿using AbsoluteCommons.Utility;
using TowerDefense.CameraComponents;
using TowerDefense.Weapons;
using UnityEngine;

namespace TowerDefense.Player {
	[AddComponentMenu("Player/Third Person Model Rotation")]
	public class ThirdPersonModelRotation : MonoBehaviour {
		private PlayerWeaponInfo _weaponInfo;
		private CameraFollow _camera;

		private Animator _thirdPersonAnimator;

		public bool ForcedLock { get; set; }

		private void Awake() {
			_weaponInfo = GetComponentInParent<PlayerWeaponInfo>();

			// NOTE: the child path may need to be changed if this script is used in a different project
			_thirdPersonAnimator = GetComponent<Animator>();

			_camera = Camera.main.GetComponent<CameraFollow>();
		}

		private void OnAnimatorIK(int layerIndex) {
			if (ForcedLock) {
				// Reset the IK
				ClearIK();
				return;
			}

			// Make the player look at the target
			const float DISTANCE = 100f;
			Vector3 target;
			if (_camera.CheckCameraRaycast(out var hit, DISTANCE))
				target = hit.point;
			else
				target = _camera.transform.position + _camera.transform.forward * DISTANCE;

			_thirdPersonAnimator.SetLookAtPosition(target);
			_thirdPersonAnimator.SetLookAtWeight(weight: 1,
				bodyWeight: 0.6f,
				headWeight: 0.8f,
				eyesWeight: 0f,
				clampWeight: 0.5f);

			// Set the IK rotation for the right arm to match the player's aim
			// (Left arm will be handled by IK position)
			Transform rightShoulder = _thirdPersonAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			Quaternion rotation = Quaternion.LookRotation(target - rightShoulder.position, Vector3.up);
			rightShoulder.rotation = rotation;
		}

		private void ClearIK() {
			_thirdPersonAnimator.SetLookAtWeight(0);
			_thirdPersonAnimator.SetLookAtPosition(Vector3.zero);

			Transform rightShoulder = _thirdPersonAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			rightShoulder.rotation = Quaternion.identity;
		}
	}
}
