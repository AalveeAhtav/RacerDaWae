//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Minimap : MonoBehaviour
//{

//    public Transform player;

//    void LateUpdate()
//    {
//        Vector3 newPosition = player.position;
//        newPosition.y = transform.position.y;
//        transform.position = newPosition;

//        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
//    }

//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public Transform player;

    void Start()
    {
        // Auto-find player if not assigned in Inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Minimap: No player assigned and no GameObject with tag 'Player' found!");
            }
        }
    }

    void LateUpdate()
    {
        // Guard clause prevents the null ref crash
        if (player == null) return;

        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}