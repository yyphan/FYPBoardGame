using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Inspector;
using System;

namespace BoardGame
{
    public class TileManager : MonoBehaviour
    {
        [ReadOnly] public int rowCount = 7;
        [ReadOnly] public int columnCount = 7;

        public TileBehaviour[] tileList = new TileBehaviour[49];

        private void Awake()
        {
            SetUpTiles();
        }

        private void SetUpTiles()
        {
            for (int index = 0; index < rowCount*columnCount; index++)
            {
                int xCoordinate = index % columnCount;
                int yCoordinate = index / columnCount;
                tileList[index].xCoordinate = xCoordinate;
                tileList[index].yCoordinate = yCoordinate;
            }
        }

        public void ResetAllTiles()
        {
            foreach(TileBehaviour tile in tileList)
            {
                tile.ResetTile();
            }
        }

        public void UpdateActiveEvents(GameTurn nextTurn)
        {
            foreach(TileBehaviour tile in tileList)
            {
                TileActiveEventsBehaviour activeEventsBehaviour = tile.GetComponent<TileActiveEventsBehaviour>();
                activeEventsBehaviour.UpdateActiveEvents(nextTurn);
            }
        }

        public TileBehaviour GetTile(int xCoordinate, int yCoordinate)
        {
            int index = GetIndex(xCoordinate, yCoordinate);

            return tileList[index];
        }

        public TileBehaviour GetCenterTile()
        {
            int index = columnCount * rowCount / 2;

            return tileList[index];
        }

        private int GetIndex(int xCoordinate, int yCoordinate)
        {
            return yCoordinate * columnCount + xCoordinate;
        }

        public void RemoveStatusForAll(TileStatus tileStatus)
        {
            for (int i = 0; i < tileList.Length; i++)
            {
                tileList[i].RemoveStatus(tileStatus);
            }
        }

        public bool IsValid(int xCoordinate, int yCoordinate)
        {
            return ((xCoordinate >= 0) && (xCoordinate < columnCount) && (yCoordinate >= 0) && (yCoordinate < rowCount));
        }

        // using diagonal distance, 1 in all directions
        public int GetIntegerDistance(int originX, int originY, int targetX, int targetY)
        {
            int dx = Math.Abs(targetX - originX);
            int dy = Math.Abs(targetY - originY);
            int distance = Math.Max(dx, dy);
            return distance;
        }

        public int GetIntegerDistance(ChampionController origin, ChampionController target)
        {
            int originX = origin.xCoordinate;
            int originY = origin.yCoordinate;
            int targetX = target.xCoordinate;
            int targetY = target.yCoordinate;

            return GetIntegerDistance(originX, originY, targetX, targetY);
        }

        public int GetIntegerDistance(ChampionController origin, TileBehaviour tile)
        {
            int originX = origin.xCoordinate;
            int originY = origin.yCoordinate;
            int targetX = tile.xCoordinate;
            int targetY = tile.yCoordinate;

            return GetIntegerDistance(originX, originY, targetX, targetY);
        }

        public int GetIntegerDistance(TileBehaviour fromTile, TileBehaviour toTile)
        {
            int originX = fromTile.xCoordinate;
            int originY = fromTile.yCoordinate;
            int targetX = toTile.xCoordinate;
            int targetY = toTile.yCoordinate;

            return GetIntegerDistance(originX, originY, targetX, targetY);
        }

        public void AddStatusForRange(int xCoordinate, int yCoordinate, int range, TileStatus tileStatus)
        {
            int lowerX = xCoordinate - range;
            if (lowerX < 0) lowerX = 0;

            int upperX = xCoordinate + range;
            if (upperX > columnCount - 1) upperX = columnCount - 1;

            int lowerY = yCoordinate - range;
            if (lowerY < 0) lowerY = 0;

            int upperY = yCoordinate + range;
            if (upperY > rowCount - 1) upperY = rowCount - 1;

            for (int i = lowerX; i <= upperX; i++)
            {
                for (int j = lowerY; j <= upperY; j++)
                {
                    if (GetIntegerDistance(xCoordinate, yCoordinate, i, j) <= range)
                    {
                        GetTile(i, j).AddStatus(tileStatus);
                    }
                }
            }
        }

        public void AddStatusForRange(TileBehaviour tile, int range, TileStatus tileStatus)
        {
            int xCoordinate = tile.xCoordinate;
            int yCoordinate = tile.yCoordinate;

            AddStatusForRange(xCoordinate, yCoordinate, range, tileStatus);
        }

        public void AddStatusForArea(TileBehaviour centerTile, int width, int length, TileStatus tileStatus)
        {
            List<TileBehaviour> tilesInArea = GetTilesInArea(centerTile, width, length); 
            foreach (TileBehaviour tile in tilesInArea)
            {
                tile.AddStatus(tileStatus);
            }
        }

        public List<TileBehaviour> GetTilesInArea(TileBehaviour tile, int width, int length)
        {
            List<TileBehaviour> tilesInArea = new List<TileBehaviour>();
            int xCoordinate = tile.xCoordinate;
            int yCoordinate = tile.yCoordinate;

            // calculating offsets
            int left = width / 2;
            int right = left;
            if (width % 2 == 0) right--;

            int up = length / 2;
            int down = up;
            if (length % 2 == 0) down--;

            // calculating the area on board
            int lowerX = xCoordinate - left;
            if (lowerX < 0) lowerX = 0;

            int upperX = xCoordinate + right;
            if (upperX > rowCount - 1) upperX = rowCount - 1;

            int lowerY = yCoordinate - down;
            if (lowerY < 0) lowerY = 0;

            int upperY = yCoordinate + up;
            if (upperY > rowCount - 1) upperY = columnCount - 1;

            for (int j = lowerY; j <= upperY; j++)
            {
                for (int i = lowerX; i <= upperX; i++)
                {
                    tilesInArea.Add(GetTile(i, j));
                }
            }

            return tilesInArea;
        }

        public void AddStatusForTile(TileBehaviour tile, TileStatus tileStatus)
        {
            RemoveStatusForAll(tileStatus);
            tile.AddStatus(tileStatus);
        }
    }
}
