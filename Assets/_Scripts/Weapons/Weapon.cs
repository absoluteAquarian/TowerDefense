using AbsoluteCommons.Objects;
using UnityEngine;

namespace TowerDefense.Weapons {
	[AddComponentMenu("Weapons/Weapon Prefab")]
	public class Weapon : MonoBehaviour {
		public GameObject model;
		public Transform rightHandBone;
	//	public Transform leftHandBone;
		public float deployShowTime = 0f;
		public float holsterHideTime = 1f;
		public float shootAnimationMultiplier = 1f;
		public float shootTime = 1f;
		public bool autoFire = false;
		public float spread = 0f;
		public GameObject projectilePrefab;
		public Transform projectileOrigin;
	}
}
