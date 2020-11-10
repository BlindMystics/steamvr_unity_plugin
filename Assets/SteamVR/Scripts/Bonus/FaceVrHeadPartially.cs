using UnityEngine;
using Valve.VR.InteractionSystem;

public class FaceVrHeadPartially : MonoBehaviour {

    [Range(0f, 1f)]
    public float amount = 0.25f;

    private void LateUpdate() {

        Vector3 lookAtPosition = Player.instance.headCollider.transform.position;
        Vector3 ourPosition = transform.position;

        Vector3 toLookAtPosition = (lookAtPosition - ourPosition).normalized;
        Quaternion lookAtPlayerHeadRotation = Quaternion.LookRotation(toLookAtPosition, Vector3.up);

        Quaternion baseRotation = transform.parent.rotation;
        float angleBetween = Quaternion.Angle(baseRotation, lookAtPlayerHeadRotation);

        float usedAmount = amount;
        if (angleBetween > 90.0f) {
            float lerpAmount = Mathf.Clamp01((angleBetween - 90.0f) / 90.0f);
            usedAmount = Mathf.Lerp(amount, 0.0f, lerpAmount);
        }

        transform.rotation = Quaternion.Slerp(baseRotation, lookAtPlayerHeadRotation, usedAmount);

    }
}
