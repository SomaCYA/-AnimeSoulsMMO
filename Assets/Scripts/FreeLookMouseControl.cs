using UnityEngine;
using Cinemachine;

public class FreeLookMouseControl : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;
    public string mouseXInputName = "Mouse X";
    public string mouseYInputName = "Mouse Y";

    void Update()
    {
        if (Input.GetMouseButton(1)) // rechte Maustaste gedrückt
        {
            freeLookCamera.m_XAxis.m_InputAxisName = mouseXInputName;
            freeLookCamera.m_YAxis.m_InputAxisName = mouseYInputName;
        }
        else
        {
            freeLookCamera.m_XAxis.m_InputAxisName = "";
            freeLookCamera.m_YAxis.m_InputAxisName = "";
            freeLookCamera.m_YAxis.m_InputAxisValue = 0; // Optional: Höhe zurücksetzen
        }
    }
}
