using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    private List<GameObject> objectsList = new List<GameObject>();

    [SerializeField] private Transform playerTransform;

    //Generate Variable
    private bool isGenerating = false;
    private bool generatingFence = false;
    private bool generatingObstacles = false;
    private Vector3 maxPositionGenerate = new Vector3(0, 0, 250);
    [SerializeField] private Transform obstaclesTransform;
    [SerializeField] private Transform leftFenceTransform;
    [SerializeField] private Transform rightFenceTransform;

    [SerializeField] private GameObject terrainPrefabs;
    [SerializeField] private List<GameObject> prefabs;

    //Grid 
    private int gridWidth = 7; 
    private int cellSize = 2; 
    public enum GridState
    {
        None,
        Path, 
        Obstacle
    }


    private bool isDestroying = false;

    // Start is called before the first frame update
    void Start()
    {
        int gridHeight = Mathf.RoundToInt((250 - obstaclesTransform.position.z) / cellSize);
        StartCoroutine(Generate(gridHeight));
    }

    // Update is called once per frame
    /*void Update()
    {
        if (CheckForGenerate() && !isGenerating)
        {
            int gridHeight = Mathf.RoundToInt((obstaclesTransform.position.z + 250) / cellSize);
            StartCoroutine(Generate(gridHeight));
        }

        if (CheckForDestroy() && !isDestroying)
        {
            StartCoroutine(Destroy());
        }

    }*/

    private bool CheckForGenerate()
    {
        return playerTransform.position.z >= maxPositionGenerate.z - 250;
    }

    private bool CheckForDestroy()
    {
        return objectsList[0].transform.position.z < playerTransform.position.z - 550;
    }

    private IEnumerator Generate(int gridHeight)
    {
        isGenerating = true;
        SpawnTerrain();

        generatingFence = true;
        StartCoroutine(GenerateFences());

        generatingObstacles = true;
        StartCoroutine(GenerateObstacles(gridHeight));

        while (generatingFence || generatingObstacles) 
        {
            yield return null;
        }

        maxPositionGenerate.z += 250;
        obstaclesTransform.position.Set(obstaclesTransform.position.x, obstaclesTransform.position.y, maxPositionGenerate.z);
        isGenerating = false;
    }

    private IEnumerator GenerateFences()
    {
        while (leftFenceTransform.position.z <= maxPositionGenerate.z + 250 || rightFenceTransform.position.z <= maxPositionGenerate.z + 250)
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
            yield return null;
        }
        generatingFence = false;
    }

    private IEnumerator GenerateObstacles(int gridHeight)
    {
        GridState[,] grid = GenerateGrid(gridWidth, gridHeight);

        yield return null;

        // Place les obstacles sur la grille
        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {



               
                 SpawnWoodRamp(new Vector3(obstaclesTransform.position.x + (x * cellSize) + (cellSize / 2), 0, obstaclesTransform.position.z + ((z + 1) * cellSize) + (cellSize / 2)));
                        
                    

                
            }
            yield return null;
        }
        generatingObstacles = false;
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

    private void SpawnWoodRamp(Vector3 _position)
    {
        GameObject new_obstacle = Instantiate(prefabs[1], new Vector3(_position.x, prefabs[1].transform.position.y, _position.z), prefabs[1].transform.rotation, obstaclesTransform.parent);
        objectsList.Add(new_obstacle);
    }

    private void SpawnWoodStack(Vector3 _position)
    {
        GameObject new_obstacle = Instantiate(prefabs[2], new Vector3(_position.x, prefabs[2].transform.position.y, _position.z), prefabs[2].transform.rotation, obstaclesTransform.parent);
        objectsList.Add(new_obstacle);
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
            totalBounds.Encapsulate(renderer.bounds);
        }

        return totalBounds;
    }

    GridState[,] GenerateGrid(int width, int height)
    {
        GridState[,] grid = new GridState[width, height];

        // Génère un chemin aléatoire traversable
        int currentX = width / 2;
        int currentX2 = width / 2 + 1;
        int direction = 0;
        int middle = width / 2;

        for (int z = 0; z < height; z++)
        {
            grid[currentX, z] = GridState.Path;
            grid[currentX2, z] = GridState.Path;

            // Décision aléatoire pour le déplacement du chemin
            if (Random.value < 0.2f) // 20% de chance de se recentrer
            {
                direction = currentX < middle ? 1 : -1; // Se déplace vers le centre
            }
            else
            {
                direction = Random.Range(-1, 2); // Direction normale
            }

            // Si changement de direction, ajoute une case supplémentaire pour atteindre 3 de large
            if (direction != 0)
            {
                if (direction == -1 && currentX > 0)
                {
                    grid[currentX - 1, z] = GridState.Path; // Étend à gauche
                }
                else if (direction == 1 && currentX2 < width - 1)
                {
                    grid[currentX2 + 1, z] = GridState.Path; // Étend à droite
                }
            }

            currentX = Mathf.Clamp(currentX + direction, 0, width - 2);
            currentX2 = Mathf.Clamp(currentX2 + direction, 1, width - 1);
        }

        return grid;
    }
}
