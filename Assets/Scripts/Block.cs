using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinjaTools;

public class Block : NinjaMonoBehaviour, IDraggable {
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Color grabbedColor;
    Color mainColor;
    private void Start() {
        mainColor = meshRenderer.material.color;
    } 
    public void OnDragStart() {
        var logId = "OnPointerEnter";
        logd(logId, name.logf()+" is being pointed => Change color.");
        meshRenderer.material.color = grabbedColor;
    }

    public void OnDragEnd() {
        var logId = "OnPointerExit";
        logd(logId, name.logf()+" on pointer exit => Change color.");
        meshRenderer.material.color = mainColor;
    }
}
