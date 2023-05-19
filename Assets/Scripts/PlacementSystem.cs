using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinjaTools;

public class PlacementSystem : NinjaMonoBehaviour {
    [SerializeField] GameObject mouseIndicator, cellIndicator;
    [SerializeField] InputManager inputManager;
    [SerializeField] Grid grid;
    [SerializeField] GameObject placeObject;
    GameObject grabbedObject;
    public Vector3 PickedUpPosition { get; private set; }
    public float smoothness = 5f;
    public float loweringSpeed = 10f;
    [SerializeField] float floorHeight = 0f;
    public float TargetHeight { get; private set; }
    public Vector3 MousePosition {
        get {
            var logId = "MousePosition_get";
            if (inputManager == null) {
                logw(logId, "InputManager is null => Returning Vector3.zero");
                return Vector3.zero;
            }
            return inputManager.GetMouseWorldPosition();
        }
    }
    public Vector3 MouseInCellPosition {
        get {
            var logId = "MouseInCellPosition_get";
            if (inputManager == null) {
                logw(logId, "InputManager is null => Returning Vector3.zero");
                return Vector3.zero;
            }
            return inputManager.GetSelectedMapPosition();
        }
    }
    public Vector3Int TargetCellPosition => grid.WorldToCell(MousePosition);

    void Start() {
        StartCoroutine(MoveGrabbedObjectRoutine());
    }

    IEnumerator MoveGrabbedObjectRoutine() {
        var logId = "MoveGrabbedObjectRoutine";
        logd(logId, "Starting MoveGrabbedObjectRoutine.");
        WaitForSeconds smallWait = new WaitForSeconds(0.1f);
        while(true) {
            if(grabbedObject==null) {
                //There is no grabbedObject or its targetHeight is on the floor.
                yield return smallWait;
                continue;
            }

            Vector3 mousePos = MousePosition;
            Vector3 objPosition = grabbedObject.transform.position;

            // Drag the object towards the mouse position
            var targetHeight = TargetHeight;
            mousePos.y = targetHeight;
            Vector3 targetPos = Vector3.Lerp(objPosition, mousePos, smoothness * Time.deltaTime);

            if(!Mathf.Approximately(objPosition.y, targetHeight)) {
                float step = grabbingSpeed * Time.deltaTime;
                float distanceToTarget = Mathf.Abs(objPosition.y - targetHeight);

                // Raise the object towards the grabbing height
                if (distanceToTarget < 0.01f || Mathf.Sign(objPosition.y - targetHeight) == Mathf.Sign(targetHeight - objPosition.y + step)) {
                    targetPos.y = targetHeight;
                } else {
                    targetPos.y = objPosition.y + step;
                }
            }

            grabbedObject.transform.position = targetPos;
            yield return null;
        }
    }

    public Vector3 PointedCellPosition {
        get {
            Vector3 mousePos = MousePosition;
            Vector3Int mouseToCell = grid.WorldToCell(mousePos);
            Vector3 pointedCellPos = grid.GetCellCenterWorld(mouseToCell);
            pointedCellPos.y = floorHeight;
            return pointedCellPos;
        }
    }

