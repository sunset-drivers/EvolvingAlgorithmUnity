using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class Monkey : MonoBehaviour
{
    [Header("Monkey Information")]
    public float m_Life = 100;
    public float m_Lifetime = 0.0f;    
    public float m_Hunger = 10;
    public float m_VisionDistance = 5f;
    public float m_JumpHeight = 0.5f;
    public bool m_CanReproduce = false;

    [Header("Behaviour Infomation")]
    public EnvironmentManager m_Environment; 
    public GameObject m_SeekingObject;
    public float m_DestinationDistanceAccuracy = 1.0f;

    [Header("Layers")]
    public LayerMask m_MonkeyLayer;
    public LayerMask m_BananaLayer;
    public LayerMask m_GroundLayer;
    
    [Header("Components")]
    public NavMeshAgent m_Agent;
    public GameObject m_DefaultMonkey;   
    
    private void Awake() {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Environment = GameObject.Find("EnvironmentManager").GetComponent<EnvironmentManager>();
        m_Environment.m_Monkeys.Add(this.gameObject);
        StartCoroutine("Growing");
    }

    private IEnumerator Growing()
    {
        gameObject.transform.localScale = Vector3.zero;
        bool m_Born = false;
        do
        {
            gameObject.transform.localScale = new Vector3(
                gameObject.transform.localScale.x + Time.deltaTime,
                gameObject.transform.localScale.y + Time.deltaTime,
                gameObject.transform.localScale.z + Time.deltaTime
            );

            if (gameObject.transform.localScale.x >= 1f)
                m_Born = true;

            yield return null;
        } while (!m_Born);
    }
 
    private void Update() {
        m_Lifetime += Time.deltaTime;
        m_Life -= Time.deltaTime * m_Hunger;
        if (m_Life <= 0.0f) Destroy(gameObject);

        if (m_Life <= 50)
            Seek(m_BananaLayer);
        else if (m_CanReproduce)
            Seek(m_MonkeyLayer);
        else
            Wander();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Banana"))
            Eat(collision.gameObject);
        if (collision.gameObject.CompareTag("Monkey") && m_CanReproduce)
            if(collision.gameObject.GetComponent<Monkey>().m_CanReproduce)
                Reproduce(collision.gameObject.GetComponent<Monkey>());
    }

    private void OnDestroy()
    {        
        m_Environment.m_Monkeys.Remove(this.gameObject);
    }

    public void Inherit(float _Hunger, float _VisionDistance, float _JumpHeight)
    {
        this.m_Hunger = _Hunger;
        this.m_VisionDistance = _VisionDistance;
        this.m_JumpHeight = _JumpHeight;
    }

    private void Reproduce(Monkey Partner)
    {
        Debug.Log("Reprodução");
        Partner.m_CanReproduce = false;
        m_CanReproduce = false;

        GameObject ChildBody = Instantiate(m_DefaultMonkey, transform.position, transform.rotation);
        ChildBody.GetComponent<Monkey>().Inherit(
            (m_Hunger + Partner.m_Hunger) / 2,
            (m_VisionDistance + Partner.m_VisionDistance) / 2,
            (m_JumpHeight + Partner.m_JumpHeight) / 2
        );

        Destroy(Partner.gameObject);
        Destroy(this.gameObject);
    }
    
    private void Eat(GameObject Food) {
        m_Environment.m_Bananas.Remove(Food);
        Destroy(Food);
        m_CanReproduce = true;
        m_Life = 100f;
    }

    private float GetDistanceBetweenObjects(Vector3 Origin, Vector3 Object)
    {        
        return Vector3.Distance(Origin, Object);
    }

    private void Wander() {
        if(GetDistanceBetweenObjects(transform.position, m_Agent.destination) <= m_DestinationDistanceAccuracy)
        {
            var target = transform.position + UnityEngine.Random.insideUnitSphere * 5.0f;
            target.y = 0.5f;
            m_Agent.SetDestination(target);
        }        
    }

    private void Seek(LayerMask layer) {
        Collider[] _CollidersFound = Physics.OverlapSphere(transform.position, m_VisionDistance, layer);
        Collider _NearestCollider = null;
        float _NearestDistance = 0.0f;
        foreach (Collider _Collider in _CollidersFound)
        {
            if(_Collider.gameObject != this.gameObject)
            {            
                if (_NearestCollider == null) { 
                    _NearestCollider = _Collider;
                    _NearestDistance = GetDistanceBetweenObjects(transform.position, _Collider.transform.position);
                }
                else {
                    float _Distance = GetDistanceBetweenObjects(transform.position, _Collider.transform.position);
                    if(_Distance < _NearestDistance) {
                        _NearestCollider = _Collider;
                        _NearestDistance = _Distance;
                    }                    
                }
            }
        }

        if(_NearestCollider != null && m_Agent.isActiveAndEnabled)
        {
            m_SeekingObject = _NearestCollider.gameObject;
            m_Agent.SetDestination(m_SeekingObject.transform.position);
        }            
        else
            Wander();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, m_VisionDistance);
    }
}
