using UnityEngine;

namespace WaterSort.Core
{
    /// <summary>
    /// Shows a short message in whichever scene is active (game HUD or main menu)
    /// </summary>
    public static class Toast
    {
        public static void Show(string message, float duration = 2f)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage(message, duration);
            else if (MainMenuUI.Instance != null)
                MainMenuUI.Instance.ShowMessage(message, duration);
            else
                Debug.Log(message);
        }
    }
}
