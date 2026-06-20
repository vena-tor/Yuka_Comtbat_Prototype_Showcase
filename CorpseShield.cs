using UnityEngine;

public class CorpseShield : MonoBehaviour
{
    [Header("Input")]
    public KeyCode pickupKey = KeyCode.E;

    [Header("Pickup")]
    public float pickupRange = 3f;

    [Header("Hold Point")]
    public Transform holdPoint;

    [Header("Corpse Pose")]
    public Transform corpsePoseAnchor;


    public Vector3 autoHoldPointLocalPosition =
        new Vector3(0f, 1.1f, 0.9f);

    public Vector3 corpseLocalPosition = Vector3.zero;

    // 캡슐을 Player 앞에서 가로로 들기 위한 기본값.
    public Vector3 corpseLocalEulerAngles =
        new Vector3(0f, 0f, 90f);

    private CorpseObject currentCorpse;

    public bool IsHoldingCorpse
    {
        get { return currentCorpse != null; }
    }

    private void Awake()
    {
        CreateHoldPointIfMissing();
        FindPoseAnchorIfMissing();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(pickupKey))
        {
            return;
        }

        // 시신이 없으면 집는다.
        if (currentCorpse == null)
        {
            TryPickUpNearestCorpse();
            return;
        }

        // 이미 들고 있으면 다시 E를 눌러 폐기한다.
        ReleaseCorpseShield();
    }
    private void ReleaseCorpseShield()
    {
        if (currentCorpse == null)
        {
            return;
        }

        CorpseObject releasedCorpse = currentCorpse;
        currentCorpse = null;

        Debug.Log("E키 재입력 - 시신 방패 폐기");

        // 현재 축소판에서는 바닥에 다시 놓지 않고 삭제한다.
        releasedCorpse.ConsumeCorpse();
    }

    private void CreateHoldPointIfMissing()
    {
        if (holdPoint != null)
        {
            return;
        }

        GameObject holdPointObject =
            new GameObject("CorpseShieldHoldPoint");

        holdPoint = holdPointObject.transform;
        holdPoint.SetParent(transform, false);
        holdPoint.localPosition = autoHoldPointLocalPosition;
        holdPoint.localRotation = Quaternion.identity;
    }

    private void FindPoseAnchorIfMissing()
    {
        if (corpsePoseAnchor != null)
        {
            return;
        }

        corpsePoseAnchor =
            transform.Find("CorpseShieldPoseAnchor");

        if (corpsePoseAnchor != null)
        {
            return;
        }

        GameObject anchorObject =
            new GameObject("CorpseShieldPoseAnchor");

        corpsePoseAnchor = anchorObject.transform;
        corpsePoseAnchor.SetParent(transform, false);

        corpsePoseAnchor.localPosition =
            Vector3.zero;

        corpsePoseAnchor.localRotation =
            Quaternion.Euler(0f, 90f, 35f);

        Debug.Log(
            "CorpseShieldPoseAnchor가 없어 자동 생성했습니다."
        );
    }

    private void TryPickUpNearestCorpse()
    {
        CorpseObject nearestCorpse = FindNearestCorpse();

        if (nearestCorpse == null)
        {
            Debug.Log("주변에 사용할 수 있는 시신이 없음");
            return;
        }

        bool pickedUp = nearestCorpse.TryPickUp(
            corpsePoseAnchor,
            holdPoint
        );

        if (!pickedUp)
        {
            return;
        }

        currentCorpse = nearestCorpse;

        Debug.Log($"{nearestCorpse.gameObject.name} 시신을 방패로 듦");
    }

    private CorpseObject FindNearestCorpse()
    {
        CorpseObject[] corpses = FindObjectsByType<CorpseObject>();

        CorpseObject nearestCorpse = null;
        float nearestDistance = float.MaxValue;

        foreach (CorpseObject corpse in corpses)
        {
            if (corpse == null || !corpse.IsAvailable)
            {
                continue;
            }

            float distance = Vector3.Distance(
                transform.position,
                corpse.transform.position
            );

            if (distance > pickupRange)
            {
                continue;
            }

            if (distance >= nearestDistance)
            {
                continue;
            }

            nearestDistance = distance;
            nearestCorpse = corpse;
        }

        return nearestCorpse;
    }

    public bool TryBlockAttack()
    {
        if (currentCorpse == null)
        {
            return false;
        }

        CorpseObject consumedCorpse = currentCorpse;
        currentCorpse = null;

        Debug.Log("시신 방패가 Enemy 공격을 대신 받음");

        consumedCorpse.ConsumeCorpse();

        return true;
    }

    public void ConsumeForPlayerAttack()
    {
        if (currentCorpse == null)
        {
            return;
        }

        CorpseObject consumedCorpse = currentCorpse;
        currentCorpse = null;

        Debug.Log("Player 공격 전환 - 시신 방패 기능 상실");

        consumedCorpse.ConsumeCorpse();
    }

    private void LateUpdate()
    {
        if (currentCorpse == null ||
            holdPoint == null ||
            corpsePoseAnchor == null)
        {
            return;
        }

        currentCorpse.AlignToShieldPose(
            corpsePoseAnchor,
            holdPoint
        );
    }
}