using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour{

    [Header("Board Settings")]
    public int width;
    public int height;
    public float animalMoveTime;
    public float animalFallTime;
    public enum BoardState { Locked, Free }
    public BoardState boardState = BoardState.Free;

    [Header("Board Game references")]
    [SerializeField] private Animal[] animals;
    [SerializeField] private Animal bomb;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private CameraSetup cameraSetup;
    public MatchFinder matchFinder;

    public Animal[,] animalsGrid;
    public float bombChance = .02f;
    public float bonusMulti;
    public float bonusValue = .5f;

    [SerializeField] private ScoreManager scoreManager;

    private void Start() {
        SetupBoard();
        cameraSetup.AdjustCameraToBoard(this.gameObject);
        SetUpAnimals();
    }

    #region Setup Board
    private void SetupBoard() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector2 coords = new Vector2(x, y);
                GameObject tile = Instantiate(tilePrefab, coords, Quaternion.identity, transform);
                tile.name = "Tile (" + x + "," + y+")";
            }
        }
    }

    private void SetUpAnimals() {
        animalsGrid = new Animal[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector2 coords = new Vector2(x, y);
                GameObject tile = Instantiate(tilePrefab, coords, Quaternion.identity, transform);
                tile.name = "Tile (" + x + "," + y + ")";
                Animal randomAnimal = animals[Random.Range(0, animals.Length)];
                while (DoesStartingMatchExist(randomAnimal, coords)) {
                    randomAnimal = animals[Random.Range(0, animals.Length)];
                }
                SpawnAnimal(randomAnimal, coords);
            }
        }
    }



    private void SpawnAnimal(Animal animal,Vector2 coords) {
        if(Random.Range(0f, 1f) <= bombChance) {
            animal = bomb;
        }

        Animal newAnimal = Instantiate(animal, coords+new Vector2(0f,height*2), Quaternion.identity, transform);
        newAnimal.name = "Animal (" + coords.x + "," + coords.y + ")";
        animalsGrid[(int)coords.x, (int)coords.y] = newAnimal;
        newAnimal.SetupAnimal(coords,this);
        newAnimal.AnimateAnimalToGridIndex();
    }

    private bool DoesStartingMatchExist(Animal randomAnimal, Vector2 coords) {
        bool results = false;
        if(coords.x > 1) {
            if(animalsGrid[(int)coords.x-1,(int)coords.y].animalType == randomAnimal.animalType && animalsGrid[(int)coords.x-2, (int)coords.y].animalType == randomAnimal.animalType) {
                results = true;
            }
        }
        if (coords.y > 1) {
            if (animalsGrid[(int)coords.x, (int)coords.y-1].animalType == randomAnimal.animalType && animalsGrid[(int)coords.x, (int)coords.y-2].animalType == randomAnimal.animalType) {
                results = true;
            }
        }
        return results;
    }

    #endregion

    #region Lock Board

    public void LockBoard() {
        boardState = BoardState.Locked;
    }

    public void UnlockBoard() {
        boardState = BoardState.Free;
    }

    public bool IsBoardLocked() {
        return boardState == BoardState.Locked;
    }

    #endregion

    #region Destroy Matches

    public void DestroyAllMatches() {
        bonusMulti++;
        if (matchFinder.currentMatches.Count == 0) { bonusMulti = 0f; UnlockBoard(); return; }
        for (int i = 0; i < matchFinder.currentMatches.Count; i++) {
            DestroyMatchAtIndex(matchFinder.currentMatches[i].gridIndex);
        }
        StartCoroutine(DropAnimals());
    }
    private void DestroyMatchAtIndex(Vector2 gridIndex) {
        if (animalsGrid[(int)gridIndex.x, (int)gridIndex.y] != null) {
            if (animalsGrid[(int)gridIndex.x, (int)gridIndex.y].isMatched) {
                Instantiate(animalsGrid[(int)gridIndex.x, (int)gridIndex.y].burstEffect,new Vector2(gridIndex.x, gridIndex.y), Quaternion.identity);
                scoreManager.AddToScore(animalsGrid[(int)gridIndex.x, (int)gridIndex.y], bonusMulti*bonusValue);
                Destroy(animalsGrid[(int)gridIndex.x, (int)gridIndex.y].gameObject);
                animalsGrid[(int)gridIndex.x, (int)gridIndex.y] = null;

            }
        }
    }
    #endregion

    #region Fill Board

    IEnumerator DropAnimals() {
        yield return new WaitForSeconds(animalFallTime);
        int nullCounter = 0;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (animalsGrid[x, y] == null) {
                    nullCounter++;
                }
                else if(nullCounter > 0) {
                    animalsGrid[x, y].gridIndex.y -=nullCounter;
                    animalsGrid[x, y-nullCounter] = animalsGrid[x, y];
                    animalsGrid[x, y] = null;
                    animalsGrid[x, y - nullCounter].AnimateAnimalToGridIndex();
                }
            }
            nullCounter = 0;
        }
        StartCoroutine(FillBoard());
    }

    IEnumerator FillBoard() {
        yield return new WaitForSeconds(animalFallTime);


        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (animalsGrid[x, y] == null) {
                    Animal randomAnimal = animals[Random.Range(0, animals.Length)];
                    SpawnAnimal(randomAnimal, new Vector2(x, y));
                }
            }
        }

        yield return new WaitForSeconds(animalFallTime);

        matchFinder.FindAllMatches();
        DestroyAllMatches();
    }

    #endregion

    #region Shuffle Board

    public void ShuffleBoard() {
        if (!IsBoardLocked()) {
            LockBoard();
            List<Animal> animalsOnBoard = new List<Animal>();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    animalsOnBoard.Add(animalsGrid[x, y]);
                    animalsGrid[x, y] = null;
                }
            }
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Animal randomAnimal = animalsOnBoard[Random.Range(0, animalsOnBoard.Count)];

                    int iterationCount = 0;
                    while (DoesStartingMatchExist(randomAnimal, new Vector2(x,y)) && iterationCount < 100 && animalsOnBoard.Count > 1) {
                        randomAnimal = animalsOnBoard[Random.Range(0, animalsOnBoard.Count)];
                        iterationCount++;
                    }

                    randomAnimal.SetupAnimal(new Vector2(x, y), this);
                    animalsGrid[x, y] = randomAnimal;
                    animalsOnBoard.Remove(randomAnimal);
                    randomAnimal.AnimateAnimalToGridIndex();
                }
            }
            matchFinder.FindAllMatches();
            DestroyAllMatches();
        }
    }
    #endregion

}
