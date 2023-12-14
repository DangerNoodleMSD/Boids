using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class Boids : MonoBehaviour
{
    [SerializeField]
    ComputeShader computeShader;
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
    Rigidbody2D[] rbBoids;

    int borderXid = Shader.PropertyToID("_borderX"),
        borderYid = Shader.PropertyToID("_borderY"),
        positionsID = Shader.PropertyToID("_Positions"),
        rotationsID = Shader.PropertyToID("_Rotations"),
        forcesID = Shader.PropertyToID("_Forces"),
        velocitiesID = Shader.PropertyToID("_Velocities"),
        forwardWeightID = Shader.PropertyToID("_ForwardWeight"),
        separationWeightID = Shader.PropertyToID("_SeparationWeight"),
        alignmentWeightID = Shader.PropertyToID("_AlignmentWeight"),
        cohesionWeightID = Shader.PropertyToID("_CohesionWeight"),
        viewRadiusID = Shader.PropertyToID("_ViewRadius"),
        numberOfBoidsID = Shader.PropertyToID("_NoBoids");

    int kernel;

    [System.Serializable]
    struct BoidBehaviour
    {
        public float cpuForwardWeight,
            cpuSeparationWeight,
            cpuAlignmentWeight,
            cpuCohesionWeight;
        [SerializeField, Range(0, 10)]
        public float cpuViewRadius;
        [SerializeField, Range(0, 10)]
        public float topSpeed;
    }
    
    [SerializeField]
    BoidBehaviour boidBehaviour;

    ComputeBuffer positions, rotations, forces, velocities;

    Vector2[] cpuPositions, cpuVelocities, cpuForces;
    float[] cpuRotations;

    private void Awake()
    {
        kernel = computeShader.FindKernel("CSMain");
        CreateWalls();
    }
    
    private void Update()
    {
        if(boids.Length != numberOfBoids)
        {
            OnDisable();
            OnEnable();
        }
        DispatchComputeShader();
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
        rbBoids = new Rigidbody2D[numberOfBoids];
        cpuPositions = new Vector2[numberOfBoids];
        cpuForces = new Vector2[numberOfBoids];
        cpuRotations = new float[numberOfBoids];
        cpuVelocities = new Vector2[numberOfBoids];


        int squareRoot = Mathf.CeilToInt(Mathf.Sqrt(numberOfBoids));
        for (int i = 0; i < boids.Length; i++)
        {
            boids[i] = Instantiate(boid);
            boids[i].transform.position = new Vector3(i % squareRoot - (squareRoot / 2f) + 0.5f, i / squareRoot - (squareRoot / 2f) + 0.5f, 0);
            boids[i].transform.parent = transform;
            boids[i].transform.localScale = new Vector2(scale / 2f, scale);

            boids[i].GetComponent<SpriteRenderer>().material.SetFloat(borderXid, borderX);
            boids[i].GetComponent<SpriteRenderer>().material.SetFloat(borderYid, borderY);

            rbBoids[i] = boids[i].GetComponent<Rigidbody2D>();
        }

        positions = new ComputeBuffer(numberOfBoids, sizeof(float) * 2);
        rotations = new ComputeBuffer(numberOfBoids, sizeof(float));
        forces = new ComputeBuffer(numberOfBoids, sizeof(float) * 2);
        velocities = new ComputeBuffer(numberOfBoids, sizeof(float) * 2);
    }

    private void OnDisable()
    {
        for (int i = 0; i < boids.Length;i++)
        {
            Destroy(boids[i]);
        }

        boids = null;
        positions.Dispose();
        rotations.Dispose();
        forces.Dispose();
        velocities.Dispose();

        positions = null;
        rotations = null;
        forces = null;
        velocities = null;
    }

    void DispatchComputeShader()
    {
        computeShader.SetBuffer(0, positionsID, positions);
        computeShader.SetBuffer(0, rotationsID, rotations);
        computeShader.SetBuffer(0, forcesID, forces);
        computeShader.SetBuffer(0, velocitiesID, velocities);
        computeShader.SetFloat(forwardWeightID, boidBehaviour.cpuForwardWeight);
        computeShader.SetFloat(separationWeightID, boidBehaviour.cpuSeparationWeight);
        computeShader.SetFloat(alignmentWeightID, boidBehaviour.cpuAlignmentWeight);
        computeShader.SetFloat(cohesionWeightID, boidBehaviour.cpuCohesionWeight);
        computeShader.SetFloat(viewRadiusID, boidBehaviour.cpuViewRadius);
        computeShader.SetInt(numberOfBoidsID, numberOfBoids);


        for (int i = 0; i < numberOfBoids; i++)
        {
            cpuPositions[i] = boids[i].transform.position;
            cpuRotations[i] = boids[i].transform.rotation.eulerAngles.z;
            cpuVelocities[i] = rbBoids[i].velocity;
        }

        positions.SetData(cpuPositions);
        rotations.SetData(cpuRotations);
        velocities.SetData(cpuVelocities);

        int groups = Mathf.CeilToInt(numberOfBoids / 64f);
        computeShader.Dispatch(kernel, groups, 1, 1);

        forces.GetData(cpuForces);
        rotations.GetData(cpuRotations);

        for (int i = 0; i < numberOfBoids; i++)
        {
            Vector2 v = new Vector2(-Mathf.Sin(Mathf.Deg2Rad * cpuRotations[i]), Mathf.Cos(Mathf.Deg2Rad * cpuRotations[i]));
            rbBoids[i].AddForce(cpuForces[i]);

            Debug.DrawLine(rbBoids[i].position, rbBoids[i].position + cpuForces[i]);
            Debug.DrawLine(rbBoids[i].position, rbBoids[i].position + v, Color.red);


            if (rbBoids[i].velocity.magnitude > boidBehaviour.topSpeed)
                rbBoids[i].velocity = rbBoids[i].velocity.normalized * boidBehaviour.topSpeed;


            if (float.IsNaN(cpuRotations[i]))
            {
                boids[i].transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }
            else
            {
                boids[i].transform.rotation = Quaternion.Euler(new Vector3(0, 0, cpuRotations[i]));
            }
        }
    }
}
