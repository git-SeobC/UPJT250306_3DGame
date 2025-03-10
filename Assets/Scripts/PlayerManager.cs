using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine; // NameSpace : 소속

public class PlayerManager : MonoBehaviour
{
    private float moveSpeed = 5.0f; // 플레이어 이동 속도
    public float mouseSensitivity = 100.0f; // 마우스 감도
    public Transform cameraTransform; // 카메라의 Transform
    public CharacterController characterController;
    public Transform playerHead; // 플레이어 머리 위치(1인칭 모드를 위해서)
    public float thirdPersonDistance = 3.0f; // 3인칭 모드 플레이어와 카메라 간격
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.5f, 0f); // 3인칭 모드에서 카메라 오프셋
    public Transform playerLookObj; // 플레이어 시야 위치

    public float zoomDistance = 1.0f; // 카메라가 확대될 때의 거리(3인칭 모드에서 사용)
    public float zoomSpeed = 5.0f; // 확대 축소가 되는 속도
    public float defaultFov = 60.0f; // 기본 카메라 시야각
    public float zoomFov = 30.0f; // 확대 시 카메라 시야각 (1인칭 모드에서 사용)

    private float currentDistance; // 현재 카메라와의 거리 (3인칭 모드)
    private float targetDistance; // 목표 카메라 거리
    private float targetFov; // 목표 FOV
    private bool isZoomed = false; // 확대 여부 확인
    private Coroutine zoomCoroutine; // 코루틴을 사용하여 확대 축소 처리
    private Camera mainCamera; // 카메라 컴포넌트

    private float pitch = 0.0f; // 위 아래 회전 값
    private float yaw = 0.0f; // 좌우 회전 값
    private bool isFirstPerson = false; // 1인칭 모드 여부
    private bool isRotaterAroundPlayer = false; // 카메라가 플레이어 주위를 회전하는지 여부

    // 중력 관련 변수
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround;

    // 애니메이션 및 이동 관련 변수
    private Animator animator;
    private float horizontal;
    private float vertical;
    private bool isRunning = false;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;

    // 총기 조준 및 발사 애니메이션 상태
    private bool isAim = false;
    private bool isFire = false;
    private bool isFireMoving = false;

    // 총기 사운드
    public AudioClip audioClipFire;
    private AudioSource audioSource;
    public AudioClip audioClipWeaponChange;

    public GameObject shotgun2;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        shotgun2.SetActive(false);
    }

    void Update()
    {
        // 마우스 입력을 받아 카메라와 플레이어 회전 처리
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        isGround = characterController.isGrounded;

        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetKeyDown(KeyCode.V)) // 1 ↔ 3 인칭 전환
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1인칭 모드" : "3인칭 모드");
        }

        if (Input.GetKeyDown(KeyCode.F)) // 1인칭 모드에선 카메라 회전을 하면 안됨 → 플레이어의 시야에 따라서 회전
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "카메라가 주위를 회전합니다." : "플레이어가 시야에 따라서 회전합니다.");
        }


        // 카메라 줌 기능
        if (Input.GetMouseButtonDown(1)) // 우측 버튼 누를 때
        {
            // 견착 상태
            isAim = true;
            animator.SetBool("IsAim", isAim);

            // 코루틴이 이미 실행중이면 실행중인 코루틴 정지
            if (zoomCoroutine != null) { StopCoroutine(zoomCoroutine); }

            if (isFirstPerson) // 1인칭 일때
            {
                SetTargetFOV(zoomFov); // zoomFov(확대시야각)으로 target FOV값 세팅
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov)); // 확대하는 코루틴 실행
            }
            else // 3인칭 일때
            {
                SetTargetDistance(zoomDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

        if (Input.GetMouseButtonUp(1)) // 우측 버튼 뗄 때
        {
            isAim = false;
            animator.SetBool("IsAim", isAim);
            isFire = false;
            animator.SetBool("IsFire", isFire);

            if (zoomCoroutine != null) { StopCoroutine(zoomCoroutine); }

            if (isFirstPerson) // 1인칭 일때
            {
                SetTargetFOV(defaultFov); // zoomFov(확대시야각)으로 target FOV값 세팅
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov)); // 확대하는 코루틴 실행
            }
            else // 3인칭 일때
            {
                SetTargetDistance(thirdPersonDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

        // 플레이어 움직임
        if (isFirstPerson) { FirstPoersonMovement(); }
        else { ThirdPersonMovement(); }

        // 총기 발사
        if (isAim && Input.GetMouseButtonDown(0))
        {
            if (!isFireMoving)
            {
                isFire = true;
                animator.SetBool("IsFire", isFire);
            }
            else
            {
                isFire = true;
                animator.SetBool("IsFire", isFire);
                animator.SetBool("IsFireMoving", isFireMoving);
            }
            audioSource.PlayOneShot(audioClipFire);
        }
        if (Input.GetMouseButtonUp(0))
        {
            isFire = false;
            isFireMoving = false;
            animator.SetBool("IsFire", isFire);
            animator.SetBool("IsFireMoving", isFireMoving);
        }

        if (Input.GetKey(KeyCode.LeftShift)) { isRunning = true; }
        else { isRunning = false; }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            audioSource.PlayOneShot(audioClipWeaponChange);
            animator.SetTrigger("IsWeaponChange");
            shotgun2.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            audioSource.PlayOneShot(audioClipWeaponChange);
            animator.SetTrigger("IsWeaponChange");
            shotgun2.SetActive(false);
        }

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("IsRunning", isRunning);
        moveSpeed = isRunning ? runSpeed : walkSpeed;
    }

    void FirstPoersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (isAim && vertical.Equals(1.0f)) // 조준 전진 상태일 때 조준 전진 애니메이션 실행
        {
            isFireMoving = true;
            animator.SetBool("IsFireMoving", isFireMoving);
            Vector3 moveDirection = cameraTransform.forward * vertical;
            moveDirection.y = 0;
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        else if (!isAim)
        {
            isFire = false;
            animator.SetBool("IsFire", isFire);
            isFireMoving = false;
            animator.SetBool("IsFireMoving", isFireMoving);
            Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
            moveDirection.y = 0; // <<< 플레이 할때 확인해 볼 부분
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime); // 플레이어 움직임 적용
        }
        else
        {
            isFire = false;
            animator.SetBool("IsFire", isFire);
            isFireMoving = false;
            animator.SetBool("IsFireMoving", isFireMoving);
        }

        cameraTransform.position = playerHead.position; // 1인칭이기 때문에 플레이어 시점으로 카메라 포지션 세팅
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0); // 회전 각에 따라 카메라 회전 값 조정

        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0); // 카메라 시점에 따른 플레이어 몸통 회전
    }

    void ThirdPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (isAim && vertical.Equals(1.0f)) // 조준 전진 상태일 때 조준 전진 애니메이션 실행
        {
            isFireMoving = true;
            animator.SetBool("IsFireMoving", isFireMoving);
            Vector3 move = transform.forward * vertical;
            characterController.Move(move * moveSpeed * Time.deltaTime);
        }
        else if (!isAim)
        {
            isFire = false;
            animator.SetBool("IsFire", isFire);
            isFireMoving = false;
            animator.SetBool("IsFireMoving", isFireMoving);
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            characterController.Move(move * moveSpeed * Time.deltaTime);
        }
        else
        {
            isFire = false;
            animator.SetBool("IsFire", isFire);
            isFireMoving = false;
            animator.SetBool("IsFireMoving", isFireMoving);
        }

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (isRotaterAroundPlayer)
        {
            // 카메라가 플레이어 오른쪽에서 회전하도록 설정
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));

        }
        else
        {
            // 플레이어가 직접 회전하는 모드
            transform.rotation = Quaternion.Euler(0f, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);

            // 카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0)); // 플레이어가 보고 있는 곳으로 LookAt
        }
    }

    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }

    public void SetTargetFOV(float fov)
    {
        targetFov = fov;
    }

    // 3인칭 줌 처리 코루틴
    IEnumerator ZoomCamera(float targetDistance)
    {
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f) // 현재 거리에서 목표 거리로 부드럽게 이동
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance; // 목표 거리에 도달한 후 값을 고정
    }

    // 1인칭 줌 처리 코루틴
    IEnumerator ZoomFieldOfView(float targetFov)
    {
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        mainCamera.fieldOfView = targetFov;
    }
}
