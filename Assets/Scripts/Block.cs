using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinjaTools;
using TMPro;

public class Block : NinjaMonoBehaviour, IDraggable, IMergeable {
    public Vector3 LastPosition { get; set; }
    public bool AtPurchase { get; private set; }
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Color grabbedColor;
    Color mainColor;
    [SerializeField] float draggingHeight = 2f;
    [SerializeField] float speed = 2f;
    [SerializeField] TextMeshProUGUI idText;
    [SerializeField] long _id = 1;
    private PurchasePlace initPurchasePlace;
    public void SetPurchasePlace(PurchasePlace purchasePlace) {
        initPurchasePlace = purchasePlace;
    }
    public long Id {
        get => _id;
        set {
            _id = value;
            idText.text = _id.ToString("F0");
        } 
    }
    void Start() {
        mainColor = meshRenderer.material.color;
        AtPurchase = true;
    } 
    public void OnDragStart() {
        var logId = "OnDragStart";
        logd(logId, name.logf()+" is drag start => Change color.");
        meshRenderer.material.color = grabbedColor;
    }

    public void OnDragEnd() {
        var logId = "OnDragEnd";
        logd(logId, name.logf()+" on drag end => Change color.");
        meshRenderer.material.color = mainColor;
    }
    public void OnDragSuccess() {
        LastPosition = transform.position;
        AtPurchase = false;
        initPurchasePlace?.FreePurchasePlace();
        initPurchasePlace = null;
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
    public void OnMerge() {
        var logId = "OnMerge";
        logd(logId, "Block merged with new Id="+Id+" => destroying object");
        Destroy(gameObject);
    }

    public void MergeWith(IMergeable mergeable) {
        var logId = "MergeWith";
        if(mergeable==null) {
            logw(logId, "Tried to merge "+this.name+" with non-Mergeable object="+mergeable.logf()+" => no-op");
            return;
        }
        logd(logId, "Merging "+this.name+" to "+mergeable.logf());
        Id += Id;
        mergeable.OnMerge();
        mainColor = Random.ColorHSV();
        meshRenderer.material.color = mainColor;
    }
    public override string ToString() => name+" Pos="+transform.position.x+","+transform.position.z+" Id="+Id;
}
