using AbsoluteCommons.Utility;
using UnityEngine;

[AddComponentMenu("Player/First Person Model Rotation")]
public class FirstPersonModelRotation : MonoBehaviour {
	private Vector3 modelOffset;

	private void Start() {
		modelOffset = transform.localPosition;
	}

	private void Update() {
		// Get the view rotation from the main camera
		FirstPersonView view = Camera.main.GetComponent<FirstPersonView>();
		Quaternion rotation = Quaternion.Euler(view.ViewRotation);

		Vector3 positionBase = transform.parent.position + rotation * modelOffset;
		Vector3 pivot = view.transform.position + rotation * modelOffset;

		// Set the position and rotation of the model
		transform.position = positionBase;
		transform.RotateWithPivot(pivot, rotation);
	}
}
