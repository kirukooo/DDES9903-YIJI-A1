using UnityEngine;

public class StageTrigger : MonoBehaviour
{
    public float triggerRadius = 1.5f;
    public string playerTag = "Player";

    private bool triggered = false;
    private Transform player;

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag(playerTag);
        if (playerGO != null)
            player = playerGO.transform;
    }

    void Update()
    {
        if (triggered || player == null)
            return;

        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x, 0, player.position.z));

        if (dist <= triggerRadius)
        {
            triggered = true;
            // Play "Full" audio via AudioManager
            AudioManager.Instance.PlaySound("Full");
        }
    }
}
