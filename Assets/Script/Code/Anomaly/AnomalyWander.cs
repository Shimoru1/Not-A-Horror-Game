using UnityEngine;
using UnityEngine.AI;

public class AnomalyWander : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float waitAtPoint = 1f;

    [Header("Walk Points")]
    public Transform[] walkPoints;

    private NavMeshAgent agent;
    private int currentPointIndex = -1;
    private float waitTimer = 0f;
    private bool waiting = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(
                gameObject.name +
                " ไม่มี NavMeshAgent!"
            );

            enabled = false;
            return;
        }

        agent.speed = walkSpeed;
    }

    private void Start()
    {
        if (walkPoints == null || walkPoints.Length == 0)
        {
            Debug.LogWarning(
                gameObject.name +
                " ยังไม่มี Walk Points"
            );

            return;
        }

        GoToNextPoint();
    }

    private void Update()
    {
        // กัน Error กรณี Object ถูกทำลาย
        if (this == null || gameObject == null)
            return;

        if (agent == null)
            return;

        if (!agent.isActiveAndEnabled)
            return;

        if (!agent.isOnNavMesh)
            return;

        // ถ้ากำลังรออยู่ที่จุด
        if (waiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                waiting = false;
                GoToNextPoint();
            }

            return;
        }

        // ถ้าไม่มีปลายทางที่ถูกต้อง
        if (!agent.hasPath)
        {
            GoToNextPoint();
            return;
        }

        // ถึงจุดหมายแล้ว
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            waiting = true;
            waitTimer = waitAtPoint;
        }
    }

    private void GoToNextPoint()
    {
        if (walkPoints == null || walkPoints.Length == 0)
            return;

        if (agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
            return;

        // หา Walk Point ถัดไปที่ยังมีอยู่จริง
        for (int attempt = 0; attempt < walkPoints.Length; attempt++)
        {
            currentPointIndex++;

            if (currentPointIndex >= walkPoints.Length)
                currentPointIndex = 0;

            Transform target = walkPoints[currentPointIndex];

            // จุดถูกลบไปแล้ว/ไม่มีการอ้างอิง
            if (target == null)
                continue;

            Vector3 targetPosition = target.position;

            agent.SetDestination(targetPosition);
            return;
        }

        Debug.LogWarning(
            gameObject.name +
            " ไม่มี Walk Point ที่ใช้งานได้"
        );
    }

    private void OnDisable()
    {
        // หยุด Agent ก่อน Object ถูกปิด/ทำลาย
        if (agent != null &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }
}
