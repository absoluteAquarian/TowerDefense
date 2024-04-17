using AbsoluteCommons.Objects;
using UnityEngine;

namespace TowerDefense.Weapons.Projectiles {
	public class AutomaticDespawn : MonoBehaviour {
		[SerializeField] private float _despawnTime = 5f;
		[SerializeField] private bool _destroyAfterOneFrame = false;
		private float _timeAlive;

		private PooledObject _pooled;

		public bool DestroyAfterOneFrame {
			get => _destroyAfterOneFrame;
			set => _destroyAfterOneFrame = value;
		}

		private void Awake() {
			_pooled = GetComponent<PooledObject>();
		}

		private void LateUpdate() {
			if (_destroyAfterOneFrame) {
				HandleDestruction();
				return;
			}

			_timeAlive += Time.deltaTime;

			if (_timeAlive >= _despawnTime)
				HandleDestruction();
		}

		private void HandleDestruction() {
			if (_pooled) {
				_pooled.ReturnToPool();

				// Reset the timer since the object could be reused
				_timeAlive = 0f;
			} else
				Destroy(gameObject);
		}
	}
}
