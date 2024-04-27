using TowerDefense.Networking;
using UnityEngine;

namespace TowerDefense.Player {
	[AddComponentMenu("Player/Third Person Model Rotation")]
	public class ThirdPersonModelRotation : MonoBehaviour {
		private PlayerWeaponInfo _weaponInfo;
		private PlayerNetcode _netcode;

		private Animator _thirdPersonAnimator;

		private bool _forcedLock;
		public bool ForcedLock {
			get => _forcedLock;
			set => _forcedLock = value;
		}

		private void Awake() {
			_weaponInfo = GetComponentInParent<PlayerWeaponInfo>();

			_thirdPersonAnimator = GetComponent<Animator>();

			_netcode = GetComponentInParent<PlayerNetcode>();
		}

		private void OnAnimatorIK(int layerIndex) {
			if (ForcedLock) {
				// Reset the IK
				ClearIK();
				return;
			}

		//	_weaponInfo.HandleThirdPersonIK(_thirdPersonAnimator);

			// Make the player look at the target
			Vector3 target = _netcode.ThirdPersonLookTarget;

			_thirdPersonAnimator.SetLookAtPosition(target);
			_thirdPersonAnimator.SetLookAtWeight(weight: 1,
				bodyWeight: 0.8f,
				headWeight: 0.8f,
				eyesWeight: 0f,
				clampWeight: 0.5f);

			// Set the IK rotation for the right arm to match the player's aim
			// (Left arm will be handled by IK position)
			/*
			Transform rightShoulder = _thirdPersonAnimator.GetBoneTransform(HumanBodyBones.RightShoulder);
			Quaternion rotation = Quaternion.LookRotation(target - rightShoulder.position, Vector3.up);
			rightShoulder.rotation = rotation;
			*/
		}

		private void ClearIK() {
			_thirdPersonAnimator.SetLookAtWeight(0);
			_thirdPersonAnimator.SetLookAtPosition(Vector3.zero);

			/*
			Transform rightShoulder = _thirdPersonAnimator.GetBoneTransform(HumanBodyBones.RightShoulder);
			rightShoulder.rotation = Quaternion.identity;
			*/

			_weaponInfo.ClearIK(_thirdPersonAnimator);
		}
	}
}
