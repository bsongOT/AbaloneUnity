using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marble : MonoBehaviour
{
    public CubeCoord position { get; private set; }
    private int playerIndex;
    private Renderer rendererRef;

    private void Awake()
    {
        rendererRef = GetComponent<Renderer>();
    }

    private void Update()
    {

    }

    public void SetColor(Color color)
    {
        rendererRef.material.color = color;
    }
}
