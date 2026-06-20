using UnityEngine;
using System.Collections.Generic;

public class LockOnSystem : MonoBehaviour
{
    [Header("References")]
    public CameraFollow cameraFollow;
    public Transform cameraTransform;

    [Header("Input")]
    public KeyCode lockOnKey = KeyCode.Tab;
    public bool useMiddleMouseButton = true;

    [Header("Target Search")]
    public LayerMask enemyLayer;
    public float lockRange = 12f;
    public float lockMaxAngle = 75f;

    [Header("Indicator")]
    public RectTransform indicatorRect;
    public Camera indicatorCamera;
    public float indicatorHeight = 1.8f;
    public string lockOnTargetPointName = "LockOnTargetPoint";

    private EnemyHealth currentTarget;

    public bool IsLockedOn
    {
        get { return currentTarget != null; }
    }

    public EnemyHealth CurrentTarget
    {
        get { return currentTarget; }
    }

    public Transform CurrentTargetTransform
    {
        get
        {
            if (currentTarget == null)
            {
                return null;
            }

            return currentTarget.transform;
        }
    }

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraFollow == null && Camera.main != null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
        }
        if (indicatorCamera == null)
        {
            indicatorCamera = Camera.main;
        }

        if (indicatorRect != null)
        {
            indicatorRect.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(lockOnKey) || (useMiddleMouseButton && Input.GetMouseButtonDown(2)))
        {
            ToggleLockOn();
        }

        if (currentTarget != null)
        {
            UpdateCurrentTarget();
        }
    }

    private void ToggleLockOn()
    {
        if (currentTarget != null)
        {
            Unlock();
            return;
        }

        EnemyHealth target = FindBestTarget();

        if (target == null)
        {
            Debug.Log("락온 가능한 Enemy가 없습니다.");
            return;
        }

        LockOn(target);
    }

    private EnemyHealth FindBestTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            lockRange,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        HashSet<EnemyHealth> candidates = new HashSet<EnemyHealth>();

        foreach (Collider collider in colliders)
        {
            EnemyHealth enemyHealth = collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth == null)
            {
                continue;
            }

            if (enemyHealth.IsDead)
            {
                continue;
            }

            candidates.Add(enemyHealth);
        }

        EnemyHealth bestTarget = null;
        float bestScore = float.MaxValue;

        Vector3 forward = transform.forward;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
        }

        forward.y = 0f;

        if (forward == Vector3.zero)
        {
            forward = transform.forward;
        }

        forward.Normalize();

        foreach (EnemyHealth candidate in candidates)
        {
            if (candidate == null)
            {
                continue;
            }

            Vector3 toTarget = candidate.transform.position - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;

            if (distance <= 0.001f)
            {
                continue;
            }

            Vector3 direction = toTarget.normalized;
            float angle = Vector3.Angle(forward, direction);

            if (angle > lockMaxAngle)
            {
                continue;
            }

            float score = distance + angle * 0.05f;

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = candidate;
            }
        }

        return bestTarget;
    }

    private void LockOn(EnemyHealth target)
    {
        currentTarget = target;

        if (indicatorRect != null)
        {
            indicatorRect.gameObject.SetActive(true);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetLockTarget(currentTarget.transform);
        }

        Debug.Log($"락온 시작: {currentTarget.gameObject.name}");
    }

    private void Unlock()
    {
        if (currentTarget != null)
        {
            Debug.Log($"락온 해제: {currentTarget.gameObject.name}");
        }

        currentTarget = null;
        
        if (cameraFollow != null)
        {
            cameraFollow.SetLockTarget(null);
        }

        if (indicatorRect != null)
        {
            indicatorRect.gameObject.SetActive(false);
        }
    }

    private void UpdateCurrentTarget()
    {
        if (currentTarget == null || currentTarget.IsDead)
        {
            Unlock();
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distance > lockRange + 3f)
        {
            Unlock();
            return;
        }

        UpdateIndicatorPosition();
    }

    private void UpdateIndicatorPosition()
    {
        if (currentTarget == null ||
            indicatorRect == null ||
            indicatorCamera == null)
        {
            return;
        }

        Vector3 targetPosition =
            currentTarget.transform.position +
            Vector3.up * indicatorHeight;

        Transform targetPoint =
            currentTarget.transform.Find(
                lockOnTargetPointName
            );

        if (targetPoint != null)
        {
            targetPosition = targetPoint.position;
        }

        Vector3 screenPosition =
            indicatorCamera.WorldToScreenPoint(
                targetPosition
            );

        // 카메라 뒤쪽에 대상이 있으면 마커를 숨긴다.
        if (screenPosition.z <= 0f)
        {
            indicatorRect.gameObject.SetActive(false);
            return;
        }

        if (!indicatorRect.gameObject.activeSelf)
        {
            indicatorRect.gameObject.SetActive(true);
        }

        indicatorRect.position = screenPosition;
    }
}