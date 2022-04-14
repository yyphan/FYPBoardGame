using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public class TilePathfinder : MonoBehaviour
    {
        public TileManager tileManager;
        private TileNode[,] tileMap;
        private int xMax;
        private int yMax;

        private void Awake()
        {
            xMax = tileManager.columnCount;
            yMax = tileManager.rowCount;
            tileMap = new TileNode[xMax, yMax];
        }

        public TileBehaviour GetNextTileInPath(TileBehaviour startTile, TileBehaviour targetTile)
        {
            TileBehaviour nextTile = null;
            // if path is found, get it
            if (AStarSearch(startTile, targetTile))
            {
                Stack<TileNode> path = FindPath(targetTile);
                // pop the next node
                TileNode nextNode = path.Pop();
                nextTile = tileManager.GetTile(nextNode.xCoord, nextNode.yCoord);
            }
            else // if no path is found, stay in position
            {
                nextTile = startTile;
            }

            return nextTile;
        }

        private Stack<TileNode> FindPath(TileBehaviour targetTile)
        {
            Stack<TileNode> path = new Stack<TileNode>();
            int x = targetTile.xCoordinate;
            int y = targetTile.yCoordinate;
            while (!(tileMap[x, y].parentX == x && tileMap[x, y].parentY == y)) // while not the start node
            {
                path.Push(tileMap[x, y]);
                int tempX = tileMap[x, y].parentX;
                int tempY = tileMap[x, y].parentY;
                x = tempX;
                y = tempY;
            }

            return path;
        }

        private bool isValid(int x, int y)
        {
            return tileManager.IsValid(x, y);
        }

        private bool isBlocked(int x, int y)
        {
            bool isBlocked = (tileManager.GetTile(x, y).GetObjectOnTile() != null);
            return isBlocked;
        }

        private bool isDestination(int x, int y, TileBehaviour targetTile)
        {
            if (x == targetTile.xCoordinate && y == targetTile.yCoordinate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AStarSearch(TileBehaviour startTile, TileBehaviour targetTile)
        {
            if (!isValid(targetTile.xCoordinate, targetTile.yCoordinate))
            {
                Debug.LogError("TilePathfinder: Target Tile out of bound: " + targetTile.Print());
                return false;
            }

            if (!isValid(startTile.xCoordinate, startTile.yCoordinate))
            {
                Debug.LogError("TilePathfinder: Start Tile out of bound: " + startTile.Print());
                return false;
            }

            if (isDestination(startTile.xCoordinate, startTile.yCoordinate, targetTile))
            {
                Debug.LogWarning("TilePathfinder: Start and Target Tile are the same: " + targetTile.Print());
                return false;
            }

            // initialize tile map
            for (int i = 0; i < xMax; i++)
            {
                for (int j = 0; j < yMax; j++)
                {
                    tileMap[i, j] = new TileNode(i, j);
                }
            }

            // starting node
            int x = startTile.xCoordinate, y = startTile.yCoordinate;
            tileMap[x, y].f = 0;
            tileMap[x, y].g = 0;
            tileMap[x, y].h = 0;
            tileMap[x, y].parentX = x;
            tileMap[x, y].parentY = y;

            // closedList: a boolean map, initialize to false
            bool[,] closedList = new bool[xMax, yMax];

            // openList: List of NodePair where f = g+h and x,y is the coordinate
            HashSet<TileNode> openList = new HashSet<TileNode>();
            openList.Add(tileMap[x, y]);

            bool foundTarget = false;

            while (openList.Count != 0)
            {
                // find the node with the least f on the open list, call it p
                TileNode p = tileMap[x,y];
                int lowestF = Int32.MaxValue;
                foreach (TileNode tileNode in openList)
                {
                    if (tileNode.f < lowestF)
                    {
                        lowestF = tileNode.f;
                        p = tileNode;
                    }
                }

                // pop p off the open list
                openList.Remove(p);

                // add p to the closed list
                x = p.xCoord;
                y = p.yCoord;
                closedList[x, y] = true;

                // f, g, h for the successors
                int fNew, gNew, hNew;

                //----------- 1st Successor (North) ------------
                if (isValid(x, y+1) == true) // if is within bound
                {
                    if (isDestination(x, y + 1, targetTile)) // if destination is found
                    {
                        tileMap[x, y + 1].parentX = x;
                        tileMap[x, y + 1].parentY = y;
                        foundTarget = true;
                        return true;
                    }
                    else if (closedList[x, y + 1] == false && isBlocked(x, y+1) == false) // if not examined (closed) nor blocked
                    {
                        gNew = tileMap[x, y].g + 1;
                        hNew = tileManager.GetIntegerDistance(x, y + 1, targetTile.xCoordinate, targetTile.yCoordinate);
                        fNew = gNew + hNew;

                        if (tileMap[x, y+1].f == Int32.MaxValue // if it is not in openList
                            || tileMap[x, y + 1].f > fNew) // or, if this route is better than previous route
                        {
                            // Update this tileNode
                            tileMap[x, y + 1].f = fNew;
                            tileMap[x, y + 1].g = gNew;
                            tileMap[x, y + 1].h = hNew;
                            tileMap[x, y + 1].parentX = x;
                            tileMap[x, y + 1].parentY = y;

                            openList.Add(tileMap[x, y + 1]); // Add it to the open list
                        }
                    }
                }

                //----------- 2nd Successor (South) ------------
                if (isValid(x, y - 1) == true) // if is within bound
                {
                    if (isDestination(x, y - 1, targetTile)) // if destination is found
                    {
                        tileMap[x, y - 1].parentX = x;
                        tileMap[x, y - 1].parentY = y;
                        foundTarget = true;
                        return true;
                    }
                    else if (closedList[x, y - 1] == false && isBlocked(x, y - 1) == false) // if not examined (closed) nor blocked
                    {
                        gNew = tileMap[x, y].g + 1;
                        hNew = tileManager.GetIntegerDistance(x, y - 1, targetTile.xCoordinate, targetTile.yCoordinate);
                        fNew = gNew + hNew;

                        if (tileMap[x, y - 1].f == Int32.MaxValue // if it is not in openList
                            || tileMap[x, y - 1].f > fNew) // or, if this route is better than previous route
                        {
                            // Update this tileNode
                            tileMap[x, y - 1].f = fNew;
                            tileMap[x, y - 1].g = gNew;
                            tileMap[x, y - 1].h = hNew;
                            tileMap[x, y - 1].parentX = x;
                            tileMap[x, y - 1].parentY = y;

                            openList.Add(tileMap[x, y - 1]); // Add it to the open list
                        }
                    }
                }

                //----------- 3rd Successor (East) ------------
                if (isValid(x + 1, y) == true) // if is within bound
                {
                    if (isDestination(x + 1, y, targetTile)) // if destination is found
                    {
                        tileMap[x + 1, y].parentX = x;
                        tileMap[x + 1, y].parentY = y;
                        foundTarget = true;
                        return true;
                    }
                    else if (closedList[x + 1, y] == false && isBlocked(x + 1, y) == false) // if not examined (closed) nor blocked
                    {
                        gNew = tileMap[x, y].g + 1;
                        hNew = tileManager.GetIntegerDistance(x + 1, y, targetTile.xCoordinate, targetTile.yCoordinate);
                        fNew = gNew + hNew;

                        if (tileMap[x + 1, y].f == Int32.MaxValue // if it is not in openList
                            || tileMap[x + 1, y].f > fNew) // or, if this route is better than previous route
                        {
                            // Update this tileNode
                            tileMap[x + 1, y].f = fNew;
                            tileMap[x + 1, y].g = gNew;
                            tileMap[x + 1, y].h = hNew;
                            tileMap[x + 1, y].parentX = x;
                            tileMap[x + 1, y].parentY = y;

                            openList.Add(tileMap[x + 1, y]); // Add it to the open list
                        }
                    }
                }

                //----------- 4th Successor (West) ------------
                if (isValid(x - 1, y) == true) // if is within bound
                {
                    if (isDestination(x - 1, y, targetTile)) // if destination is found
                    {
                        tileMap[x - 1, y].parentX = x;
                        tileMap[x - 1, y].parentY = y;
                        foundTarget = true;
                        return true;
                    }
                    else if (closedList[x - 1, y] == false && isBlocked(x - 1, y) == false) // if not examined (closed) nor blocked
                    {
                        gNew = tileMap[x, y].g + 1;
                        hNew = tileManager.GetIntegerDistance(x - 1, y, targetTile.xCoordinate, targetTile.yCoordinate);
                        fNew = gNew + hNew;

                        if (tileMap[x - 1, y].f == Int32.MaxValue // if it is not in openList
                            || tileMap[x - 1, y].f > fNew) // or, if this route is better than previous route
                        {
                            // Update this tileNode
                            tileMap[x - 1, y].f = fNew;
                            tileMap[x - 1, y].g = gNew;
                            tileMap[x - 1, y].h = hNew;
                            tileMap[x - 1, y].parentX = x;
                            tileMap[x - 1, y].parentY = y;

                            openList.Add(tileMap[x - 1, y]); // Add it to the open list
                        }
                    }
                }

                //----------- 5th Successor (NorthEast) ------------
                if (isValid(x + 1, y + 1) == true) // if is within bound
                {
                    if (isDestination(x + 1, y + 1, targetTile)) // if destination is found
                    {
                        tileMap[x + 1, y + 1].parentX = x;
                        tileMap[x + 1, y + 1].parentY = y;
                        foundTarget = true;
                        return true;
                    }
                    else if (closedList[x + 1, y + 1] == false && isBlocked(x + 1, y + 1) == false) // if not examined (closed) nor blocked
                    {
                        gNew = tileMap[x, y].g + 1;
                        hNew = tileManager.GetIntegerDistance(x + 1, y + 1, targetTile.xCoordinate, targetTile.yCoordinate);
                        fNew = gNew + hNew;

                        if (tileMap[x + 1, y + 1].f == Int32.MaxValue // if it is not in openList
                            || tileMap[x + 1, y + 1].f > fNew) // or, if this route is better than previous route
                        {
                            // Update this tileNode
                            tileMap[x + 1, y + 1].f = fNew;
                            tileMap[x + 1, y + 1].g = gNew;
                            tileMap[x + 1, y + 1].h = hNew;
                            tileMap[x + 1, y + 1].parentX = x;
                            tileMap[x + 1, y + 1].parentY = y;

                            openList.Add(tileMap[x + 1, y + 1]); // Add it to the open list
                        }
                    }
                }

                //----------- 6th Successor (SouthEast) ------------
                if (isValid(x + 1, y - 1) == true) // if is within bound
                {
                    if (isDestination(x + 1, y - 1, targetTile)) // if destination is found
                    {
                        tileMap[x + 1, y - 1].parentX = x;
                        tileMap[x + 1, y - 1].parentY = y;
                        foundTarget = true;
                        return true;
                    }
                    else if (closedList[x + 1, y - 1] == false && isBlocked(x + 1, y - 1) == false) // if not examined (closed) nor blocked
                    {
                        gNew = tileMap[x, y].g + 1;
                        hNew = tileManager.GetIntegerDistance(x + 1, y - 1, targetTile.xCoordinate, targetTile.yCoordinate);
                        fNew = gNew + hNew;

                        if (tileMap[x + 1, y - 1].f == Int32.MaxValue // if it is not in openList
                            || tileMap[x + 1, y - 1].f > fNew) // or, if this route is better than previous route
                        {
                            // Update this tileNode
                            tileMap[x + 1, y - 1].f = fNew;
                            tileMap[x + 1, y - 1].g = gNew;
                            tileMap[x + 1, y - 1].h = hNew;
                            tileMap[x + 1, y - 1].parentX = x;
                            tileMap[x + 1, y - 1].parentY = y;

                            openList.Add(tileMap[x + 1, y - 1]); // Add it to the open list
                        }
                    }
                }

                //----------- 7th Successor (NorthWest) ------------
                if (isValid(x - 1, y + 1) == true) // if is within bound
                {
                    if (isDestination(x - 1, y + 1, targetTile)) // if destination is found
                    {
                        tileMap[x - 1, y + 1].parentX = x;
                        tileMap[x - 1, y + 1].parentY = y;
                        foundTarget = true;
                        return true;
                    }
                    else if (closedList[x - 1, y + 1] == false && isBlocked(x - 1, y + 1) == false) // if not examined (closed) nor blocked
                    {
                        gNew = tileMap[x, y].g + 1;
                        hNew = tileManager.GetIntegerDistance(x - 1, y + 1, targetTile.xCoordinate, targetTile.yCoordinate);
                        fNew = gNew + hNew;

                        if (tileMap[x - 1, y + 1].f == Int32.MaxValue // if it is not in openList
                            || tileMap[x - 1, y + 1].f > fNew) // or, if this route is better than previous route
                        {
                            // Update this tileNode
                            tileMap[x - 1, y + 1].f = fNew;
                            tileMap[x - 1, y + 1].g = gNew;
                            tileMap[x - 1, y + 1].h = hNew;
                            tileMap[x - 1, y + 1].parentX = x;
                            tileMap[x - 1, y + 1].parentY = y;
                            
                            openList.Add(tileMap[x - 1, y + 1]); // Add it to the open list
                        }
                    }
                }

                //----------- 8th Successor (SouthWest) ------------
                if (isValid(x - 1, y - 1) == true) // if is within bound
                {
                    if (isDestination(x - 1, y - 1, targetTile)) // if destination is found
                    {
                        tileMap[x - 1, y - 1].parentX = x;
                        tileMap[x - 1, y - 1].parentY = y;
                        foundTarget = true;
                        return true;
                    }
                    else if (closedList[x - 1, y - 1] == false && isBlocked(x - 1, y - 1) == false) // if not examined (closed) nor blocked
                    {
                        gNew = tileMap[x, y].g + 1;
                        hNew = tileManager.GetIntegerDistance(x - 1, y - 1, targetTile.xCoordinate, targetTile.yCoordinate);
                        fNew = gNew + hNew;

                        if (tileMap[x - 1, y - 1].f == Int32.MaxValue // if it is not in openList
                            || tileMap[x - 1, y - 1].f > fNew) // or, if this route is better than previous route
                        {
                            // Update this tileNode
                            tileMap[x - 1, y - 1].f = fNew;
                            tileMap[x - 1, y - 1].g = gNew;
                            tileMap[x - 1, y - 1].h = hNew;
                            tileMap[x - 1, y - 1].parentX = x;
                            tileMap[x - 1, y - 1].parentY = y;

                            openList.Add(tileMap[x - 1, y - 1]); // Add it to the open list
                        }
                    }
                }
            }

            // open list is empty now
            // if not found, means there is no way to the target tile
            if (foundTarget == false) Debug.LogWarning("TilePathfinder: no possible path from " + startTile.Print() + " to " + targetTile.Print() +  ".");

            return false;
        }
    }

}

