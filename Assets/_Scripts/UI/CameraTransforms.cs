using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Debug/Camera-Transforms")]
public class CameraTransforms : MonoBehaviour, ISerializationCallbackReceiver {
	public static bool DisplayCameraLines { get; private set; } = false;

	[SerializeField] private bool showCameraDebugLines;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	private void OnGUI() {
		// Get the "Player" object
		GameObject player = GameObject.Find("Player");

		// Get the "CameraFollow" component of the main camera
		CameraFollow view = Camera.main.GetComponent<CameraFollow>();

		// Get the "FirstPersonView" component of the main camera
		FirstPersonView viewScript = Camera.main.GetComponent<FirstPersonView>();

		GUILayout.BeginArea(new Rect(10, 100, 300, 400));

		// Print the rotations of the "Player" and "View" objects
		GUILayout.Label("Player: ");
		GUILayout.Label("  Pos: " + FormatVector(player.transform.position));
		GUILayout.Label("  For: " + FormatVector(player.transform.forward));
		GUILayout.Label("  Rot: " + FormatVector(player.transform.rotation.eulerAngles));
		GUILayout.Label("View: ");
		GUILayout.Label("  Pos: " + FormatVector(view.transform.position));
		GUILayout.Label("  For: " + FormatVector(view.transform.forward));
		GUILayout.Label("  Rot: " + FormatVector(view.transform.rotation.eulerAngles));
		GUILayout.Label("Control: ");
		GUILayout.Label("  Locked: " + viewScript.IsLocked);
		GUILayout.Label("  Rot: " + FormatVector(viewScript.ViewRotation));

		GUILayout.EndArea();
	}

	private static string FormatVector(Vector3 vector) {
		return $"({Format(vector.x)}, {Format(vector.y)}, {Format(vector.z)})";
	}

	private static string Format(float value) {
		return $"{value:+0.000;-0.000}";
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize() {
		DisplayCameraLines = showCameraDebugLines;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() {
		// showCameraDebugLines = DisplayCameraLines;
	}
}
