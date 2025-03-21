using System.Collections;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public static DoorManager Instance { get; private set; }
    public GameObject doorPart;
    public bool isOpen = false;

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

    public void DoorAction()
    {
        isOpen = !isOpen;
        StartCoroutine(DoorActionCoroutine());
    }

    IEnumerator DoorActionCoroutine()
    {
        Debug.Log("DoorAction");
        float targetAngle = isOpen ? 90.0f : 0.0f;
        float currentAngle = doorPart.transform.rotation.y;
        float fixX = doorPart.transform.rotation.eulerAngles.x;
        float fixZ = doorPart.transform.rotation.eulerAngles.z;
        Quaternion target = Quaternion.Euler(fixX, targetAngle, fixZ);

        float elapsedTime = 0f;
        while (elapsedTime < 1.5f)
        {
            doorPart.transform.rotation = Quaternion.Lerp(doorPart.transform.rotation, target, elapsedTime / 1.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
