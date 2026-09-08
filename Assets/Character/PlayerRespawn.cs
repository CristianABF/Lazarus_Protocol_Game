using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform respawnP;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (respawnP == null) return;

        if (transform.position.y < -5.0f)
        {
            if (controller != null)
            {
                controller.enabled = false;
                transform.position = respawnP.position;
                controller.enabled = true;
            }
            else
            {
                transform.position = respawnP.position;
            }
        }
    }
}
