using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ProceduralGeneration : MonoBehaviour
{
    private List<GameObject> objectsList = new List<GameObject>();

    [SerializeField] private Transform playerTransform;

    //Generate Variable
    private bool isGenerating = false;
    private bool generatingFence = false;
    private bool generatingObstacles = false;
    private Vector3 maxPositionGenerate = new Vector3(0, 0, 0);
    [SerializeField] private Transform obstaclesTransform;
    [SerializeField] private Transform leftFenceTransform;
    [SerializeField] private Transform rightFenceTransform;

    [SerializeField] private GameObject terrainPrefabs;
    [SerializeField] private List<GameObject> prefabs;

    //Grid 
    private int gridWidth = 7; 
    private int gridHeight;
    private int cellSize = 2;
    private int currentX = 3;
    private int currentX2 = 4;
    public enum GridState
    {
        None,
        Path, 
        Obstacle
    }


    private bool isDestroying = false;
    private bool isRestarting = false;

    // Start is called before the first frame update
    void Start()
    {
        gridHeight = Mathf.RoundToInt((250 - obstaclesTransform.position.z) / cellSize);
        StartCoroutine(Generate());
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRestarting)
        {
            if (CheckForGenerate() && !isGenerating)
            {
                gridHeight = Mathf.RoundToInt(250 / cellSize);
                SpawnTerrain();
                StartCoroutine(Generate());
            }

            if (CheckForDestroy() && !isDestroying)
            {
                StartCoroutine(Destroy());
            }
        }
    }

    private bool CheckForGenerate()
    {
        return playerTransform.position.z >= maxPositionGenerate.z - 250;
    }

    private bool CheckForDestroy()
    {
        if(objectsList.Count == 0)
        {
            return false;
        }
        return objectsList[0].transform.position.z < playerTransform.position.z - 550;
    }

    private IEnumerator Generate()
    {
        isGenerating = true;

        generatingFence = true;
        StartCoroutine(GenerateFences());

        generatingObstacles = true;
        StartCoroutine(GenerateObstacles());

        while (generatingFence || generatingObstacles) 
        {
            yield return null;
        }
       
        maxPositionGenerate.z += 250;
        obstaclesTransform.SetPositionAndRotation(new Vector3(obstaclesTransform.position.x, obstaclesTransform.position.y, maxPositionGenerate.z), obstaclesTransform.rotation);
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

            if (isRestarting)
            {
                break;
            }
            yield return null;
        }
        generatingFence = false;
    }

    private IEnumerator GenerateObstacles()
    {
        GridState[,] grid = GenerateGrid(gridWidth, gridHeight);

        yield return null;

        // Place les obstacles sur la grille
        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (CheckGridCellValueState(grid, x, z, GridState.None) && Random.value < 0.5f)
                {
                    // Sélectionner un type d'obstacle aléatoire
                    int type = Random.Range(0, 5);

                    if(type == 0)
                    {
                        SpawnWoodRamp(grid, x, z);
                    }
                    else if(type == 1)
                    {
                        SpawnWoodStack(grid, x, z);
                    }
                    else if(type == 2)
                    {
                        SpawnWoodHeap(grid, x, z);
                    }
                    else if(type == 3)
                    {
                        SpawnLog(grid, x, z);
                    }
                    else if(type == 4)
                    {
                        SpawnRock(grid, x, z);
                    }
                }           
            }
            if (isRestarting)
            {
                break;
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

    private void SpawnWoodRamp(GridState[,] grid , int x, int z)
    {
        if(CheckGridCellValueState(grid, x, z + 1, GridState.None) && CheckGridCellValueState(grid, x, z + 2, GridState.None))
        {
            grid[x, z] = GridState.Obstacle;
            grid[x, z + 1] = GridState.Obstacle;
            grid[x, z + 2] = GridState.Obstacle;
            Vector3 _position = new Vector3(obstaclesTransform.position.x + (x * cellSize) + (cellSize / 2), prefabs[1].transform.position.y, obstaclesTransform.position.z + ((z + 2) * cellSize) + (cellSize / 2));
            GameObject new_obstacle = Instantiate(prefabs[1], _position, prefabs[1].transform.rotation, obstaclesTransform.parent);
            objectsList.Add(new_obstacle);
            SpawnWoodStack(grid, x, z + 3);
        }
    }

    private void SpawnWoodStack(GridState[,] grid, int x, int z)
    {
        int nb_wood_stack = Random.Range(1, 7);
        for(int zd = 0; zd < nb_wood_stack; zd++)
        {
            if (CheckGridCellValueState(grid, x, z + zd, GridState.None))
            {
                grid[x, z + zd] = GridState.Obstacle;
                Vector3 _position = new Vector3(obstaclesTransform.position.x + (x * cellSize) + (cellSize / 2), prefabs[2].transform.position.y, obstaclesTransform.position.z + ((z + zd) * cellSize) + (cellSize / 2));
                GameObject new_obstacle = Instantiate(prefabs[2], _position, prefabs[2].transform.rotation, obstaclesTransform.parent);
                objectsList.Add(new_obstacle);
            }
            else
            {
                break;
            }
        }
    }

    private void SpawnWoodHeap(GridState[,] grid, int x, int z)
    {
        grid[x, z] = GridState.Obstacle;
        Vector3 _position = new Vector3(obstaclesTransform.position.x + (x * cellSize) + (cellSize / 2), prefabs[3].transform.position.y, obstaclesTransform.position.z + (z * cellSize) + (cellSize / 2));
        GameObject new_obstacle = Instantiate(prefabs[3], _position, prefabs[3].transform.rotation, obstaclesTransform.parent);
        objectsList.Add(new_obstacle);
    }

    private void SpawnLog(GridState[,] grid, int x, int z)
    {
        if (CheckGridCellValueState(grid, x + 1, z, GridState.None) && CheckGridCellValueState(grid, x + 2, z, GridState.None))
        {
            grid[x, z] = GridState.Obstacle;
            grid[x + 1, z] = GridState.Obstacle;
            grid[x + 2, z] = GridState.Obstacle;
            Vector3 _position = new Vector3(obstaclesTransform.position.x + ((x + 1) * cellSize) + (cellSize / 2), prefabs[4].transform.position.y, obstaclesTransform.position.z + (z * cellSize) + (cellSize / 2));
            GameObject new_obstacle = Instantiate(prefabs[4], _position, prefabs[4].transform.rotation, obstaclesTransform.parent);
            objectsList.Add(new_obstacle);
        }
        else if (CheckGridCellValueState(grid, x - 1, z, GridState.None) && CheckGridCellValueState(grid, x + 1, z, GridState.None))
        {
            grid[x, z] = GridState.Obstacle;
            grid[x - 1, z] = GridState.Obstacle;
            grid[x + 1, z] = GridState.Obstacle;
            Vector3 _position = new Vector3(obstaclesTransform.position.x + (x * cellSize) + (cellSize / 2), prefabs[4].transform.position.y, obstaclesTransform.position.z + (z * cellSize) + (cellSize / 2));
            GameObject new_obstacle = Instantiate(prefabs[4], _position, prefabs[4].transform.rotation, obstaclesTransform.parent);
            objectsList.Add(new_obstacle);
        }
        else if (CheckGridCellValueState(grid, x - 1, z, GridState.None) && CheckGridCellValueState(grid, x - 2, z, GridState.None))
        {
            grid[x, z] = GridState.Obstacle;
            grid[x - 1, z] = GridState.Obstacle;
            grid[x - 2, z] = GridState.Obstacle;
            Vector3 _position = new Vector3(obstaclesTransform.position.x + ((x - 1) * cellSize) + (cellSize / 2), prefabs[4].transform.position.y, obstaclesTransform.position.z + (z * cellSize) + (cellSize / 2));
            GameObject new_obstacle = Instantiate(prefabs[4], _position, prefabs[4].transform.rotation, obstaclesTransform.parent);
            objectsList.Add(new_obstacle);
        }
    }

    private void SpawnRock(GridState[,] grid, int x, int z)
    {
        grid[x, z] = GridState.Obstacle;
        Vector3 _position = new Vector3(obstaclesTransform.position.x + (x * cellSize) + (cellSize / 2), prefabs[5].transform.position.y, obstaclesTransform.position.z + (z * cellSize) + (cellSize / 2));
        GameObject new_obstacle = Instantiate(prefabs[5], _position, prefabs[5].transform.rotation, obstaclesTransform.parent);
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
        int direction;
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

    private bool CheckGridCellValueState(GridState[,] _grid, int x, int z, GridState _state)
    {
        if(x >= 0 && x <= gridWidth - 1 && z >= 0 && z <= gridHeight - 1) 
        {
            return _grid[x, z] == _state;
        }
        return false;
    }

    public void ResetMap()
    {
        isRestarting = true;
        maxPositionGenerate = new Vector3(0, 0, 0);
        obstaclesTransform.SetPositionAndRotation(new Vector3(18,0,20), Quaternion.identity);
        leftFenceTransform.SetPositionAndRotation(new Vector3(17.5f, 0.5f, 15.9239836f), Quaternion.identity);
        rightFenceTransform.SetPositionAndRotation(new Vector3(32.5f, 0.5f, 15.3578234f), Quaternion.identity);
        currentX = 3;
        currentX2 = 4;
        gridHeight = Mathf.RoundToInt((250 - obstaclesTransform.position.z) / cellSize);
        DestroyAll();
        isRestarting = false;
    }

    private void DestroyAll()
    {
        while(objectsList.Count != 0)
        {
            Destroy(objectsList[0], 0.1f);
            objectsList.RemoveAt(0);
        }
    }
}
