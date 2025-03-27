using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    // 로딩 Slider UI를 가져오는 변수
    public Slider loadingSlider;

    private string nextSceneName;

    public void StartLoading(string sceneName)
    {
        nextSceneName = sceneName;
        StartCoroutine(LoadLoadingSceneAndNextScene());
    }

    IEnumerator LoadLoadingSceneAndNextScene() // 로딩 씬을 로드하고 NextScene이 로드될 때까지 대기하는 코루틴
    {
        // 로딩 씬을 비동기적으로 로드(로딩 상태 표시용으로 사용하는 씬)
        AsyncOperation loadingSceneOp = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
        loadingSceneOp.allowSceneActivation = false;

        while (!loadingSceneOp.isDone) // 로딩씬이 로드될 때까지 대기
        {
            if (loadingSceneOp.progress >= 9.0f)
            {
                loadingSceneOp.allowSceneActivation = true; // 로딩씬 준비 완료되면 씬 활성화
            }
            yield return null;
        }
        FindLoadingSliderInScene(); // 로딩 씬에서 로딩 Slider를 찾아오기

        AsyncOperation nextSceneOp = SceneManager.LoadSceneAsync(nextSceneName); // NextScene을 비동기적으로 로드
        while (!nextSceneOp.isDone) // 로딩 진행률을 Slider에 표시 다음 신 로드 될때까지 대기하면서 진행률을 슬라이더에 표시
        {
            loadingSlider.value = nextSceneOp.progress; // 로딩진행도 업데이트 (0 ~ 1)
            yield return null;
        }
        SceneManager.UnloadSceneAsync("LoadingScene"); // nextSceneOp 완전히 로드된 후, 로딩씬을 언로드
    }

    private void FindLoadingSliderInScene()
    {
        loadingSlider = GameObject.Find("LoadingSlider").GetComponent<Slider>();
    }
}
