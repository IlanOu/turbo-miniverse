using System;
using System.Collections;
using UnityEngine;

[Serializable]
public struct CameraPreset
{
    public Vector3 position;
    public Quaternion rotation;
}

public class CameraPresets : MonoBehaviour
{
    [Header("Presets")]
    [SerializeField] private CameraPreset[] cameraPositionsPresets;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine currentTransition;

    /// <summary>
    /// Déplacement instantané de la caméra.
    /// </summary>
    public void SetCameraPosition(int index)
    {
        transform.position = cameraPositionsPresets[index].position;
        transform.rotation = cameraPositionsPresets[index].rotation;
    }

    /// <summary>
    /// Démarre une transition fluide vers le preset choisi.
    /// </summary>
    public void SmoothToCameraPosition(int index)
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(SmoothCameraPosition(index));
    }

    /// <summary>
    /// Coroutine avec interpolation personnalisée via AnimationCurve.
    /// </summary>
    private IEnumerator SmoothCameraPosition(int index)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 targetPosition = cameraPositionsPresets[index].position;
        Quaternion targetRotation = cameraPositionsPresets[index].rotation;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            float curvedT = transitionCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, curvedT);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curvedT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }
}
