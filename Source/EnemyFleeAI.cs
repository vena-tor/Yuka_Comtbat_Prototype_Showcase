using UnityEngine;

public class EnemyFleeAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Flee")]
    public float moveSpeed = 3f;
    public float fleeStartDistance = 5f;
    public float stopDistance = 8f;
    public float turnSpeed = 10f;

    private Rigidbody rb;
    private bool isFleeing;
    private float pauseTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("EnemyFleeAI가 붙은 오브젝트에 Rigidbody가 없습니다.");
            return;
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null || target == null)
        {
            return;
        }

        rb.angularVelocity = Vector3.zero;

        if (pauseTimer > 0f)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= fleeStartDistance)
        {
            isFleeing = true;
        }
        else if (distance >= stopDistance)
        {
            isFleeing = false;
        }

        if (isFleeing)
        {
            FleeFromTarget();
        }
        else
        {
            StopMoving();
        }
    }

    private void FleeFromTarget()
    {
        Vector3 fleeDirection = transform.position - target.position;
        fleeDirection.y = 0f;

        if (fleeDirection == Vector3.zero)
        {
            fleeDirection = transform.forward;
        }

        fleeDirection.Normalize();

        rb.linearVelocity = new Vector3(
            fleeDirection.x * moveSpeed,
            rb.linearVelocity.y,
            fleeDirection.z * moveSpeed
        );

        Quaternion targetRotation = Quaternion.LookRotation(fleeDirection);

        Quaternion nextRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(nextRotation);
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    public void PauseMovement(float duration)
    {
        pauseTimer = duration;
    }
}
