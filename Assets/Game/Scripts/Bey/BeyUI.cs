using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BeyUI : MonoBehaviour
{
    [SerializeField]
    Image rpmBar;

    [SerializeField]
    TMP_Text rpmText;

    BeyData data;

    public void Initialize(BeyData data)
    {
        this.data = data;

        data.OnRPMChanged += UpdateRPM;

        UpdateRPM(data.CurrentRPM);

        Debug.Log($"{gameObject.name} UI initialized with {data.CurrentRPM} RPM");
    }

    private void UpdateRPM(float rpm)
    {
        rpmBar.fillAmount =
            rpm / data.Base.maxRPM;

        rpmText.text =
            Mathf.RoundToInt(rpm).ToString();
        rpmText.ForceMeshUpdate();

        Debug.Log($"{gameObject.name} RPM updated: {rpm} RPM");
    }
}