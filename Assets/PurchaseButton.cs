using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinjaTools;

public class PurchaseButton : NinjaMonoBehaviour, IButton {
    [SerializeField] InputManager inputManager;
    [SerializeField] Block ammoBlock;
    public List<PurchasePlace> purchasePlaces = new List<PurchasePlace>(3);
    public void OnButtonClick() {
        var ammoPlacesCount = purchasePlaces.Count;
        for (int i = 0; i < ammoPlacesCount; i++) {
            var currentAmmoPlace = purchasePlaces[i];
            if(!currentAmmoPlace.IsOccupied) {
                currentAmmoPlace.PlaceBlock(ammoBlock);
                break;
            }
        }
    }
}
