using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    private Vector2 dir;

    public Vector3Int Goal;

    public Vector3Int Next;

    public Vector3Int Cur;

    private Animator _ani;

    public int Master;

    public int Amount;

    private GameObject Enemy;

    private GameObject prefab_Barrack;
    private GameObject prefab_Fight;
    private GameObject prefab_Obstacle;

    private Color _color;

    private float timer;
    private float fightingtimer;
    private Vector3Int vec3_temp;
    private Vector3 vec3temp;
    // Start is called before the first frame update

    public int UnitState;

    private Collider2D[] _col;

    private bool IsYChanged;

    private List<GameObject> CharMerge;

    private bool IsMoving;
    
    public bool IsOnBarrack;

    public bool IsStand;

    public bool IsChase;

    public bool IsBridge;

    private Vector3Int BridgeLocation;

    public bool IsBarrigate;

    public GameObject ChaseUnit;

    enum ActState
    {
        run,
        stand,
        attack,
        build,
        chase,
        barrigate,
        bridge,
        merge
    }

    void Start()
    {
        UnitState = (int)ActState.stand;
        //Cur = GameManager.instance.map.WorldToCell(transform.position);
        IsMoving = false; IsBarrigate = false; IsStand = false; IsChase = false; IsBridge = false;
          vec3temp = new Vector3(0, 0.25f, 0);
        transform.position = GameManager.instance.map.CellToWorld(Cur);

        Goal = Cur;
        _ani = GetComponent<Animator>();
        Amount = 1;
        IsOnBarrack = false;
        if (Master == 1) _color = new Color(48 / 255f, 144 / 255f, 32 / 255f);
        else if (Master == 2) _color = new Color(208 / 255f, 64 / 255f, 16 / 255f);
        else if (Master == 3) _color = new Color(40 / 255f, 121 / 255f, 177 / 255f);
        else if (Master == 4) _color = new Color(205 / 255f, 205 / 255f, 44 / 255f);
        else _color = Color.black;

        GetComponent<SpriteRenderer>().color = _color;

        switch (Master)
        {
            case 1:
                CharMerge = GameManager.instance.TileChar;
                break;
            case 2:
                CharMerge = GameManager.instance.TileChar1;
                break;
            case 3:
                CharMerge = GameManager.instance.TileChar2;
                break;
            case 4:
                CharMerge = GameManager.instance.TileChar3;
                break;
        }
        //transform.Translate(vec3temp * Cur.z);
    }

    

    void Update()
    {
        if (Amount <= 0)
        {
            CharMerge.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
        if (IsOnBarrack)
        {
            GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
        else
        {
            GetComponent<SpriteRenderer>().sortingOrder = 0;
        }
        timer += Time.deltaTime;
        if (timer >= 1)
        {
            timer = 0;
            if (GameManager.instance.money[Master] > 0)
                GameManager.instance.money[Master] -= Mathf.CeilToInt((Amount ^ 2) / 100);
        }
        switch (UnitState)
        {            
            case (int)ActState.stand:
                _ani.SetBool("Run", false);
                _ani.SetBool("Attack", false);
                if(Goal==Cur)
                    Goal = ChooseGoal();
                if (Goal == Cur) //주변에 점령할 것이 없는경우 또는 맨처음!
                {
                    if(CharMerge.Count != 0)
                    {
                        UnitState = (int)ActState.merge;
                        break;
                    }                    
                }
                else
                {
                    if (!CharMerge.Contains(this.gameObject))
                        CharMerge.Add(this.gameObject);
                }                
                StartCoroutine(Move(Goal));
                UnitState = (int)ActState.run;
                break;

            case (int)ActState.merge:
                if(CharMerge.Count ==0)
                {
                    UnitState = (int)ActState.run;
                    break;
                }

                Goal = CharMerge[Random.Range(0, CharMerge.Count)].GetComponent<Unit>().Cur;               
                StartCoroutine(Move(Goal));
                UnitState = (int)ActState.run;
                break;

            case (int)ActState.run:
                break;

            case (int)ActState.attack:
                if(Enemy==null)
                {
                    _ani.SetBool("Attack", false);
                    UnitState = (int)ActState.run;
                    StartCoroutine(Move(Cur));
                    break;
                }
                if(!prefab_Fight)
                {
                    prefab_Fight = Instantiate(GameManager.instance.prefabs[0], (transform.position + Enemy.transform.position)/2, Quaternion.identity);
                    prefab_Fight.transform.localScale *= 0.5f;
                    Destroy(prefab_Fight, 0.5f);
                }
                fightingtimer += Time.deltaTime;
                if (fightingtimer >= 1)
                {
                    fightingtimer = 0;
                    
                    if (Enemy == null)
                    {
                        _ani.SetBool("Attack", false);
                        StartCoroutine(Move(Cur));
                    }
                    else if (Enemy.GetComponent<Unit>() != null) //적이 캐릭터면
                    {
                        Enemy.GetComponent<Unit>().Amount -= Mathf.CeilToInt(Amount/10f);
                        Amount--;                        
                    }
                    else if (Enemy.GetComponent<Node>() != null) //적이 배럭이면
                    {
                        Enemy.GetComponent<Node>().HP -= Amount*5;
                        Amount--;                        
                    }
                    else if(Enemy.GetComponent<TreeAndBush>() != null) //적이 방해물이면
                    {
                        Enemy.GetComponent<TreeAndBush>().HP -= Amount * 5;
                        Amount--;
                    }
                }
                break;

            case (int)ActState.build:
                //가장 가까운 빌드 할수있는곳 찾아서 move하고 move 한뒤 그곳에 빌드 할수 있는지 체크하고 지음
                break;

            case (int)ActState.chase:
                if(ChaseUnit==null)
                {
                    IsChase = false;
                    UnitState = (int)ActState.stand;
                    break;
                }
                StopAllCoroutines();
                StartCoroutine(Chase(ChaseUnit));
                UnitState = (int)ActState.run;
                //케릭터를 누르고 버튼을 눌러 그 캐릭터의 cur 위치로 move하도록 함
                break;

            case (int)ActState.barrigate:
                if (Next == Goal)
                {
                    IsBarrigate = false;
                    StopAllCoroutines();                   

                    prefab_Obstacle = Instantiate(GameManager.instance.Obstacle, GameManager.instance.change.CellToWorld(Goal), Quaternion.identity);
                    prefab_Obstacle.GetComponent<TreeAndBush>().Pos = Goal;
                    Next = Cur;                    

                    GameManager.instance.TileMaster[Goal.x + GameManager.instance.mapsize / 2, Goal.y + GameManager.instance.mapsize / 2] = 5;
                    //GameManager.instance.change.SetTile(Goal, _tile);
                    _ani.SetBool("Run", false);

                    UnitState = (int)ActState.run;
                }
                //가장 가까운 빌드 할수있는곳 찾아서 move하고 move 한뒤 그곳에 빌드 할수 있는지 체크하고 지음
                break;

            case (int)ActState.bridge:
                //가장 가까운 빌드 할수있는곳 찾아서 move하고 move 한뒤 그곳에 빌드 할수 있는지 체크하고 지음
                if (Goal == Cur)
                {
                    float distance = Vector2.Distance(this.transform.position, GameManager.instance.map.CellToWorld(Goal));
                    if (distance < 0.1f)
                    {
                        IsBridge = false;

                        Tile _tile = Resources.Load<Tile>("Tile/BridgeLU"); //지형에 따라 RU

                        GameManager.instance.map.SetTile(BridgeLocation, _tile);

                        //UnitState = (int)ActState.stand;  //Move에서 이미 Stand로 넘어감
                    }
                }

                break;
        }
    }

    public class Astar
    {
        public int f;
        public int h;        
        public int g;
        public Vector3Int pos;
        public Astar parent;


        public bool Compare(int x, int y)
        {
            // TODO: Handle x or y being null, or them not having names
            return x < y;
        }

        public Astar()
        {
            pos = Vector3Int.zero;
            parent = null;
            f = 0;
            g = 0;
            h = 0;
        }

        public Astar Clone()
        {
            Astar astar = new Astar();
            astar.pos = this.pos;
            astar.parent = this.parent;
            astar.f = this.f;
            astar.g = this.g;
            astar.h = this.h;
            return astar;
        }
    }

    private List<Astar> GetNeighborTiles(Astar cur)
    {
        List<Astar> neighbors = new List<Astar>();

        Astar origin = new Astar();
        origin.parent = cur;
        origin.g = cur.g;

        Astar temp1 = origin.Clone();
        Astar temp2 = origin.Clone();
        Astar temp3 = origin.Clone();
        Astar temp4 = origin.Clone();

        Astar temp5 = origin.Clone();
        Astar temp6 = origin.Clone();
        Astar temp7 = origin.Clone();
        Astar temp8 = origin.Clone();

        Astar temp9 = origin.Clone();
        Astar temp10 = origin.Clone();
        Astar temp11 = origin.Clone();
        Astar temp12 = origin.Clone();

        temp1.pos = cur.pos + Vector3Int.right;
        neighbors.Add(temp1);
        temp2.pos = cur.pos + Vector3Int.left;
        neighbors.Add(temp2);
        temp3.pos = cur.pos + Vector3Int.up;
        neighbors.Add(temp3);
        temp4.pos = cur.pos + Vector3Int.down;
        neighbors.Add(temp4);

        Vector3Int temp = new Vector3Int(0, 0, 1);

        temp5.pos = cur.pos + Vector3Int.right + temp;
        neighbors.Add(temp5);
        temp6.pos = cur.pos + Vector3Int.left + temp;
        neighbors.Add(temp6);
        temp7.pos = cur.pos + Vector3Int.up + temp;
        neighbors.Add(temp7);
        temp8.pos = cur.pos + Vector3Int.down + temp;
        neighbors.Add(temp8);

        temp9.pos = cur.pos + Vector3Int.right - temp;
        neighbors.Add(temp9);
        temp10.pos = cur.pos + Vector3Int.left - temp;
        neighbors.Add(temp10);
        temp11.pos = cur.pos + Vector3Int.up - temp;
        neighbors.Add(temp11);
        temp12.pos = cur.pos + Vector3Int.down - temp;
        neighbors.Add(temp12);
        
        int rand;
        List<Astar> ret = new List<Astar>();
        while (neighbors.Count>0)
        {
            rand = Random.Range(0, neighbors.Count);
            ret.Add(neighbors[rand]);
            neighbors.RemoveAt(rand);
        }
        return ret;
        //return neighbors;
    }

    private Astar FindNextTile(List<Astar> openList)
    {
        if (openList.Count == 0) return null;

        Astar result = openList[0];

        foreach (Astar tile in openList)
        {
            if (tile.f < result.f) result = tile;
        }

        return result;

    }


    private List<Vector3Int> PathFinder(Vector3Int Start , Vector3Int Target) //걸을수 없는곳 나중에 추가
    {
        List<Vector3Int> path = new List<Vector3Int>();
        List<Astar> openList = new List<Astar>();
        List<Vector3Int> closedList = new List<Vector3Int>();
        Astar currentTile = new Astar();
        currentTile.pos = Start;
        List<Astar> neighbors;
        bool InOpenlist;

        while (true)
        {
            neighbors = GetNeighborTiles(currentTile);
            
            for (int i=0;i<neighbors.Count;i++)
            {

                if (!GameManager.instance.CanGo(neighbors[i].pos))continue; //못가는 곳인경우 또는 이미 끝인경우
                if (closedList.Contains(neighbors[i].pos) == true)continue;

                InOpenlist = false;
                for (int j=0;j<openList.Count;j++)
                {
                    if (openList[j].pos == neighbors[i].pos)
                    {
                        InOpenlist = true;
                        break;
                    }
                }

                if (InOpenlist)
                {
                    Astar oldParent = neighbors[i].parent;
                    int oldG = neighbors[i].g;
                    neighbors[i].parent = currentTile;
                    neighbors[i].g = currentTile.g + 1;
                    neighbors[i].h = Mathf.Abs(Target.x - neighbors[i].pos.x) + Mathf.Abs(Target.y - neighbors[i].pos.y);
                    neighbors[i].f = neighbors[i].g + neighbors[i].h;
                    if (neighbors[i].g >= oldG)
                    {
                        neighbors[i].parent = oldParent;
                        neighbors[i].g = oldParent.g + 1;
                        neighbors[i].h = Mathf.Abs(Target.x - neighbors[i].pos.x) + Mathf.Abs(Target.y - neighbors[i].pos.y);
                        neighbors[i].f = neighbors[i].g + neighbors[i].h;
                    }                        
                }
                else
                {
                    neighbors[i].parent = currentTile;
                    neighbors[i].g = currentTile.g + 1;
                    neighbors[i].h = Mathf.Abs(Target.x - neighbors[i].pos.x) + Mathf.Abs(Target.y - neighbors[i].pos.y);
                    neighbors[i].f = neighbors[i].g + neighbors[i].h;
                    openList.Add(neighbors[i]);
                }
            }

            closedList.Add(currentTile.pos);

            openList.Remove(currentTile);

            if (closedList.Contains(Target) == true)
                break;

            currentTile = FindNextTile(openList);

           

            if (currentTile == null)
            {
                UnitState = (int)ActState.run;
                return null;
            }
        }
        while (currentTile.pos != Start)
        {
            path.Add(currentTile.pos);
            currentTile = currentTile.parent;
        }
        path.Reverse();

        return path;
    }

    IEnumerator Chase(GameObject targetUnit)
    {
        Vector2 pos;
        Vector2 dir;
        Vector3Int target = targetUnit.GetComponent<Unit>().Cur;
        float distance;
        float distance_temp;

        float _distance;
        Vector2 post_pos;
        List<Vector3Int> path = new List<Vector3Int>();//A STAR 를 통해 target까지 가는 vec3int  List

        path = PathFinder(Cur, target);

        dir = GameManager.instance.map.CellToWorld(Cur) - transform.position;
        dir.Normalize();
        distance = Vector2.Distance(this.transform.position, GameManager.instance.map.CellToWorld(Cur));
        distance_temp = 99999999;
        _ani.SetBool("Run", true);

        if (distance > 0.1f) //처음에 위치 맞추기
        {
            _ani.SetFloat("DirX", dir.x);
            _ani.SetFloat("DirY", dir.y);

            while (true)
            {
                this.transform.Translate(dir * Time.deltaTime);
                distance = Vector2.Distance(this.transform.position, GameManager.instance.map.CellToWorld(Cur));
                yield return null;

                if (distance > distance_temp || distance < 0.01f) //dist가 작아지다가 커지면
                    break;

                distance_temp = distance;
                post_pos = this.transform.position;
            }
        }

        if (path == null)
        {
            Goal = ChooseGoal();

            target = Goal;

            path = PathFinder(Cur, target);
        }
        Tile _tile = Resources.Load<Tile>("Tile/1");
        if (path.Count == 0)
        {
            if (GameManager.instance.IsTileChangeOK(Cur))
                GameManager.instance.map.SetTile(Cur, _tile);
            GameManager.instance.map.SetTileFlags(Cur, TileFlags.None);
            GameManager.instance.map.SetColor(Cur, _color);
            transform.position += Vector3.forward * (-transform.position.z + Cur.z + 2);

            dir = GameManager.instance.map.CellToWorld(Cur) - transform.position;
            dir.Normalize();

            distance_temp = 99999999;
            distance = Vector2.Distance(this.transform.position, GameManager.instance.map.CellToWorld(Cur));

            if (distance > 0.1f)
            {
                _ani.SetFloat("DirX", dir.x);
                _ani.SetFloat("DirY", dir.y);
            }
            while (true)
            {
                this.transform.Translate(dir * Time.deltaTime);
                distance = Vector2.Distance(this.transform.position, GameManager.instance.map.CellToWorld(Cur));
                yield return null;

                if (distance > distance_temp || distance < 0.01f) //dist가 작아지다가 커지면
                    break;

                distance_temp = distance;
                post_pos = this.transform.position;
            }
        }
        else
        {
            Next = path[0];


            vec3_temp = path[0] - Cur;
            if (vec3_temp.x == 0 && vec3_temp.y == 1)
            {
                dir.x = -1f;
                dir.y = 0.5f;
            }
            else if (vec3_temp.x == 0 && vec3_temp.y == -1)
            {
                dir.x = 1f;
                dir.y = -0.5f;
            }
            else if (vec3_temp.x == 1 && vec3_temp.y == 0)
            {
                dir.x = 1f;
                dir.y = 0.5f;
            }
            else
            {
                dir.x = -1f;
                dir.y = -0.5f;
            }
            pos = GameManager.instance.map.CellToWorld(Next);
            dir.Normalize();

            _ani.SetFloat("DirX", dir.x);
            _ani.SetFloat("DirY", dir.y);
            distance_temp = 99999999;
            _distance = 0;
            post_pos = transform.position;
            IsYChanged = false;
            IsMoving = true;
            while (true)
            {
                this.transform.Translate(dir * Time.deltaTime);
                distance = Vector2.Distance(this.transform.position, pos);
                _distance += Vector2.Distance(this.transform.position, post_pos);
                yield return null;

                if (distance > distance_temp || distance < 0.01f) //dist가 작아지다가 커지면
                    break;

                if (!IsYChanged && _distance > 0.2f) //거의다 도착했을때
                {
                    IsYChanged = true;
                    transform.position += Vector3.up * (Next.z - Cur.z) * 0.25f;
                    transform.position += Vector3.forward * (Next.z - Cur.z);
                    if (IsOnBarrack)
                    {
                        transform.position += Vector3.forward;
                    }
                    distance_temp = 99999999;
                    Cur = Next;
                    continue;
                }
                distance_temp = distance;
                post_pos = this.transform.position;
            }
            Cur = Next;
            IsMoving = false;

            this.transform.position = pos;
            transform.position += Vector3.forward * (-transform.position.z + Cur.z + 2);
            if (IsOnBarrack)
            {
                transform.position += Vector3.forward;
            }
            if (GameManager.instance.IsTileChangeOK(Cur))
                GameManager.instance.map.SetTile(Cur, _tile);
            GameManager.instance.map.SetTileFlags(Cur, TileFlags.None);
            GameManager.instance.map.SetColor(Cur, _color);
        }        
        UnitState = (int)ActState.chase;        

        yield return null;
    }

    IEnumerator Move(Vector3Int target)
    {
        Vector2 pos;
        Vector2 dir;
        float distance;
        float distance_temp;

        float _distance;
        Vector2 post_pos;
        List<Vector3Int>path = new List<Vector3Int>();//A STAR 를 통해 target까지 가는 vec3int  List

        path = PathFinder(Cur, target);

        dir = GameManager.instance.map.CellToWorld(Cur) - transform.position;
        dir.Normalize();
        distance = Vector2.Distance(this.transform.position, GameManager.instance.map.CellToWorld(Cur));
        distance_temp = 99999999;
        if (distance > 0.1f) //처음에 위치 맞추기
        {
            _ani.SetBool("Run", true);
            _ani.SetFloat("DirX", dir.x);
            _ani.SetFloat("DirY", dir.y);
            
            while (true)
            {
                this.transform.Translate(dir * Time.deltaTime);
                distance = Vector2.Distance(this.transform.position, GameManager.instance.map.CellToWorld(Cur));
                yield return null;

                if (distance > distance_temp || distance < 0.01f) //dist가 작아지다가 커지면
                    break;

                distance_temp = distance;
                post_pos = this.transform.position;
            }
            _ani.SetBool("Run", false);
        }


        Tile _tile = Resources.Load<Tile>("Tile/1");
        if(path==null)
        {
            Goal = ChooseGoal();

            target = Goal;

            path = PathFinder(Cur, target);
        }       
        if (path.Count == 0)
        {
            if (GameManager.instance.IsTileChangeOK(Cur))
                GameManager.instance.map.SetTile(Cur, _tile);
            GameManager.instance.map.SetTileFlags(Cur, TileFlags.None);
            GameManager.instance.map.SetColor(Cur, _color);
            transform.position += Vector3.forward * (-transform.position.z + Cur.z + 2);

            dir = GameManager.instance.map.CellToWorld(Cur) - transform.position;
            dir.Normalize();

            distance_temp = 99999999;
            distance = Vector2.Distance(this.transform.position, GameManager.instance.map.CellToWorld(Cur));

            if (distance > 0.1f)
            {
                _ani.SetBool("Run", true);
                _ani.SetFloat("DirX", dir.x);
                _ani.SetFloat("DirY", dir.y);
            }
            while (true)
            {
                this.transform.Translate(dir * Time.deltaTime);
                distance = Vector2.Distance(this.transform.position, GameManager.instance.map.CellToWorld(Cur));
                yield return null;

                if (distance > distance_temp || distance < 0.01f) //dist가 작아지다가 커지면
                    break;

                distance_temp = distance;
                post_pos = this.transform.position;
            }
            _ani.SetBool("Run", false);
        }
        while (path.Count != 0)
        {
            _ani.SetBool("Run", true);
            Next = path[0];

            vec3_temp = path[0] - Cur;
            if (vec3_temp.x == 0 && vec3_temp.y == 1)
            {
                dir.x = -1f;
                dir.y = 0.5f;
            }
            else if (vec3_temp.x == 0 && vec3_temp.y == -1)
            {
                dir.x = 1f;
                dir.y = -0.5f;
            }
            else if (vec3_temp.x == 1 && vec3_temp.y == 0)
            {
                dir.x = 1f;
                dir.y = 0.5f;
            }
            else
            {
                dir.x = -1f;
                dir.y = -0.5f;
            }
            pos = GameManager.instance.map.CellToWorld(Next);
            dir.Normalize();

            _ani.SetFloat("DirX", dir.x);
            _ani.SetFloat("DirY", dir.y);
            distance_temp = 99999999;
            _distance = 0;
            post_pos = transform.position;
            IsYChanged = false;
            IsMoving = true;
            while (true)
            {
                this.transform.Translate(dir * Time.deltaTime);
                distance = Vector2.Distance(this.transform.position, pos);
                _distance += Vector2.Distance(this.transform.position, post_pos);
                yield return null;

                if (distance > distance_temp || distance < 0.01f) //dist가 작아지다가 커지면
                    break;

                if (!IsYChanged && _distance > 0.2f) //거의다 도착했을때
                {
                    IsYChanged = true;
                    transform.position += Vector3.up * (Next.z - Cur.z) * 0.25f;
                    transform.position += Vector3.forward * (Next.z - Cur.z);
                    if (IsOnBarrack)
                    {
                        transform.position += Vector3.forward;
                    }
                    distance_temp = 99999999;
                    Cur = Next;
                    continue;
                }
                distance_temp = distance;
                post_pos = this.transform.position;
            }
            Cur = Next;
            IsMoving = false;
            path.RemoveAt(0);

            this.transform.position = pos;
            transform.position += Vector3.forward * (-transform.position.z + Cur.z + 2);
            if (IsOnBarrack)
            {
                transform.position += Vector3.forward;
            }
            if (GameManager.instance.IsTileChangeOK(Cur))
                GameManager.instance.map.SetTile(Cur, _tile);
            GameManager.instance.map.SetTileFlags(Cur, TileFlags.None);
            GameManager.instance.map.SetColor(Cur, _color);
        }
        _ani.SetBool("Run", false);

        yield return new WaitForSeconds(0.5f);
        if (GameManager.instance.IsBarrackOK(Cur))//주변에 건물 있는지
        {
            prefab_Barrack = Instantiate(GameManager.instance.Barrack, GameManager.instance.change.CellToWorld(Cur), Quaternion.identity);
            prefab_Barrack.transform.position += 2 * Vector3.forward;
            prefab_Barrack.GetComponent<Node>().Master = Master;
            prefab_Barrack.GetComponent<Node>().Pos = Cur;
            IsOnBarrack = true;
            GameManager.instance.TileMaster[Cur.x + GameManager.instance.mapsize / 2, Cur.y + GameManager.instance.mapsize / 2] = Master;
        }
        if(!IsStand)
        {
            UnitState = (int)ActState.stand;            
        }
        else
        {
            IsStand = false;
        }
        if(IsChase)
        {
            UnitState = (int)ActState.chase;
        }

        yield return null;
    
    }


    private Vector3Int ChooseGoal()
    {
        Vector3Int vec3ret = Cur;
        Vector3Int vec3temp;

        List<Vector3Int> vec3list = new List<Vector3Int>();
        List<Vector3Int> visited = new List<Vector3Int>();

        Vector3Int temp = new Vector3Int(0, 0, 1);

        int count = 0;

        //넓혀가는식으로 가장 가까운 것 찾음
        vec3list.Add(Cur);
        while(vec3list.Count!=0)
        {
            vec3temp = vec3list[0];
            vec3list.RemoveAt(0);

            if (!GameManager.instance.CanGo(vec3temp))
                continue;

            if (count++ > 200)
                break;

            if (GameManager.instance.map.GetColor(vec3temp) != _color)
            {
                vec3ret = vec3temp;
                break;
            }

            if (visited.Contains(vec3temp) == true)
                continue;

            visited.Add(vec3temp);


            int rand = Random.Range(0, 4);

            switch(rand)
            {
                case 0:
                    vec3list.Add(vec3temp + Vector3Int.up);
                    vec3list.Add(vec3temp + Vector3Int.down);
                    vec3list.Add(vec3temp + Vector3Int.right);
                    vec3list.Add(vec3temp + Vector3Int.left);
                    vec3list.Add(vec3temp + Vector3Int.up + temp);
                    vec3list.Add(vec3temp + Vector3Int.down + temp);
                    vec3list.Add(vec3temp + Vector3Int.right + temp);
                    vec3list.Add(vec3temp + Vector3Int.left + temp);
                    vec3list.Add(vec3temp + Vector3Int.up - temp);
                    vec3list.Add(vec3temp + Vector3Int.down - temp);
                    vec3list.Add(vec3temp + Vector3Int.right - temp);
                    vec3list.Add(vec3temp + Vector3Int.left - temp);
                    break;
                case 1:
                    vec3list.Add(vec3temp + Vector3Int.down);
                    vec3list.Add(vec3temp + Vector3Int.right);
                    vec3list.Add(vec3temp + Vector3Int.left);
                    vec3list.Add(vec3temp + Vector3Int.up);
                    vec3list.Add(vec3temp + Vector3Int.down + temp);
                    vec3list.Add(vec3temp + Vector3Int.right + temp);
                    vec3list.Add(vec3temp + Vector3Int.left + temp);
                    vec3list.Add(vec3temp + Vector3Int.up + temp);
                    vec3list.Add(vec3temp + Vector3Int.down - temp);
                    vec3list.Add(vec3temp + Vector3Int.right - temp);
                    vec3list.Add(vec3temp + Vector3Int.left - temp);
                    vec3list.Add(vec3temp + Vector3Int.up - temp);                    
                    break;
                case 2:
                    vec3list.Add(vec3temp + Vector3Int.right);
                    vec3list.Add(vec3temp + Vector3Int.left);
                    vec3list.Add(vec3temp + Vector3Int.up);
                    vec3list.Add(vec3temp + Vector3Int.down);
                    vec3list.Add(vec3temp + Vector3Int.right + temp);
                    vec3list.Add(vec3temp + Vector3Int.left + temp);
                    vec3list.Add(vec3temp + Vector3Int.up + temp);
                    vec3list.Add(vec3temp + Vector3Int.down + temp);                    
                    vec3list.Add(vec3temp + Vector3Int.right - temp);
                    vec3list.Add(vec3temp + Vector3Int.left - temp);
                    vec3list.Add(vec3temp + Vector3Int.up - temp);
                    vec3list.Add(vec3temp + Vector3Int.down - temp);
                    break;
                case 3:                   
                    vec3list.Add(vec3temp + Vector3Int.left);
                    vec3list.Add(vec3temp + Vector3Int.up);
                    vec3list.Add(vec3temp + Vector3Int.down);
                    vec3list.Add(vec3temp + Vector3Int.right);                    
                    vec3list.Add(vec3temp + Vector3Int.left + temp);
                    vec3list.Add(vec3temp + Vector3Int.up + temp);
                    vec3list.Add(vec3temp + Vector3Int.down + temp);
                    vec3list.Add(vec3temp + Vector3Int.right + temp);                    
                    vec3list.Add(vec3temp + Vector3Int.left - temp);
                    vec3list.Add(vec3temp + Vector3Int.up - temp);
                    vec3list.Add(vec3temp + Vector3Int.down - temp);
                    vec3list.Add(vec3temp + Vector3Int.right - temp);
                    break;
            }            
        }
        return vec3ret;
    }

    public void GoOrder(Vector3Int goPosition)
    {
        UnitState = (int)ActState.run;
        StopAllCoroutines();
        Goal = goPosition;
        StartCoroutine(Move(goPosition));
    }
    public void GoBridgeOrder(Vector3Int BridgePosition)
    {
        BridgeLocation = BridgePosition;
        List<Vector3Int> path = new List<Vector3Int>();
        List<Vector3Int> path_temp = new List<Vector3Int>();
        int index = -1;
        path = PathFinder(Cur, BridgePosition + Vector3Int.up);


        if(path!=null)
        {
            index = 0;
        }
        path_temp = PathFinder(Cur, BridgePosition + Vector3Int.down);
        if (path_temp != null)
        {
            if (path == null)
            {
                path = path_temp;
                index = 1;
            }
            else if (path_temp.Count < path.Count)
            {
                path = path_temp;
                index = 1;
            }
        }
        path_temp = PathFinder(Cur, BridgePosition + Vector3Int.right);

        if (path_temp != null)
        {
            if (path == null)
            {
                path = path_temp;
                index = 2;
            }
            else if (path_temp.Count < path.Count)
            {
                path = path_temp;
                index = 2;
            }
        }
        path_temp = PathFinder(Cur, BridgePosition + Vector3Int.left);
        if (path_temp != null)
        {
            if (path == null)
            {
                path = path_temp;
                index = 3;
            }
            else if (path_temp.Count < path.Count)
            {
                path = path_temp;
                index = 3;
            }
        }
        if(index == 0)
            Goal = BridgePosition + Vector3Int.up;
        else if(index == 1)
            Goal = BridgePosition + Vector3Int.down;
        else if (index == 2)
            Goal = BridgePosition + Vector3Int.right;
        else if (index == 3)
            Goal = BridgePosition + Vector3Int.left;
        if(index!=-1)
        {
            UnitState = (int)ActState.bridge;
            StopAllCoroutines();
            IsBridge = true;
            StartCoroutine(Move(Goal));
        }
    }
    public void ChaseOrder()
    {
        UnitState = (int)ActState.chase;
        IsChase = true;
    }
    public void ObstacleOrder(Vector3Int goPosition)
    {
        IsBarrigate = true;
        Goal = goPosition;
        UnitState = (int)ActState.barrigate;
        StopAllCoroutines();
        StartCoroutine(Move(goPosition));
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Unit") && collision.GetComponent<Unit>().Cur ==Cur)
        {
            if (collision.GetComponent<Unit>().Master == Master) //우리팀이랑 만났을떄
            {
                if (collision.GetComponent<Unit>().Amount > Amount)
                {
                    collision.GetComponent<Unit>().Amount += Amount;
                    CharMerge.Remove(this.gameObject);
                    Destroy(this.gameObject);
                }
                else if (collision.GetComponent<Unit>().Amount == Amount)
                {
                    if(collision.GetComponent<Unit>().UnitState > UnitState)
                    {
                        collision.GetComponent<Unit>().Amount += Amount;
                        CharMerge.Remove(this.gameObject);
                        Destroy(this.gameObject);
                    }
                    else if(collision.GetComponent<Unit>().UnitState == UnitState)
                    {
                        if (collision.transform.position.y < transform.position.y)
                        {
                            collision.GetComponent<Unit>().Amount += Amount;
                            CharMerge.Remove(this.gameObject);
                            Destroy(this.gameObject);
                        }
                    }                        
                }
            }
            else //상대팀과 만났을떄
            {
                if (UnitState != (int)ActState.attack && collision.GetComponent<Unit>().Cur == Cur)
                {
                    StopAllCoroutines(); //멈추고
                    _ani.SetBool("Run", false);
                    _ani.SetBool("Attack", true);

                    Enemy = collision.gameObject;

                    Enemy.GetComponent<Animator>().SetFloat("DirX", transform.position.x - Enemy.transform.position.x);
                    Enemy.GetComponent<Animator>().SetFloat("DirY", transform.position.y - Enemy.transform.position.y);                     
                    UnitState = (int)ActState.attack;
                }
            }
        }

        if (collision.CompareTag("Node") && collision.GetComponent<Node>().Pos == Next)
        {
            if (collision.GetComponent<Node>().Master != Master) //만났는데 다른 팀 배럭이면
            {
                if (UnitState != (int)ActState.attack)
                {
                    StopAllCoroutines(); //멈추고
                    _ani.SetBool("Run", false);
                    _ani.SetBool("Attack", true);

                    Enemy = collision.gameObject;
                    UnitState = (int)ActState.attack;
                }                    
            }
        }
        if (collision.CompareTag("Node") && collision.GetComponent<Node>().Master == Master)
        {
            if(collision.GetComponent<Node>().Pos == Next || collision.GetComponent<Node>().Pos == Cur)
            {
                IsOnBarrack = true;
            }
            else
            {
                IsOnBarrack = false;
            }
        }
        if (collision.CompareTag("TreeAndBush") && collision.GetComponent<TreeAndBush>().Pos == Next)
        {
            if (UnitState != (int)ActState.attack /*&& Amount>10*/)
            {
                StopAllCoroutines(); //멈추고
                _ani.SetBool("Run", false);
                _ani.SetBool("Attack", true);

                Enemy = collision.gameObject;
                UnitState = (int)ActState.attack;
            }
        }


    }
}
