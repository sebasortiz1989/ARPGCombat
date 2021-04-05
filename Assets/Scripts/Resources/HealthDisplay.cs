using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Resources
{
    public class HealthDisplay : MonoBehaviour
    {
        // Cached references
        Health health;
        Text healthText;

        // String const
        private const string PLAYER_TAG = "Player";

        private void Awake()
        {
            health = GameObject.FindWithTag(PLAYER_TAG).GetComponent<Health>();
            healthText = GetComponent<Text>();      
        }

        void Update()
        {
            healthText.text = (String.Format("{0:0}%", health.GetPercentage()));
        }
    }
}