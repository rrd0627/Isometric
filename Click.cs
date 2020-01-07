using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class Click : MonoBehaviour
{
    //카메라이동 케릭터 선택
    public GameObject ClickedChar;

    private Vector2 pos_down;

    private Vector2 pos_up;

    private Collider2D[] _col;

    public Text amount;

    public GameObject Buttons;

    public BoxCollider2D Boundary;

    private GameObject SelectPrefab;

    private bool IsMapSelect;
    private bool IsMapSelect_1;

    private Vector3Int SelectedTile;

    private Vector2 pos;
    private RaycastHit2D[] hit;

    private bool IsMove;
    private bool IsStand;
    private bool IsChase;
    private bool IsBridge;
    private bool IsObstacle;

    // Start is called before the first frame update
    void Start()
    {
        IsMapSelect = false;
        IsMapSelect_1 = false;
        IsMove = false;
        IsStand = false;
        IsChase = false;
        IsBridge = false;
        IsObstacle = false;
        hit = new RaycastHit2D[2];
    }

    public void MapClick()
    {
        IsMapSelect = true;        
    }
    public void MoveOrder()
    {
        Buttons.SetActive(false);

        IsStand = false;
        IsChase = false;
        IsBridge = false;
        IsObstacle = false;

        IsMove = true;
    }
    public void StandOrder()
    {
        Buttons.SetActive(false);

        IsMove = false;
        IsChase = false;
        IsBridge = false;
        IsObstacle = false;

        IsStand = true;         
    }
    public void ChaseOrder()
    {
        Buttons.SetActive(false);
        
        IsMove = false;
        IsStand = false;
        IsBridge = false;
        IsObstacle = false;

        IsChase = true;
    }
    public void BridgeOrder()
    {
        Buttons.SetActive(false);

        IsMove = false;
        IsStand = false;
        IsChase = false;
        IsObstacle = false;

        IsBridge = true;
    }
    public void ObstacleOrder()
    {
        Buttons.SetActive(false);

        IsMove = false;
        IsStand = false;
        IsChase = false;
        IsBridge = false;

        IsObstacle = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(ClickedChar!=null)
        {
            amount.enabled = true;
            amount.text = "Amount : " + ClickedChar.GetComponent<Unit>().Amount.ToString();
        }
        else
        {
            amount.enabled = false;
            Buttons.SetActive(false);
            IsMapSelect = false;
            IsMapSelect_1 = false;
        }

        if(Input.GetMouseButtonDown(0))
        {
            pos_down = Camera.main.ViewportToWorldPoint(Input.mousePosition);

            if (IsMapSelect)
                IsMapSelect_1 = true;

            if(IsChase)
            {
                IsChase = false;
                pos_up = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _col = Physics2D.OverlapCircleAll(pos_up, 1);
                int index = -1;
                float min_dist = 99999999;
                for (int i = 0; i < _col.Length; i++)
                {
                    if (_col[i].CompareTag("Unit"))
                    {
                        if (Vector2.Distance(pos_up, _col[i].transform.position) < min_dist)
                        {
                            min_dist = Vector2.Distance(pos_up, _col[i].transform.position);
                            index = i;
                        }
                    }
                }
                if(index!=-1)
                {
                    ClickedChar.GetComponent<Unit>().ChaseUnit = _col[index].gameObject;
                    ClickedChar.GetComponent<Unit>().ChaseOrder();
                }
                
            }
        }

        if (Input.GetMouseButton(0))
        {
              
        }

        if (Input.GetMouseButtonUp(0))
        {
            pos_up = Camera.main.ViewportToWorldPoint(Input.mousePosition);
            
            if (Vector2.Distance(pos_down, pos_up) > 100)
            {
                return;
            }
            else
            {
                if (IsMapSelect_1)
                {
                    if (ClickedChar != null)
                    {
                        if(IsBridge)
                        {
                            SelectedTile = ClickWaterTile();
                            IsMapSelect_1 = false;
                            IsMapSelect = false;
                            IsBridge = false;
                            if (GameManager.instance.water.GetTile(SelectedTile) == null)
                                return;
                            ClickedChar.GetComponent<Unit>().GoBridgeOrder(SelectedTile);
                            return;
                        }


                        SelectedTile = ClickTile();
                        IsMapSelect_1 = false;
                        IsMapSelect = false;
                        if (GameManager.instance.map.GetTile(SelectedTile) == null)
                            return;
                        //ClickedChar.GetComponent<Unit>().Goal = SelectedTile;
                        if(IsMove)
                        {
                            ClickedChar.GetComponent<Unit>().GoOrder(SelectedTile);
                            IsMove = false;
                        }
                        else if(IsStand)
                        {
                            ClickedChar.GetComponent<Unit>().GoOrder(SelectedTile);
                            ClickedChar.GetComponent<Unit>().IsChase = false;
                            ClickedChar.GetComponent<Unit>().IsStand = true;
                            IsStand = false;
                        }
                        else if(IsObstacle)
                        {
                            ClickedChar.GetComponent<Unit>().ObstacleOrder(SelectedTile);
                            ClickedChar.GetComponent<Unit>().IsChase = false;
                        }
                        //if(IsChase)
                        //{
                        //    ClickedChar.GetComponent<Unit>().GoOrder(SelectedTile);
                        //    IsChase = false;
                        //}
                        return;
                    }
                }
                if (IsMapSelect||IsChase)
                {
                    return;
                }

                pos_up = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                _col = Physics2D.OverlapCircleAll(pos_up, 1);
                int index = -1;
                float min_dist = 99999999;
                for (int i = 0; i < _col.Length; i++)
                {
                    if (_col[i].CompareTag("Unit"))
                    {
                        if (_col[i].GetComponent<Unit>().Master == 1 && Vector2.Distance(pos_up, _col[i].transform.position) < min_dist)
                        {
                            min_dist = Vector2.Distance(pos_up, _col[i].transform.position);
                            index = i;
                        }
                    }
                }
                if (index != -1)
                {
                    Destroy(SelectPrefab);
                    ClickedChar = _col[index].gameObject;
                    Buttons.SetActive(true);
                    SelectPrefab = Instantiate(GameManager.instance.prefabs[1], ClickedChar.transform);
                    SelectPrefab.transform.Translate(Vector3.up * 0.5f);
                }
                else
                {
                    if (ClickedChar != null)
                    {
                        ClickedChar = null;
                        Destroy(SelectPrefab);
                    }
                }
            }
            
        }
    }
    
    

    private Vector3Int ClickTile()
    {
        Vector3Int v3Int = v3Int = new Vector3Int(0,0,-1);

        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(pos, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit.transform != null)
        {
            for (int i = 10; i >= 0; i--)
            {
                pos.z = i;
                v3Int = GameManager.instance.map.WorldToCell(pos);
                if (GameManager.instance.map.GetTile(v3Int)!=null )
                {
                    if (GameManager.instance.map.GetTile(v3Int).name == "Pillar")
                        continue;
                    GameManager.instance.map.SetTileFlags(v3Int, TileFlags.None);

                    GameManager.instance.map.SetColor(v3Int, (Color.red));

                    break;
                }
            }
        }
        return v3Int;
    }
    private Vector3Int ClickWaterTile()
    {
        Vector3Int v3Int = new Vector3Int(0, 0, -1);
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(pos, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit.transform != null)
        {
            for (int i = 10; i >= 0; i--)
            {
                pos.z = i;

                v3Int = GameManager.instance.water.WorldToCell(pos);
                v3Int += Vector3Int.up + Vector3Int.right;
                if (GameManager.instance.water.GetTile(v3Int) != null)
                {
                    if (GameManager.instance.water.GetTile(v3Int).name == "Pillar")
                        continue;
                    GameManager.instance.water.SetTileFlags(v3Int, TileFlags.None);

                    if(GameManager.instance.water.GetColor(v3Int) == Color.red)
                    {
                        GameManager.instance.water.SetColor(v3Int, (Color.blue));
                    }
                    else
                        GameManager.instance.water.SetColor(v3Int, (Color.red));

                    break;
                }
            }
        }
        return v3Int;
    }
}
