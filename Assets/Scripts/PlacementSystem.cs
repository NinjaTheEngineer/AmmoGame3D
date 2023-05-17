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
    public Vector3 PickedUpPosition {get; private set;}
    public Vector3 MousePosition {
        get {
            var logId = "MouseInCellPosition_get";
            Vector3 mousePos = Vector3.zero;
            if(inputManager==null) {
                logw(logId, "InputManager="+inputManager.logf());
                return mousePos;
            }
            mousePos = inputManager.GetMouseWorldPosition();
            return mousePos;
        }
    }
    public Vector3 MouseInCellPosition {
        get {
            var logId = "MouseInCellPosition_get";
            if(inputManager==null) {
                logw(logId, "InputManager="+inputManager.logf());
                return Vector3.zero;
            }
            return inputManager.GetSelectedMapPosition();
        }
    }
    public Vector3Int TargetCellPosition {
        get {
            Vector3Int cellPosition = grid.WorldToCell(MousePosition);
            return cellPosition;
        }
    }
    void Start() {
        var logId = "Start";
        StartCoroutine(MoveGrabbedObjectRoutine());    
    }
    public float smoothness = 5f;
    bool raisingObject = false;
    public float TargetHeight { get; private set;}
    IEnumerator MoveGrabbedObjectRoutine() {
        var logId = "MoveGrabbedObjectRoutine";
        var smallWait = new WaitForSeconds(0.1f);
        logd(logId, "Starting MoveGrabbedObjectRoutine");

        while (true) {
            if (grabbedObject == null) {
                yield return smallWait;
                continue;
            }
            var mousePos = MousePosition;
            var objTransform = grabbedObject.transform;
            var objPosition = grabbedObject.transform.position;

            // Drag the object towards the mouse position
            mousePos.y = objPosition.y;
            var targetPos = Vector3.Lerp(objPosition, mousePos, smoothness * Time.deltaTime);

            if(raisingObject) {
                float step = grabbingSpeed * Time.deltaTime;
                float distanceToTarget = Mathf.Abs(objPosition.y - grabbingHeight);
                targetPos = new Vector3(targetPos.x, objPosition.y + step, targetPos.z);

                // Raise the object towards the grabbing height
                if (distanceToTarget < 0.01f || ((objPosition.y - grabbingHeight) * (grabbingHeight - objPosition.y + step) >= 0)) {
                    raisingObject = false;
                    targetPos = new Vector3(targetPos.x, grabbingHeight, targetPos.z);
                } else {
                    targetPos = new Vector3(targetPos.x, objPosition.y + step, targetPos.z);
                }
            }
            objTransform.position = targetPos;
            yield return null;
        }
    }
    public Vector3 PointedCellPosition { 
        get {
            var logId = "PointedCellPosition_get";
            var mousePos = MousePosition;
            var mouseToCell = grid.WorldToCell(mousePos);
            var pointedCellPos = grid.GetCellCenterWorld(mouseToCell);
            pointedCellPos.y = 0;
            return pointedCellPos;
        }
    }
    void Update() {
        var logId = "Update";
        var mousePos = MousePosition;
        var mouseToCell = grid.WorldToCell(mousePos);
        mouseIndicator.transform.position = mousePos;
        if(grabbedObject==null) {
            cellIndicator.transform.position = PointedCellPosition;
        }

        if(Input.GetMouseButtonDown(0)) {
            OnCellInteraction();
        }
    }
    public void ReleaseGrabbedObject() {
        var logId = "ReleaseGrabbedObject";
        if(grabbedObject==null) {
            logw(logId, "GrabbedObject="+grabbedObject.logf());
            return;
        }
        var isHoveringMap = inputManager.IsHoveringMap;
        var dropPosition = isHoveringMap ? PointedCellPosition : PickedUpPosition;
        logd(logId, "Dropping object="+grabbedObject.logf()+" at ="+dropPosition.logf()+" while IsHoveringMap="+isHoveringMap+" => DragEnd");
        grabbedObject.transform.position = dropPosition;
        grabbedObject.GetComponent<IDraggable>().OnDragEnd();
        grabbedObject = null;
        cellIndicator.transform.position = dropPosition;
        cellIndicator.SetActive(true);
    }
    void OnCellInteraction() {
        var logId = "HandleObjectPlacement";
        if(inputManager==null) {
            logw(logId, "InputManager="+inputManager.logf()+" => no-op");
            return;
        }
        if(grabbedObject!=null) {
            logd(logId, "GrabbedObject="+grabbedObject.logf()+" => ReleasingGrabbedObject");
            ReleaseGrabbedObject();
            return;
        } 
        var grabbableObject = inputManager.GetGrabbableObject();
        if(grabbableObject==null) {
            var isHoveringMap = inputManager.IsHoveringMap;
            if(isHoveringMap) {
                SpawnObject();
            }
            return;
        }
        PickedUpPosition = grabbableObject.transform.position;
        grabbedObject = grabbableObject;
        TargetHeight = grabbingHeight;
        raisingObject = true;
        //StartCoroutine(GrabObjectRoutine(grabbableObject));
        grabbableObject.GetComponent<IDraggable>().OnDragStart();
        cellIndicator.SetActive(false);
    }
    void SpawnObject() {
        var logId = "SpawnObject";
        var pointedCellPos = PointedCellPosition;
        var newObject = Instantiate(placeObject, pointedCellPos, Quaternion.identity);
        logd(logId, "GrabbableObject="+grabbedObject.logf()+" => Instantiating new object="+newObject.logf()+" at pointedCellPos="+pointedCellPos, true);
    }
    [SerializeField] float grabbingHeight = 2f;
    [SerializeField] float grabbingSpeed = 5f;
    IEnumerator GrabObjectRoutine(GameObject obj) {
        var logId = "GrabObjectRoutine";
        logd(logId, "Starting GrabObjectRoutine");
        if(obj==null) {
            logw(logId, "Tried to grab object="+obj.logf()+" => no-op");
            yield break;
        }
        grabbedObject = obj;
        var objTransform = grabbedObject.transform;
        var objPosition = grabbedObject.transform.position;
        float distanceToTarget = Mathf.Abs(objPosition.y - grabbingHeight);
        float step = grabbingSpeed * Time.deltaTime; // Calculate the step using a fixed speed

        while(grabbedObject!=null && distanceToTarget > 0.01f) {
            objPosition = new Vector3(objPosition.x, objPosition.y + step, objPosition.z);
            objTransform.position = objPosition;
            distanceToTarget = Mathf.Abs(objPosition.y - grabbingHeight);

            if((objPosition.y - grabbingHeight) * (grabbingHeight - objPosition.y + step) >= 0) {
                objPosition = new Vector3(objPosition.x, grabbingHeight, objPosition.z);
                objTransform.position = objPosition;
                break;
            }

            yield return null;
        }
        logd(logId, "Ended GrabObjectRoutine");
    }
    public void PlaceObjectInCell(GameObject placedObject) {
        var logId = "PlaceObjectInCell";
        if(placedObject==null) {
            logw(logId, "Tried to place object="+placedObject.logf()+" => no-op");
            return;
        }
        placedObject.transform.position = grid.CellToWorld(TargetCellPosition);
    }
}
