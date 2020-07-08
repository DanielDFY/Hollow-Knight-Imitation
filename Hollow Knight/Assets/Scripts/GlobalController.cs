using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalController : MonoBehaviour
{
    public static GlobalController Instance { get; private set; }

    public GameObject player;
    public string nextScene;

    void Awake()
    {
        Instance = this;
    }
}
