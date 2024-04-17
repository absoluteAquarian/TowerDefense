namespace TowerDefense.Weapons {
	public enum DeployState {
		Holstered,
		Holstering,
		Deploying,
		Deployed
	}

	public static class DeployStateExtensions {
		public static bool IsNotHolstered(this DeployState state) => state != DeployState.Holstered && state != DeployState.Holstering;

		public static bool IsNotDeployed(this DeployState state) => state != DeployState.Deployed && state != DeployState.Deploying;
	}
}
