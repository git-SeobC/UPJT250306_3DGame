using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleMenuManager : MonoBehaviour
{
    public static TitleMenuManager Instance { get; private set; }

    public GameObject player;
    public Transform playerHead;
    public Camera camera;
    public GameObject TitleUI;
    public GameObject SettingsUI;
    public GameObject PlayerHPUI;
    public GameObject BulletCountUI;
    public Transform passPoint;
    public Transform point1;
    public Transform point2;
    private bool isMovingBetweenPoints = true;

    public Image EndPanel;
    public Text EndText;
    public Text RetryText;
    public float fadeDuration = 3.0f;
    public string nextSceneName;
    private bool isGameEnd = false;

    private bool isSettings = false;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(FadeInAndLoadScene());
            //GameManager.Instance.OnCaption("NoHouseKey");
        }
        if (isGameEnd && Input.GetKeyDown(KeyCode.Space))
        {
            isGameEnd = false;
            LoadScene(nextSceneName);
        }
    }

    public void TheEnd()
    {
        StartCoroutine(FadeInAndLoadScene());
    }

    private IEnumerator MoveBetweenPoints()
    {
        while (isMovingBetweenPoints)
        {
            yield return StartCoroutine(MoveCamera(point1.position, point2.position)); // point1 -> point2로 이동
            yield return new WaitForSeconds(5);
            yield return StartCoroutine(MoveCamera(point2.position, point1.position)); // point2 -> point1로 이동
            yield return new WaitForSeconds(5);
        }
    }

    public void OnClickSettingBtn()
    {
        TitleUI.SetActive(false);
    }

    public void GameStart()
    {
        isMovingBetweenPoints = false;
        StopAllCoroutines();

        TitleUI.SetActive(false);

        StartCoroutine(CameraMoveToPlayer());

        player.SetActive(true);
        //PlayerManager playerManager = player.GetComponent<PlayerManager>();
        //if (playerManager != null)
        //{
        //    playerManager.enabled = true;
        //}
        //else
        //{
        //    Debug.LogError("PlayerManager 컴포넌트를 찾을 수 없습니다.");
        //}
    }

    private IEnumerator CameraMoveToPlayer()
    {
        Vector3 startPosition = camera.transform.position;
        Vector3 targetPosition = player.transform.position + new Vector3(0.739f, 1.9f, -2);

        float duration = 4.0f;

        yield return StartCoroutine(MoveCameraBezier(startPosition, passPoint.position, targetPosition, duration));
        GameManager.Instance.OnExplain(true);
        PlayerHPUI.SetActive(true);
        BulletCountUI.SetActive(true);
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

        camera.transform.position = end;
    }

    private IEnumerator MoveCamera(Vector3 from, Vector3 end)
    {
        float elapsedTime = 0f;

        while (elapsedTime < 6.0f)
        {
            camera.transform.position = Vector3.Lerp(from, end, elapsedTime / 6.0f);
            camera.transform.LookAt(passPoint);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        camera.transform.position = end;
    }

    public void LoadScene(string pSceneName)
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.SetSFXVolume(0.5f);
        SoundManager.Instance.PlaySfx("EquipGun", Vector3.zero, 0f);

        SceneManager.LoadScene(pSceneName);
        Cursor.lockState = CursorLockMode.Confined;
        Debug.Log("Scene 변경 :" + pSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    IEnumerator FadeInAndLoadScene()
    {
        isGameEnd = true;
        EndPanel.gameObject.SetActive(true);
        yield return StartCoroutine(FadeImage(0, 1, fadeDuration));
        //yield return StartCoroutine(FadeImage(1, 0, fadeDuration));
    }

    IEnumerator FadeImage(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0.0f;

        Color panelColor = EndPanel.color;
        Color textColor = EndText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            float textAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            panelColor.a = newAlpha;
            textColor.a = textAlpha;
            EndPanel.color = panelColor;
            EndText.color = textColor;
            yield return null;
        }
        panelColor.a = endAlpha;
        EndPanel.color = panelColor;

        RetryText.gameObject.SetActive(true);
    }
}
