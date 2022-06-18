using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum CellState
{
    Empty,
    Fill,
    line
}

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; set; }
    const int CELL_SIZE = 10;

    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] Transform[] BlockSpawnPoints;
    [SerializeField] GameObject[] BlockObj;
    public int[,] Cell = new int[CELL_SIZE, CELL_SIZE];

    [SerializeField] private Transform BorderFront;
    [SerializeField] private GameObject[] cellObject = new GameObject[100];
    [SerializeField] private Text ScoreText;
    [SerializeField] private GameObject DieText;

    private Color CellColor;
    private int score = 0;
    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            ScoreText.text = $"{score}";
        }
    } 
    void Awake()
    {
        Inst = this;
        SpawnBlock();
        LoadCells();
        CellColor = cellObject[0].GetComponent<SpriteRenderer>().color;
    }

    #region ���� ó��
    //�����鳢�� ��Ҵ��� Ȯ��
    bool BlockCollision(int x, int y)
    {
        if (Cell[x, y] == (int)CellState.Fill) return true;
        return false;
    }

    //������ 10x10 ĭ�� �Ѿ���� Ȯ��
    bool OutCheck(int x, int y)
    {
        if (x < 0 || y < 0 || x >= CELL_SIZE || y >= CELL_SIZE) return true;
        return false;
    }

    //x,y ���� �����Ǵ� cellObject�� ã�Ƽ� ��ȯ��
    GameObject GetCell(int x, int y)
    {
        return cellObject[(y * 10) + x];
    }
    #endregion

    //������ ������ �� ó��
    public void PutPuzzle(Block block, Vector3[] ShapeCell, Vector3 lastPos)
    {
        for (int i = 0; i < ShapeCell.Length; i++)
        {
            Vector3 Pos = ShapeCell[i] + lastPos;
            if (OutCheck((int)Pos.x, (int)Pos.y))
            {
                block.BlockReturn();
                return;
            }
            if (BlockCollision((int)Pos.x, (int)Pos.y))
            {
                block.BlockReturn(); 
                return;
            }
        }

        for (int i = 0; i < ShapeCell.Length; i++)
        {
            Vector3 Pos = ShapeCell[i] + lastPos;
            print("ä����");
            Cell[(int)Pos.x, (int)Pos.y] = (int)CellState.Fill;
            GetCell((int)Pos.x, (int)Pos.y).GetComponent<SpriteRenderer>().color =
                block.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color;
        }
        block.DeleteBlock();
        ChekcLine(ShapeCell.Length);
        Invoke("CheckLogic", 0.25f);
    }

    void ChekcLine(int ShapeCount)
    {
        int line = 0;
        //������ �ϼ������ üũ
        for (int y = 0; y < CELL_SIZE; y++)
        {
            int HorizontalCount = 0;
            int VerticalCount = 0;

            for (int x = 0; x < CELL_SIZE; x++)
            {
                if (Cell[x, y] == (int)CellState.Fill) HorizontalCount++;
                if (Cell[y, x] == (int)CellState.Fill) VerticalCount++;

                if(HorizontalCount == 10)
                {
                    line++;
                    for (int i = 0; i < CELL_SIZE; i++) Cell[i, y] = (int)CellState.line;
                }

                if (VerticalCount == 10)
                {
                    line++;
                    for (int i = 0; i < CELL_SIZE; i++) Cell[y, i] = (int)CellState.line;
                }
            }
        }

        //�ı�
        for(int x = 0; x < CELL_SIZE; x++)
        {
            for (int y = 0; y < CELL_SIZE; y++)
            {
                if(Cell[x,y] == (int)CellState.line)
                {                   
                    StartCoroutine(BlockMove(GetCell(x, y), Vector3.zero, false, 0.25f));
                    StartCoroutine(CellRecycling(GetCell(x,y)));
                }
            }
        }
        Score += line * 10 + ShapeCount; 
    }

    //������ ������ ��� ��ġ�Ǿ����� Ȯ��
    void CheckLogic()
    {
        int count = 0;
        int dieCheck = 0;
        for (int i = 0; i < BlockSpawnPoints.Length; i++)
        {
            if (BlockSpawnPoints[i].childCount != 0)
            {
                count++;
                if (PutUnable(BlockSpawnPoints[i].GetComponentInChildren<Block>().ShapePos)) dieCheck++;   
            }
        }

        if (count == 0)
        {
            SpawnBlock(); return;
        }          
        else if (count == dieCheck) Die();
    }

    //��� ��ǥ�� �ش� ������ �� ��ǥ�� �ִ� �� ��ü������ Ȯ��
    bool PutUnable(Vector3[] ShapePos)
    {
        int Check = 0;
        for (int y = 0; y < CELL_SIZE; y++)
        {
            for (int x = 0; x < CELL_SIZE; x++)
            {
                if (!UnableChecking(x, y, ShapePos)) Check++;
            }
        }
        if (Check == 0) return true;
        else return false;
    }

    //���ڷ� ���� ��ǥ�� �ش� ������ �� �� �ִ� �� üũ
    bool UnableChecking(int x, int y, Vector3[] ShapePos)
    {
        for (int i = 0; i < ShapePos.Length; i++)
        {
            Vector3 SumPos = GetCell(x, y).transform.position + ShapePos[i];

            if (OutCheck((int)SumPos.x, (int)SumPos.y)) return true;
            if (BlockCollision((int)SumPos.x, (int)SumPos.y)) return true;
        }
        return false;
    }

    //���� ���� ó��
    void Die()
    {
        print("Die");
        DieText.SetActive(true);
    }

    //���� ����
    void SpawnBlock()
    {

        for (int i = 0; i < BlockSpawnPoints.Length; i++)
        {
            int Rand = Random.Range(0, BlockObj.Length);

            GameObject clone = Instantiate(BlockObj[Rand],
                BlockSpawnPoints[i].position + new Vector3(10, 0, 0), Quaternion.identity, BlockSpawnPoints[i].transform);

            clone.GetComponent<Block>().SetUP(BlockSpawnPoints[i].position, 0.5f);
        }
    }

    //Cell���� �迭�� ���������� ���
    void LoadCells()
    {
        int num = 0;
        foreach(Transform cell in BorderFront)
        {
            cellObject[num] = cell.gameObject;
            num++;
        }
    }

    //�ε巯�� ������
    public IEnumerator BlockMove(GameObject obj, Vector3 endPos, bool isMove, float time)
    {
        Vector3 startPos;
        startPos = (isMove) ? obj.transform.position : obj.transform.localScale; 

        float current = 0;
        float percent = 0;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            if (isMove) obj.transform.position = Vector3.Lerp(startPos, endPos, movementCurve.Evaluate(percent));
            else obj.transform.localScale = Vector3.Lerp(startPos, endPos, movementCurve.Evaluate(percent));

            yield return null;
        }
    }

    private IEnumerator CellRecycling(GameObject Cell)
    {
        yield return new WaitForSeconds(0.25f);
        Cell.transform.localScale = Vector3.one * 0.9f;
        Cell.GetComponent<SpriteRenderer>().color = CellColor;
    }
}