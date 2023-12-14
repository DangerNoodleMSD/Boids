using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToOrigin : MonoBehaviour
{
    [SerializeField]
    int noPositions = 20;

    [SerializeField]
    ComputeShader computeShader;
    ComputeBuffer positions;
    ComputeBuffer results;

    int positionsSizeID = Shader.PropertyToID("_SizeOfPositons"),
        positionsID = Shader.PropertyToID("_Positions"),
        resultsID = Shader.PropertyToID("_Results"),
        originID = Shader.PropertyToID("_Origin");

    Vector2[] cpuPositions;
    Vector2[] cpuResults;

    private void OnEnable()
    {
        positions = new ComputeBuffer(noPositions * noPositions, 2 * sizeof(float));
        results = new ComputeBuffer(noPositions * noPositions, 2 * sizeof(float));
        cpuPositions = new Vector2[noPositions * noPositions];
        cpuResults = new Vector2[noPositions * noPositions];
        
        for(int i = 0; i < noPositions; i++)
        {
            for(int j = 0; j < noPositions; j++)
            {
                cpuPositions[i * noPositions + j] = new Vector2(i - (noPositions - 1) / 2f, j - (noPositions - 1) / 2f);
            }
        }
        computeShader.SetBuffer(0, positionsID, positions);
        positions.SetData(cpuPositions);
    }

    private void OnDisable()
    {
        positions.Release(); 
        results.Release();

        positions = null; 
        results = null;
        cpuPositions = null;
    }

    private void OnValidate()
    {
        if (positions != null)
        {
            OnDisable();
            OnEnable();
        }
    }


    private void Update()
    {
        DispatchComputeShader();

        //SetResults();

        for (int i = 0; i < noPositions; i++)
        {
            for (int j = 0; j < noPositions; j++)
            {
                Debug.DrawLine(cpuPositions[i * noPositions + j], cpuResults[i * noPositions + j]);
            }
        }
    }

    void SetResults()
    {
        for(int i = 0;i < noPositions;i++)
        {
            for( int j = 0;j < noPositions;j++)
            {
                Vector2 toOrigin = new Vector2(transform.position.x, transform.position.y) - cpuPositions[i * noPositions + j];
                toOrigin.Normalize();
                cpuResults[i * noPositions + j] = toOrigin + cpuPositions[i * noPositions + j];
            }
        }
    }

    void DispatchComputeShader()
    {
        computeShader.SetBuffer(0, resultsID, results);
        computeShader.SetInt(positionsSizeID, noPositions);
        computeShader.SetFloats(originID, new float[] { transform.position.x, transform.position.y });


        int groups = Mathf.CeilToInt(noPositions / 8f);

        computeShader.Dispatch(0, groups, groups, 1);

        results.GetData(cpuResults);
    }
}
