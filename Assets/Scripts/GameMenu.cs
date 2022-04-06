using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameMenu : MonoBehaviour
{
    public bool isGameStart = true;
    public Button startGameButton;
    void Start()
    {
        Button btn = startGameButton.GetComponent<Button>();
        btn.onClick.AddListener(LoadLevel1);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadLevel1()
    {
        SceneManager.LoadScene("Level1");
    }
}
