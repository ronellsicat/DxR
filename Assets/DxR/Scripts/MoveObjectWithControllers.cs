using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;


#nullable enable
public class MoveObjectWithControllers : XRBaseInteractable {
	[Range(0, 1)]
	public float rotationSmoothing = 0.6f;
	public Vector3 enableRotation = Vector3.one;
	public Vector3 enableMoving = Vector3.one;

	RaycastHit initialRaycastHit;
	Quaternion initialRotation, parentInitialRotation;
	XRRayInteractor? activeInteractor;
	bool selected = true;

	void Start() {
		selectEntered.AddListener(MoveOnSelectEntered);
		lastSelectExited.AddListener(StopOnLastSelectExited);

	}

	void MoveOnSelectEntered(SelectEnterEventArgs args) {
		var interactor = args.interactorObject as XRRayInteractor;
		if (selected || interactor == null) {
			return;
		}
		selected = interactor.TryGetCurrent3DRaycastHit(out initialRaycastHit);
		if (selected) {
			activeInteractor = interactor;
			initialRotation = activeInteractor.transform.rotation;
			parentInitialRotation = transform.parent.rotation;
		}
	}

	void StopOnLastSelectExited(SelectExitEventArgs args) {
		selected = false;
		activeInteractor = null;
	}
	/*
	void UpdateState() {
		switch (selections.Count) {
			case 0:
				if (state != TransformInteractableState.NotSelected) undoTransformManager.Push();
				state = TransformInteractableState.NotSelected;
				firstPivot.SetActive(false);
				secondPivot.SetActive(false);
				break;
			case 1:
				state = TransformInteractableState.SelectedOnce;
				firstPivot.SetActive(true);
				secondPivot.SetActive(false);
				var sel = (selections[0] as SelectionInfo)!;
				sel.prevPivotRotation = null;
				sel.prevPivotPosition = null;
				break;
			case 2:
				state = TransformInteractableState.SelectedTwice;
				firstPivot.SetActive(true);
				secondPivot.SetActive(true);
				break;
			default:
				throw new NotImplementedException("Should not be able to select more than twice!");
		}
	}

	void PositionPivot(GameObject pivot, SelectionInfo sel) {
		if (sel.prevPivotPosition.HasValue) {
			sel.prevPivotPosition = pivot.transform.position;
			sel.prevPivotRotation = pivot.transform.rotation;
		}

		pivot.transform.position = sel.interactor.transform.TransformPoint(new Vector3(0, 0, sel.initialRaycastHit.distance));
		pivot.transform.rotation = sel.interactor.transform.rotation;

		// apply rotation smoothing
		pivot.transform.rotation = Quaternion.Lerp(pivot.transform.rotation, (Quaternion)sel.prevPivotRotation!, rotationSmoothing);
	}

	void TransformObject() {
		var p1 = (selections[0] as SelectionInfo)!;
		transformer.position = (Vector3)p1.prevPivotPosition!;
		transformer.rotation = (Quaternion)p1.prevPivotRotation!;
		var prevParent = transform.parent;
		transform.SetParent(transformer, true);

		// Position
		transformer.position = LimitChange(transformer.position, firstPivot.transform.position, enableMoving);

		switch (state) {
			case TransformInteractableState.SelectedOnce:
				// Rotation
				transformer.rotation = LimitRotation(transformer.rotation, firstPivot.transform.rotation, enableRotation);
				break;
			case TransformInteractableState.SelectedTwice:
				var p2 = (selections[1] as SelectionInfo)!;

				// Scale
				var currentDistance = (secondPivot.transform.position - firstPivot.transform.position).magnitude;
				var prevDistance = (p2.prevPivotPosition - p1.prevPivotPosition)!.Value.magnitude;
				transform.localScale = LimitChange(transform.localScale, transform.localScale * currentDistance/prevDistance, enableScaling);

				// Rotation
				var previousRotation = Quaternion.FromToRotation(Vector3.forward, (p1.prevPivotPosition - p2.prevPivotPosition)!.Value);
				var rotation = Quaternion.FromToRotation(Vector3.forward, firstPivot.transform.position - secondPivot.transform.position);
				secondPivot.transform.rotation = rotation;
				transformer.rotation = LimitRotation(transformer.rotation, transformer.rotation * rotation * Quaternion.Inverse(previousRotation), enableRotation2H);
				//transformer.rotation *= rotation * Quaternion.Inverse(previousRotation);


				break;
			default:
				throw new Exception("Invalid state");
		}

		transform.SetParent(prevParent, true);
	}*/

	Vector3 LimitChange(Vector3 from, Vector3 to, Vector3 scaler) {
		var rotate = to - from;
		rotate.Scale(scaler);
		return from + rotate;
	}
	Vector3 LimitChange(Vector3 change, Vector3 scaler) => LimitChange(Vector3.zero, change, scaler);

	Quaternion LimitRotation(Quaternion from, Quaternion to, Vector3 limits) => Quaternion.Euler(LimitChange(from.eulerAngles, to.eulerAngles, limits));

	void Update() {
		if (!selected || activeInteractor == null) return;
		var move = activeInteractor.transform.TransformPoint(new Vector3(0, 0, initialRaycastHit.distance)) - transform.position;
		transform.parent.position += LimitChange(move, enableMoving);
		//Debug.Log($"Move {LimitChange(move, enableMoving)} ({move}) -> {transform.parent.position}");

		//var rotate = activeInteractor.transform.rotation - transform.rotation;
		//transform.parent.rotation = LimitRotation(transform.parent.rotation, firstPivot.transform.rotation, enableRotation);
		var rotate = activeInteractor.transform.rotation * Quaternion.Inverse(initialRotation);
		//transform.parent.rotation = parentInitialRotation * rotate;
		transform.parent.rotation = LimitRotation(transform.parent.rotation, parentInitialRotation * rotate, enableRotation);
		Debug.Log($"Rotate {rotate.eulerAngles}");
	}
}
