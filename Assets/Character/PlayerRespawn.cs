using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform respawnP;
    private CharacterController controller;
    PlayerBehaviour playerBehaviour;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerBehaviour = GetComponent<PlayerBehaviour>();
    }

    void Update()
    {
        if (respawnP == null) return;

        if (transform.position.y < -5.0f || playerBehaviour.GetState() == true)
        {
            pRespawn();
        }
    }

    void pRespawn()
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
        playerBehaviour.ResetState();
    }
}
