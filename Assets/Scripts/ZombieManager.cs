using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PostProcessing;

public enum EZombieState
{
    Idle,       // 대기
    Patrol,     // 순찰
    Chase,      // 추적
    Attack,     // 공격
    Evade,      // 도망
    Damage,     // 피격
    Die,        // 죽음
    Error       // 버그 있을 때
}

public class ZombieManager : MonoBehaviour
{
    public float hp = 100f;
    private Animator animator;
    //private float dieAnimationLength;

    // 사운드 클립
    private AudioSource audioSource;
    public AudioClip audioClipIdle;
    public AudioClip audioClipHit;
    public AudioClip audioClipDie;

    public EZombieState currentState = EZombieState.Idle;
    public Transform target;
    public float attackRange = 1.0f;         // 공격 범위
    public float attackDelay = 2.0f;         // 공격 딜레이
    private float nextAttackTime = 0.0f;     // 다음 공격 시간 관리
    public Transform[] patrolPoints;         // 순찰 경로 지점들
    private int currentPoint = 0;            // 현재 순찰 경로 지점 인덱스
    public float moveSpeed = 2.0f;           // 이동속도
    public float trackingRange = 7.0f;       // 추적 범위 설정
    private float tempTrackingRange;         // 기존 추적 범위
    private float evadeRange = 5.0f;         // 도망 상태 회피 거리
    private float distanceToTarget;          // Target과의 거리 계산 값
    public float idleTime = 2.0f;            // 각 상태 전환 후 대기 시간
    private float zombieHp = 10.0f;
    private Coroutine stateRoutine;          // 진행중인 상태 코루틴

    // 상태 Boolean
    //private bool isAttack = false;           // 공격 상태
    //private bool isWaiting = false;          // 상태 전환 후 대기 상태 여부
    //private bool isDie = false;              // 죽은 상태
    //private bool isPatrol = false;           // 순찰 상태
    //private bool isIdle = false;             // 다음 작업 대기상태
    //private bool isChase = false;            // 추적 상태

    private NavMeshAgent agent;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        ChangeState(currentState);
        tempTrackingRange = trackingRange;

