using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Node : MonoBehaviour
{
    public int Master; //master 1  ==  유저

    private GameObject Unit;
    
    private GameObject Unit_prefab;


    private Vector2 Pos_Node;

    public int HP;

    private Color _color;

    public Vector3Int Pos;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        Unit = GameManager.instance.Character;

        //Pos = GameManager.instance.change.WorldToCell(transform.position);

        if (Master == 1) _color = new Color(48 / 255f, 144 / 255f, 32 / 255f);
        else if (Master == 2) _color = new Color(208 / 255f, 64 / 255f, 16 / 255f);
        else if (Master == 3) _color = new Color(40 / 255f, 121 / 255f, 177 / 255f);
        else if (Master == 4) _color = new Color(205 / 255f, 205 / 255f, 44 / 255f);
        else _color = Color.black;

        GetComponent<SpriteRenderer>().color = _color;

        //Pos_Node = GameManager.instance._grid.GetCellCenterWorld();

        //StartCoroutine(MakeUnit());
    }
    /*IEnumerator MakeUnit()
    {
        while(true)
        {
            if(Master > 0)
            {
                Unit_prefab = Instantiate(Unit, this.transform.position, Quaternion.identity);                
            }
            yield return gametime;
        }
    }*/

    void Update()
    {
        timer += Time.deltaTime;

        if(HP<0)
        {
            GameManager.instance.TileMaster[Pos.x+ GameManager.instance.mapsize / 2, Pos.y+ GameManager.instance.mapsize / 2] =0;
            Destroy(this.gameObject);
        }

        if(timer>=1)
        {
            timer = 0;

            GameManager.instance.money[Master] += 2;

            if(HP<10)
                HP += 1;   
        }
        if (HP >= 10 && GameManager.instance.money[Master] > 20)
        {
            GameManager.instance.money[Master] -= 20;
            HP = 0;
            Unit_prefab = Instantiate(Unit, this.transform.position, Quaternion.identity);
            Unit_prefab.GetComponent<Unit>().Master = Master;
            Unit_prefab.GetComponent<Unit>().Cur = Pos;
            Unit_prefab.GetComponent<SpriteRenderer>().color = _color;
            GameManager.instance.TileMaster[Pos.x + GameManager.instance.mapsize / 2, Pos.y + GameManager.instance.mapsize / 2] = Master;
        }
    }
    //IEnumerator Move(Vector3 target)
    //{
    //    Unit_prefab
    //}

    /*
    private void OnMouseOver()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Ray2D ray = new Ray2D(pos, Vector2.zero);

        Debug.DrawRay(ray.origin, ray.direction * 10, Color.blue, 3.5f);
               
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        Debug.Log("?");
        if (hit.transform!=null)
        {
            GameManager.instance.map.RefreshAllTiles();

            int x, y;
            x = GameManager.instance.map.WorldToCell(ray.origin).x;
            y = GameManager.instance.map.WorldToCell(ray.origin).y;

            Vector3Int v3Int = new Vector3Int(x, y, 0);

            GameManager.instance.map.SetTileFlags(v3Int, TileFlags.None);

            GameManager.instance.map.SetColor(v3Int, (Color.red));

            Debug.Log(GameManager.instance.map.WorldToCell(pos));
        }
        
    }*/
}
