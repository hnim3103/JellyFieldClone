using System;
using UnityEngine;
using JellyGame;

public class DragSystem : MonoBehaviour {
    [SerializeField] private LayerMask jellyLayer;

    [SerializeField] private float liftHeight = 0.6f;
    [SerializeField] private float fingerOffsetY = 0.3f;

    public event Action<JellyGroup> OnDragBegan;
    public event Action<JellyGroup, Vector3> OnDragging;
  
    public event Action<JellyGroup, Vector3, Vector3> OnDragEnded;

    private JellyGroup selectedJelly;
    private Vector3 originalPosition;
    private Camera mainCamera;
    private Plane dragPlane;

    private void Start() {
        mainCamera = Camera.main;
    }

    private void Update() {
        HandleInput();
    }

    private void HandleInput() {
        if (IsInputBegan(out Vector3 inputScreenPos)) {
            Ray ray = mainCamera.ScreenPointToRay(inputScreenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, jellyLayer)) {
                JellyGroup jelly = hit.collider.GetComponentInParent<JellyGroup>();
                if (jelly != null) {
                    selectedJelly = jelly;
                    originalPosition = selectedJelly.transform.position;
                    dragPlane = new Plane(Vector3.up, selectedJelly.transform.position);

                    OnDragBegan?.Invoke(selectedJelly);
                }
            }
        }

        if (IsInputHeld(out Vector3 holdScreenPos) && selectedJelly != null) {
            Ray ray = mainCamera.ScreenPointToRay(holdScreenPos);
            if (dragPlane.Raycast(ray, out float distance)) {
                Vector3 targetPoint = ray.GetPoint(distance);
                Vector3 currentDragPos = new Vector3(
                    targetPoint.x,
                    originalPosition.y + liftHeight,
                    targetPoint.z + fingerOffsetY
                );

                selectedJelly.transform.position = currentDragPos;
                OnDragging?.Invoke(selectedJelly, currentDragPos);
            }
        }

        if (IsInputEnded() && selectedJelly != null) {
            Vector3 dropCheckPosition = selectedJelly.transform.position - new Vector3(0f, liftHeight, fingerOffsetY);
            
            OnDragEnded?.Invoke(selectedJelly, dropCheckPosition, originalPosition);
            
            selectedJelly = null;
        }
    }

    private bool IsInputBegan(out Vector3 position) {
        position = Vector3.zero;
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                position = touch.position;
                return true;
            }
        } else if (Input.GetMouseButtonDown(0)) {
            position = Input.mousePosition;
            return true;
        }

        return false;
    }

    private bool IsInputHeld(out Vector3 position) {
        position = Vector3.zero;
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
                position = touch.position;
                return true;
            }
        } else if (Input.GetMouseButton(0)) {
            position = Input.mousePosition;
            return true;
        }

        return false;
    }

    private bool IsInputEnded() {
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
        }

        return Input.GetMouseButtonUp(0);
    }
}
