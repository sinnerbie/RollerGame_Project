using System;
using System.Collections;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;
    public float currentHealth
    {
        get
        {
            return health;
        }

        set
        {
            health = value;
            CheckHealth();
        }
    }

    [SerializeField] private bool isPlayer;
    [SerializeField] private Vector3 spawnPointPos;
    [SerializeField] private Quaternion spawnPointRot;
    private IEnumerator respawning;

    private void Start()
    {
        spawnPointPos = gameObject.transform.position;
        spawnPointRot = gameObject.transform.rotation;
        health = maxHealth;
    }

    public static Action OnKillPlayer;
    private void CheckHealth()
    {
        if (health <= 0)
        {
            // Die
            if (isPlayer)
            {
                OnKillPlayer?.Invoke();
                respawning = RespawnPlayer();
                StartCoroutine(respawning);
            }
        }
    }

    public static Action OnPlayerRespawn;
    IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(0.75f);
        currentHealth = maxHealth;
        transform.position = spawnPointPos;
        transform.rotation = spawnPointRot;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<PlayerMovement>().maxSpeedID = 0;
        OnPlayerRespawn?.Invoke();
        StopCoroutine(respawning);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("KillArea"))
        {
            currentHealth = 0;
        }
    }
}
