using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    float[,] timer;

    private int sizeX;
    private int sizeY;

    Vector3Int vec3temp;

    private GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        sizeX = 10;
        sizeY = 10;
        timer = new float[2*sizeX+1, 2*sizeY+1];
        //StartCoroutine(MakeUnit());
    }

    IEnumerator MakeUnit()
    {
        while(true)
        {
            for (int i = -sizeX; i < sizeX; i++)
            {
                for (int j = -sizeY; j < sizeY; j++)
                {
                    vec3temp = new Vector3Int(i, j, 0);
                    if (GameManager.instance.change.GetTile(vec3temp) != null)
                    {
                        if (timer[i + sizeX, j + sizeY] > 15)
                        {
                            timer[i + sizeX, j + sizeY] = 0;

                            prefab = Instantiate(GameManager.instance.Character, GameManager.instance.map.CellToWorld(vec3temp), Quaternion.identity);

                            if (GameManager.instance.change.GetColor(vec3temp) == Color.green)
                            {
                                prefab.GetComponent<Unit>().Master = 1;
                                prefab.GetComponent<SpriteRenderer>().color = Color.green;
                            }
                            else if (GameManager.instance.change.GetColor(vec3temp) == Color.red)
                            {
                                prefab.GetComponent<Unit>().Master = 2;
                                prefab.GetComponent<SpriteRenderer>().color = Color.red;
                            }
                        }
                        timer[i + sizeX, j + sizeY] += Time.deltaTime;
                    }
                }
            }
            yield return null;
        }
    }
}
