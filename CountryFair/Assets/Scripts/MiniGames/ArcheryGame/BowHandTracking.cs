using UnityEngine;
using Oculus.Interaction.Input;

public class BowHandTracking : MonoBehaviour
{
    public OVRHand rightHand;
    public Transform arrowSpawn;
    public GameObject arrowPrefab;

    private GameObject currentArrow;
    private bool arrowReady = false;

    // Thresholds baseados nos teus valores reais
    private float indexClose = 0.15f;
    private float middleClose = 0.30f;
    private float ringClose = 0.20f;

    private float openThreshold = 0.10f;

    void Update()
    {
        float index = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float middle = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float ring = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);

        Debug.Log($"Index: {index:F2} | Middle: {middle:F2} | Ring: {ring:F2}");

        bool handClosed = IsHandClosed();
        bool handOpen = IsHandOpen();

        // PREPARA A SETA
        if (handClosed && !arrowReady)
            PrepareArrow();

        // DISPARA A SETA
        if (arrowReady && handOpen)
            FireArrow();
    }

    void PrepareArrow()
    {
        arrowReady = true;
currentArrow = Instantiate(arrowPrefab, arrowSpawn.position, arrowSpawn.rotation, arrowSpawn);
    }

    void FireArrow()
    {
        if (currentArrow)
        {
            Rigidbody rb = currentArrow.GetComponentInChildren<Rigidbody>();
            rb.isKinematic = false;

            rb.AddForce(arrowSpawn.forward * 20f, ForceMode.VelocityChange);

            currentArrow = null;
        }

        arrowReady = false;
    }

    bool IsHandClosed()
    {
        float index = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float middle = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float ring = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);

        // mão fechada = qualquer dedo ultrapassa o limiar de fechar
        return index > indexClose || middle > middleClose || ring > ringClose;
    }

    bool IsHandOpen()
    {
        float index = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float middle = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float ring = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);

        // mão aberta = todos os dedos abaixo do threshold
        return index < openThreshold && middle < openThreshold && ring < openThreshold;
    }
}
