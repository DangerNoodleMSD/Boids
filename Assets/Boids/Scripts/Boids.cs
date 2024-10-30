using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class Boids : MonoBehaviour
{
    [SerializeField]
    ComputeShader computeShader;
    [SerializeField, Range(1, 2000)]
    int numberOfBoids = 30;
    [SerializeField]
    float scale = 0.5f;
    [SerializeField, Range(0, 200)]
    float borderX = 15f;
    [SerializeField, Range(0, 200)]
    float borderY = 9f;

    [SerializeField]
    GameObject boid;

    [SerializeField]
    GameObject wall;

    GameObject[] walls;

    float wallThickness = 0.2f;

    GameObject[] boids;
    Rigidbody2D[] rbBoids;
    Transform[] tBoids;

    // We take the IDs to be able to set these variables in the compute shader
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

    // kernel index     gets initialized in the OnEnable() function
    int kernel;

    [System.Serializable]
    struct BoidBehaviour
    {
        public float forceMultiplier;
        public float forwardWeight,
            separationWeight,
            alignmentWeight,
            cohesionWeight;
        [Range(0, 10)]
        public float viewRadius;
        [Range(0, 10)]
        public float topSpeed;
        [Range(0, 180)]
        public float viewAngle;
        public bool noise;
        [SerializeField, Range(0f, 10f)]
        public float noiseUpdate;
        [SerializeField, Range(0f, 10f)]
        public float noiseWeight;
        [SerializeField, Range(0, 90)]
        public float noiseAngleMax;
    }
    
    [SerializeField]
    BoidBehaviour boidBehaviour;

    ComputeBuffer gpuPositions, gpuRotations, gpuForces, gpuVelocities, gpuQuaternions, gpuNoiseAngles;

    Vector2[] positions, velocities, forces;
    float[] rotations;
    float[] noiseAngles;
    float noiseDuration = 0f;
    RaycastHit2D[] circleHit;

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
        MoveBoids();

        //circleHit = Physics2D.CircleCastAll(new Vector2(0, 0), borderX > borderY ? borderX * (Mathf.Sqrt(2f) / 2f) : borderY * (Mathf.Sqrt(2) / 2f), new Vector2(0, 0), 0f);
        //foreach (var hit in circleHit)
        //{
        //    Debug.DrawLine(new Vector2(0, 0), hit.point, Color.cyan);
        //    Debug.DrawLine(hit.point, hit.point + hit.normal.normalized, Color.magenta);
        //}
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(0, 0, 0), borderX > borderY ? borderX * (Mathf.Sqrt(2f) / 2f) : borderY * (Mathf.Sqrt(2) / 2f) );
    }

    // creates the 4 walls and puts the objects into "walls" variable
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
        tBoids = new Transform[numberOfBoids];
        positions = new Vector2[numberOfBoids];
        forces = new Vector2[numberOfBoids];
        rotations = new float[numberOfBoids];
        velocities = new Vector2[numberOfBoids];
        noiseAngles = new float[numberOfBoids];


        // spawns the boids into a square pattern
        int squareRoot = Mathf.CeilToInt(Mathf.Sqrt(numberOfBoids));
        for (int i = 0; i < boids.Length; i++)
        {
            boids[i] = Instantiate(boid);
            boids[i].transform.position = new Vector3(i % squareRoot - (squareRoot / 2f) + 0.5f, i / squareRoot - (squareRoot / 2f) + 0.5f, 0);
            boids[i].transform.rotation = Quaternion.Euler(0, 0, Random.Range(-180, 180));
            boids[i].transform.parent = transform;
            boids[i].transform.localScale = new Vector2(scale / 2f, scale);

            boids[i].GetComponent<SpriteRenderer>().material.SetFloat(borderXid, borderX);
            boids[i].GetComponent<SpriteRenderer>().material.SetFloat(borderYid, borderY);

            rbBoids[i] = boids[i].GetComponent<Rigidbody2D>();
            tBoids[i] = boids[i].GetComponent<Transform>();
        }
        var vcam = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
        //vcam.Follow = boids[0].transform;

        gpuPositions = new ComputeBuffer(numberOfBoids, sizeof(float) * 2);
        gpuRotations = new ComputeBuffer(numberOfBoids, sizeof(float));
        gpuForces = new ComputeBuffer(numberOfBoids, sizeof(float) * 2);
        gpuVelocities = new ComputeBuffer(numberOfBoids, sizeof(float) * 2);
        gpuQuaternions = new ComputeBuffer(numberOfBoids, sizeof(float) * 4);
        gpuNoiseAngles = new ComputeBuffer(numberOfBoids, sizeof(float));

        // We have to init the _Rotations buffer
        computeShader.SetBuffer(kernel, rotationsID, gpuRotations);

        for (int i = 0; i < numberOfBoids; i++)
        {
            rotations[i] = boids[i].transform.rotation.eulerAngles.z;
        }
        gpuRotations.SetData(rotations);

        UpdateNoiseBuffer();
    }

    private void OnDisable()
    {
        for (int i = 0; i < boids.Length;i++)
        {
            Destroy(boids[i]);
        }

        boids = null;
        gpuPositions.Dispose();
        gpuRotations.Dispose();
        gpuForces.Dispose();
        gpuVelocities.Dispose();
        gpuQuaternions.Dispose();
        gpuNoiseAngles.Dispose();

        gpuPositions = null;
        gpuRotations = null;
        gpuForces = null;
        gpuVelocities = null;
        gpuQuaternions = null;
        gpuNoiseAngles = null;
    }

    void DispatchComputeShader()
    {
        noiseDuration += Time.deltaTime;
        if(noiseDuration > boidBehaviour.noiseUpdate)
        {
            noiseDuration -= boidBehaviour.noiseUpdate;
            UpdateNoiseBuffer();
        }

        //setting up the variables in compute shader and binding buffers
        computeShader.SetBuffer(kernel, positionsID, gpuPositions);
        
        computeShader.SetBuffer(kernel, forcesID, gpuForces);
        computeShader.SetBuffer(kernel, velocitiesID, gpuVelocities);
        computeShader.SetBuffer(kernel, "_Quaternions", gpuQuaternions);
        computeShader.SetBuffer(kernel, "_NoiseAngle", gpuNoiseAngles);
        computeShader.SetFloat(forwardWeightID, boidBehaviour.forwardWeight);
        computeShader.SetFloat(separationWeightID, boidBehaviour.separationWeight);
        computeShader.SetFloat(alignmentWeightID, boidBehaviour.alignmentWeight);
        computeShader.SetFloat(cohesionWeightID, boidBehaviour.cohesionWeight);
        computeShader.SetFloat(viewRadiusID, boidBehaviour.viewRadius);
        computeShader.SetInt(numberOfBoidsID, numberOfBoids);
        computeShader.SetFloat("_ViewAngle", Mathf.Cos(Mathf.Deg2Rad * boidBehaviour.viewAngle));
        computeShader.SetBool("_Noise", boidBehaviour.noise);
        computeShader.SetFloat("_NoiseWeight", boidBehaviour.noiseWeight);

        //filling the arrays that will be given to gpu
        for (int i = 0; i < numberOfBoids; i++)
        {
            positions[i] = boids[i].transform.position;
            velocities[i] = rbBoids[i].velocity;
        }

        //loading buffers with data filled above
        gpuPositions.SetData(positions);
        gpuVelocities.SetData(velocities);

        int groups = Mathf.CeilToInt(numberOfBoids / 64f);
        computeShader.Dispatch(kernel, groups, 1, 1);

        //getting data back
        gpuForces.GetData(forces);
        gpuRotations.GetData(rotations);
    }

    void MoveBoids()
    {
        Debug.Log(rotations[0]);
        Debug.Log(forces[0]);
        for (int i = 0; i < numberOfBoids; i++)
        {
            Vector2 v = new Vector2(-Mathf.Sin(Mathf.Deg2Rad * rotations[i]), Mathf.Cos(Mathf.Deg2Rad * rotations[i]));
            rbBoids[i].AddForce(new Vector2(float.IsNaN(forces[i].x) ? 0 : forces[i].x * boidBehaviour.forceMultiplier, float.IsNaN(forces[i].y) ? 1 : forces[i].y * boidBehaviour.forceMultiplier) );

            Debug.DrawLine(rbBoids[i].position, rbBoids[i].position + forces[i]);
            Debug.DrawLine(rbBoids[i].position, rbBoids[i].position + v, Color.red);


            if (rbBoids[i].velocity.magnitude > boidBehaviour.topSpeed)
                rbBoids[i].velocity = rbBoids[i].velocity.normalized * boidBehaviour.topSpeed;


            if (float.IsNaN(rotations[i])) //at first frame cpuRotations is filled with non numbers
            {
                boids[i].transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }
            else
            {
                boids[i].transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotations[i]));
            }

            GetBoidInBounds(i);
        }
    }

    void GetBoidInBounds(int i)
    {
        if (boids[i].transform.position.x > borderX / 2f)
        {
            float delta = boids[i].transform.position.x - borderX / 2f;
            tBoids[i].position = new Vector2(-borderX / 2f + delta, tBoids[i].position.y);
        }
        if (boids[i].transform.position.x < -borderX / 2f)
        {
            float delta = boids[i].transform.position.x + borderX / 2f;
            tBoids[i].position = new Vector2(borderX / 2f + delta, tBoids[i].position.y);
        }

        if (boids[i].transform.position.y > borderY / 2f)
        {
            float delta = boids[i].transform.position.y - borderY / 2f;
            tBoids[i].position = new Vector2(tBoids[i].position.x, -borderY / 2f + delta);
        }
        if (boids[i].transform.position.y < -borderY / 2f)
        {
            float delta = boids[i].transform.position.y + borderY / 2f;
            tBoids[i].position = new Vector2(tBoids[i].position.x, borderY / 2f + delta);
        }
    }

    void UpdateNoiseBuffer()
    {
        for (int i = 0; i < noiseAngles.Length; i++)
        {
            noiseAngles[i] = Random.Range(-boidBehaviour.noiseAngleMax, boidBehaviour.noiseAngleMax);
        }
        gpuNoiseAngles.SetData(noiseAngles);
    }
}
