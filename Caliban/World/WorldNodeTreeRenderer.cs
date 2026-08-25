using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Treasures.Resources;

namespace Caliban.Core.World
{
    public static class WorldNodeTreeRenderer
    {
        public static void SavePng(
            WorldNode root,
            string filePath,
            int nodeRadius = 12,
            int horizontalSpacing = 40,
            int verticalSpacing = 80,
            int padding = 40)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            List<List<WorldNode>> levels = GetLevels(root);

            Dictionary<WorldNode, Point> positions =
                new Dictionary<WorldNode, Point>();

            // Find the widest level first.
            int maxWidth = 0;

            for (int depth = 0; depth < levels.Count; depth++)
            {
                List<WorldNode> level = levels[depth];

                int levelWidth =
                    (level.Count - 1) * horizontalSpacing +
                    level.Count * nodeRadius * 2;

                maxWidth = Math.Max(maxWidth, levelWidth);
            }

            // Canvas width is based on the widest level.
            int width = maxWidth + padding * 2;

            // Position every level relative to the same canvas center.
            for (int depth = 0; depth < levels.Count; depth++)
            {
                List<WorldNode> level = levels[depth];

                int levelWidth =
                    (level.Count - 1) * horizontalSpacing +
                    level.Count * nodeRadius * 2;

                int startX = (width - levelWidth) / 2;

                for (int i = 0; i < level.Count; i++)
                {
                    int x =
                        startX +
                        nodeRadius +
                        i * (horizontalSpacing + nodeRadius * 2);

                    int y =
                        padding +
                        depth * verticalSpacing;

                    positions[level[i]] = new Point(x, y);
                }
            }

            int height =
                padding * 2 +
                (levels.Count - 1) * verticalSpacing +
                nodeRadius * 2;

            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Pen linePen = new Pen(Color.Black, 2))
            {
                graphics.Clear(Color.White);

                graphics.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw parent -> child lines first so they appear behind nodes.
                foreach (KeyValuePair<WorldNode, Point> entry in positions)
                {
                    WorldNode node = entry.Key;

                    if (node.ParentNode == null)
                        continue;

                    Point parent;

                    if (!positions.TryGetValue(node.ParentNode, out parent))
                        continue;

                    Point child = entry.Value;

                    graphics.DrawLine(
                        linePen,
                        parent.X,
                        parent.Y + nodeRadius,
                        child.X,
                        child.Y - nodeRadius);
                }

                // Draw circles.
                foreach (KeyValuePair<WorldNode, Point> entry in positions)
                {
                    WorldNode node = entry.Key;
                    Point position = entry.Value;

                    // Safely check for the victory condition
                    bool containsVictory = node.Treasures != null &&
                                           node.Treasures.Exists(t => t.type == TreasureType.SIMPLE_VICTORY);

                    Color nodeColor = containsVictory ? Color.ForestGreen : Color.IndianRed;

                    // Properly dispose of the brush and pen for each node
                    using (Brush nodeBrush = new SolidBrush(nodeColor))
                    using (Pen nodeOutlinePen = new Pen(Color.Black, 2))
                    {
                        graphics.FillEllipse(
                            nodeBrush,
                            position.X - nodeRadius,
                            position.Y - nodeRadius,
                            nodeRadius * 2,
                            nodeRadius * 2);

                        graphics.DrawEllipse(
                            nodeOutlinePen,
                            position.X - nodeRadius,
                            position.Y - nodeRadius,
                            nodeRadius * 2,
                            nodeRadius * 2);
                    }
                }

                bitmap.Save(filePath, ImageFormat.Png);
            }
        }

        private static List<List<WorldNode>> GetLevels(WorldNode root)
        {
            List<List<WorldNode>> levels =
                new List<List<WorldNode>>();

            Queue<WorldNode> nodes =
                new Queue<WorldNode>();

            Dictionary<WorldNode, int> depths =
                new Dictionary<WorldNode, int>();

            nodes.Enqueue(root);
            depths[root] = 0;

            while (nodes.Count > 0)
            {
                WorldNode node = nodes.Dequeue();
                int depth = depths[node];

                while (levels.Count <= depth)
                    levels.Add(new List<WorldNode>());

                levels[depth].Add(node);

                foreach (WorldNode child in node.ChildNodes)
                {
                    depths[child] = depth + 1;
                    nodes.Enqueue(child);
                }
            }

            return levels;
        }
    }
}