using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButtonActions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //public void OnStartGame()
    //{
    //    SceneManager.LoadScene(1);
    //}
    public void OnClick()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
