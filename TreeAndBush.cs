using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAndBush : MonoBehaviour
{
    public Vector3Int Pos;

    public int HP;

    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = GameManager.instance.map.CellToWorld(Pos);
        transform.position += Vector3.forward;
    }
    private void Update()
    {
        if(HP<=0)
        {
            GameManager.instance.TileMaster[Pos.x + GameManager.instance.mapsize / 2, Pos.y + GameManager.instance.mapsize / 2] = 0;

            Destroy(this.gameObject);
        }
    }
}
