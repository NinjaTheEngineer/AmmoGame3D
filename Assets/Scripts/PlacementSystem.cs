using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinjaTools;

public class PlacementSystem : NinjaMonoBehaviour {
    [SerializeField] GameObject mouseIndicator, cellIndicator;
    [SerializeField] InputManager inputManager;
    [SerializeField] Grid grid;
    [SerializeField] Block placeBlock;
    Block grabbedBlock;
    public float smoothness = 5f;
    public float loweringSpeed = 10f;
    [SerializeField] float floorHeight = 0f;
    [SerializeField] float grabbingHeight = 2f;
    [SerializeField] float grabbingSpeed = 5f;
    List<Block> placedBlocks = new List<Block>(49);
    public Vector3 PickedUpPosition { get; private set; }
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
            if(grabbedBlock==null) {
                //There is no grabbedObject or its targetHeight is on the floor.
                yield return smallWait;
                continue;
            }

            Vector3 mousePos = MousePosition;
            Vector3 objPosition = grabbedBlock.transform.position;

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

            grabbedBlock.transform.position = targetPos;
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

        if(grabbedBlock == null) {
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

        if(grabbedBlock!=null) {
            logd(logId, "Already grabbing another object="+grabbedBlock.logf()+" => ReleaseGrabbedObject");
            ReleaseGrabbedBlock();
            return;
        }

        GameObject grabbableObject = inputManager.GetGrabbableObject();
        if(grabbableObject==null) {
            bool isHoveringMap = inputManager.IsHoveringMap;
            if (isHoveringMap) {
                logd(logId, "");
                SpawnBlock();
            }
            return;
        } else {
            logd(logId, "GrabObject="+grabbableObject.logf());
            Block grabbableBlock = grabbableObject.GetComponent<Block>();
            if(grabbableBlock==null) {
                logw(logId, "GrabbableBlock="+grabbableBlock.logf()+" => no-op");
                return;
            }
            GrabBlock(grabbableBlock);
        }
    }
    public void GrabBlock(Block grabbableBlock) {
        var logId = "GrabObject";
        var objectCellPos = grid.WorldToCell(grabbableBlock.transform.position);
        var pickedUpPos = grid.GetCellCenterWorld(objectCellPos);
        pickedUpPos.y = floorHeight;
        PickedUpPosition = pickedUpPos;
        grabbedBlock = grabbableBlock;
        TargetHeight = grabbingHeight;
        grabbableBlock.GetComponent<IDraggable>()?.OnDragStart();
        placedBlocks.Remove(grabbedBlock);
        cellIndicator.SetActive(false);
    }
    public void ReleaseGrabbedBlock() {
        var logId = "ReleaseGrabbedBlock";
        if(grabbedBlock == null) {
            logw(logId,"GrabbedBlock is null => no-op");
            return;
        }
        bool isHoveringMap = inputManager != null && inputManager.IsHoveringMap;
        Vector3 dropPosition = isHoveringMap ? PointedCellPosition : PickedUpPosition;
        var blockAtDropPosition = GetBlockAt(dropPosition);
        var canMerge = blockAtDropPosition?.Id == grabbedBlock.Id;
        logd(logId, "CanMerge="+canMerge+" BlockAtDropPosition="+blockAtDropPosition.logf()+" GrabbedBlock="+grabbedBlock.logf());
        if(blockAtDropPosition==null || canMerge) {
            StartCoroutine(LowerGrabbedBlockRoutine(dropPosition, blockAtDropPosition));
        }
        cellIndicator.transform.position = dropPosition;
        cellIndicator.SetActive(true);
    }

    private Block GetBlockAt(Vector3 position) {
        var logId = "GetBlockAt";
        Block blockAtPos = null;
        for (int i = 0; i < placedBlocks.Count; i++) {
            var currentBlock = placedBlocks[i];
            if(currentBlock==null) {
                continue;
            }
            var currentBlockPosition = currentBlock.transform.position;
            if(currentBlockPosition.x==position.x && currentBlockPosition.z==position.z){
                blockAtPos = currentBlock;
            }
        }
        return blockAtPos;
    }

    IEnumerator LowerGrabbedBlockRoutine(Vector3 dropPosition, Block blockToMerge=null) {
        var logId = "LowerGrabbedObjectRoutine";
        var loweredBlock = grabbedBlock;
        var loweredBlockTransform = loweredBlock.transform;
        grabbedBlock.GetComponent<IDraggable>()?.OnDragEnd();
        grabbedBlock = null;

        while(grabbedBlock==null || grabbedBlock.transform!=loweredBlockTransform) {
            if(loweredBlockTransform==null) {
                logd(logId, "LoweredObjectTransform is null, probably merged => breaking");
                yield break;
            }
            float distanceToTarget = Mathf.Abs((loweredBlockTransform.position - dropPosition).magnitude);
            Vector3 targetPos = Vector3.Lerp(loweredBlockTransform.position, dropPosition, loweringSpeed * Time.deltaTime);
            // Lower the object towards the drop position
            if(distanceToTarget < 0.01f) {
                logd(logId, "DistanceToTarget=" + distanceToTarget + " => breaking");
                break;
            }

            loweredBlockTransform.position = targetPos;
            yield return true;
        }
        if(blockToMerge) {
            placedBlocks.Remove(blockToMerge);
            loweredBlock.MergeWith(blockToMerge);
        }
        loweredBlockTransform.position = dropPosition;
        placedBlocks.Add(loweredBlock);
    }
    void SpawnBlock() {
        var logId = "SpawnBlock";
        Vector3 pointedCellPos = PointedCellPosition;
        Block newBlock = Instantiate(placeBlock, pointedCellPos, Quaternion.identity);
        placedBlocks.Add(newBlock);
        logd(logId, "Spawned block="+newBlock.logf()+" at CellPosition="+pointedCellPos.logf());
    }
    public void PlaceBlockInCell(Block blockToPlace) {
        var logId = "PlaceBlockInCell";
        if(blockToPlace == null) {
            logw(logId, "Tried to place null object");
            return;
        }

        blockToPlace.transform.position = grid.CellToWorld(TargetCellPosition);
    }
}