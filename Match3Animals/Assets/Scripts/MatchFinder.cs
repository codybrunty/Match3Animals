using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchFinder : MonoBehaviour{

    private Board board;
    public List<Animal> currentMatches = new List<Animal>();

    private void Awake() {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches() {
        currentMatches.Clear();
        for (int x = 0; x < board.width; x++) {
            for (int y = 0; y < board.height; y++) {
                Animal currentAnimal = board.animalsGrid[x, y];
                if(currentAnimal == null) { continue; }
                if(x>0 && x<board.width-1) {
                    Animal leftAnimal = board.animalsGrid[x-1,y];
                    Animal rightAnimal = board.animalsGrid[x + 1, y];
                    if(leftAnimal != null && rightAnimal != null) {
                        if(leftAnimal.animalType == currentAnimal.animalType && rightAnimal.animalType == currentAnimal.animalType) {
                            currentAnimal.Matched();
                            leftAnimal.Matched();
                            rightAnimal.Matched();
                            if (!currentMatches.Contains(currentAnimal)) { currentMatches.Add(currentAnimal); }
                            if (!currentMatches.Contains(leftAnimal)) { currentMatches.Add(leftAnimal); }
                            if (!currentMatches.Contains(rightAnimal)) { currentMatches.Add(rightAnimal); }
                        }
                    }
                }
                if (y > 0 && y < board.height - 1) {
                    Animal downAnimal = board.animalsGrid[x, y-1];
                    Animal upAnimal = board.animalsGrid[x, y+1];
                    if (downAnimal != null && upAnimal != null) {
                        if (downAnimal.animalType == currentAnimal.animalType && upAnimal.animalType == currentAnimal.animalType) {
                            currentAnimal.Matched();
                            downAnimal.Matched();
                            upAnimal.Matched();
                            if (!currentMatches.Contains(currentAnimal)) { currentMatches.Add(currentAnimal); }
                            if (!currentMatches.Contains(downAnimal)) { currentMatches.Add(downAnimal); }
                            if (!currentMatches.Contains(upAnimal)) { currentMatches.Add(upAnimal); }
                        }
                    }
                }
            }
        }
        CheckForBombs();
    }

    public void CheckForBombs() {
        for (int i = 0; i < currentMatches.Count; i++) {
            Animal match = currentMatches[i];
            int x = (int)match.gridIndex.x;
            int y = (int)match.gridIndex.y;

            if (x > 0) {
                if (board.animalsGrid[x - 1, y] != null) {
                    if (board.animalsGrid[x - 1, y].animalType == Animal.AnimalType.Bomb) {
                        MarkBombArea(new Vector2(x - 1, y), board.animalsGrid[x - 1, y]);
                    }
                }
            }

            if (x < board.width - 1) {
                if (board.animalsGrid[x + 1, y] != null) {
                    if (board.animalsGrid[x + 1, y].animalType == Animal.AnimalType.Bomb) {
                        MarkBombArea(new Vector2(x + 1, y), board.animalsGrid[x + 1, y]);
                    }
                }
            }


            if (y > 0) {
                if (board.animalsGrid[x, y - 1] != null) {
                    if (board.animalsGrid[x, y - 1].animalType == Animal.AnimalType.Bomb) {
                        MarkBombArea(new Vector2(x, y - 1), board.animalsGrid[x, y - 1]);
                    }
                }
            }

            if (y < board.height - 1) {
                if (board.animalsGrid[x, y + 1] != null) {
                    if (board.animalsGrid[x, y + 1].animalType == Animal.AnimalType.Bomb) {
                        MarkBombArea(new Vector2(x, y + 1), board.animalsGrid[x, y + 1]);
                    }
                }
            }
        }
    }

    private void MarkBombArea(Vector2 gridIndex, Animal bomb) {
        for (int x = (int)gridIndex.x-bomb.blastRadius; x <= (int)gridIndex.x + bomb.blastRadius; x++) {
            for (int y = (int)gridIndex.y - bomb.blastRadius; y <= (int)gridIndex.y + bomb.blastRadius; y++) {
                if (x >= 0 && x < board.width && y >= 0 && y < board.height ) {
                    if (board.animalsGrid[x, y] != null) {
                        board.animalsGrid[x, y].isMatched = true;
                        if (!currentMatches.Contains(board.animalsGrid[x, y])) { currentMatches.Add(board.animalsGrid[x, y]); }
                    }
                }
            }
        }
    }

}
