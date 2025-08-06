using UnityEngine;

public class CameraTargetSmoothing : MonoBehaviour
{
    public Transform targetToFollow;
    public Vector3 offset = new Vector3(0, 2, 0);
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        Vector3 desiredPosition = targetToFollow.position + targetToFollow.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);

        // Optional: folgt auch Rotation, falls du Rotation der Kamera anpassen willst
        transform.rotation = Quaternion.Lerp(transform.rotation, targetToFollow.rotation, Time.deltaTime * smoothSpeed);
    }
}
