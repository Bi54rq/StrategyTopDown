using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderUI : MonoBehaviour
{
    public Slider volumeSlider;

    private void Start()
    {
        if (volumeSlider == null) return;

        if (AudioManager.Instance != null)
            volumeSlider.value = AudioManager.Instance.GetVolume();

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume(value);
    }
}