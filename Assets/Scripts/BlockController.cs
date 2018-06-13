using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockController : MonoBehaviour
{
    const int GRIDsize = 4;
    const int INITIALnumberCOUNT = 2;

    [SerializeField]
    Button[] gatherButtons;
    [SerializeField]
    GameObject gridOrigin;//Prefab化したマスのオブジェクト
    [SerializeField]
    float fourGenerateRate = 0.3f;//初期4の生成率
    [SerializeField]
    Text scoreText;
    [SerializeField]
    GameObject gameOverWin;

    int[,] numbers;
    int[,] subNumbers;
    Text[,] numberTexts;
    int score;
    bool gameOver;

    // Use this for initialization
    void Start()
    {
        NumberSetup();
        TextSetup();
        ButtonSetup();
        PlaySetup();
    }

    // Update is called once per frame
    void Update()
    {
        //キーボード入力受付
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            GatherOnSide(Side.Up);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            GatherOnSide(Side.Right);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            GatherOnSide(Side.Down);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GatherOnSide(Side.Left);
        }
    }

    #region Gathering Method
    void GatherOnSide(Side side)
    {
        if (gameOver) return;
        KeepNumbers();

        Vector2Int gatherDire = Vector2Int.down;
        Vector2Int rowDire = Vector2Int.right;
        Vector2Int firstPos = Vector2Int.zero;

        switch (side)
        {
            case Side.Up:
                gatherDire = Vector2Int.down;//unity座標系とは逆にとっている、下に行くほどy増
                rowDire = Vector2Int.right;
                firstPos = Vector2Int.up;
                break;
            case Side.Right:
                gatherDire = Vector2Int.right;
                rowDire = Vector2Int.up;
                firstPos = Vector2Int.right * (GRIDsize - 2);
                break;
            case Side.Down:
                gatherDire = Vector2Int.up;
                rowDire = Vector2Int.right;
                firstPos = Vector2Int.up * (GRIDsize - 2);
                break;
            case Side.Left:
                gatherDire = Vector2Int.left;
                rowDire = Vector2Int.up;
                firstPos = Vector2Int.right;
                break;
        }
        if (!CanMove(gatherDire)) return;//移動できなければ戻る

        Vector2Int currentPos;
        for (int direI = 0; direI < GRIDsize - 1; direI++)
        {
            currentPos = firstPos - gatherDire * direI;

            for (int rowI = 0; rowI < GRIDsize; rowI++)
            {
                if (numbers[currentPos.x, currentPos.y] > 0)
                {
                    Gather(gatherDire, currentPos.x, currentPos.y);
                }
                currentPos += rowDire;
            }
        }

        for (int xI = 0; xI < GRIDsize; xI++)
        {
            for (int yI = 0; yI < GRIDsize; yI++)
            {
                if (numbers[xI, yI] < 0)
                {
                    numbers[xI, yI] = -numbers[xI, yI];
                    UpdateNumberText(xI, yI);
                    AddScore(numbers[xI, yI]);
                }
            }
        }

        PlaceNewNumber();

        //ゲームオーバー判定
        if (CanMove(Vector2Int.up) || CanMove(Vector2Int.right)
            || CanMove(Vector2Int.down) || CanMove(Vector2Int.left)) return;

        gameOver = true;
        gameOverWin.SetActive(true);
    }

    //指定位置の数字を指定方向に寄せる再帰メソッド
    void Gather(Vector2Int dire, int x, int y)
    {
        int targetX = x + dire.x;
        int targetY = y + dire.y;
        if ((targetX < 0 || GRIDsize <= targetX)
            || (targetY < 0 || GRIDsize <= targetY)) return;

        if (numbers[targetX, targetY] == numbers[x, y])//同じ数字を足し合わせる
        {
            numbers[targetX, targetY] *= -2;//既にくっついたところに新たな数が足されないよう、-に
            numbers[x, y] = 0;
            UpdateNumberText(x, y);
            return;
        }
        else if (numbers[targetX, targetY] == 0)
        {
            numbers[targetX, targetY] = numbers[x, y];
            numbers[x, y] = 0;
            UpdateNumberText(targetX, targetY);
            UpdateNumberText(x, y);
            Gather(dire, targetX, targetY);
        }
        return;
    }
    #endregion

    void UpdateNumberText(int x, int y)
    {
        int val = numbers[x, y];
        string s;
        if (val == 0)
        {
            s = "";//何も表示しない
        }
        else
        {
            s = val.ToString();
        }
        //s = val == 0 ? "" : val.ToString();  //↑は←のような書き方もできる

        numberTexts[x, y].text = s;
    }

    #region Setup Method
    public void NumberSetup()
    {
        numbers = new int[GRIDsize, GRIDsize];
        subNumbers = new int[GRIDsize, GRIDsize];
        for (int xI = 0; xI < GRIDsize; xI++)
        {
            for (int yI = 0; yI < GRIDsize; yI++)
            {
                numbers[xI, yI] = 0;
                subNumbers[xI, yI] = 0;
            }
        }
    }

    //numberTextを初期化し、数値配列とText配列とを対応
    //事前に配置したものをSerializeFieldにして手動で割り当てても良いが、かなり面倒
    void TextSetup()
    {
        numberTexts = new Text[GRIDsize, GRIDsize];

        Vector2 centerPos = Vector2.down * 120;
        float length = gridOrigin.GetComponent<RectTransform>().sizeDelta.x;//横幅取得
        float margin = 10;
        Transform parent = new GameObject().transform;//空オブジェクトを生成
        parent.SetParent(transform);//このスクリプトがついているオブジェクトの子要素に設定
        parent.localScale = Vector3.one;//大きさを等倍に

        for (int xI = 0; xI < GRIDsize; xI++)
        {
            for (int yI = 0; yI < GRIDsize; yI++)
            {
                GameObject g = Instantiate(gridOrigin);
                g.transform.SetParent(parent);//空オブジェクトの子に設定
                g.transform.localPosition
                    = new Vector2((length + margin) * xI, -(length + margin) * yI);
                g.transform.localScale = Vector3.one;

                numberTexts[xI, yI] = g.GetComponentInChildren<Text>();
                UpdateNumberText(xI, yI);
            }
        }
        parent.localPosition = centerPos
            + new Vector2(-0.5f, 0.5f) * (length + margin) * (GRIDsize - 1);//位置調整
        parent.SetSiblingIndex(5);
    }

    //各ボタンにクリックイベント追加
    void ButtonSetup()
    {
        gatherButtons[0].onClick.AddListener(() => GatherOnSide(Side.Up));
        gatherButtons[1].onClick.AddListener(() => GatherOnSide(Side.Right));
        gatherButtons[2].onClick.AddListener(() => GatherOnSide(Side.Down));
        gatherButtons[3].onClick.AddListener(() => GatherOnSide(Side.Left));
    }

    void PlaySetup()
    {
        for (int i = 0; i < INITIALnumberCOUNT; i++)
        {
            PlaceNewNumber();
        }

        score = 0;
        AddScore(0);//表示更新
        gameOver = false;
        gameOverWin.SetActive(false);
    }
    #endregion

    //指定方向に動かせるかチェック
    bool CanMove(Vector2Int dire)
    {
        int targetX, targetY;
        for (int xI = 0; xI < GRIDsize; xI++)
        {
            for (int yI = 0; yI < GRIDsize; yI++)
            {
                targetX = xI + dire.x;
                targetY = yI + dire.y;
                if ((targetX < 0 || GRIDsize <= targetX)
                    || (targetY < 0 || GRIDsize <= targetY)) continue;

                if (numbers[targetX, targetY] == 0
                    || numbers[targetX, targetY] == numbers[xI, yI]) return true;
            }
        }
        return false;
    }

    void PlaceNewNumber()
    {
        List<int> placeableIndex = new List<int>();
        for (int xI = 0; xI < GRIDsize; xI++)
        {
            for (int yI = 0; yI < GRIDsize; yI++)
            {
                if (numbers[xI, yI] == 0)
                {
                    placeableIndex.Add(xI + GRIDsize * yI);
                }
            }
        }

        int placeIndex = Random.Range(0, placeableIndex.Count);
        int val;
        if (Random.value < fourGenerateRate)
        {
            val = 4;
        }
        else
        {
            val = 2;
        }
        int x = placeableIndex[placeIndex] % GRIDsize;
        int y = placeableIndex[placeIndex] / GRIDsize;
        numbers[x, y] = val;
        UpdateNumberText(x, y);
    }

    void AddScore(int newVal)
    {
        score += newVal;
        scoreText.text = score.ToString();
    }

    void KeepNumbers()
    {
        System.Array.Copy(numbers, subNumbers, numbers.Length);
    }

    public void UndoAction()
    {
        System.Array.Copy(subNumbers, numbers, numbers.Length);

        for (int xI = 0; xI < GRIDsize; xI++)
        {
            for (int yI = 0; yI < GRIDsize; yI++)
            {
                UpdateNumberText(xI, yI);
            }
        }
    }

    public void NewGame()
    {
        NumberSetup();
        PlaySetup();

        for (int xI = 0; xI < GRIDsize; xI++)
        {
            for (int yI = 0; yI < GRIDsize; yI++)
            {
                UpdateNumberText(xI, yI);
            }
        }
    }
}

enum Side
{
    Up, Right, Down, Left
}