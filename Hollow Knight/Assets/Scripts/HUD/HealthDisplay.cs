using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public GameObject heart1;
    public GameObject heart2;
    public GameObject heart3;
    public GameObject heart4;
    public GameObject heart5;

    public Sprite healthFull;
    public Sprite healthEmpty;

    private Image[] _hearts;
    private PlayerController playerController;

    void Start()
    {
        _hearts = new Image[5];

        _hearts[0] = heart1.GetComponent<Image>();
        _hearts[1] = heart2.GetComponent<Image>();
        _hearts[2] = heart3.GetComponent<Image>();
        _hearts[3] = heart4.GetComponent<Image>();
        _hearts[4] = heart5.GetComponent<Image>();

        playerController = GlobalController.Instance.player.GetComponent<PlayerController>();
    }

    void Update()
    {
        int healthRemain = playerController.health;
        for (int i = 0; i < healthRemain; ++i)
        {
            _hearts[i].sprite = healthFull;
        }

        for (int i = healthRemain; i < 5; ++i)
        {
            _hearts[i].sprite = healthEmpty;
        }
    }
}
