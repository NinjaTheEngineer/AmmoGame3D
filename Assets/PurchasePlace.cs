using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinjaTools;

public class PurchasePlace : NinjaMonoBehaviour {
    public bool IsOccupied { get; private set; }
    public void PlaceBlock(Block block) {
        IsOccupied = true;
        Block newBlock = Instantiate(block, transform.position, Quaternion.identity);
        newBlock.SetPurchasePlace(this);
    }
    public void FreePurchasePlace() {
        IsOccupied = false;
    }
}
