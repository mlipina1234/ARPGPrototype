﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private int _maxEnemies;
    [SerializeField] private string _prefabName;
    [SerializeField] private float _spawnInterval;
    [SerializeField] private EnvironmentPhysics _startEnvironment;
    [SerializeField] private GameObject _playerPhysics;

    [SerializeField] private NavigationManager _navManager;
    [SerializeField] private bool canRespawnEnemies; //toggle object pooling
    private Dictionary<int, GameObject> _enemyPool;
    private float _timer;
    private int enemiesAlive;

	void Start ()
    {
        enemiesAlive = 0;
        _timer = _spawnInterval;
        if (_enemyPool != null) return;

        Debug.Log("Pool populating...");
        _enemyPool = new Dictionary<int, GameObject>();
        GameObject tempEnemy;
        for (int i = 0; i < _maxEnemies; i++)
        {
            tempEnemy = Instantiate(Resources.Load("Prefabs/Enemies/" + _prefabName)) as GameObject;
            //tempBullet.GetComponentInChildren<Rigidbody2D>().position = new Vector2(1000, 1000);
            tempEnemy.GetComponentInChildren<EntityAI>().navManager = _navManager;
            tempEnemy.GetComponentInChildren<EntityPhysics>().navManager = _navManager;
            tempEnemy.GetComponentInChildren<TestEnemyAI>().target = _playerPhysics;
            tempEnemy.GetComponentInChildren<EntityPhysics>()._spawner = this;
            tempEnemy.SetActive(false);
            _enemyPool.Add(tempEnemy.GetInstanceID(), tempEnemy);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        //pool contains "dead" enemies
        //goal of spawner is to have all enemies alive
        //only can spawn one every so often

        //time management (a skill I find myself lacking)
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else
        {
            _timer = 0f;
        }


        if (enemiesAlive == _maxEnemies)
        {
            //all enemies are alive, do nothin
        }
        else
        {
            //spawn a bad guy if timer is up
            if (_timer == 0)
            {
                GameObject tempEnemy = GetFromPool();
                EntityPhysics tempPhysics = tempEnemy.GetComponentInChildren<EntityPhysics>();
                tempPhysics.SetObjectElevation(_startEnvironment.GetTopHeight() + 0.5f);
                // tempPhysics.GetComponent<Rigidbody2D>().MovePosition((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset);
                tempPhysics.transform.parent.position = (Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset - new Vector2(0f, 2f);
                Debug.Log("<color=red>" + _startEnvironment + "</color>");
                Debug.Log((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset);
                tempPhysics.GetComponent<Rigidbody2D>().MovePosition((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset - new Vector2(0f, 2f));
                tempEnemy.GetComponentInChildren<TestEnemyAI>().SetPath(_startEnvironment);

                //move enemy into position

                _timer = _spawnInterval;
            }
            else
            {
                //if timer aint up, wait
            }
        }
        

	}





    //========================================| Object Pooling |======================================

    public GameObject GetFromPool()
    {
        foreach (KeyValuePair<int, GameObject> entry in _enemyPool)
        {
            if (!entry.Value.activeSelf)
            {
                enemiesAlive++;
                
                entry.Value.SetActive(true);
                Debug.Log("Deploying");
                return entry.Value;
            }
        }

        //if there are no more inactive objects
        Debug.Log("Expanding enemy pool (" + _prefabName + ")");
        GameObject tempEnemy = Instantiate(Resources.Load("Prefabs/Enemies/" + _prefabName)) as GameObject;
        //tempEnemy.GetComponentInChildren<BulletHandler>().SourceWeapon = this;
        _enemyPool.Add(tempEnemy.GetInstanceID(), tempEnemy);
        return tempEnemy;
    }

    public void ReturnToPool(int instanceID)
    {
        if (!canRespawnEnemies)
        {
            _enemyPool[instanceID].gameObject.SetActive(false);
            _enemyPool.Remove(instanceID);
            return;
        }

        //Debug.Log("Returning to Pool");
        if (_enemyPool.ContainsKey(instanceID))
        {
            if (_enemyPool[instanceID].activeSelf)
            {
                enemiesAlive--;
                Debug.Log("Retracting");
                //_enemyPool[instanceID].GetComponentInChildren<Rigidbody2D>().MovePosition((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset - new Vector2(0f, 5f));
                //_enemyPool[instanceID].GetComponentInChildren<Rigidbody2D>().position = (Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset - new Vector2(0f, 5f);
                //_bulletPool[instanceID].GetComponentInChildren<DynamicPhysics>().MoveCharacterPosition();
                _enemyPool[instanceID].SetActive(false);
            }
            else
            {
                Debug.Log("Attempting to return object to pool that is already in pool.");
            }
        }
        else
        {
            Debug.Log("Invalid InstanceID - Object not in pool.");
        }
    }



}