using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DoorManager : MonoBehaviour
{
    public static DoorManager Instance { get; private set; }

    [System.Serializable]
    public class Door
    {
        public string name; // 문 이름
        public GameObject doorObject; // 문 GameObject
        public bool isLock = false; // 잠겨 있는지 여부
        public GameObject lockKey;
        public bool isOpen = false; // 열려 있는지 여부
        public Animator animator; // 문 애니메이터 (선택 사항)
    }

    [Header("Door List")]
    public List<Door> doors = new List<Door>(); // 여러 개의 문 관리

    public bool lastOpenedForward = true;

    //public bool IsPlayerInFront(Transform player)
    //{
    //    Vector3 toPlayer = (player.position - transform.position).normalized;
    //    // 플레이어와 문 사이의 벡터를 계산
    //    float dotProduct = Vector3.Dot(transform.forward, toPlayer);
    //    // 문이 향하는 방향과 플레이어의 방향을 비교 (내적연산)
    //    return dotProduct > 0; // dotProduct > 0 이면 플레이어가 문앞에 있음
    //    // 문 양옆에 있는 경우의 수는 배제
    //}

    //public bool Open(Transform player)
    //{
    //    if (!isOpen)
    //    {
    //        isOpen = true; // 문이 열린 상태로 설정

    //        if (IsPlayerInFront(player))// 플레이어가 앞에 있으면 정방향 애니 재생, 뒤에있으면 역방향 애니 재생
    //        {
    //            animator.SetTrigger("OpentFoward"); // 정방향
    //            lastOpenedForward = true; // 문이 정방향으로 열림
    //        }
    //        else
    //        {
    //            animator.SetTrigger("OpenBackward"); // 역방향
    //            lastOpenedForward = false; // 문이 역방향으로 열림
    //        }
    //        return true;
    //    }
    //    return false;
    //}

    //public void CloseForward(Transform player)
    //{
    //    if (isOpen)
    //    {
    //        isOpen = false;
    //        animator.SetTrigger("CloseForward");
    //    }
    //}

    //public void CloseBackward(Transform player)
    //{
    //    if (isOpen)
    //    {
    //        isOpen = false;
    //        animator.SetTrigger("CloseBackward");
    //    }
    //} 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DoorAction(GameObject door)
    {
        StartCoroutine(DoorActionCoroutine(door));
    }

    private IEnumerator DoorActionCoroutine(GameObject door)
    {
        float X = door.transform.rotation.eulerAngles.x;
        float Y = door.transform.rotation.eulerAngles.y;
        float Z = door.transform.rotation.eulerAngles.z;
        Quaternion targetRotation = Quaternion.Euler(X, Y, Z + 90);

        float elapsedTime = 0f;
        float duration = 10.0f; // 문 열고 닫는 시간

        while (elapsedTime < duration)
        {
            door.transform.rotation = Quaternion.Lerp(
                door.transform.rotation,
                targetRotation,
                elapsedTime / duration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        door.transform.rotation = targetRotation;
    }

    //public void DoorAction(string doorName)
    //{
    //    Door door = doors.Find(d => d.name == doorName); // 이름으로 문 찾기
    //    if (door == null)
    //    {
    //        Debug.LogWarning($"Door with name {doorName} not found!");
    //        return;
    //    }

    //    if (door.isLock)
    //    {
    //        Debug.Log($"The door '{doorName}' is locked!");
    //        return;
    //    }

    //    door.isOpen = !door.isOpen; // 상태 전환
    //    StartCoroutine(DoorActionCoroutine(door));
    //}

    //private IEnumerator DoorActionCoroutine(Door door)
    //{
    //    Debug.Log($"DoorAction for {door.name}");

    //    float targetAngle = door.isOpen ? 90.0f : 0.0f; // 열리면 90도, 닫히면 0도
    //    float currentAngle = door.doorObject.transform.rotation.eulerAngles.y;
    //    float fixX = door.doorObject.transform.rotation.eulerAngles.x;
    //    float fixZ = door.doorObject.transform.rotation.eulerAngles.z;
    //    Quaternion targetRotation = Quaternion.Euler(fixX, targetAngle, fixZ);

    //    float elapsedTime = 0f;
    //    float duration = 1.5f; // 문 열고 닫는 시간

    //    while (elapsedTime < duration)
    //    {
    //        door.doorObject.transform.rotation = Quaternion.Lerp(
    //            door.doorObject.transform.rotation,
    //            targetRotation,
    //            elapsedTime / duration
    //        );
    //        elapsedTime += Time.deltaTime;
    //        yield return null;
    //    }

    //    door.doorObject.transform.rotation = targetRotation;

    //    Debug.Log($"Door '{door.name}' is now {(door.isOpen ? "open" : "closed")}");
    //}
}
