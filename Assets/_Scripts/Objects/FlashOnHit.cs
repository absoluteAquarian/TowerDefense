using AbsoluteCommons.Attributes;
using TowerDefense.Meta;
using UnityEngine;

namespace TowerDefense.Objects {
	[RequireComponent(typeof(DamageableCollider))]
	public class FlashOnHit : MonoBehaviour, IDamageable {
		public Color flashColor = Color.red;
		[SerializeField] private float _flashDuration = 0.1f;
		[SerializeField, ReadOnly] private float _flashTimer;

		private Renderer[] _renderers;
		private Color[] _originalColors;

		private void Awake() {
			// Get all renderers
			_renderers = GetComponentsInChildren<Renderer>();
			_originalColors = new Color[_renderers.Length];
			for (int i = 0; i < _renderers.Length; i++)
				_originalColors[i] = _renderers[i].material.color;
		}

		private void Update() {
			// Flash the object
			if (_flashTimer > 0) {
				_flashTimer -= Time.deltaTime;

				for (int i = 0; i < _renderers.Length; i++)
					_renderers[i].material.color = Color.Lerp(_originalColors[i], flashColor, 1 - (_flashTimer / _flashDuration));
			} else {
				// Restore the original color
				for (int i = 0; i < _renderers.Length; i++)
					_renderers[i].material.color = _originalColors[i];
			}
		}

		public void OnStrike(StruckObjectMeta info) {
			if (info.target != gameObject)
				return;

			// Flash the object
			_flashTimer = _flashDuration;
		}
	}
}
