using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinjaTools;
using System;

public class InputManager : NinjaMonoBehaviour {
    [SerializeField] Camera sceneCamera;
    [SerializeField] LayerMask placementLayerMask;
    [SerializeField] LayerMask grabbableLayerMask;
    [SerializeField] LayerMask interactableLayerMask;
    [SerializeField] LayerMask groundLayerMask;

    public Vector3 MousePosition {
        get {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = sceneCamera.nearClipPlane;
            return mousePos;
        }
    }
    public bool IsHoveringMap => CheckIfMouseIsHoveringMap();
    public Vector3 GetSelectedMapPosition() => GetMouseHitPosition(placementLayerMask);
    public Vector3 GetMouseWorldPosition() => GetMouseHitPosition(groundLayerMask);
    public GameObject GetGrabbableObject() => GetMouseHitObject(grabbableLayerMask, typeof(IDraggable));
    private void Update() {
        HandleInteractableClick();
    }
    
    private bool CheckIfMouseIsHoveringMap() {
        Ray ray = sceneCamera.ScreenPointToRay(MousePosition);
        return Physics.Raycast(ray, 100, placementLayerMask);
    }

    private Vector3 GetMouseHitPosition(LayerMask layerMask) {
        Ray ray = sceneCamera.ScreenPointToRay(MousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, layerMask))
            return hit.point;

        return Vector3.zero;
    }

    private GameObject GetMouseHitObject(LayerMask layerMask, Type componentType) {
        Ray ray = sceneCamera.ScreenPointToRay(MousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, layerMask)) {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.GetComponent(componentType) != null)
                return hitObject;
        }

        return null;
    }

    private void HandleInteractableClick() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = sceneCamera.ScreenPointToRay(MousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, interactableLayerMask)) {
                var interactableObject = hit.collider.gameObject.GetComponent<IButton>();
                interactableObject?.OnButtonClick();
            }
        }
    }
}
