using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public event System.Action OnReachedEndOfLevel;

    public float moveSpeed = 7;
    public float smoothMoveTime = .1f;
    public float turnSpeed = 8;

    float angle;
    float smoothInputMagnitude;
    float smoothMoveVelocity;
    Vector3 velocity;

    Rigidbody rigidBody;
    bool disabled;

    private void Start() {
        rigidBody = GetComponent<Rigidbody>();

        Gaurd.OnGaurdHasSpottedPlayer += Disable;
    }

    void Update() {
        Vector3 inputDirection = Vector3.zero;

        if (!disabled) {
            inputDirection = new Vector3(
                Input.GetAxisRaw("Horizontal"),
                0,
                Input.GetAxisRaw("Vertical")
            ).normalized;
        }

        float inputMagnitude = inputDirection.magnitude;

        smoothInputMagnitude = Mathf.SmoothDamp(
            smoothInputMagnitude,
            inputMagnitude,
            ref smoothMoveVelocity,
            smoothMoveTime
        );

        float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude);

        velocity = transform.forward * moveSpeed * smoothInputMagnitude;
    }

    private void OnTriggerEnter(Collider hitCollider) {
        if (hitCollider.tag == "Finish") {
            Disable();

            if (OnReachedEndOfLevel != null)
                OnReachedEndOfLevel();
        }

    }

    void Disable() {
        disabled = true;
    }

    private void FixedUpdate() {
        rigidBody.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        rigidBody.MovePosition(rigidBody.position + velocity * Time.fixedDeltaTime);
    }

    private void OnDestroy() {
        Gaurd.OnGaurdHasSpottedPlayer -= Disable;
    }
}
