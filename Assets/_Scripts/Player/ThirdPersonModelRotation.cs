using UnityEngine;

namespace TowerDefense.Player {
	[AddComponentMenu("Player/Third Person Model Rotation")]
	public class ThirdPersonModelRotation : MonoBehaviour {
		private void LateUpdate() {
			// TODO: "current weapon" field or something
			HandleRotation_HoldingAWeapon();
		}

		private void HandleRotation_HoldingAWeapon() {

		}
	}
}
