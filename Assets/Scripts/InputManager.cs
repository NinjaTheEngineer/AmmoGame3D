using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinjaTools;

public class InputManager : NinjaMonoBehaviour {
    [SerializeField] Camera sceneCamera;
    Vector3 lastPosition;
    [SerializeField] LayerMask placementLayerMask;
    [SerializeField] LayerMask grabbableLayerMask;
    [SerializeField] LayerMask groundLayerMask;
    public Vector3 MousePosition {
        get {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = sceneCamera.nearClipPlane;
            return mousePos;
        }
    }
    void Awake() {
        sceneCamera = sceneCamera??Camera.current;
    }
    public bool IsHoveringMap { get; private set;}
    public Vector3 GetSelectedMapPosition() {
        Ray ray = sceneCamera.ScreenPointToRay(MousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100, placementLayerMask)) {
            lastPosition = hit.point;
            IsHoveringMap = true;
        } else {
            IsHoveringMap = false;
        }
        return lastPosition;
    }
    Vector3 mouseWorldPosition = Vector3.zero;
    public Vector3 GetMouseWorldPosition() {
        Ray ray = sceneCamera.ScreenPointToRay(MousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100, groundLayerMask)) {
            mouseWorldPosition = hit.point;
        }
        return mouseWorldPosition;
    }
    public GameObject GetGrabbableObject() {
        var logId = "GetGrabbableObject";
        Ray ray = sceneCamera.ScreenPointToRay(MousePosition);
        RaycastHit hit;
        GameObject grabbableObject = null;
        if(Physics.Raycast(ray, out hit, 100, grabbableLayerMask)) {
            var hitGameObject = hit.collider.gameObject;
            if(hitGameObject.GetComponent<IDraggable>()!=null) {
                grabbableObject = hitGameObject;
            }
            logd(logId, "Ray="+ray.logf()+" HitGameObject="+hitGameObject.logf()+" GrabbableObject="+grabbableObject.logf(), true);
        }
        return grabbableObject;
    }
}
