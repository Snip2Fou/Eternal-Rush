using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    private Vector3 max_position_generate = new Vector3(0,0,250); 
    [SerializeField] private GameObject terrain;

    [SerializeField] private List<GameObject> objectsList;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckForGenerate())
        {
            SpawnTerrain();
            max_position_generate.z += 250;
            transform.position = max_position_generate;
        }

        if (CheckForDestroy())
        {
            for (int i = 0; i < objectsList.Count; i++ )
            {
                if (objectsList[i].transform.position.z < playerTransform.position.z - 550)
                {
                    Destroy(objectsList[i]);
                    objectsList.RemoveAt(i);
                }
            }
        }

    }

    private bool CheckForGenerate()
    {
        return playerTransform.position.z >= max_position_generate.z - 250;
    }

    private bool CheckForDestroy()
    {
        return objectsList[0].transform.position.z < playerTransform.position.z - 550;
    }

    private void SpawnTerrain()
    {
        GameObject new_terrain = Instantiate(terrain, transform.position, Quaternion.identity);
        objectsList.Add(new_terrain);
    }
}
