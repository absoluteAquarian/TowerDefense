using Unity.Netcode.Components;
using UnityEngine;

namespace TowerDefense.Networking {
	[AddComponentMenu("Tower Defense/Networking/Client Authoritative Animator")]
	[RequireComponent(typeof(Animator))]
	public class ClientAuthoritativeAnimator : NetworkAnimator {
		protected override bool OnIsServerAuthoritative() => false;
	}
}
