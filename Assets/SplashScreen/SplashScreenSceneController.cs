using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenSceneController : MonoBehaviour
{
    // Replace these with the actual scene names for the server and client
    public const string serverScene = "ServerScene";
    public const string clientScene = "ClientScene";

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if UNITY_SERVER
        StartCoroutine(LoadSceneAsync(serverScene)); 
#else
        StartCoroutine(LoadSceneAsync(clientScene));
#endif

    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
