/// <summary>
/// ALGO III - tidy tree layout.
///
/// Reingold-Tilford / Walker algorithm in the linear-time version of
/// C. Buchheim, M. Juenger, S. Leipert: "Improving Walker's Algorithm to Run in Linear Time" (2002).
///
/// - every subtree is placed as close to its left neighbour as the contours of both subtrees allow
///   (a subtree may slide under a neighbour on levels where the neighbour has no events),
/// - a parent is centred above its first and last child,
/// - the order of siblings is kept on all levels, so events never overlap and lines do not cross,
/// - hidden (collapsed) events are ignored, their parent is laid out as a leaf,
/// - O(n) time.
///
/// FTAitem.X is the left edge of the event; all events have the same width.
/// </summary>
internal static class TidyTreeLayout
{
    private const int TopY = 50;

    private sealed class Node
    {
        public readonly FTAitem Item;
        public readonly Node? Parent;
        public readonly List<Node> Children = new();
        public readonly int Number;     // 1-based position among siblings
        public readonly int Depth;

        public double Prelim, Mod, Shift, Change;
        public Node? Thread;
        public Node Ancestor;

        public Node(FTAitem item, Node? parent, int number, int depth)
        {
            Item = item;
            Parent = parent;
            Number = number;
            Depth = depth;
            Ancestor = this;
        }

        public bool IsLeaf => Children.Count == 0;
        public Node? LeftSibling => Parent != null && Number > 1 ? Parent.Children[Number - 2] : null;
        public Node LeftmostSibling => Parent != null ? Parent.Children[0] : this;
        public Node? NextLeft => IsLeaf ? Thread : Children[0];
        public Node? NextRight => IsLeaf ? Thread : Children[^1];
    }

    public static void Apply(FTAlogic logic)
    {
        FTAitem? top = logic.GetItem(logic.TopEventGuid);
        if (top == null)
            return;

        logic.FindAllChilren();
        Apply(logic.FTAStructure, top);
    }

    /// <summary>Lays out the tree below <paramref name="root"/>; Children lists of the items must be filled.</summary>
    public static void Apply(Dictionary<Guid, FTAitem> structure, FTAitem root)
    {
        // +1 px so that rounded positions never touch (IsAnyItemOverlapping uses strict intersection)
        double distance = Constants.EventWidth + Constants.EventHorizontalSpacing + 1;
        int levelHeight = Constants.EventHeight + Constants.EventVerticalSpacing;

        Node rootNode = Build(structure, root, null, 1, 0, new HashSet<Guid>());
        FirstWalk(rootNode, distance);

        var positions = new List<(Node Node, double X)>();
        SecondWalk(rootNode, -rootNode.Prelim, positions);

        double minX = positions.Min(p => p.X);
        foreach (var (node, x) in positions)
        {
            node.Item.X = (int)Math.Round(x - minX);
            node.Item.Y = TopY + node.Depth * levelHeight;
        }
    }

    private static Node Build(Dictionary<Guid, FTAitem> structure, FTAitem item, Node? parent, int number, int depth, HashSet<Guid> visited)
    {
        visited.Add(item.GuidCode);
        var node = new Node(item, parent, number, depth);

        int childNumber = 1;
        foreach (Guid guid in item.Children)
        {
            if (!structure.TryGetValue(guid, out FTAitem? child) || child.IsHidden || visited.Contains(guid))
                continue;
            node.Children.Add(Build(structure, child, node, childNumber++, depth + 1, visited));
        }
        return node;
    }

    private static void FirstWalk(Node v, double distance)
    {
        if (v.IsLeaf)
        {
            Node? w = v.LeftSibling;
            v.Prelim = w != null ? w.Prelim + distance : 0;
            return;
        }

        Node defaultAncestor = v.Children[0];
        foreach (Node w in v.Children)
        {
            FirstWalk(w, distance);
            defaultAncestor = Apportion(w, defaultAncestor, distance);
        }
        ExecuteShifts(v);

        double midpoint = (v.Children[0].Prelim + v.Children[^1].Prelim) / 2;
        Node? left = v.LeftSibling;
        if (left != null)
        {
            v.Prelim = left.Prelim + distance;
            v.Mod = v.Prelim - midpoint;
        }
        else
        {
            v.Prelim = midpoint;
        }
    }

    /// <summary>Pushes subtree v to the right of the subtrees of its left siblings (contour comparison).</summary>
    private static Node Apportion(Node v, Node defaultAncestor, double distance)
    {
        Node? w = v.LeftSibling;
        if (w == null)
            return defaultAncestor;

        Node vip = v, vop = v;          // inside / outside contour of the right subtree
        Node vim = w;                   // inside contour of the left forest
        Node vom = v.LeftmostSibling;   // outside contour of the left forest
        double sip = vip.Mod, sop = vop.Mod, sim = vim.Mod, som = vom.Mod;

        while (vim.NextRight != null && vip.NextLeft != null)
        {
            vim = vim.NextRight;
            vip = vip.NextLeft;
            vom = vom.NextLeft!;
            vop = vop.NextRight!;
            vop.Ancestor = v;

            double shift = (vim.Prelim + sim) - (vip.Prelim + sip) + distance;
            if (shift > 0)
            {
                Node ancestor = vim.Ancestor.Parent == v.Parent ? vim.Ancestor : defaultAncestor;
                MoveSubtree(ancestor, v, shift);
                sip += shift;
                sop += shift;
            }

            sim += vim.Mod;
            sip += vip.Mod;
            som += vom.Mod;
            sop += vop.Mod;
        }

        if (vim.NextRight != null && vop.NextRight == null)
        {
            vop.Thread = vim.NextRight;
            vop.Mod += sim - sop;
        }

        if (vip.NextLeft != null && vom.NextLeft == null)
        {
            vom.Thread = vip.NextLeft;
            vom.Mod += sip - som;
            defaultAncestor = v;
        }

        return defaultAncestor;
    }

    private static void MoveSubtree(Node wl, Node wr, double shift)
    {
        int subtrees = wr.Number - wl.Number;
        wr.Change -= shift / subtrees;
        wr.Shift += shift;
        wl.Change += shift / subtrees;
        wr.Prelim += shift;
        wr.Mod += shift;
    }

    /// <summary>Spreads the shifts of moved subtrees evenly over the subtrees between them.</summary>
    private static void ExecuteShifts(Node v)
    {
        double shift = 0, change = 0;
        for (int i = v.Children.Count - 1; i >= 0; i--)
        {
            Node w = v.Children[i];
            w.Prelim += shift;
            w.Mod += shift;
            change += w.Change;
            shift += w.Shift + change;
        }
    }

    private static void SecondWalk(Node v, double m, List<(Node Node, double X)> result)
    {
        result.Add((v, v.Prelim + m));
        foreach (Node w in v.Children)
            SecondWalk(w, m + v.Mod, result);
    }
}
