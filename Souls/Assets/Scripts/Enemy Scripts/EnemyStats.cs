using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SL {
    public class EnemyStats : MonoBehaviour
    {
        public int healthLevel = 10;
        public int maxHealth;
        public int currentHealth;

        Animator animator;

        private void Awake() {
            animator = GetComponentInChildren<Animator>();
        }

        private void Start() {
            maxHealth = SetMaxHealthFromHealthLevel();
            currentHealth = maxHealth;
        }

        private int SetMaxHealthFromHealthLevel() {
            maxHealth = healthLevel * 10;
            return maxHealth;
        }

        private void Update() {
            animator.Play("Idle");
        }

        public void TakeDamage(int damage) {
            currentHealth = currentHealth - damage;

            animator.Play("Damage_01");

            if (currentHealth <= 0) {
                currentHealth = 0;
                animator.Play("Dead_01");
                // Handle Player Death
            }
        }
    }
}