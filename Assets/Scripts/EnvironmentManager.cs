using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Spawners")]
    public Collider m_AreaA;
    public Collider m_AreaB;

    [Header("Spawnables")]
    public bool m_FinishedSpawning = false;
    public GameObject[] m_MonkeyPrefabs;
    public GameObject m_BananaPrefab;

    [Header("Environment")]
    public List<GameObject> m_Monkeys;
    public List<GameObject> m_Bananas;

    [Header("LastGeneration")]
    private bool m_HasBestFromLastGeneration = false;
    public float m_BestLifetime;
    public float m_BestHungerResistence;
    public float m_BestVisionDistance;
    public float m_BestJumpHeight;

    [Header("Conditions")]
    public int m_MaxPopulationSize = 10;
    public int m_MaxGeneration = 5;
    public int m_Generation = 0;

    [Header("Debug")]
    [Range(0.0f, 100f)] public float m_SceneSpeed = 1f;
    public Text txtGeneration;
    public Text txtBestLifetime;
    public Text txtBestHungerResistence;
    public Text txtBestVisionDistance;
    public Text txtBestJumpHeight;
    public Text txtTime;

    private void Update()
    {
        Time.timeScale = m_SceneSpeed;
        UpdateUI();
        
        if (m_Monkeys.Count == 1 && m_FinishedSpawning)
            m_HasBestFromLastGeneration = GetMonkeyInfo(m_Monkeys[0].GetComponent<Monkey>());

        if(m_Generation < m_MaxGeneration)
        {
            if (m_Monkeys.Count == 0)
            {
                if (m_HasBestFromLastGeneration)
                    m_Generation++;

                StartCoroutine("SpawnMonkeys");
            }

            if (m_Bananas.Count <= m_Monkeys.Count)
                StartCoroutine("SpawnBananas");
        }        
    }

    private IEnumerator SpawnMonkeys()
    {
        m_FinishedSpawning = false;
        do
        {
            GameObject _Monkey = GetRandomMonkey();
            Vector3 _position = GetPositionInCollider(m_AreaA);
            GameObject _SpawnedMonkey = Instantiate(_Monkey, _position, Quaternion.identity);            
            yield return null;
        } while (m_Monkeys.Count < m_MaxPopulationSize);
        m_FinishedSpawning = true;
    }

    private IEnumerator SpawnBananas()
    {
        do
        {
            Collider m_Area = GetRandomArea();
            Vector3 _position = GetPositionInCollider(m_Area);
            GameObject _SpawnedBanana = Instantiate(m_BananaPrefab, _position, Quaternion.identity);
            m_Bananas.Add(_SpawnedBanana);
            yield return null;
        } while (m_Bananas.Count < m_Monkeys.Count);
    }

    private Collider GetRandomArea() {
        float _RandomFloat = Random.Range(0.0f, 100f);
        
        float m_GenerationClamp = (m_Generation >= 100) ? 100 : m_Generation;

        if(_RandomFloat >= 100 - m_GenerationClamp)
            return m_AreaB;
        else
            return m_AreaA;        
    }


    public bool GetMonkeyInfo(Monkey monkey)
    {
        m_BestLifetime = (monkey.m_Lifetime > m_BestLifetime) ? monkey.m_Lifetime : m_BestLifetime;
        m_BestHungerResistence = monkey.m_Hunger;
        m_BestVisionDistance = monkey.m_VisionDistance;
        Destroy(monkey.gameObject);
        return true;
    }

    private void UpdateUI()
    {
        txtGeneration.text = $"Generation: {m_Generation}";        
        if (m_HasBestFromLastGeneration)
        {
            txtBestLifetime.text = $"Best Lifetime: {m_BestLifetime}";
            txtBestHungerResistence.text = $"Best Hunger Resistence: {m_BestHungerResistence}";            
            txtBestVisionDistance.text = $"Best Vision Distance: {m_BestVisionDistance}";
            txtBestJumpHeight.text = $"Best Jump Height: {m_BestJumpHeight}";
        }
    }

    private GameObject InheritLastGenerationInfo(GameObject MonkeyPrefab)
    {        
        Monkey _NewMonkeyInfo = MonkeyPrefab.GetComponent<Monkey>();
        _NewMonkeyInfo.Inherit(
            (_NewMonkeyInfo.m_Hunger + m_BestHungerResistence) / 2,
            (_NewMonkeyInfo.m_VisionDistance + m_BestVisionDistance) / 2,
            (_NewMonkeyInfo.m_JumpHeight + m_BestJumpHeight) / 2
        );

        return MonkeyPrefab;
    }

    private GameObject GetRandomMonkey()
    {
        int index = Helper.RandomInt(m_MonkeyPrefabs.Length);

        GameObject _NewMonkey = (m_HasBestFromLastGeneration) 
            ? InheritLastGenerationInfo(m_MonkeyPrefabs[index]) 
            : m_MonkeyPrefabs[index];

        return _NewMonkey;
    }

    private Vector3 GetPositionInCollider(Collider Area)
    {
        Vector3 _Position = new Vector3(
            Random.Range(Area.bounds.min.x, Area.bounds.max.x),
            Random.Range(Area.bounds.min.y, Area.bounds.max.y),
            Random.Range(Area.bounds.min.z, Area.bounds.max.z)
        );
        return _Position;
    }
}
