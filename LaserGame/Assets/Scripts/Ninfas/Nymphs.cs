using UnityEngine;

public class Nymphs : MonoBehaviour
{
    [SerializeField] private GameObject nymphPrefab;
    [SerializeField] private int nymphCount = 5;
    CockroachController player;
    private Transform Playertarget;
    void Start()
    {
        player = CockroachController.instance;
    }

    void Update()
    {
        
    }
    void FixedUpdate()
    {
        Playertarget = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }
    public void SpawnNymphs()
    {
        for (int i = 0; i < nymphCount; i++)
        {
            Vector3 spawnPosition = player.transform.position + Random.insideUnitSphere * 2f;
            spawnPosition.y = player.transform.position.y; 
            Instantiate(nymphPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void FollowTarget(Transform target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position, 2f * Time.deltaTime);
    }

}
