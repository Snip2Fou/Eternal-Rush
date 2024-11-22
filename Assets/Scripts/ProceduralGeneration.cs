using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    private List<GameObject> objectsList = new List<GameObject>();

    [SerializeField] private Transform playerTransform;

    //Generate Variable
    private bool isGenerating = false;
    private Vector3 maxPositionGenerate = new Vector3(0, 0, 250);
    [SerializeField] private Transform globalTransform;
    [SerializeField] private Transform leftFenceTransform;
    [SerializeField] private Transform rightFenceTransform;

    [SerializeField] private GameObject terrainPrefabs;
    [SerializeField] private List<GameObject> prefabs;

    private bool isDestroying = false;

    // Start is called before the first frame update
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckForGenerate() && !isGenerating)
        {
            StartCoroutine(Generate());
        }

        if (CheckForDestroy() && !isDestroying)
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

    private IEnumerator Generate()
    {
        isGenerating = true;
        SpawnTerrain();

        while (leftFenceTransform.position.z <= maxPositionGenerate.z + 250 || rightFenceTransform.position.z <= maxPositionGenerate.z + 250) 
        {
            GenerateFence();

            yield return null;
        }
        maxPositionGenerate.z += 250;
        isGenerating = false;
    }

    private void GenerateFence()
    {
        if (leftFenceTransform.position.z <= maxPositionGenerate.z + 250)
        {
            Bounds bounds = CalculateObjectBounds(prefabs[0]);
            GameObject new_left_fence = Instantiate(prefabs[0], new Vector3(leftFenceTransform.position.x, leftFenceTransform.position.y, leftFenceTransform.position.z + (bounds.size.z / 2)), Quaternion.Euler(-90, 90, 0), leftFenceTransform.parent);
            objectsList.Add(new_left_fence);
            leftFenceTransform.SetPositionAndRotation(new Vector3(leftFenceTransform.position.x, leftFenceTransform.position.y, leftFenceTransform.position.z + bounds.size.z), Quaternion.identity);
        }

        if (rightFenceTransform.position.z <= maxPositionGenerate.z + 250)
        {
            Bounds bounds = CalculateObjectBounds(prefabs[0]);
            GameObject new_right_fence = Instantiate(prefabs[0], new Vector3(rightFenceTransform.position.x, rightFenceTransform.position.y, rightFenceTransform.position.z + (bounds.size.z / 2)), Quaternion.Euler(-90, -90, 0), rightFenceTransform.parent);
            objectsList.Add(new_right_fence);
            rightFenceTransform.SetPositionAndRotation(new Vector3(rightFenceTransform.position.x, rightFenceTransform.position.y, rightFenceTransform.position.z + bounds.size.z), Quaternion.identity);
        }
    }

    private IEnumerator Destroy()
    {
        isDestroying = true;
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
        isDestroying = false;
    }

    private void SpawnTerrain()
    {
        GameObject new_terrain = Instantiate(terrainPrefabs, maxPositionGenerate, Quaternion.identity);
        objectsList.Add(new_terrain);
    }

    private Bounds CalculateObjectBounds(GameObject obj)
    {
        List<Renderer> renderers = new List<Renderer>(obj.GetComponentsInChildren<Renderer>());

        if (obj.GetComponent<Renderer>() != null) 
        {
            renderers.Add(obj.GetComponent<Renderer>());
        }

        if (renderers.Count == 0)
        {
            return new Bounds(obj.transform.position, Vector3.zero);
        }

        Bounds totalBounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.name.Contains("Floor"))
            {
                totalBounds.Encapsulate(renderer.bounds);
            }
        }

        return totalBounds;
    }
}
