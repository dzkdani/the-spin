using UnityEngine;

public class BeyDebug : MonoBehaviour
{
    [SerializeField]
    BeyData data;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            data.ReduceRPM(100);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            data.AddRPM(100);
        }
    }
}