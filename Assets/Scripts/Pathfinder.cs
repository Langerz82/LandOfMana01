using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder
{
    public static class Utils
    {
        public static bool Contains(List<Node> list, Node node)
        {
            return list.Any(x => x.X == node.X && x.Y == node.Y);
        }
        public static bool Contains(HashSet<Node> list, Node node)
        {
            return list.Any(x => x.X == node.X && x.Y == node.Y);
        }
    }

    public class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float G { get; set; } // Cost from start to this node
        public float H { get; set; } // Heuristic cost to the end node
        public float F => G + H; // Total cost
        public Node Parent { get; set; }

        public Node(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class AStar
    {
        public static readonly AStar GetInstance = new AStar();

        public List<Node> FindPath(Node start, Node goal, bool[,] grid)
        {
            // Make sure start and goal node lie within grid.
            if (start.X > grid.GetLength(0) || goal.X > grid.GetLength(0) ||
                start.Y > grid.GetLength(1) || goal.Y > grid.GetLength(1))
                return new List<Node>();

            // Make sure start and goal node have no collision.
            if (grid[start.X, start.Y] || grid[goal.X, goal.Y])
                return new List<Node>();

            var openSet = new List<Node> { start };
            var closedSet = new HashSet<Node>();

            while (openSet.Count > 0)
            {
                // Get the node with the lowest F value
                var current = openSet.OrderBy(node => node.F).First();

                if (current.X == goal.X && current.Y == goal.Y)
                    return ReconstructPath(current); // Return the path if goal reached

                if (closedSet.Contains(current))
                {
                    //openSet.Remove(current);
                    continue; // Ignore already evaluated neighbors
                }

                openSet.Remove(current);

                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current, grid))
                {                    
                    if (Utils.Contains(closedSet, neighbor))
                        continue; // Ignore already evaluated neighbors

                    // Tentative G cost
                    float tentativeG = current.G + 1;

                    if (!Utils.Contains(openSet, neighbor))
                    {
                        openSet.Add(neighbor); // Discover a new node
                    }
                    else if (tentativeG >= neighbor.G)
                    {
                        continue; // Not a better path
                    }

                    // Update neighbor values
                    neighbor.Parent = current;
                    neighbor.G = tentativeG;
                    neighbor.H = Heuristic(neighbor, goal);
                }
            }

            return new List<Node>(); // Return an empty path if no path is found
        }

        private List<Node> GetNeighbors(Node node, bool[,] grid)
        {
            var neighbors = new List<Node>();
            int[,] directions = { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } }; // 4-directional movement

            for(int i = 0; i < directions.GetLength(0); i++)
            {
                int newX = node.X + directions[i,0];
                int newY = node.Y + directions[i,1];

                if (IsInBounds(newX, newY, grid) && !grid[newX, newY])
                {
                    neighbors.Add(new Node(newX, newY));
                }
            }

            return neighbors;
        }

        private bool IsInBounds(int x, int y, bool[,] grid)
        {
            return x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1);
        }

        private float Heuristic(Node a, Node b)
        {
            // Using Manhattan distance as heuristic
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static List<Node> ReconstructPath(Node current)
        {
            var path = new List<Node>();
            while (current != null)
            {
                path.Add(current);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }

        // Generated from ChatGPT
        // https://chat.chatbotapp.ai/chats/-OOUzMQXKwk1fjmb6UCF?model=gpt-3.5
        public List<Node> SimplifyNodeList(List<Node> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return new List<Node>();
            }

            if (nodes.Count < 3)
            {
                return nodes;
            }

            List<Node> simplifiedNodes = new List<Node> { nodes[0] };
            (int, int)? lastDirection = null;

            for (int i = 1; i < nodes.Count; i++)
            {
                Node currentNode = nodes[i];
                Node previousNode = nodes[i - 1];

                // Calculate direction vector
                var direction = (currentNode.X - previousNode.X, currentNode.Y - previousNode.Y);

                if (i == 1)
                {
                    // For the second node, initialize the lastDirection
                    lastDirection = direction;
                    continue;
                }

                // Check if the direction has changed
                if (lastDirection != direction)
                {
                    // simplifiedNodes.Add(currentNode); // was returning an incorrect result.
                    simplifiedNodes.Add(previousNode);
                    lastDirection = direction; // Update lastDirection
                }
            }
            simplifiedNodes.Add(nodes.Last()); // needs to be added.
            return simplifiedNodes;
        }
    }


}
