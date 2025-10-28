using UnityEngine;
using UnityEngine.SceneManagement;

public class AbilitySelectController : MonoBehaviour
{
    // abilityIndex:
    // 0 = None, 1 = EnhancedHealth, 2 = SlowFalling, 3 = SuperJump
    public void ChooseAndLoad(int abilityIndex)
    {
        // Save the selected ability
        PlayerPrefs.SetInt("ability", abilityIndex);
        PlayerPrefs.Save();

        // Load your game scene
        SceneManager.LoadScene("SampleScene");
    }
}
