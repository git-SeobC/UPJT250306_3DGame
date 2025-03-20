using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuManager : MonoBehaviour
{
    public static TitleMenuManager Instance { get; private set; }

    public GameObject player;
    public Transform playerHead;
    public Camera camera;
    public GameObject TitleUI;
    public Transform passPoint;
    public Transform point1;
    public Transform point2;
    private bool isMovingBetweenPoints = true;

    void Awake()
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

    private void Start()
    {
        player.SetActive(false);
        TitleUI.SetActive(true);

        StartCoroutine(MoveBetweenPoints());
    }

    private IEnumerator MoveBetweenPoints()
    {
        while (isMovingBetweenPoints)
        {
            yield return StartCoroutine(MoveCamera(point1.position, point2.position, 2.0f)); // point1 -> point2로 이동
            yield return StartCoroutine(MoveCamera(point2.position, point1.position, 2.0f)); // point2 -> point1로 이동
        }
    }


    public void GameStart()
    {
        isMovingBetweenPoints = false;
        StopAllCoroutines();

        TitleUI.SetActive(false);

        StartCoroutine(CameraMoveToPlayer());

        player.SetActive(true);
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        if (playerManager != null)
        {
            playerManager.enabled = true;
        }
        else
        {
            Debug.LogError("PlayerManager 컴포넌트를 찾을 수 없습니다.");
        }
    }

    private IEnumerator CameraMoveToPlayer()
    {
        Vector3 startPosition = camera.transform.position;
        Vector3 targetPosition = player.transform.position + new Vector3(0.739f, 1.9f, -2);

        float duration = 4.0f;

        yield return StartCoroutine(MoveCameraBezier(startPosition, passPoint.position, targetPosition, duration));

        //float durationToTarget = 5.0f;
        //float durationToMovePoint = 3.0f;
        //yield return StartCoroutine(MoveCamera(startPosition, movePoint.position, durationToMovePoint));
        //yield return StartCoroutine(MoveCamera(movePoint.position, targetPosition, durationToTarget));

        //float elapsedTime = 0f;
        //while (elapsedTime < duration)
        //{
        //    camera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
        //    camera.transform.LookAt(player.transform);
        //    elapsedTime += Time.deltaTime;
        //    yield return null;
        //}
        //camera.transform.position = targetPosition; // 최종 위치 설정
    }

    private IEnumerator MoveCameraBezier(Vector3 start, Vector3 control, Vector3 end, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration; // 현재 시간 비율 (0 ~ 1)

            // Bezier 곡선 계산: B(t) = (1 - t)^2 * P0 + 2 * (1 - t) * t * P1 + t^2 * P2
            Vector3 bezierPoint = Mathf.Pow(1 - t, 2) * start +
                                  2 * (1 - t) * t * control +
                                  Mathf.Pow(t, 2) * end;

            camera.transform.position = bezierPoint; // 카메라 위치 업데이트
            camera.transform.LookAt(playerHead);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        camera.transform.position = end; // 최종 위치 설정
    }

    private IEnumerator MoveCamera(Vector3 from, Vector3 to, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            camera.transform.position = Vector3.Lerp(from, to, elapsedTime / duration); // 선형 보간
            camera.transform.LookAt(passPoint);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        camera.transform.position = to; // 최종 위치 설정
    }

    public void LoadScene(string pSceneName)
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.SetSFXVolume(0.5f);
        SoundManager.Instance.PlaySfx("EquipGun", Vector3.zero, 0f);

        SceneManager.LoadScene(pSceneName);

        Debug.Log("Scene 변경 :" + pSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
