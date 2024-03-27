using UnityEngine;

namespace AbsoluteCommons.Components {
	[RequireComponent(typeof(PhysicsMetricsInitializer), typeof(PhysicsMetrics))]
	[DefaultExecutionOrder(-99)]
	public class PhysicsMetricsFinalizer : MonoBehaviour {
		private void Update() {
			// Restore the global gravity
			PhysicsMetrics metrics = GetComponent<PhysicsMetrics>();
			if (metrics.useGravityOverride)
				Physics.gravity = GetComponent<PhysicsMetricsInitializer>().cachedGravity;
		}
	}
}
