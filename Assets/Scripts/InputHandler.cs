using UnityEngine;
using NinjaTools;

public class InputHandler : NinjaClass {
    private InputManager inputManager;
    private PlacementSystem placementSystem;

    public InputHandler(PlacementSystem placementSystem, InputManager inputManager) {
        this.placementSystem = placementSystem;
        this.inputManager = inputManager;
    }

    public void HandleInput() {
        if (Input.GetMouseButtonDown(0)) {
            OnMouseClick();
        }
    }

    private void OnMouseClick() {
        if (placementSystem.GrabbedBlock != null) {
            placementSystem.ReleaseGrabbedBlock();
            return;
        }

        GameObject grabbableObject = inputManager.GetGrabbableObject();
        if(grabbableObject==null) {
            if (placementSystem.IsHoveringMap) {
                placementSystem.SpawnBlock();
            }
            return;
        } else {
            Block grabbableBlock = grabbableObject.GetComponent<Block>();
            if (grabbableBlock == null) {
                return;
            }
            placementSystem.GrabBlock(grabbableBlock);
        }
    }
}