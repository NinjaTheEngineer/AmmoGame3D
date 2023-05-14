using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinjaTools;

public class PlacementSystem : NinjaMonoBehaviour {
    [SerializeField] GameObject mouseIndicator, cellIndicator;
    [SerializeField] InputManager inputManager;
    [SerializeField] Grid grid;
    [SerializeField] GameObject placeObject;
    GameObject draggingObject;
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

    void Update() {
        var logId = "Update";
        var mousePos = MousePosition;
        var mouseToCell = grid.WorldToCell(mousePos);
        mouseToCell.y = 0;
        var targetCellPos = grid.GetCellCenterWorld(mouseToCell);
        targetCellPos.y = 0;
        mouseIndicator.transform.position = mousePos;
        if(draggingObject==null) {
            cellIndicator.transform.position = targetCellPos;
        } else {
            draggingObject.transform.position = mousePos;
        }

        if(Input.GetMouseButtonDown(0)) {
            OnCellInteraction(targetCellPos);
        }
    }
    void OnCellInteraction(Vector3 targetCellPos) {
        var logId = "HandleObjectPlacement";
        if(draggingObject!=null) {
            logd(logId, "Placing DraggingObject="+draggingObject.logf());
            draggingObject.transform.position = targetCellPos;
            draggingObject.GetComponent<IDraggable>().OnDragEnd();
            draggingObject = null;
            cellIndicator.SetActive(true);
            return;
        } 
        draggingObject = inputManager?.GetGrabbableObject();
        if(draggingObject==null) {
            SpawnObject(targetCellPos);
            return;
        }
        draggingObject.GetComponent<IDraggable>().OnDragStart();
        cellIndicator.SetActive(false);
    }
    void SpawnObject(Vector3 targetCellPos) {
        var logId = "SpawnObject";
        var newObject = Instantiate(placeObject, targetCellPos, Quaternion.identity);
        logd(logId, "GrabbableObject="+draggingObject.logf()+" => Instantiating new object="+newObject.logf()+" at cellPos="+targetCellPos, true);

    }
    void PickupObject() {
        
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
