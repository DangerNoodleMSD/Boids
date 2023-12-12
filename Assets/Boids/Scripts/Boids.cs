using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids : MonoBehaviour
{
    [SerializeField, Range(1, 81)]
    int numberOfBoids = 30;
    [SerializeField]
    float scale = 0.5f;
    [SerializeField, Range(0, 19)]
    float borderX = 15f;
    [SerializeField, Range(0, 10)]
    float borderY = 9f;

    [SerializeField]
    GameObject boid;

    [SerializeField]
    GameObject wall;

    GameObject[] walls;

    float wallThickness = 0.2f;

    GameObject[] boids;

    int borderXid = Shader.PropertyToID("_borderX"),
        borderYid = Shader.PropertyToID("_borderY");

    private void Awake()
    {
        CreateWalls();
    }

    void CreateWalls()
    {
        walls = new GameObject[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject currWall;
            switch (i)
            {
                case 0:
                    currWall = Instantiate(wall);
                    currWall.name = "Wall " + i;

                    currWall.transform.position = new Vector2(0, (borderY + wallThickness) / 2f);
                    currWall.transform.localScale = new Vector2(borderX + 2 * wallThickness, wallThickness);

                    walls[i] = currWall;
                    break;
                case 1:
                    currWall = Instantiate(wall);
                    currWall.name = "Wall " + i;

                    currWall.transform.position = new Vector2((borderX + wallThickness) / 2f, 0);
                    currWall.transform.localScale = new Vector2(wallThickness, borderY + 2 * wallThickness);

                    walls[i] = currWall;
                    break;
                case 2:
                    currWall = Instantiate(wall);
                    currWall.name = "Wall " + i;

                    currWall.transform.position = new Vector2(0, (-borderY - wallThickness) / 2f);
                    currWall.transform.localScale = new Vector2(borderX + 2 * wallThickness, wallThickness);

                    walls[i] = currWall;
                    break;
                case 3:
                    currWall = Instantiate(wall);
                    currWall.name = "Wall " + i;

                    currWall.transform.position = new Vector2((-borderX - wallThickness) / 2f, 0);
                    currWall.transform.localScale = new Vector2(wallThickness, borderY + 2 * wallThickness);

                    walls[i] = currWall;
                    break;
            }
        }
    }

    private void OnEnable()
    {
        boids = new GameObject[numberOfBoids];
        int squareRoot = Mathf.CeilToInt(Mathf.Sqrt(numberOfBoids));
        for (int i = 0; i < boids.Length; i++)
        {
            boids[i] = Instantiate(boid);
            boids[i].transform.position = new Vector3(i % squareRoot - (squareRoot / 2f) + 0.5f, i / squareRoot - (squareRoot / 2f) + 0.5f, 0);
            boids[i].transform.parent = transform;
            boids[i].transform.localScale = new Vector2(scale / 2f, scale);

            boids[i].GetComponent<SpriteRenderer>().material.SetFloat(borderXid, borderX);
            boids[i].GetComponent<SpriteRenderer>().material.SetFloat(borderYid, borderY);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < boids.Length;i++)
        {
            Destroy(boids[i]);
        }

        boids = null;
    }

    private void Update()
    {
        if(boids.Length != numberOfBoids)
        {
            OnDisable();
            OnEnable();
        }
    }
}
