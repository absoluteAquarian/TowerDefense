using UnityEngine;
using UnityEngine.Rendering;

namespace AbsoluteCommons.Components {
	[AddComponentMenu("Absolute Commons/Rendering/Render In First Person")]
	public class RenderInFirstPerson : MonoBehaviour {
		public bool renderInFirstPerson = true;
		public bool showShadowsInFirstPerson = true;

		private void Update() {
			bool inFirstPerson = Camera.main.GetComponent<CameraFollow>().FirstPersonRenderingMode;

			foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>()) {
				if (renderInFirstPerson == inFirstPerson) {
					renderer.enabled = true;
					renderer.shadowCastingMode = showShadowsInFirstPerson ? ShadowCastingMode.On : ShadowCastingMode.Off;
				} else {
					renderer.enabled = showShadowsInFirstPerson;
					renderer.shadowCastingMode = showShadowsInFirstPerson ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.Off;
				}
			}
		}
	}
}