        //// Die 애니메이션 클립의 길이
        //foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        //{
        //    if (clip.name == "Z_FallingBack")
        //    {
        //        dieAnimationLength = clip.length;
        //        break;
        //    }
        //}
    }

    void Update()
    {
        if (target == null)
        {
            Debug.Log("지정된 타겟이 없습니다.");
            return;
        }

        distanceToTarget = Vector3.Distance(transform.position, target.position);

        #region Comment
        //switch (currentState)
        //{
        //    case EZombieState.Idle:
        //        if (distanceToTarget < trackingRange)
        //        {
        //            //isChase = true;
        //            currentState = EZombieState.Chase;
        //        }
        //        else if (distanceToTarget > trackingRange)
        //        {
        //            currentState = EZombieState.Patrol;
        //        }
        //        else
        //        {
        //            currentState = EZombieState.Idle;
        //        }
        //        break;
        //    case EZombieState.Patrol:
        //        Patrol();
        //        currentState = EZombieState.Idle;
        //        break;
        //    case EZombieState.Die:
        //        Die();
        //        currentState = EZombieState.Idle;
        //        break;
        //    case EZombieState.Chase:
        //        Chase(target);
        //        currentState = EZombieState.Idle;
        //        break;
        //    case EZombieState.Evade:
        //        Evade();
        //        currentState = EZombieState.Idle;
        //        break;
        //    case EZombieState.Damage:
        //        Damage(10);
        //        currentState = EZombieState.Idle;
        //        break;
        //    case EZombieState.Attack:
        //        if (distanceToTarget < attackRange)
        //        {
        //            Attack();
        //        }
        //        currentState = EZombieState.Idle;
        //        break;
        //    case EZombieState.Error:
        //        // 버그처리
        //        break;
        //} 
        #endregion
    }

    public void ChangeState(EZombieState newState)
    {
        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
        }

        currentState = newState;

        switch (currentState)
        {
            case EZombieState.Idle:
                stateRoutine = StartCoroutine(Idle());
                break;
            case EZombieState.Patrol:
                stateRoutine = StartCoroutine(Patrol());
                break;
            case EZombieState.Die:
                stateRoutine = StartCoroutine(Die());
                break;
            case EZombieState.Chase:
                stateRoutine = StartCoroutine(Chase(target));
                break;
            case EZombieState.Evade:
                stateRoutine = StartCoroutine(Evade());
                break;
            case EZombieState.Damage:
                stateRoutine = StartCoroutine(TakeDamage(20));
                break;
            case EZombieState.Attack:
                stateRoutine = StartCoroutine(Attack());
                break;
        }
    }

    private IEnumerator Idle()
    {
        //Debug.Log($"{gameObject.name} : 대기중");
        animator.Play("Idle_Z");
        audioSource.Play();
        trackingRange = tempTrackingRange;
        //if (!audioSource.isPlaying) audioSource.PlayOneShot(audioClipIdle);

        while (currentState == EZombieState.Idle)
        {
            animator.SetBool("IsWalk", false);

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance < trackingRange)
            {
                ChangeState(EZombieState.Chase);
                yield break;
            }
            else if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
                yield break;
            }
            else if (patrolPoints.Length > 0)
            {
                yield return new WaitForSeconds(3.0f);
                ChangeState(EZombieState.Patrol);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator Patrol()
    {
        //Debug.Log($"{gameObject.name} : 순찰중");

        while (currentState == EZombieState.Patrol)
        {
            if (patrolPoints.Length > 0)
            {
                animator.SetBool("IsWalk", true);
                Transform targetPoint = patrolPoints[currentPoint];
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                //transform.position += direction * moveSpeed * Time.deltaTime;
                //transform.LookAt(targetPoint.transform);
                agent.speed = moveSpeed;
                agent.isStopped = false;
                agent.destination = targetPoint.position;

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }

                float distance = Vector3.Distance(transform.position, target.position);

                if (distance < trackingRange)
                {
                    ChangeState(EZombieState.Chase);
                    yield break;
                }
                else if (distance < attackRange)
                {
                    ChangeState(EZombieState.Attack);
                    yield break;
                }
            }
            yield return null;
        }
    }

    private IEnumerator Chase(Transform target)
    {
        //Debug.Log($"{gameObject.name} : 추적중");

        while (currentState == EZombieState.Chase)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            Vector3 direction = (target.position - transform.position).normalized;
            //transform.position += direction * moveSpeed * Time.deltaTime;
            //transform.LookAt(target.position);
            agent.speed = moveSpeed;
            agent.isStopped = false;
            agent.destination = this.target.position;

            animator.SetBool("IsWalk", true);

            if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
                yield break;
            }
            else if (distance > trackingRange)
            {
                ChangeState(EZombieState.Idle);
                yield break;
            }
            //else if (distance < evadeRange)
            //{
            //ChangeState(EZombieState.Evade);
            //}

            yield return null;
        }
    }

    private IEnumerator Attack()
    {
        //Debug.Log($"{gameObject.name} : 공격중");
        transform.LookAt(target.position);
        agent.isStopped = true;
        agent.destination = target.position;

        animator.SetBool("IsWalk", false);
        animator.SetTrigger("Attack");
        audioSource.PlayOneShot(audioClipHit);

        yield return new WaitForSeconds(attackDelay);

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRange)
        {
            ChangeState(EZombieState.Chase);
            yield break;
        }
        else
        {
            ChangeState(EZombieState.Attack);
            yield break;
        }
    }

    private IEnumerator Evade()
    {
        //Debug.Log($"{gameObject.name} : 도망중");
        animator.SetBool("IsWalk", true);
        Vector3 evadeDirection = (transform.position - target.position).normalized;

        float evadeTime = 3.0f;
        float timer = 0.0f;

        Quaternion targetRotation = Quaternion.LookRotation(evadeDirection);
        //transform.rotation = targetRotation;
        // LookAt을 통해 바라볼 타겟이 없기 때문에 자체 Rotation을 통해 회전

        while (currentState == EZombieState.Evade && timer < evadeTime)
        {
            //transform.position += evadeDirection * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        ChangeState(EZombieState.Idle);
        yield break;
    }

    private IEnumerator TakeDamage(float pDamaged)
    {
        //Debug.Log($"{gameObject.name} : 공격당함 -({pDamaged})-");
        gameObject.GetComponent<Rigidbody>().isKinematic = false;

        while (currentState == EZombieState.Damage)
        {
            animator.SetBool("IsWalk", false);
            animator.SetTrigger("Hit");
            audioSource.PlayOneShot(audioClipHit);
            hp -= pDamaged;

            if (hp <= 0)
            {
                ChangeState(EZombieState.Die);
                yield break;
            }
            else
            {
                tempTrackingRange = trackingRange;
                trackingRange *= 2;
                yield return new WaitForSeconds(attackDelay);
                ChangeState(EZombieState.Chase);
            }
        }
    }

    private IEnumerator Die()
    {
        //Debug.Log($"{gameObject.name} : Zombie Dead");
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        animator.SetBool("IsWalk", false);
        animator.SetTrigger("Dying");
        audioSource.PlayOneShot(audioClipDie);
        Destroy(gameObject, 3);
        yield return null;
    }
}

