using UnityEngine;

namespace TowerDefense.Weapons {
	[AddComponentMenu("Weapons/Weapon Prefab")]
	public class Weapon : MonoBehaviour {
		public GameObject model;
		public GameObject rightHandBone;
		public GameObject leftHandBone;
		public float deployShowTime = 0f;
		public float holsterHideTime = 1f;
	}
}
