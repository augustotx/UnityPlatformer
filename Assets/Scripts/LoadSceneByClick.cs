using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickToLoadScene : MonoBehaviour
{
    [Header("Scene to Load")]
    [SerializeField] private string sceneName = "Lvl1";
    [SerializeField] private float delay = 0f;

    private bool loading = false;

    private void Update()
    {
        if (!loading && Input.GetMouseButtonDown(0))
        {
            loading = true;
            LoadTargetScene();
        }
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty! Please assign a scene in the Inspector.");
            loading = false;
            return;
        }

        if (delay > 0f)
        {
            StartCoroutine(LoadWithDelay());
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private System.Collections.IEnumerator LoadWithDelay()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}