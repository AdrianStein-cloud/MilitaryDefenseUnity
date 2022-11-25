using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTank : Enemy
{
    public GameObject soldierPrefab;
    public int numberOfSoldierToSpawn = 4;

    public override void TakeDamage(float damagePoint){
        health -= damagePoint;
        if(health <= 0 && !dead){
            dead = true;
            gameMaster.UpdateMoney(worth);
            for (int i = 0; i < numberOfSoldierToSpawn; i++)
            {
                Instantiate(soldierPrefab).GetComponent<Enemy>().SetDistanceTravelled(this.GetDistanceTravelled() - (((float)i/3f) - 1));
            }
            
            Destroy(this.gameObject);
        }
        healthBar.UpdateHealthBar(health, maxHealth);
    }
}
