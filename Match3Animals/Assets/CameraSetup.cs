using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetup : MonoBehaviour{

    public Vector3 screenSpacePadding;
    private Bounds targetBounds;

    public void AdjustCameraToBoard(GameObject board) {
        targetBounds = GetBoundingBox(board);
        targetBounds.Expand(screenSpacePadding);
        Camera.main.orthographicSize = (targetBounds.size.x / Camera.main.aspect) / 2;
        Camera.main.transform.position = new Vector3(targetBounds.center.x, targetBounds.center.y, -1f);
    }
    private Bounds GetBoundingBox(GameObject objeto) {
        Bounds bounds;
        Renderer childRender;
        bounds = GetRenderBounds(objeto);
        if (bounds.extents.x == 0) {
            bounds = new Bounds(objeto.transform.position, Vector3.zero);
            foreach (Transform child in objeto.transform) {
                childRender = child.GetComponent<Renderer>();
                if (childRender) {
                    bounds.Encapsulate(childRender.bounds);
                }
                else {
                    bounds.Encapsulate(GetBoundingBox(child.gameObject));
                }
            }
        }
        return bounds;
    }

    private Bounds GetRenderBounds(GameObject objeto) {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        Renderer render = objeto.GetComponent<Renderer>();
        if (render != null) {
            return render.bounds;
        }
        return bounds;
    }

}
