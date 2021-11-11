using System;
using UnityEngine;

public class VoxelTile : MonoBehaviour
{
    public float VoxelSize = 0.1f;
    public int TileSideVoxels = 10;

    [Range(1, 100)]
    public int Weight = 50;

    public RotationType Rotation;

    public enum RotationType
    {
        OnlyRotation,
        TwoRotations,
        FourRotations
    }

    [HideInInspector] public int[] ColorsRight;
    [HideInInspector] public int[] ColorsForward;
    [HideInInspector] public int[] ColorsLeft;
    [HideInInspector] public int[] ColorsBack;
    [HideInInspector] public int[] ColorsUp;
    [HideInInspector] public int[] ColorsDown;

    public void CalculateSidesColors()
    {
        ColorsRight = new int[TileSideVoxels * TileSideVoxels];
        ColorsForward = new int[TileSideVoxels * TileSideVoxels];
        ColorsLeft = new int[TileSideVoxels * TileSideVoxels];
        ColorsBack = new int[TileSideVoxels * TileSideVoxels];
        ColorsUp = new int[TileSideVoxels * TileSideVoxels];
        ColorsDown = new int[TileSideVoxels * TileSideVoxels];

        for (int y = 0; y < TileSideVoxels; y++)
        {
            for (int i = 0; i < TileSideVoxels; i++)
            {
                ColorsRight[y * TileSideVoxels + i] = GetVoxelColor(y, i, Direction.Right);
                ColorsForward[y * TileSideVoxels + i] = GetVoxelColor(y, i, Direction.Forward);
                ColorsLeft[y * TileSideVoxels + i] = GetVoxelColor(y, i, Direction.Left);
                ColorsBack[y * TileSideVoxels + i] = GetVoxelColor(y, i, Direction.Back);
                ColorsUp[y * TileSideVoxels + i] = GetVoxelColor(y, i, Direction.Up);
                ColorsDown[y * TileSideVoxels + i] = GetVoxelColor(y, i, Direction.Down);
            }
        }
    }

    public void Rotate90()
    {
        transform.Rotate(0, 90, 0);

        int[] colorsRightNew = new int[TileSideVoxels * TileSideVoxels];
        int[] colorsForwardNew = new int[TileSideVoxels * TileSideVoxels];
        int[] colorsLeftNew = new int[TileSideVoxels * TileSideVoxels];
        int[] colorsBackNew = new int[TileSideVoxels * TileSideVoxels];
        int[] colorsUpNew = new int[TileSideVoxels * TileSideVoxels];
        int[] colorsDownNew = new int[TileSideVoxels * TileSideVoxels];


        for (int layer = 0; layer < TileSideVoxels; layer++)
        {
            for (int offset = 0; offset < TileSideVoxels; offset++)
            {
                colorsRightNew[layer * TileSideVoxels + offset] = ColorsForward[layer * TileSideVoxels + TileSideVoxels - offset - 1];
                colorsForwardNew[layer * TileSideVoxels + offset] = ColorsLeft[layer * TileSideVoxels + offset];
                colorsLeftNew[layer * TileSideVoxels + offset] = ColorsBack[layer * TileSideVoxels + TileSideVoxels - offset - 1];
                colorsBackNew[layer * TileSideVoxels + offset] = ColorsRight[layer * TileSideVoxels + offset];
                colorsUpNew[layer * TileSideVoxels + offset] = ColorsUp[offset * TileSideVoxels + (TileSideVoxels - layer - 1)];
                colorsDownNew[layer * TileSideVoxels + offset] = ColorsDown[offset * TileSideVoxels + (TileSideVoxels - layer - 1)];
            }
        }

        ColorsRight = colorsRightNew;
        ColorsForward = colorsForwardNew;
        ColorsLeft = colorsLeftNew;
        ColorsBack = colorsBackNew;
        ColorsDown = colorsDownNew;
        ColorsUp = colorsUpNew;

        //Debug.Log(string.Join(",", ColorsRight));
    }

    private int GetVoxelColor(int verticalLayer, int horizontalOffset, Direction direction)
    {
        var meshCollider = GetComponentInChildren<MeshCollider>();

        float vox = VoxelSize;
        float half = VoxelSize / 2;

        Vector3 rayStart;
        Vector3 rayDir;
        if (direction == Direction.Right)
        {
            rayStart = meshCollider.bounds.min +
                       new Vector3(-half, 0, half + horizontalOffset * vox);
            rayDir = Vector3.right;
        }
        else if (direction == Direction.Forward)
        {
            rayStart = meshCollider.bounds.min +
                       new Vector3(half + horizontalOffset * vox, 0, -half);
            rayDir = Vector3.forward;
        }
        else if (direction == Direction.Left)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(half, 0, -half - (TileSideVoxels - horizontalOffset - 1) * vox);
            rayDir = Vector3.left;
        }
        else if (direction == Direction.Back)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(-half - (TileSideVoxels - horizontalOffset - 1) * vox, 0, half);
            rayDir = Vector3.back;
        }
        else if (direction == Direction.Down)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(-half - horizontalOffset * vox, half, -half - verticalLayer * vox);
            rayDir = Vector3.down;
        }
        else if (direction == Direction.Up)
        {
            rayStart = meshCollider.bounds.min +
                       new Vector3(half + horizontalOffset * vox, -half, half + verticalLayer * vox);
            rayDir = Vector3.up;
        }
        else
        {
            throw new ArgumentException("Wrong direction value, should be Direction.left/right/back/forward/up/down",
                nameof(direction));
        }

        if (direction == Direction.Up) rayStart.y = meshCollider.bounds.min.y - half;
        else if (direction == Direction.Down) rayStart.y = meshCollider.bounds.max.y + half;
        else rayStart.y = meshCollider.bounds.min.y + half + verticalLayer * vox;

        //Debug.DrawRay(rayStart, rayDir * .1f, Color.blue, 2);

        if (Physics.Raycast(new Ray(rayStart, rayDir), out RaycastHit hit, vox))
        {
            int colorIndex = (int)(hit.textureCoord.x * 512);

            if (colorIndex == 0) Debug.LogWarning("Found color 0 in mesh palette, this can cause conflicts");

            return colorIndex;
        }

        return 0;
    }
}