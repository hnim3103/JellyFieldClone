using UnityEngine;
using JellyGame;
using System;

public class PlacementSystem : MonoBehaviour {
    [SerializeField] private DragSystem dragSystem;
    [SerializeField] private LayerMask cellLayer;

    public event Action<Cell> OnJellyPlaced;

    private void OnEnable() {
        if (dragSystem != null) {
            dragSystem.OnDragEnded += HandleJellyDropped;
        }
    }

    private void OnDisable() {
        if (dragSystem != null) {
            dragSystem.OnDragEnded -= HandleJellyDropped;
        }       
    }

    private void HandleJellyDropped(JellyGroup jelly, Vector3 dropPostition, Vector3 originalPosition) {
        Ray ray = new Ray(dropPostition + Vector3.up * 2f, Vector3.down);

        Cell targetCell = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 10f, cellLayer)) {
            targetCell = hit.collider.GetComponent<Cell>();
        }

        if (targetCell != null && !targetCell.IsOccupied) {
            jelly.IsPlaced = true;
            targetCell.SetJelly(jelly);

            jelly.Squish();

            OnJellyPlaced?.Invoke(targetCell);
        }
        else {
            jelly.transform.position = originalPosition;
            jelly.Squish();
        }
    }
}