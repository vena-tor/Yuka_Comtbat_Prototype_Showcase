using UnityEngine;

public class CorpseObject : MonoBehaviour
{
    [Header("Corpse Shield Visual")]
    public Vector3 heldLocalPosition = new Vector3(0.02f, -0f, 0.10f);
    public Vector3 heldLocalEulerAngles = new Vector3(15f, 90f, -40f);

    private Vector3 originalLossyScale;
    private Vector3 originalLocalScale;

    [Header("Corpse Shield Grip")]
    public Transform corpseGripPoint;
    public bool IsAvailable { get; private set; }
    public bool IsHeld { get; private set; }

    private Rigidbody rb;
    private Collider[] corpseColliders;
    private Canvas[] corpseCanvases;

    private void Awake()
    {
        originalLossyScale = transform.lossyScale;
        originalLocalScale = transform.localScale;
        CacheComponents();
    }

    private void CacheComponents()
    {
        rb = GetComponent<Rigidbody>();

        corpseColliders =
            GetComponentsInChildren<Collider>(true);

        corpseCanvases =
            GetComponentsInChildren<Canvas>(true);

        if (corpseGripPoint == null)
        {
            corpseGripPoint =
                FindChildRecursive(
                    transform,
                    "CorpseGripPoint"
                );
        }
    }

    public void PrepareAsCorpse()
    {
        CacheComponents();

        IsAvailable = true;
        IsHeld = false;

        if (rb != null)
        {
            // 이미 Kinematic이면 속도를 설정할 수 없으므로 건드리지 않는다.
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.isKinematic = true;
        }

        // 죽은 Enemy의 몸통과 EnemyHitBox가
        // 공격/락온/물리 판정에 계속 잡히지 않게 한다.
        SetCollidersEnabled(false);

        // 죽은 뒤 머리 위 HP Bar는 숨긴다.
        foreach (Canvas corpseCanvas in corpseCanvases)
        {
            if (corpseCanvas != null)
            {
                corpseCanvas.enabled = false;
            }
        }

        Debug.Log($"{gameObject.name} 시신 사용 가능");
    }

    public bool TryPickUp(
    Transform poseAnchor,
    Transform gripTarget)
    {
        if (!IsAvailable ||
            IsHeld ||
            poseAnchor == null ||
            gripTarget == null)
        {
            return false;
        }

        IsAvailable = false;
        IsHeld = true;

        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.useGravity = false;
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        SetCollidersEnabled(false);

        AlignToShieldPose(
            poseAnchor,
            gripTarget
        );

        return true;
    }



    public void ConsumeCorpse()
    {
        if (!IsHeld)
        {
            return;
        }

        IsHeld = false;
        IsAvailable = false;

        Debug.Log($"{gameObject.name} 시신 방패 파괴");

        Destroy(gameObject);
    }

    private void SetCollidersEnabled(bool isEnabled)
    {
        foreach (Collider corpseCollider in corpseColliders)
        {
            if (corpseCollider != null)
            {
                corpseCollider.enabled = isEnabled;
            }
        }
    }

    private void ApplyHeldScale(Transform holdPoint)
    {
        Vector3 parentScale = holdPoint.lossyScale;

        float x = Mathf.Approximately(parentScale.x, 0f)
            ? originalLocalScale.x
            : originalLossyScale.x / parentScale.x;

        float y = Mathf.Approximately(parentScale.y, 0f)
            ? originalLocalScale.y
            : originalLossyScale.y / parentScale.y;

        float z = Mathf.Approximately(parentScale.z, 0f)
            ? originalLocalScale.z
            : originalLossyScale.z / parentScale.z;

        transform.localScale = new Vector3(x, y, z);
    }
    public void AlignToShieldPose(
    Transform poseAnchor,
    Transform gripTarget)
    {
        if (poseAnchor == null || gripTarget == null)
        {
            return;
        }

        // 왼손 뼈가 아니라 Player 기준 자세 Anchor 아래에 둔다.
        if (transform.parent != poseAnchor)
        {
            transform.SetParent(poseAnchor, true);
        }

        ApplyHeldScale(poseAnchor);

        // 시신 전체 방향은 PoseAnchor가 결정한다.
        transform.position = poseAnchor.position;
        transform.rotation = poseAnchor.rotation;

        if (corpseGripPoint == null)
        {
            return;
        }

        // 현재 방향에서 Enemy 루트와 GripPoint 사이의 간격을 구한다.
        Vector3 rootToGripOffset =
            corpseGripPoint.position -
            transform.position;

        // GripPoint가 Player 왼손 위치에 정확히 오도록
        // Enemy 루트 전체를 이동시킨다.
        transform.position =
            gripTarget.position -
            rootToGripOffset;
    }
    private Transform FindChildRecursive(
    Transform root,
    string targetName)
    {
        foreach (Transform child in root)
        {
            if (child.name == targetName)
            {
                return child;
            }

            Transform found =
                FindChildRecursive(
                    child,
                    targetName
                );

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}