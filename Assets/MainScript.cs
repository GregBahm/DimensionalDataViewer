using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class MainScript : MonoBehaviour 
{
    public Texture DataInSquare;
    [Range(0, 1)]
    public float Width = 1;
    [Range(0, 1)]
    public float Height = 1;

    public Material BaseMaterial;
    public Mesh BoxMesh;
    public Transform RootTransform;

    private List<Material> _materials;

	void Start ()
    {
        //DeathData deathData = DeathData.LoadDeathData();
        //WriteImage(deathData);

        _materials = CreateTextureSquares();
    }

    private List<Material> CreateTextureSquares()
    {
        List<Material> ret = new List<Material>();
        GameObject boxParent = new GameObject();
        for (int row = 0; row < 11; row++)
        {
            for (int column = 0; column < 17; column++)
            {
                GameObject newBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newBox.name = row + " x " + column;
                newBox.transform.position = new Vector3(row, 0, column);
                newBox.transform.parent = boxParent.transform;
                Material newMat =  new Material(BaseMaterial);
                ret.Add(newMat);
                newBox.GetComponent<MeshFilter>().mesh = BoxMesh;
                newBox.GetComponent<MeshRenderer>().material = newMat;
            }
        }
        boxParent.transform.localScale = new Vector3(1, 25, 1);
        boxParent.transform.SetParent(RootTransform, false);
        return ret;
    }

    private void Update()
    {
        float uvScaleX = (float)1 / 11;
        float uvScaleY = (float)1 / 17;
        Shader.SetGlobalTexture("_DataInSquare", DataInSquare);
        Shader.SetGlobalFloat("_CubeWidth", Width);
        Shader.SetGlobalFloat("_CubeHeight", Height);
        int increment = 0;
        for (int row = 0; row < 11; row++)
        {
            for (int column = 0; column < 17; column++)
            {
                _materials[increment].SetVector("_UvScale", new Vector4(uvScaleX, uvScaleY));
                _materials[increment].SetVector("_UvOffset", new Vector4(uvScaleX * row + uvScaleX, uvScaleY * column + uvScaleY, 0, 0));
                increment++;
            }
        }
    }
    
    private void WriteImage(DeathData deathData)
    {
        Texture2D texture = new Texture2D(11, 17);
        int peak = deathData.TimeData.SelectMany(item => item.ByEachAgeRange).Max(item => item.Total);
        int row = 0;
        foreach (TimesliceData timeData in deathData.TimeData)
        {
            int column = 0;
            foreach (AgeRangeData ageData in timeData.ByEachAgeRange)
            {
                float val = (float)ageData.Total / peak;
                texture.SetPixel(row, column, new Color(val, val, val));
                column++;
            }
            row++;
        }
        File.WriteAllBytes(Application.dataPath + "\\imageData.png", texture.EncodeToPNG());
    }

    private void DrawWithBoxes(DeathData deathData)
    {
        GameObject boxParent = new GameObject();
        int row = 0;
        foreach (TimesliceData timeData in deathData.TimeData)
        {
            int column = 0;
            foreach (AgeRangeData ageData in timeData.ByEachAgeRange)
            {
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                indicator.name = row + " x " + column;
                float scale = (float)ageData.Total / deathData.TotalDeaths;
                indicator.transform.localScale = new Vector3(.5f, scale, .5f);
                indicator.transform.position = new Vector3(row, (float)scale / 2, column);
                indicator.transform.parent = boxParent.transform;
                column++;
            }
            row++;
        }
    }
}
