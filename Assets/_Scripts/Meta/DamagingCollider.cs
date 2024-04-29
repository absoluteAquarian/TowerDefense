using UnityEngine;

namespace TowerDefense.Meta {
	public class DamagingCollider : MonoBehaviour {
		public float damage;

		// TODO: repeated strikes, not hitting the owner, layer mask ignore
		private void OnCollisionEnter(Collision collision) {
			if (collision.gameObject.TryGetComponent(out DamageableCollider damageable))
				damageable.Strike(damage);
		}
	}
}
