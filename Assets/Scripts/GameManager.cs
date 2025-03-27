using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Dictionary<string, string> captionDictionary = new Dictionary<string, string>();
    public GameObject explanationUI; // 도움말 UI
    public Image explainWASD;
    public Image explainTab;
    public Image explainShift;
    public Image explainRun;
    public Text goalText;

    public Text CaptionText;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCaption();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCaption()
    {
        captionDictionary.Add("NoHouseKey", "I don't have the house key. \n I have to find it");
        captionDictionary.Add("NoEscapeKey", "I don't have the Gate Key. \n I have to find it");
        captionDictionary.Add("DoorOpen", "Unlock the Door");
    }

    public void OnExplain(bool pSet)
    {
        explanationUI.SetActive(pSet);
    }

    public void SetExplanationText(string pEx)
    {
        explanationUI.SetActive(true);
        goalText.gameObject.SetActive(true);
        explainWASD.gameObject.SetActive(false);
        goalText.text = pEx;
    }

    public void OnCaption(string pName)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(ShowCaptionWithFade(pName));
    }

    private IEnumerator ShowCaptionWithFade(string pCaption)
    {
        // 자막 텍스트 가져오기
        string caption;
        GameManager.Instance.captionDictionary.TryGetValue(pCaption, out caption);
        if (caption == null) caption = pCaption;
        CaptionText.text = caption;

        // FadeIn 효과
        float fadeDuration = 1.0f; // 페이드 지속 시간 (초)
        float displayDuration = 3.0f; // 자막 표시 시간 (초)
        float elapsedTime = 0f;

        CaptionText.enabled = true; // 자막 활성화
        Color textColor = CaptionText.color;
        textColor.a = 0f; // 투명도 초기화
        CaptionText.color = textColor;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textColor.a = Mathf.Clamp01(elapsedTime / fadeDuration); // 투명도를 점점 증가
            CaptionText.color = textColor;
            yield return null;
        }

        // 자막 표시 시간 대기
        yield return new WaitForSeconds(displayDuration);

        // FadeOut 효과
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textColor.a = Mathf.Clamp01(1 - (elapsedTime / fadeDuration)); // 투명도를 점점 감소
            CaptionText.color = textColor;
            yield return null;
        }

        CaptionText.enabled = false; // 자막 비활성화
    }
}
