using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    private List<GameObject> objectsList;

    [SerializeField] private Transform playerTransform;

    //Generate Variable
    private Vector3 maxPositionGenerate = new Vector3(0, 0, 250);
    [SerializeField] private Transform leftFenceTransform;
    [SerializeField] private Transform rightFenceTransform;

    [SerializeField] private GameObject terrainPrefabs;
    [SerializeField] private GameObject fencePrefabs;


    // Start is called before the first frame update
    void Start()
    {
        Instantiate(fencePrefabs, leftFenceTransform);
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckForGenerate())
        {
            SpawnTerrain();

            maxPositionGenerate.z += 250;
            transform.position = maxPositionGenerate;
        }

        if (CheckForDestroy())
        {
            StartCoroutine(Destroy());
        }

    }

    private bool CheckForGenerate()
    {
        return playerTransform.position.z >= maxPositionGenerate.z - 250;
    }

    private bool CheckForDestroy()
    {
        return objectsList[0].transform.position.z < playerTransform.position.z - 550;
    }
/*
    private IEnumerator Generate()
    {

    }*/

    private IEnumerator Destroy()
    {
        int i = 0;
        while (i < objectsList.Count)
        {
            if (objectsList[i].transform.position.z < playerTransform.position.z - 550)
            {
                Destroy(objectsList[i]);
                objectsList.RemoveAt(i);
            }
            else
            {
                break;
            }
            i++;
            yield return null;
        }
    }

    private void SpawnTerrain()
    {
        GameObject new_terrain = Instantiate(terrainPrefabs, transform.position, Quaternion.identity);
        objectsList.Add(new_terrain);
    }
}
