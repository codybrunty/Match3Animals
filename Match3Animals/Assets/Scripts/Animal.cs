using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Animal : MonoBehaviour{

    public enum AnimalType { Narwhal, Snake, Parrot, Giraffe, Chicken }
    public AnimalType animalType;

    public Vector2 gridIndex;
    private Board board;

    private Vector2 startTouch;
    private Vector2 endTouch;

    private bool pressed = false;
    private float swipeAngle = 0f;

    private Animal swapAnimal;
    public bool isMatched = false;
    public Vector2 oldGridIndex;

    private void OnMouseDown() {
        if (!board.IsLocked()) {
            pressed = true;
            startTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp() {
        if (pressed) {
            endTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (IsSwipeBigEnough()) {
                CalculateSwipeAngle();
                MovePieces();
            }
            pressed = false;
        }
    }

    private void CalculateSwipeAngle() {
        swipeAngle = (180*(Mathf.Atan2(endTouch.y-startTouch.y, endTouch.x-startTouch.x)))/Mathf.PI;
    }

    private bool IsSwipeBigEnough() {
        if (Vector3.Distance(startTouch,endTouch)>.5f) {
            return true;
        }
        return false;
    }
    private void MovePieces() {
        if (swipeAngle >= -45 && swipeAngle <= 45 && gridIndex.x < board.width - 1) {
            MoveRight();
        }
        else if (swipeAngle > 45 && swipeAngle < 135 && gridIndex.y < board.height - 1) {
            MoveUp();
        }
        else if (swipeAngle > -135 && swipeAngle < -45 && gridIndex.y > 0) {
            MoveDown();
        }
        else if (swipeAngle <= -135 || swipeAngle >= 135 && gridIndex.x > 0) {
            MoveLeft();
        }
        else {
            return;
        }
        board.LockBoard();
        SwapAnimalsOnBoardGrid();
        AnimateAnimals();

    }

    private void AnimateAnimals(bool runCheckForNoMatches = true) {
        transform.DOLocalMove(gridIndex, 0.5f).SetEase(Ease.OutCubic);
        swapAnimal.transform.DOLocalMove(swapAnimal.gridIndex,board.animalMoveTime).SetEase(Ease.OutCubic).OnComplete(() => { 
            board.matchFinder.FindAllMatches(); 
            if (runCheckForNoMatches) { 
                CheckForNoMatches(); 
            } 
            else { 
                board.UnlockBoard();  
            } });
    }

    public void AnimateAnimalFalling() {
        transform.DOLocalMove(gridIndex, board.animalFallTime).SetEase(Ease.OutCubic);
    }

    private void CheckForNoMatches() {
        if (isMatched || swapAnimal.isMatched) {
            board.DestroyAllMatches();
        }
        else {
            if (swipeAngle >= -45 && swipeAngle <= 45 && gridIndex.x < board.width - 1) {
                MoveLeft();
            }
            else if (swipeAngle > 45 && swipeAngle < 135 && gridIndex.y < board.height - 1) {
                MoveDown();
            }
            else if (swipeAngle > -135 && swipeAngle < -45 && gridIndex.y > 0) {
                MoveUp();
            }
            else if (swipeAngle <= -135 || swipeAngle >= 135 && gridIndex.x > 0) {
                MoveRight();
            }
            SwapAnimalsOnBoardGrid();
            AnimateAnimals(false);
        }
    }

    private void SwapAnimalsOnBoardGrid() {
        board.animalsGrid[(int)gridIndex.x, (int)gridIndex.y] = this;
        board.animalsGrid[(int)swapAnimal.gridIndex.x, (int)swapAnimal.gridIndex.y] = swapAnimal;
    }

    private void MoveRight() {
        oldGridIndex = gridIndex;
        swapAnimal = board.animalsGrid[(int)gridIndex.x + 1, (int)gridIndex.y];
        swapAnimal.gridIndex.x--;
        gridIndex.x++;
    }

    private void MoveUp() {
        oldGridIndex = gridIndex;
        swapAnimal = board.animalsGrid[(int)gridIndex.x, (int)gridIndex.y+1];
        swapAnimal.gridIndex.y--;
        gridIndex.y++;
    }

    private void MoveDown() {
        oldGridIndex = gridIndex;
        swapAnimal = board.animalsGrid[(int)gridIndex.x, (int)gridIndex.y-1];
        swapAnimal.gridIndex.y++;
        gridIndex.y--;
    }
    private void MoveLeft() {
        oldGridIndex = gridIndex;
        swapAnimal = board.animalsGrid[(int)gridIndex.x - 1, (int)gridIndex.y];
        swapAnimal.gridIndex.x++;
        gridIndex.x--;
    }

    #region Setup Animal
    public void SetupAnimal(Vector2 gridIndex, Board board) {
        SetIndex(gridIndex);
        SetBoard(board);
        SetType();
    }

    private void SetIndex(Vector2 gridIndex) {
        this.gridIndex = gridIndex;
    }
    private void SetBoard(Board board) {
        this.board = board;
    }

    private void SetType() {
        //animalType = 
    }
    #endregion

    #region Matching
    public void Matched() {
        isMatched = true;
    }

    #endregion

}
