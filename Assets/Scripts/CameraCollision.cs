using UnityEngine;

// Attach to the camera pivot (the transform the camera orbits around).
// The camera should be a child of this pivot.
// This script prevents the camera from clipping through geometry.
public class CameraCollision : MonoBehaviour
{
    public Transform cameraTransform; // the actual Camera transform
    public float defaultDistance = -3f; // camera local Z distance from pivot
    public float minDistance = -0.5f;
    public float sphereRadius = 0.25f;
    public LayerMask collisionLayers = ~0; // layers to collide with
    public float smoothSpeed = 10f;

    Vector3 initialLocalPos;
    float currentDistance;

    void Start()
    {
        if (cameraTransform == null)
        {
            // Find camera child
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraTransform = cam.transform;
        }

        if (cameraTransform != null)
            initialLocalPos = cameraTransform.localPosition;
        else
            Debug.LogWarning("CameraCollision: cameraTransform not assigned and no Camera found in children!");

        currentDistance = defaultDistance;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 pivotPos = transform.position;
        float maxDist = -initialLocalPos.z; // Assuming Z is negative (e.g. -3)

        // Check 5 points: Center, Up, Down, Left, Right
        // This ensures the camera doesn't clip when looking 90 degrees up/down/left/right
        Vector3[] offsets = new Vector3[]
        {
            Vector3.zero,
            Vector3.up * sphereRadius,
            Vector3.down * sphereRadius,
            Vector3.left * sphereRadius,
            Vector3.right * sphereRadius
        };

        float minFraction = 1.0f;
        bool hitAny = false;

        foreach (Vector3 offset in offsets)
        {
            // Calculate target point in world space for this offset
            Vector3 targetLocal = initialLocalPos + offset;
            Vector3 targetWorld = transform.TransformPoint(targetLocal);
            
            Vector3 dir = targetWorld - pivotPos;
            float dist = dir.magnitude;
            
            if (Physics.Raycast(pivotPos, dir.normalized, out RaycastHit hit, dist, collisionLayers))
            {
                hitAny = true;
                float fraction = hit.distance / dist;
                if (fraction < minFraction)
                {
                    minFraction = fraction;
                }
            }
        }

        float targetZ = -maxDist;
        if (hitAny)
        {
            targetZ = -(maxDist * minFraction);
            // Ensure we don't get closer than minDistance (e.g. -0.5)
            // targetZ is negative. We want targetZ <= minDistance.
            targetZ = Mathf.Min(targetZ, minDistance);
        }

        // Smoothly adjust camera Z position
        Vector3 curLocal = cameraTransform.localPosition;
        float newZ = Mathf.Lerp(curLocal.z, targetZ, Time.deltaTime * smoothSpeed);
        cameraTransform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, newZ);
    }
}
