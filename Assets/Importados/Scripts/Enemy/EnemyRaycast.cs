using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRaycast : MonoBehaviour
{
    private Ray sight;
    [SerializeField] private float detectRange = 5f;

    private PlayerBehaviour player;

    private void FixedUpdate()
    {
        sight.origin = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        sight.direction = transform.forward;

        RaycastHit rayHit;

        if (Physics.Raycast(sight, out rayHit, detectRange))
        {
            Debug.DrawLine(sight.origin, rayHit.point, Color.red);

            player = rayHit.transform.GetComponent<PlayerBehaviour>();

            if (player != null)
            {
                GetComponent<EnemyAI>().TargetDetected();
            }
        }
    }
}
