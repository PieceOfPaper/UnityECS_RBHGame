using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Scenes;
using UnityEngine.SceneManagement;

public class SampleScene : MonoBehaviour
{
    public GameObject mainUI;
    public GameObject resultUI;

    public enum StepType
    {
        None,
        Start,
        Play,
        Result,
    }

    public StepType step;

    public ECSPlayerData lastPlayerData;
    public float playTime;

    private void Awake()
    {
        lastPlayerData = default;
        step = StepType.None;
        mainUI.SetActive(true);
        resultUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void LateUpdate()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(ECSPlayerData), typeof(ECSCharacterData), typeof(ECSShootableData), typeof(LocalTransform));
        if (query.IsEmpty == false)
        {
            var entity = query.GetSingletonEntity();
            var playerData = entityManager.GetComponentData<ECSPlayerData>(entity);

            lastPlayerData = playerData;

            if (step == StepType.Start)
            {
                step = StepType.Play;
            }
            
        }
        else
        {
            if (step == StepType.Play)
            {
                step = StepType.Result;
                Invoke("OpenResult", 1.0f);
            }
        }
        
        if (step == StepType.Play)
            playTime += Time.deltaTime;
    }

    private void OpenResult()
    {
        resultUI.SetActive(true);
        resultUI.GetComponent<UIResult>().SetData(playTime, lastPlayerData);
    }

    public void OnClick_Start()
    {
        lastPlayerData = default;
        step = StepType.Start;
        playTime = 0f;
        
        mainUI.SetActive(false);
        resultUI.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene("PlayScene", LoadSceneMode.Additive);
    }

    public void OnClick_GoToMain()
    {
        lastPlayerData = default;
        step = StepType.None;
        
        mainUI.SetActive(true);
        resultUI.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("PlayScene");
    }
}
