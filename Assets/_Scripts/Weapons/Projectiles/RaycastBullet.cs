using AbsoluteCommons.Attributes;
using AbsoluteCommons.Components;
using UnityEngine;

namespace TowerDefense.Weapons.Projectiles {
	[AddComponentMenu("Tower Defense/Projectiles/Raycast Bullet")]
	[RequireComponent(typeof(AutomaticDespawn), typeof(BulletTrailHandler))]
	public class RaycastBullet : MonoBehaviour {
		[SerializeField] private float _range = 100f;
		[SerializeField] private LayerMask _ignoreMask;

		public float Range => _range;

		private BulletTrailHandler _trail;

		[ReadOnly] public Vector3 trailStart;
		[ReadOnly] public Vector3 trailEnd;

		private void Awake() {
			_trail = GetComponent<BulletTrailHandler>();
		}

		private void Start() {
			_trail.CreateTrail(trailStart, trailEnd);
		}
	}
}
