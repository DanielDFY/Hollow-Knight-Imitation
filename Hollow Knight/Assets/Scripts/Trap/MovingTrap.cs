using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTrap : Trap
{
    public float movingSpeed;
    public float movingLimit;
    public float movingOffset;

    private Vector3 basePosition;

    private Transform _transform;

    // Start is called before the first frame update
    void Start()
    {
        _transform = gameObject.GetComponent<Transform>();
        basePosition = _transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float newOffset = movingOffset + Time.deltaTime * movingSpeed;
        if (Math.Abs(newOffset) >= movingLimit)
        {
            movingSpeed = -movingSpeed;
            basePosition.x += movingOffset;
            movingOffset = 0;
        }
        else
        {
            movingOffset = newOffset;
        }

        Vector3 newPosition = basePosition;
        newPosition.x += movingOffset;
        _transform.position = newPosition;
    }

    public override void trigger()
    {
        
    }
}