    void Update() {
        Vector3 mousePos = MousePosition;
        Vector3Int mouseToCell = grid.WorldToCell(mousePos);
        mouseIndicator.transform.position = mousePos;

        if(grabbedObject == null) {
            cellIndicator.transform.position = PointedCellPosition;
        }

        if(Input.GetMouseButtonDown(0)) {
            OnMouseClick();
        }
    }
    void OnMouseClick() {
        var logId = "OnMouseClick";
        if(inputManager==null) {
            logw(logId, "InputManager is null => no-op");
            return;
        }

        if(grabbedObject!=null) {
            logd(logId, "Already grabbing another object="+grabbedObject.logf()+" => ReleaseGrabbedObject");
            ReleaseGrabbedObject();
            return;
        }

        GameObject grabbableObject = inputManager.GetGrabbableObject();
        if(grabbableObject==null) {
            bool isHoveringMap = inputManager.IsHoveringMap;
            if (isHoveringMap) {
                logd(logId, "");
                SpawnObject();
            }
            return;
        }
        var objectCellPos = grid.WorldToCell(grabbableObject.transform.position);
        var pickedUpPos = grid.GetCellCenterWorld(objectCellPos);
        pickedUpPos.y = floorHeight;
        PickedUpPosition = pickedUpPos;
        grabbedObject = grabbableObject;
        TargetHeight = grabbingHeight;
        grabbableObject.GetComponent<IDraggable>()?.OnDragStart();
        cellIndicator.SetActive(false);
    }
    public void ReleaseGrabbedObject() {
        var logId = "ReleaseGrabbedObject";
        if(grabbedObject == null) {
            logw(logId,"GrabbedObject is null => no-op");
            return;
        }

        bool isHoveringMap = inputManager != null && inputManager.IsHoveringMap;
        Vector3 dropPosition = isHoveringMap ? PointedCellPosition : PickedUpPosition;
        StartCoroutine(LowerGrabbedObjectRoutine(dropPosition));
        cellIndicator.transform.position = dropPosition;
        cellIndicator.SetActive(true);
    }

IEnumerator LowerGrabbedObjectRoutine(Vector3 dropPosition) {
    var logId = "LowerGrabbedObjectRoutine";
    Transform objTransform = grabbedObject.transform;
    grabbedObject.GetComponent<IDraggable>()?.OnDragEnd();
    grabbedObject = null;

    while(grabbedObject==null || grabbedObject.transform!=objTransform) {
        float distanceToTarget = Mathf.Abs((objTransform.position - dropPosition).magnitude);
        Vector3 targetPos = Vector3.Lerp(objTransform.position, dropPosition, loweringSpeed * Time.deltaTime);
        // Lower the object towards the drop position
        if(distanceToTarget < 0.01f) {
            logd(logId, "DistanceToTarget=" + distanceToTarget + " => breaking");
            break;
        }

        objTransform.position = targetPos;
        yield return true;
    }
    objTransform.position = dropPosition;
}

    void SpawnObject() {
        var logId = "SpawnObject";
        Vector3 pointedCellPos = PointedCellPosition;
        GameObject newObject = Instantiate(placeObject, pointedCellPos, Quaternion.identity);
        logd(logId, "Spawned object="+newObject.logf()+" at CellPosition="+pointedCellPos.logf());
    }

    [SerializeField] float grabbingHeight = 2f;
    [SerializeField] float grabbingSpeed = 5f;

    IEnumerator GrabObjectRoutine(GameObject obj) {
        if (obj == null)
        {
            Debug.LogWarning("Tried to grab null object");
            yield break;
        }

        grabbedObject = obj;
        Transform objTransform = grabbedObject.transform;
        Vector3 objPosition = grabbedObject.transform.position;
        float distanceToTarget = Mathf.Abs(objPosition.y - grabbingHeight);
        float step = grabbingSpeed * Time.deltaTime;

        while (grabbedObject != null && distanceToTarget > 0.01f)
        {
            objPosition = new Vector3(objPosition.x, objPosition.y + step, objPosition.z);
            objTransform.position = objPosition;
            distanceToTarget = Mathf.Abs(objPosition.y - grabbingHeight);

            if (Mathf.Sign(objPosition.y - grabbingHeight) == Mathf.Sign(grabbingHeight - objPosition.y + step))
            {
                objPosition = new Vector3(objPosition.x, grabbingHeight, objPosition.z);
                objTransform.position = objPosition;
                break;
            }

            yield return null;
        }
    }

    public void PlaceObjectInCell(GameObject placedObject)
    {
        if (placedObject == null)
        {
            Debug.LogWarning("Tried to place null object");
            return;
        }

        placedObject.transform.position = grid.CellToWorld(TargetCellPosition);
    }
}