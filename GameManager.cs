using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;

    public Grid _grid;

    public Tilemap map;
    public Tilemap obstacle;
    public Tilemap change;
    public Tilemap water;

    public GameObject Character;

    public GameObject Barrack;

    public GameObject Obstacle;

    public int[,] TileMaster;
    
    public List<GameObject> TileChar;
    public List<GameObject> TileChar1;
    public List<GameObject> TileChar2;
    public List<GameObject> TileChar3;

    public int mapsize = 200;

    public int[] money;

    public Text Moneytext;

    public GameObject[] prefabs;

    public TilemapCollider2D TilemapCollider2D;
    

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        TileMaster = new int[mapsize, mapsize];
        TileChar = new List<GameObject>();
        TileChar1 = new List<GameObject>();
        TileChar2 = new List<GameObject>();
        TileChar3 = new List<GameObject>();
    }
        // Start is called before the first frame update
    void Start()
    {
        money = new int[5];
    }

    private void Update()
    {
        Moneytext.text = "Money : " + money[1].ToString();
    }

    public bool IsBarrackOK(Vector3Int Cur)
    {
        Vector3Int vec3;
        vec3 = Cur;

        vec3.x += mapsize / 2;
        vec3.y += mapsize / 2;

        if (TileMaster[vec3.x + 1, vec3.y + 1] > 0 ||
            TileMaster[vec3.x + 1, vec3.y] > 0 ||
            TileMaster[vec3.x + 1, vec3.y - 1] > 0 ||
            TileMaster[vec3.x, vec3.y - 1] > 0 ||
            TileMaster[vec3.x - 1, vec3.y - 1] > 0 ||
            TileMaster[vec3.x - 1, vec3.y] > 0 ||
            TileMaster[vec3.x - 1, vec3.y + 1] > 0 ||
            TileMaster[vec3.x, vec3.y + 1] > 0 ||
            TileMaster[vec3.x, vec3.y] > 0||
            map.GetTile(Cur).name == "Dirt" || 
            map.GetTile(Cur).name == "BridgeLU" || 
            map.GetTile(Cur).name == "BridgeRU"
            )
            return false;
        return true;
    }

    public bool IsTileChangeOK(Vector3Int Cur)
    {
        if (
            map.GetTile(Cur).name == "Dirt" ||
            map.GetTile(Cur).name == "BridgeLU" ||
            map.GetTile(Cur).name == "BridgeRU"
            )
            return false;
        return true;
    }

    public bool CanGo(Vector3Int pos)
    {
        if (obstacle.GetTile(pos) != null || map.GetTile(pos) == null || map.GetTile(pos).name == "Pillar" || TileMaster[pos.x + mapsize / 2, pos.y + mapsize / 2] == 5)
        {
            return false;
        } //못가는 곳인경우 또는 이미 끝인경우

        return true;
    }



    public void fortest()
    {
        Debug.Log("test!!!!!!!!!!!!!!!!!!!!!!!");
    }

}
