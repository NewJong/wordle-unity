using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class Board : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[] {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z,
    };
    [Header("States")]
    public Tile.State emptyState;
    public Tile.State occupiedState;
    public Tile.State correctState;
    public Tile.State wrongState;
    public Tile.State incorrectState;

    [Header("UI")]
    public GameObject invalidWordText;
    public GameObject newWordButton;
    public GameObject tryAgainButton;
    public GameObject showRandomLetterButton;

    private Row[] rows;
    private int rowIndex;
    private int columnIndex;

    private string[] solutions;
    private string[] validWords;

    private string word;
    private List<int> openedPositions = new List<int>();
    
    public AudioSource audioPromptSource;
    public AudioSource audioBoardSource1;
    public AudioSource audioBoardSource2;
    public AudioClip clickPromptSound;
    public AudioClip winSound;
    public AudioClip failSound;
    public AudioClip clickButtonSound;
    private bool isGameWon;

    private void Awake() {
        rows = GetComponentsInChildren<Row>();
    }

    public void ShowRandomLetter()
    {
        List<int> unopenedIndices = new List<int>();
        List<int> alreadyGuessedIndices = new List<int>();

        for (int i = 0; i < word.Length; i++) {
            if (!openedPositions.Contains(i) && !IsAlreadyGuessed(i)) {
                unopenedIndices.Add(i);
            } else if (!openedPositions.Contains(i)) {
                alreadyGuessedIndices.Add(i);
            }
        }

        List<int> targetIndices = unopenedIndices.Count > 0 ? unopenedIndices : alreadyGuessedIndices;
        if (targetIndices.Count > 0) {
            int randomIndex = Random.Range(0, targetIndices.Count);
            int index = targetIndices[randomIndex];

            Row currentRow = rows[rowIndex];
            currentRow.Tiles[index].SetLetter(word[index]);
            currentRow.Tiles[index].SetState(correctState);

            openedPositions.Add(index);
        }
    }
    private bool IsAlreadyGuessed(int index) {
        for (int i = 0; i < rowIndex; i++) {
            Row row = rows[i];
            if (row.Tiles[index].Letter == word[index]) {
                return true;
            }
        }
        return false;
    }
    private void Start() { 
        LoadData();
        NewGame();
    }
    public void NewGame()
    {
        ClearBoard();
        SetRandomWord();
        showRandomLetterButton.SetActive(true);
        enabled = true;
        tryAgainButton.SetActive(false);
        newWordButton.SetActive(false);
        openedPositions.Clear();
        audioBoardSource1.Stop();
        audioBoardSource2.Stop();
        isGameWon = false;
    }

    public void TryAgain()
    {
        ClearBoard();
        showRandomLetterButton.SetActive(true);
        enabled = true;
        tryAgainButton.SetActive(false);
        newWordButton.SetActive(false);
        openedPositions.Clear();
        audioBoardSource1.Stop();
        audioBoardSource2.Stop();
        isGameWon = false;
    }

    private void LoadData() {
        TextAsset textFile = Resources.Load("wordle_all") as TextAsset;
        validWords = textFile.text.Split('\n');
        textFile = Resources.Load("wordle_common") as TextAsset;
        solutions = textFile.text.Split('\n');
    }
    private void SetRandomWord () {
        word = solutions[Random.Range(0, solutions.Length)];
        word = word.ToLower().Trim();
    }

    private void Update() {
        Row currentRow = rows[rowIndex];

        if (Input.GetKeyDown(KeyCode.Backspace)) {
            columnIndex = Mathf.Max(columnIndex - 1, 0);

            if (currentRow.Tiles[columnIndex].Letter != '\0') {
                currentRow.Tiles[columnIndex].SetLetter('\0');
                currentRow.Tiles[columnIndex].SetState(emptyState);
            }
            invalidWordText.SetActive(false);
        }
        else if (columnIndex >= currentRow.Tiles.Length) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                SubmitRow(currentRow);
            }
        }
        else {
        for (int i = 0; i < SUPPORTED_KEYS.Length; i++) {
            if (Input.GetKeyDown(SUPPORTED_KEYS[i])) {
                while (columnIndex < currentRow.Tiles.Length && currentRow.Tiles[columnIndex].Letter != '\0') {
                    columnIndex++;
                }
                if (columnIndex < currentRow.Tiles.Length) {
                    currentRow.Tiles[columnIndex].SetLetter((char)SUPPORTED_KEYS[i]);
                    currentRow.Tiles[columnIndex].SetState(occupiedState);
                    columnIndex++;
                }
                break;
            }
        }
    }
    if (!tryAgainButton.activeSelf && !newWordButton.activeSelf) {
            if (HasWon(currentRow)) {
                tryAgainButton.SetActive(true);
                newWordButton.SetActive(true);
                audioBoardSource1.PlayOneShot(winSound);
            }
           
        }
    }
    private void SubmitRow(Row row) {
        if (!IsValidWord(row.Word)) {
            invalidWordText.SetActive(true);
            return;
        }

        string remaining = word;

        for (int i = 0; i < row.Tiles.Length; i++) {
            Tile tile = row.Tiles[i];

            if (tile.Letter == word[i]) {
                tile.SetState(correctState);
                remaining = remaining.Remove(i, 1);
                remaining = remaining.Insert(i, " ");
            }
            else if (!word.Contains(tile.Letter)) {
                tile.SetState(incorrectState);
            }
        }

        for (int i = 0; i < row.Tiles.Length; i++) {
            Tile tile = row.Tiles[i];
            if (tile.StateTile != correctState && tile.StateTile != incorrectState) {
                if (remaining.Contains(tile.Letter)) {
                    tile.SetState(wrongState);
                    int index = remaining.IndexOf(tile.Letter);
                    remaining = remaining.Remove(index, 1);
                    remaining = remaining.Insert(index, " ");
                }
                else {
                    tile.SetState(incorrectState);
                }
            }
        }
        if (rowIndex == rows.Length - 1 && !HasWon(row) || HasWon(row)) {
            showRandomLetterButton.SetActive(false);
            if (HasWon(row)) {
                audioBoardSource1.PlayOneShot(winSound);
            }
            else {
                audioBoardSource2.PlayOneShot(failSound);
            }
            enabled = false;
        }
        rowIndex++;
        columnIndex = 0;
        if (rowIndex >= rows.Length) {
            enabled = false;
        }
    }
    private bool IsValidWord(string word) {
        for (int i = 0; i < validWords.Length; i++) {
            if (validWords[i] == word) {
                return true;
            }
        }
        return false;
    }

    private bool HasWon(Row row) { 
        for (int i = 0; i < row.Tiles.Length; i++) {
            if (row.Tiles[i].StateTile != correctState) {
                return false;
            }
        }
        isGameWon = true;
        return true;
    }

    private void ClearBoard() {
        for (int row = 0; row < rows.Length; row++) {
            for (int col = 0; col < rows[row].Tiles.Length; col++) {
                rows[row].Tiles[col].SetLetter('\0');
                rows[row].Tiles[col].SetState(emptyState);
            }
        }
        rowIndex = 0;
        columnIndex = 0;
    }

    private void OnEnable() {
        tryAgainButton.SetActive(false);
        newWordButton.SetActive(false);
    }
    private void OnDisable() {
        tryAgainButton.SetActive(true);
        newWordButton.SetActive(true);
    }

    public void PlaySound() {
        if (!isGameWon) { 
            audioPromptSource.PlayOneShot(clickPromptSound);
        }
    }
    public void PlayClickSound() {
        AudioSource.PlayClipAtPoint(clickButtonSound, Camera.main.transform.position);
    }
}