using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] private float health;
    [SerializeField] private Transform head;
    [SerializeField] private AudioSource hitSound;

    private void Update()
    {
        if (Input.GetKey("escape"))
        {
            ExitGame();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // TODO: Make so that when the health reach 0, game over menu is displayed
    public void TakeDamage(float damage)
    {
        health -= damage;
        hitSound.Play();
        Debug.LogError(string.Format("Player health: {0}", health));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("pistol_bullet"))
        {
            Weapon weapon = other.GetComponent<PhysicsProjectile>().GetWeapon();
            TakeDamage(weapon.GetDamage());
        }
    }

    public Vector3 GetHeadPosition()
    {
        return head.position;
    }
    
    // method to reset the scene
    public static void ResetScene()
    {
        // Resetting scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
