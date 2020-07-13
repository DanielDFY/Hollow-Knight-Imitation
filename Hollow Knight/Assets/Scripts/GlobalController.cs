using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalController : MonoBehaviour
{
    // Singleton
    public static GlobalController Instance { get; private set; }

    public GameObject player;
    public string nextScene;

    void Awake()
    {
        Instance = this;
    }
}
