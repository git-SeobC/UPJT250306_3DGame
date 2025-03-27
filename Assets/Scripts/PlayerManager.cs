using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;


public enum WeaponMode
{
    Pistol,
    Shotgun,
    Rifle,
    Sniper
}


public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

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
    private int animationSpeed = 1;
    private string currentAnimation = "";
    private float horizontal;
    private float vertical;
    private bool isRunning = false;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;

    // 총기 조준 및 발사 애니메이션 상태
    private bool isAim = false;
    private bool isFire = false;

    // 사운드 클립
    private AudioSource audioSource;
    public AudioClip audioClipFire;
    public AudioClip audioClipWeaponChange;
    public AudioClip audioClipHitPlayer;
    public AudioClip audioClipPikcup;
    public AudioClip audioClipDefaultStep;
    public AudioClip audioClipEmptyGunShot;

    public Transform aimTarget;

    // Raycast 관련
    public LayerMask targetLayerMask;
    public MultiAimConstraint multiAimConstraint;
    private float weaponMaxDistance = 100.0f;
    public LayerMask groundLayerMask;

    // Item pick 관련 변수
    public Vector3 boxSize = new Vector3(1f, 1f, 1f);
    public float castDistance = 0.005f;
    public LayerMask itemLayer;
    public Transform itemGetPos;

    // 총기 관련 조건 및 변수
    private bool isGetGunItem = false;
    private bool isUseWeapon = false;
    private float rifleFireDelay = 0.5f;
    public GameObject shotgun2; // 플레이어 손 총기 오브젝트
    public ParticleSystem gunFireEffect;

    // 파티클 관련
    public ParticleSystem DamageParticleSystem;
    public AudioClip audioClipDamage;

    // UI
    public GameObject keyEIconObj; // 총 아이콘 오브젝트, 총기 아이템 근처에서 On
    public GameObject crosshairObj; // 크로스헤어 오브젝트
    public GameObject BulletCountBtn;
    public GameObject PlayerHpBtn;
    public Text bulletText;
    public Text playerHpText;
    public int playerHP = 100;
    private int fireBulletCount = 0;
    private int saveBulletCount = 0;
    private bool isOnExplain = true; // 도움말 상태 체크

    // Pause UI
    public GameObject pauseObj;
    private bool isPause = false;

    // Flash Light
    public GameObject flashLightObj;
    public AudioClip audioClipLightOn;
    private bool isFlashLightOn = false;
    private Transform flashLightPos;

    private WeaponMode currentWeaponMode = WeaponMode.Sniper;
    private int ShotgunRayCount = 5; // 탄 퍼짐 개수
    private float shotgunSpreadAngle = 10.0f; // 샷건 탄 퍼짐 각도
    private float recoilStrength = 2.0f; // 반동 세기
    private float maxRecoilAngle = 10.0f; // 반동 각도
    private float currentRecoil = 0.0f; // 현재 반동
    private float shakeDuration = 0.1f; // 흔들림 지속 시간
    private float shakeMagnitude = 0.1f; // 흔들림 크기
    private Vector3 originalCameraPosition; // 반동전 기존 포지션
    private Coroutine cameraShakeCoroutine; // 반동 코루틴

    private bool lastOpenedForward = true;

    // 열쇠 획득 여부
    public bool isGetHouseKey = false;
    public bool isGetEscapeKey = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

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
        animator.speed = animationSpeed;
        bulletText.text = $"{fireBulletCount}/{saveBulletCount}";
        bulletText.gameObject.SetActive(true);
        flashLightObj.SetActive(false);
        flashLightPos = flashLightObj.transform;
        //PlayerHpBtn.SetActive(true);
        //BulletCountBtn.SetActive(true);
        //explanationUI.SetActive(true);

        SoundManager.Instance.StopBGM();
        SoundManager.Instance.SetSFXVolume(0.5f);
        SoundManager.Instance.PlaySfx("StartGame", Vector3.zero, 0f);

        //RenderSettings.fog = true; // 안개 효과 활성화
        //RenderSettings.fogColor = Color.gray; // 안개 색상 설정
        //RenderSettings.fogDensity = 1.0f; // 안개의 밀도 설정
        //RenderSettings.fogStartDistance = 10f; // 안개 시작 거리
        //RenderSettings.fogEndDistance = 100f; // 종료 거리 (Linear 모드에서)
        //RenderSettings.fogMode = FogMode.Exponential; // 지수 함수 기반 안개

        //if (mainCamera != null) // 카메라의 clear Flags를 solid Color로 설정하고, 배경색을 안개색으로 설정
        //{
        //    mainCamera.clearFlags = CameraClearFlags.SolidColor;
        //    mainCamera.backgroundColor = RenderSettings.fogColor;
        //}
    }

    void Update()
    {
        MouseSet();
        CameraToggle();
        AimSet();
        PersonMovement();
        GunFire();
        ChangeWeapon();
        //PickItemCheck();
        Operate();
        if (Input.GetKeyDown(KeyCode.F))
        {
            ActionFlashLigh();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPause = !isPause;
            if (isPause)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Pause();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                ReGame();
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isOnExplain = !isOnExplain;
            GameManager.Instance.OnExplain(isOnExplain);
        }

        if (currentRecoil > 0)
        {
            currentRecoil -= recoilStrength * Time.deltaTime;
            currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle);
            Quaternion currentRotation = Camera.main.transform.rotation;
            Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0);
            Camera.main.transform.rotation = currentRotation * recoilRotation; // 카메라를 제어하는 코드를 꺼야함
        }
    }

    void FireShotgun()
    {
        for (int i = 0; i < ShotgunRayCount; i++)
        {
            RaycastHit hit;

            Vector3 origin = Camera.main.transform.position;
            Vector3 spreadDirection = GetSpreadDirection(Camera.main.transform.forward, shotgunSpreadAngle);
            Debug.DrawRay(origin, spreadDirection * castDistance, Color.green, 2.0f);
            if (Physics.Raycast(origin, spreadDirection, out hit, castDistance, targetLayerMask))
            {
                Debug.Log("Shotgun Hit : " + hit.collider.name);
            }
        }
    }

    Vector3 GetSpreadDirection(Vector3 forwardDirection, float spreadAngle)
    {
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        Vector3 spreadDirection = Quaternion.Euler(spreadX, spreadY, 0) * forwardDirection;
        return spreadDirection;
    }

    void ApplyRecoil()
    {
        Quaternion currentRotation = Camera.main.transform.rotation; // 현재 카메라 월드 회전값
        Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0); // 반동 계산 X축 상하 회전
        Camera.main.transform.rotation = currentRotation * recoilRotation; // 현재 회전 값에 반동 곱, 새로운 회전값
        currentRecoil += recoilStrength; // 반동 증가
        currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle); // 반동값 제한
    }

    void StartCameraShake()
    {
        if (cameraShakeCoroutine != null)
        {
            StopCoroutine(cameraShakeCoroutine);
        }
        cameraShakeCoroutine = StartCoroutine(CameraShake(shakeDuration, shakeMagnitude));
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        Vector3 originalPosition = Camera.main.transform.position;
        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1.0f, 1.0f) * magnitude;
            float offsetY = Random.Range(-1.0f, 1.0f) * magnitude;

            Camera.main.transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }
        Camera.main.transform.position = originalPosition;
    }

    public void ReGame()
    {
        //audioSource.PlayOneShot(audioClipFire);
        SoundManager.Instance.PlaySfx("GunFireSniper", Vector3.zero, 0f);
        pauseObj.SetActive(false);
        Time.timeScale = 1; // 게임 시간 재개
    }

    void Pause()
    {
        //audioSource.PlayOneShot(audioClipDamage);
        SoundManager.Instance.PlaySfx("PlayerHit", Vector3.zero, 0f);
        pauseObj.SetActive(true);
        Time.timeScale = 0; // 게임 시간 정지
    }

    public void Exit()
    {
        //audioSource.PlayOneShot(audioClipHitPlayer);
        SoundManager.Instance.PlaySfx("PlayerDie", Vector3.zero, 0f);
        pauseObj.SetActive(false);
        Time.timeScale = 1;
        Application.Quit(); // 종료
        isPause = false;
    }

    /// <summary>
    /// 마우스 세팅
    /// </summary>
    public void MouseSet()
    {
        if (isPause) return;

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
        else
        {
            velocity.y = Mathf.Clamp((velocity.y + gravity * Time.deltaTime), -50f, -2f);
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// 1 ↔ 3 인칭 전환
    /// </summary>
    public void CameraToggle()
    {
        if (Input.GetKeyDown(KeyCode.V)) // 1 ↔ 3 인칭 전환
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1인칭 모드" : "3인칭 모드");
        }

        if (Input.GetKeyDown(KeyCode.T)) // 1인칭 모드에선 카메라 회전을 하면 안됨 → 플레이어의 시야에 따라서 회전
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "카메라가 주위를 회전합니다." : "플레이어가 시야에 따라서 회전합니다.");
        }
    }

    void AimSet()
    {
        // 카메라 줌 기능
        if (Input.GetMouseButtonDown(1) && isGetGunItem && isUseWeapon) // 우측 버튼 누를 때
        {
            crosshairObj.SetActive(true); // 크로스헤어 On
            // 견착 상태
            isAim = true;
            multiAimConstraint.data.offset = new Vector3(-45, 0, 5);
            //animator.SetBool("IsAim", isAim);
            animator.SetLayerWeight(1, 1);


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

            flashLightObj.transform.Rotate(50, 0, 0);
        }

        if (Input.GetMouseButtonUp(1) && isGetGunItem && isUseWeapon) // 우측 버튼 뗄 때
        {
            crosshairObj.SetActive(false); // 크로스헤어 Off
            isAim = false;
            multiAimConstraint.data.offset = new Vector3(0, 0, 0);
            //animator.SetBool("IsAim", isAim);
            animator.SetLayerWeight(1, 0);
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

            flashLightObj.transform.Rotate(-50, 0, 0);
        }
    }

    public void PersonMovement()
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("IsRunning", isRunning);
        moveSpeed = isRunning ? runSpeed : walkSpeed;

        // 플레이어 움직임
        if (isFirstPerson) { FirstPoersonMovement(); }
        else { ThirdPersonMovement(); }

        if (Input.GetKey(KeyCode.LeftShift)) { isRunning = true; }
        else { isRunning = false; }
    }

    public void PickItemCheck()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            //if (!(stateInfo.IsName("PickItemFloor")) && stateInfo.normalizedTime >= 1.0f)
            {
                animator.SetTrigger("Pick");
            }
        }

        #region 애니메이션 속도 조절 Comment
        //// 0번 애니메이션 레이어의 정보
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        //// 0번 레이어의 재생중인 애니메이션이 Hit이고 애니메이션이 재생중이면
        //if (stateInfo.IsName(currentAnimation) && stateInfo.normalizedTime >= 1.0f)
        //{
        //    currentAnimation = "Attack";
        //    animator.Play(currentAnimation);
        //} 
        #endregion
    }

    public void UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10.0f);
    }

    public void GunFire()
    {
        // 총기 발사
        if (Input.GetMouseButtonDown(0))
        {
            if (isAim && !isFire)
            {
                if (currentWeaponMode == WeaponMode.Pistol) { }
                else if (currentWeaponMode == WeaponMode.Pistol) { }
                else if (currentWeaponMode == WeaponMode.Pistol) { }


                if (fireBulletCount > 0)
                {
                    fireBulletCount--;
                    bulletText.text = $"{fireBulletCount}/{saveBulletCount}";

                    // Weapon Type에 따라 MaxDistance 를 Set
                    weaponMaxDistance = 1000.0f;

                    isFire = true;

                    StartCoroutine(FireTimer());
                    animator.SetTrigger("Fire");

                    ApplyRecoil();
                    StartCameraShake();

                    Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                    RaycastHit hit;

                    #region Comment
                    //RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, targetLayerMask);
                    //if (hits.Length > 0)
                    //{
                    //    int count = 0;
                    //    Debug.Log($"hits.lengh : {hits.Length}");
                    //    foreach (var hit in hits)
                    //    {
                    //        if (count > 1) break;
                    //        Debug.Log($"충돌 : {hit.collider.name}");
                    //        Debug.DrawLine(ray.origin, hit.point, Color.red, 3.0f);
                    //        count++;
                    //    }
                    //}
                    //else
                    //{
                    //    Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 3.0f);
                    //} 
                    #endregion

                    if (Physics.Raycast(ray, out hit, weaponMaxDistance))
                    {
                        Debug.Log($"Hit : {hit.collider.gameObject.name}");
                        Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f);
                        ParticleSystem particle = Instantiate(DamageParticleSystem, hit.point, Quaternion.identity);
                        particle.Play();
                        SoundManager.Instance.PlaySfx("TakeBullet", hit.collider.transform.position);

                        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                        {
                            hit.collider.gameObject.GetComponent<ZombieManager>().ChangeState(EZombieState.Damage);
                        }
                        //hit.collider.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 2.0f);
                    }
                }
                else
                {
                    //audioSource.PlayOneShot(audioClipEmptyGunShot);
                    SoundManager.Instance.PlaySfx("EmptyGunFire", transform.position);
                    return;
                }
            }
        }
        //if (Input.GetMouseButtonUp(0))
        //{
        //    isFire = false;
        //}
    }

    IEnumerator FireTimer()
    {
        yield return new WaitForSeconds(rifleFireDelay);
        isFire = false;
    }

    public void ChangeWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && isGetGunItem)
        {
            bulletText.text = $"{fireBulletCount}/{saveBulletCount}";
            animator.SetTrigger("IsWeaponChange");
            shotgun2.SetActive(true);
            isUseWeapon = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            bulletText.text = "0/0";
            animator.SetTrigger("IsWeaponChange");
            shotgun2.SetActive(false);
            isUseWeapon = false;
        }
    }

    void ActionFlashLigh()
    {
        audioSource.PlayOneShot(audioClipLightOn);
        SoundManager.Instance.PlaySfx("LightOnOff", transform.position);
        isFlashLightOn = !isFlashLightOn;
        flashLightObj.SetActive(isFlashLightOn);
    }

    void Operate()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (Input.GetKeyDown(KeyCode.E) && !stateInfo.IsName("PickItem"))
        {
            animator.SetTrigger("Pick");
        }
        if (Input.GetKeyDown(KeyCode.R) && isUseWeapon)
        {
            if (saveBulletCount <= 0)
            {
                // 총알 없음
                return;
            }
            else
            {
                animator.SetTrigger("Reload");
                SoundManager.Instance.PlaySfx("Reload", transform.position);

                if (saveBulletCount < 5)
                {
                    fireBulletCount = saveBulletCount;
                    saveBulletCount = 0;
                }
                else
                {
                    saveBulletCount -= 5 - fireBulletCount; // 총기 별 총알 장전 수 수정 필요
                    fireBulletCount = 5;
                }

                bulletText.text = $"{fireBulletCount}/{saveBulletCount}";
            }
        }
    }

    public void ItemPick() // E 버튼 클릭
    {
        Vector3 origin = itemGetPos.position;
        Vector3 direction = itemGetPos.forward;
        RaycastHit[] hits;
        hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);
        DebugBox(origin, direction);
        foreach (var hit in hits)
        {
            Debug.Log($"{hit.collider.name}");
            if (hit.collider.name == "shotgun2")
            {
                hit.collider.gameObject.SetActive(false);
                //audioSource.PlayOneShot(audioClipPikcup);
                SoundManager.Instance.PlaySfx("TakeItem", transform.position);
                keyEIconObj.SetActive(false);
                //Debug.Log($"Item : {hit.collider.name}");
                isGetGunItem = true;
                saveBulletCount = 40;
                fireBulletCount = 0;
                break;
            }
            else if (hit.collider.name == "ItemBullet")
            {
                hit.collider.gameObject.SetActive(false);
                //audioSource.PlayOneShot(audioClipPikcup);
                SoundManager.Instance.PlaySfx("TakeItem", transform.position);

                saveBulletCount += 5;
                if (saveBulletCount > 20)
                {
                    saveBulletCount = 20;
                }
                bulletText.text = $"{fireBulletCount}/{saveBulletCount}";
                break;
            }
            else if (hit.collider.gameObject.tag == "Door")
            {
                #region Comment
                // DoorManager 컴포넌트
                //DoorManager doormanager = hit.collider.GetComponent<DoorManager>();
                //if (doormanager != null)
                //{
                //    if (doormanager.isOpen) // 현재 문 상태 확인
                //    {
                //        if (lastOpenedForward)
                //        {
                //            doormanager.CloseForward(transform);
                //        }
                //        else
                //        {
                //            doormanager.CloseBackward(transform);
                //        }
                //    }
                //    else
                //    {
                //        if (doormanager.Open(transform))
                //        {
                //            lastOpenedForward = doormanager.lastOpenedForward;
                //        }
                //    }
                //    return;
                //} 
                #endregion

                string doorName = hit.collider.gameObject.name;
                DoorManager.Door door = DoorManager.Instance.doors.Find(d => d.name == doorName);

                if (door != null)
                {
                    if (door.isLock)
                    {
                        if (door.lockKey.name == "HouseKey" && isGetHouseKey)
                        {
                            door.isLock = false;
                            GameManager.Instance.OnCaption("DoorOpen");
                            SoundManager.Instance.PlaySfx("UnLock", hit.collider.transform.position);
                        }
                        else
                        {
                            GameManager.Instance.OnCaption("NoHouseKey");
                            break;
                        }
                    }

                    if (door.isOpen)
                    {
                        // 문이 열려 있는 경우
                        if (door.animator != null)
                        {
                            door.animator.SetTrigger("OpenBackward");
                            SoundManager.Instance.PlaySfx("DoorClose", hit.collider.transform.position);
                        }
                        door.isOpen = false;
                    }
                    else
                    {
                        // 문이 닫혀 있는 경우
                        if (door.animator != null)
                        {
                            door.animator.SetTrigger("OpenForward");
                            SoundManager.Instance.PlaySfx("DoorOpen", hit.collider.transform.position);
                        }
                        door.isOpen = true;
                    }
                }
                else
                {
                    if (hit.collider.gameObject.name == "Door3" && isGetEscapeKey)
                    {
                        DoorManager.Instance.DoorAction(hit.collider.gameObject);
                        SoundManager.Instance.PlaySfx("GateOpen", hit.collider.transform.position);
                    }
                    else if (hit.collider.gameObject.name == "Door3" && !isGetEscapeKey)
                    {
                        GameManager.Instance.OnCaption("NoEscapeKey");
                    }
                    else
                    {
                        Debug.LogWarning($"Door '{doorName}' not found in DoorManager.");
                    }
                }
                break;
            }
            else if (hit.collider.gameObject.name == "HouseKey")
            {
                hit.collider.gameObject.SetActive(false);
                SoundManager.Instance.PlaySfx("TakeItem", transform.position);
                isGetHouseKey = true;
                keyEIconObj.SetActive(false);
                GameManager.Instance.SetExplanationText("Find Gate Key");
                GameManager.Instance.OnCaption("All right, let's go get the gate key now");
            }
            else if (hit.collider.gameObject.name == "EscapeKey")
            {
                hit.collider.gameObject.SetActive(false);
                SoundManager.Instance.PlaySfx("TakeItem", transform.position);
                isGetEscapeKey = true;
                keyEIconObj.SetActive(false);
                GameManager.Instance.SetExplanationText("Escape");
                GameManager.Instance.OnCaption("it's better get out of here quickly");
            }
        }
    }



    void FirstPoersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        cameraTransform.position = playerHead.position;
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);
        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0);
    }

    void ThirdPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);

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

            UpdateAimTarget();
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

    public void WeaponChangeSoundEvent()
    {
        //audioSource.PlayOneShot(audioClipWeaponChange);
        SoundManager.Instance.PlaySfx("EquipGun", transform.position);
        Debug.Log("총 체인지 사운드");
    }

    public void WeaponFireSoundEvent()
    {
        //audioSource.PlayOneShot(audioClipFire);
        SoundManager.Instance.PlaySfx("GunFireSniper", transform.position);
        //ParticleManager.Instance.ParticlePlay(ParticleType.WeaponFire, );
    }

    public void PlayerPositionReset()
    {
        characterController.enabled = false;
        transform.position = Vector3.zero;
        characterController.enabled = true;
    }

    public void FootStepSoundOn(AnimationEvent pEvent)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward + new Vector3(0, -1, 0), out hit, 10.0f, groundLayerMask))
        {
            if (hit.collider.gameObject.tag == "Wood")
            {
                if (pEvent.animatorClipInfo.weight > 0.4f)
                {
                    //audioSource.PlayOneShot(audioClipDefaultStep);
                    SoundManager.Instance.PlaySfx("StepDefault", transform.position);
                }
                //if (!audioSource.isPlaying) audioSource.PlayOneShot(audioClipDefaultStep);
            }
            else if (hit.collider.gameObject.tag == "Rock")
            {
                if (pEvent.animatorClipInfo.weight > 0.4f)
                {
                    //audioSource.PlayOneShot(audioClipDefaultStep);
                    SoundManager.Instance.PlaySfx("StepDefault", transform.position);
                }
                //if (!audioSource.isPlaying) audioSource.PlayOneShot(audioClipDefaultStep);
            }
            else
            {
                if (pEvent.animatorClipInfo.weight > 0.4f)
                {
                    //audioSource.PlayOneShot(audioClipDefaultStep);
                    SoundManager.Instance.PlaySfx("StepDefault", transform.position);
                }
                //if (!audioSource.isPlaying) audioSource.PlayOneShot(audioClipDefaultStep);
            }
        }
    }

    private void PlayerHealth(int damage)
    {
        playerHP += damage;
        if (playerHP <= 0)
        {
            animator.SetTrigger("Dying");
            gameObject.GetComponent<CharacterController>().enabled = false;
            Destroy(gameObject, 5.0f);
            TitleMenuManager.Instance.TheEnd();
        }
        else if (playerHP > 100)
        {
            playerHP = 100;
        }

        playerHpText.text = $"{playerHP}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (other.gameObject.CompareTag("Attack"))
            {
                //audioSource.PlayOneShot(audioClipHitPlayer);
                SoundManager.Instance.PlaySfx("PlayerHit", transform.position);

                animator.SetTrigger("Hit");

                PlayerHealth(-other.GetComponentInParent<ZombieManager>().ZombiePower);

                //Debug.Log("Player Trigger Collision");
                //Debug.Log("앙 마자띠");
            }
            //PlayerPositionReset();
        }
        //Debug.Log($"    objLayer : {other.gameObject.layer}");
        //Debug.Log($"    item Layer : {LayerMask.NameToLayer("Item")}");
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            //Debug.Log("Gun Icon Set true");
            keyEIconObj.SetActive(true);

            // 오브젝트 상속관계 변경 (.SetParent)
            //other.gameObject.transform.SetParent(transform);
        }

        if (other.gameObject.name == "ExplainOn")
        {
            GameManager.Instance.SetExplanationText("Find house key");
            GameManager.Instance.OnCaption("I need to find the keys to the house. Maybe it's in a storage room in the grave");
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.name == "ExplainOn2")
        {
            GameManager.Instance.OnCaption("To get out of the gate, I need the gate key in the locked door of the house");
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.name == "TheEnd")
        {
            TitleMenuManager.Instance.TheEnd();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            Debug.Log("Gun Icon Set false");
            keyEIconObj.SetActive(false);
        }
    }

    /// <summary>
    /// Boxcast 영역
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    private void DebugBox(Vector3 origin, Vector3 direction)
    {
        Vector3 endPoint = origin + direction * castDistance;

        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;

        Debug.DrawLine(corners[0], corners[1], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[3], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[2], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[0], Color.green, 3.0f);
        Debug.DrawLine(corners[4], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[5], corners[7], Color.green, 3.0f);
        Debug.DrawLine(corners[7], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[6], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[0], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[7], Color.green, 3.0f);
        Debug.DrawRay(origin, direction * castDistance, Color.green);
    }

    // Coroutine으로 자막 FadeIn/FadeOut 처리


    //void OnSceneLoaded(Scene scene, LoadSceneMode mode) // 씬이 로드될 때 호출되는 메소드
    //{
    //    Debug.Log("Loaded Scene : " + scene.name);
    //    // 어떤 씬일 때 어떤 작업을 할지 조건문이 있어야함
    //}

}
