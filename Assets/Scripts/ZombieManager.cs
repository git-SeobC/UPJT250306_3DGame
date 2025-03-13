using Unity.VisualScripting;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public int hp = 100;
    private Animator animator;
    private float dieAnimationLength;

    public EZombieState currentState = EZombieState.Idle;
    public Transform target;
    public float attackRange = 1.0f;            // 공격 범위
    public float attackDelay = 2.0f;            // 공격 딜레이
    private float nextAttackTime = 0.0f;        // 다음 공격 시간 관리
    public Transform[] patrolPoints;            // 순찰 경로 지점들
    private int currentPoint = 0;             // 현재 순찰 경로 지점 인덱스
    public float moveSpeed = 2.0f;              // 이동속도
    private float trackingRange = 7.0f;         // 추적 범위 설정
    private bool isAttack = false;              // 공격 상태
    private float evadeRange = 5.0f;            // 도망 상태 회피 거리
    private float distanceToTarget;             // Target과의 거리 계산 값
    private bool isWaiting = false;             // 상태 전환 후 대기 상태 여부
    public float idleTime = 2.0f;               // 각 상태 전환 후 대기 시간
    private float zombieHp = 10.0f;
    private bool isDie = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        // Die 애니메이션 클립의 길이를 가져옵니다.
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Z_FallingBack")
            {
                dieAnimationLength = clip.length;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < trackingRange && !isDie) // 추적 범위 내에 들어오면 추적 시작
        {
            Debug.Log($"Zombies start tracking");
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(target);
        }
        else if (distanceToTarget < attackRange && !isDie)
        {
            isAttack = true;
            Debug.Log("Player Attack");
        }
        else
        {
            if (patrolPoints.Length > 0)
            {
                Debug.Log("zomie Patrol");
                Transform targetPoint = patrolPoints[currentPoint];
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
                transform.LookAt(targetPoint.position);

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }
            }
        }


    }

    public void TakeDamage(int pDamaged)
    {
        hp -= pDamaged;
        if (hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        isDie = true;
        animator.SetTrigger("Die");
        Destroy(gameObject, dieAnimationLength);
    }
}

public enum EZombieState
{
    Patrol, //
    Chase,
    Attack,
    Evade,
    Damage,
    Idle,
    Die
}
