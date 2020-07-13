using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragPlayer : MonoBehaviour
{
    private MovingTrap _movingTrap;

    // Start is called before the first frame update
    void Start()
    {
        // the dragging speed is from script MovingTrap
        _movingTrap = gameObject.GetComponent<MovingTrap>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject != GlobalController.Instance.player)
            return;

        // drag the player on it
        Transform playerTransform = GlobalController.Instance.player.GetComponent<Transform>();
        Vector3 playerNewPosition = playerTransform.position;
        playerNewPosition.x += _movingTrap.movingSpeed * Time.deltaTime;
        playerTransform.position = playerNewPosition;
    }
}
