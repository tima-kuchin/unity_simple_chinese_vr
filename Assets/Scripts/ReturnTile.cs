using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class ReturnToStartPosition : MonoBehaviour
{
    public Vector3 initialPosition { get; private set; }
    private Quaternion initialRotation;
    private XRGrabInteractable grabInteractable;
    private Coroutine returnCoroutine;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        grabInteractable = GetComponent<XRGrabInteractable>();


        if (grabInteractable == null)
        {
            Debug.LogWarning("XR Grab Interactable component is not found on this object.");
        }
        else
        {
            grabInteractable.onSelectExited.AddListener(OnReleased);
        }
    }

    void OnReleased(XRBaseInteractor interactor)
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        returnCoroutine = StartCoroutine(ReturnAfterDelay());
    }

    IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        ReturnToInitialPosition();
    }

    void ReturnToInitialPosition()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}