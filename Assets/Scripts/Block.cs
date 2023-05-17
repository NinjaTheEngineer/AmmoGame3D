using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinjaTools;

public class Block : NinjaMonoBehaviour, IDraggable {
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Color grabbedColor;
    Color mainColor;
    [SerializeField] float draggingHeight = 2f;
    [SerializeField] float speed = 2f;
    private void Start() {
        mainColor = meshRenderer.material.color;
    } 
    public void OnDragStart() {
        var logId = "OnPointerEnter";
        logd(logId, name.logf()+" is being pointed => Change color.");
        meshRenderer.material.color = grabbedColor;
        //StartCoroutine(AnimatePositionRoutine());
    }

    public void OnDragEnd() {
        var logId = "OnPointerExit";
        logd(logId, name.logf()+" on pointer exit => Change color.");
        meshRenderer.material.color = mainColor;
    }
    IEnumerator AnimatePositionRoutine() {
        var logId = "AnimatePositionRoutine";
        logd(logId, "Started AnimatePositionRoutine");
        while(Mathf.Abs(transform.position.y - draggingHeight) > 0.01f) {
            var newPos = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, draggingHeight, Time.deltaTime*speed), transform.position.z);
            transform.position = newPos;
            yield return true;
        }
        logd(logId, "Ended AnimatePositionRoutine");
    }
}
