using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LaunchOverlay : MonoBehaviour
{
    [SerializeField] private Image powerFill;
    [SerializeField] private TMP_Text hint;

    private void Awake()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetPower(0f, true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetPower(float power, bool clockwise)
    {
        if (powerFill == null)
            return;

        powerFill.fillClockwise = clockwise;
        powerFill.fillAmount = Mathf.Clamp01(power);
    }
}
