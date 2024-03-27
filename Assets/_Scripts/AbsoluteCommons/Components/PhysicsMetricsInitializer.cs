using UnityEngine;

namespace AbsoluteCommons.Components {
	[RequireComponent(typeof(PhysicsMetrics), typeof(PhysicsMetricsFinalizer))]
	[DefaultExecutionOrder(-101)]
	public class PhysicsMetricsInitializer : MonoBehaviour {
		internal Vector3 cachedGravity;

		private void Update() {
			// Override the global gravity for components that use it
			PhysicsMetrics metrics = GetComponent<PhysicsMetrics>();
			
			cachedGravity = Physics.gravity;
			if (metrics.useGravityOverride)
				Physics.gravity = metrics.gravityOverride;

			Physics.gravity *= metrics.gravityScale;
		}
	}
}
