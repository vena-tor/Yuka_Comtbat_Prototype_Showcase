using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float distance = 6f;
    public float height = 2f;
    public float mouseSensitivity = 2f;

    public float minPitch = -20f;
    public float maxPitch = 60f;

    private float yaw;
    private float pitch = 20f;

    [Header("Shake")]
    private float shakeTimer;
    private float shakeDuration;
    private float shakeStrength;

    [Header("Lock On")]
    public Transform lockTarget;
    public bool isLockOnActive;

    [Tooltip("락온 대상 기준으로 카메라가 좌우로 얼마나 벗어날 수 있는지")]
    public float lockOnMaxYawOffset = 55f;

    [Tooltip("락온 대상 기준으로 카메라가 위아래로 얼마나 벗어날 수 있는지")]
    public float lockOnMaxPitchOffset = 30f;

    [Tooltip("락온 대상의 기준 높이. 보통 가슴/몸통 높이")]
    public float lockOnTargetHeight = 1.2f;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = 20f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (isLockOnActive && lockTarget != null)
        {
            ApplySoftLockOnCameraLimit();
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 focusPoint = target.position + Vector3.up * height;
        Vector3 cameraOffset = rotation * new Vector3(0f, 0f, -distance);
        Vector3 finalPosition = focusPoint + cameraOffset;

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            Vector3 shakeOffset = Random.insideUnitSphere * shakeStrength;
            shakeOffset.y *= 0.5f;

            finalPosition += shakeOffset;
        }

        transform.position = finalPosition;
        transform.LookAt(focusPoint);
    }

    private void ApplySoftLockOnCameraLimit()
    {
        Vector3 focusPoint = target.position + Vector3.up * height;
        Vector3 lockPoint = lockTarget.position + Vector3.up * lockOnTargetHeight;

        Vector3 directionToTarget = lockPoint - focusPoint;

        if (directionToTarget.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetLookRotation = Quaternion.LookRotation(directionToTarget.normalized);
        Vector3 targetAngles = targetLookRotation.eulerAngles;

        float targetYaw = targetAngles.y;
        float targetPitch = NormalizeAngle(targetAngles.x);

        float yawOffset = Mathf.DeltaAngle(targetYaw, yaw);

        if (Mathf.Abs(yawOffset) > lockOnMaxYawOffset)
        {
            float clampedYawOffset = Mathf.Clamp(
                yawOffset,
                -lockOnMaxYawOffset,
                lockOnMaxYawOffset
            );

            yaw = targetYaw + clampedYawOffset;
        }

        float pitchOffset = pitch - targetPitch;

        if (Mathf.Abs(pitchOffset) > lockOnMaxPitchOffset)
        {
            float clampedPitchOffset = Mathf.Clamp(
                pitchOffset,
                -lockOnMaxPitchOffset,
                lockOnMaxPitchOffset
            );

            pitch = targetPitch + clampedPitchOffset;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }

    public void Shake(float duration, float strength)
    {
        shakeDuration = duration;
        shakeTimer = duration;
        shakeStrength = strength;
    }

    public void SetLockTarget(Transform target)
    {
        lockTarget = target;
        isLockOnActive = lockTarget != null;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }
}