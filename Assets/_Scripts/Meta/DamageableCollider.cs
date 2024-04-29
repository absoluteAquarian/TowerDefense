using AbsoluteCommons.Attributes;
using AbsoluteCommons.Utility;
using Unity.Netcode;
using UnityEngine;

namespace TowerDefense.Meta {
	public class DamageableCollider : NetworkBehaviour, IDamageable {
		public float maximumHealth;

		private NetworkVariable<float> _currentHealth;

#if UNITY_EDITOR
		[SerializeField, ReadOnly] private float currentHealth;
#endif

		private void Awake() {
			_currentHealth = new NetworkVariable<float>(maximumHealth, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
		}

		public void Strike(float damage) {
			Debug.Log($"Requesting strike on {gameObject.name} for {damage} damage.");

			TakeDamageServerRpc(damage);
		}

		[ServerRpc(RequireOwnership = false)]
		private void TakeDamageServerRpc(float damage) {
			_currentHealth.Value -= damage;

			SendMessage(nameof(IDamageable.OnStrike), new StruckObjectMeta(gameObject, gameObject, damage, _currentHealth.Value), SendMessageOptions.DontRequireReceiver);

			Debug.Log($"{gameObject.name} took {damage} damage and has {_currentHealth.Value} health remaining.");

			if (_currentHealth.Value <= 0) {
				GameObject self = gameObject;
				TypeExtensions.DestroyDespawnOrReturnToPoolAndSetNull(ref self);
			}
		}

		private void LateUpdate() {
			#if UNITY_EDITOR
			currentHealth = _currentHealth.Value;
			#endif
		}

		public void OnStrike(StruckObjectMeta info) { }
	}

	public class StruckObjectMeta {
		public readonly GameObject actor;
		public readonly GameObject target;
		public readonly float damage;
		public readonly float remainingHealth;

		public StruckObjectMeta(GameObject actor, GameObject target, float damage, float remainingHealth) {
			this.actor = actor;
			this.target = target;
			this.damage = damage;
			this.remainingHealth = remainingHealth;
		}
	}

	public interface IDamageable {
		void OnStrike(StruckObjectMeta info);
	}
}
