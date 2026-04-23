using UnityEngine;

public class TestSoundButton : MonoBehaviour
{
    public void PlayTestSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTestSound();
    }
}