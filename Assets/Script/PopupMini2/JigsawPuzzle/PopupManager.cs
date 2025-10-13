using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public GameObject popupSessionCanvas; // ¿ÜºÎ Ã¢¹®(PopupSessionCanvas)
    public MonoBehaviour playerMoveScript;   // ÇÃ·¹ÀÌ¾î ÀÌµ¿ ½ºÅ©¸³Æ®

    void Start()
    {
        popupSessionCanvas.SetActive(false); // ½ÃÀÛÇÒ ¶© ²¨µÒ
    }

    public void ShowPopup()
    {
        Time.timeScale = 0f; // °ÔÀÓ ½Ã°£ ¸ØÃã
        playerMoveScript.enabled = false;
        popupSessionCanvas.SetActive(true);
    }

    public void HidePopup()
    {
        popupSessionCanvas.SetActive(false);
        playerMoveScript.enabled = true;
        Time.timeScale = 1f; // °ÔÀÓ ½Ã°£ ´Ù½Ã Èå¸£°Ô
    }
}