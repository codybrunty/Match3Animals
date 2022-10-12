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

    [Header("Board Game Objects")]
    [SerializeField] private Animal[] animals;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private CameraSetup cameraSetup;
    public MatchFinder matchFinder;

    public Animal[,] animalsGrid;

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
        Animal newAnimal = Instantiate(animal, coords+new Vector2(0f,height*2), Quaternion.identity, transform);
        newAnimal.name = "Animal (" + coords.x + "," + coords.y + ")";
        animalsGrid[(int)coords.x, (int)coords.y] = newAnimal;
        newAnimal.SetupAnimal(coords,this);
        newAnimal.AnimateAnimalFalling();
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

    public bool IsLocked() {
        return boardState == BoardState.Locked;
    }

    #endregion

    #region Destroy Matches

    public void DestroyAllMatches() {
        if (matchFinder.currentMatches.Count == 0) { UnlockBoard(); return; }
        for (int i = 0; i < matchFinder.currentMatches.Count; i++) {
            DestroyMatchAtIndex(matchFinder.currentMatches[i].gridIndex);
        }
        StartCoroutine(DropAnimals());
    }
    private void DestroyMatchAtIndex(Vector2 gridIndex) {
        if (animalsGrid[(int)gridIndex.x, (int)gridIndex.y] != null) {
            if (animalsGrid[(int)gridIndex.x, (int)gridIndex.y].isMatched) {
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
                    animalsGrid[x, y - nullCounter].AnimateAnimalFalling();
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

}
