using UnityEngine;

public class MicHoldFollow : MonoBehaviour
{
    private Holdable holdable;
    private bool locked = false;

    void Start()
    {
        holdable = GetComponent<Holdable>();
    }

    void Update()
    {
        if (locked || holdable == null) return;

        if (holdable.moving && holdable.myRayManipulator != null)
        {
            locked = true;
            var ri = holdable.myRayManipulator;

            // Disable collider so raycast can't hit it again
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Disable interaction so it can't be clicked/dropped
            var ig = GetComponent<InteractableGeneral>();
            if (ig != null) ig.enabled = false;

            // Detach from RaycastInteractor without dropping
            // Mic stays parented to pickupAttachPoint, following the player
            holdable.moving = false;
            ri.holdableSubject = null;
            ri.subject = null;
            holdable.myRayManipulator = null;

            Debug.Log("MicHold locked to player, following camera");
        }
    }
}
