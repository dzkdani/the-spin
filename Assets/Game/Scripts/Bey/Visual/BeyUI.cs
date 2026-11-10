using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BeyUI : MonoBehaviour
{
    [SerializeField] private Image rpmBar;
    [SerializeField] private TMP_Text rpmText;

    private BeyData data;

    private Vector3 originalScale;
    private Color originalTextColor;
    private float previousRPM;

    private void Awake()
    {
        if (rpmBar == null || rpmText == null)
        {
            Debug.LogError($"{name}: BeyUI references are not assigned.");
            enabled = false;
            return;
        }

        originalScale = rpmBar.transform.localScale;
        originalTextColor = rpmText.color;
    }

    public void Bind(BeyData newData)
    {
        if (data != null)
            data.OnRPMChanged -= UpdateRPM;

        data = newData;

        if (data == null)
            return;

        previousRPM = data.CurrentRPM;

        data.OnRPMChanged += UpdateRPM;

        UpdateRPM(data.CurrentRPM);
    }

    private void Update()
    {
        if (data == null)
            return;

        rpmBar.transform.localScale = Vector3.Lerp(
            rpmBar.transform.localScale,
            originalScale,
            Time.deltaTime * 12f);

        rpmText.color = Color.Lerp(
            rpmText.color,
            originalTextColor,
            Time.deltaTime * 8f);
    }

    private void UpdateRPM(float rpm)
    {
        if (data == null || data.Base == null || data.Base.maxRPM <= 0f)
        {
            Debug.LogError("BeyUI cannot display RPM without valid BeyData tuning.", this);
            return;
        }

        rpmBar.fillAmount = rpm / data.Base.maxRPM;
        rpmText.text = Mathf.RoundToInt(rpm).ToString();

        if (rpm < previousRPM)
        {
            rpmBar.transform.localScale = originalScale * 1.15f;
            rpmText.color = Color.red;
        }

        previousRPM = rpm;
    }

    private void OnDestroy()
    {
        if (data != null)
            data.OnRPMChanged -= UpdateRPM;
    }
}
