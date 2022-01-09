using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gaurd : MonoBehaviour {

    public static event System.Action OnGaurdHasSpottedPlayer;

    public float speed = 5;
    public float waitTime = .3f;
    public float turnSpeed = 90;
    public float timeToSpotPlayer = .5f;

    public Light spotLight;
    public float viewDistance;
    public LayerMask viewMask;

    float viewAngle;
    float playerVisibleTimer;

    public Transform pathHolder;
    Transform player;
    Color originalSpotLightColour;

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originalSpotLightColour = spotLight.color;

        viewAngle = spotLight.spotAngle;

        Vector3[] wayPoints = new Vector3[pathHolder.childCount];

        for (int i = 0; i < wayPoints.Length; ++i) { 
            wayPoints[i] = pathHolder.GetChild(i).position;
            wayPoints[i] = new Vector3(wayPoints[i].x, transform.position.y, wayPoints[i].z);
        }

        StartCoroutine(FollowPath(wayPoints));
    }

    private void Update() {
        if (CanSeePlayer()) {
            playerVisibleTimer += Time.deltaTime;
        } else {
            playerVisibleTimer -= Time.deltaTime;
        }

        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
        spotLight.color = Color.Lerp(originalSpotLightColour, Color.red, playerVisibleTimer / timeToSpotPlayer);

        if (playerVisibleTimer >= timeToSpotPlayer) {
            if (OnGaurdHasSpottedPlayer != null) {
                OnGaurdHasSpottedPlayer();
            }
        }
    }

    bool CanSeePlayer() {
        if (Vector3.Distance(transform.position, player.position) < viewDistance) {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGaurdAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);

            if (angleBetweenGaurdAndPlayer < viewAngle / 2f) {
                if (!Physics.Linecast(transform.position, player.position, viewMask)) {
                    return true;
                }
            }
        }

        return false;
    }

    IEnumerator FollowPath(Vector3[] wayPoints) {
        transform.position = wayPoints[0];

        int targetWayPointIndex = 1;
        Vector3 targetWayPoint = wayPoints[targetWayPointIndex];
        transform.LookAt(targetWayPoint);

        while (true) {
            transform.position = Vector3.MoveTowards(
               transform.position,
               targetWayPoint,
               speed * Time.deltaTime
            );

            if (transform.position == targetWayPoint) {
                targetWayPointIndex = (targetWayPointIndex + 1) % wayPoints.Length;
                targetWayPoint = wayPoints[targetWayPointIndex];

                yield return new WaitForSeconds(waitTime);

                yield return StartCoroutine(TurnToFace(targetWayPoint));
            }

            yield return null;
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget) {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f) {
            float angle = Mathf.MoveTowardsAngle(
                transform.eulerAngles.y,
                targetAngle,
                turnSpeed * Time.deltaTime
            );

            transform.eulerAngles = Vector3.up * angle;

            yield return null;
        }
    }

    private void OnDrawGizmos() {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder) { 
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);

            previousPosition = waypoint.position;
        }

        Gizmos.DrawLine(startPosition, previousPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }

}
