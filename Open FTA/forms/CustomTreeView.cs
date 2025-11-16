namespace Open_FTA.forms
{
    public class CustomTreeView : TreeView
    {
        private const int WM_LBUTTONDBLCLK = 0x203;

        // Sledujeme, či práve prebieha akcia od používateľa
        private bool _userAction = false;

        /// <summary>
        /// True, ak aktuálna akcia pochádza od používateľa (myš alebo klávesnica)
        /// </summary>
        public bool IsUserAction => _userAction;

        /// <summary>
        /// Vlastná udalosť pre double-click na uzol (bez expand/collapse)
        /// </summary>
        public event EventHandler<TreeNode> NodeDoubleClickCustom;

        public CustomTreeView()
        {
            // Pripojenie na myš a klávesnicu
            this.MouseDown += (_, __) => _userAction = true;
            this.MouseUp += (_, __) => _userAction = false;
            this.KeyDown += (_, __) => _userAction = true;
            this.KeyUp += (_, __) => _userAction = false;

            // Pripojenie na štandardné udalosti stromu
            this.BeforeExpand += CustomTreeView_BeforeExpand;
            this.AfterExpand += CustomTreeView_AfterExpand;
            this.BeforeCollapse += CustomTreeView_BeforeCollapse;
            this.AfterCollapse += CustomTreeView_AfterCollapse;
            this.AfterSelect += CustomTreeView_AfterSelect;
        }

        // --- Zachytenie double-clicku bez expand/collapse ---
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDBLCLK)
            {
                Point clickPoint = this.PointToClient(Cursor.Position);
                TreeNode clickedNode = this.GetNodeAt(clickPoint);

                if (clickedNode != null)
                {
                    OnNodeDoubleClickCustom(clickedNode);
                }

                // Potlačíme expand/collapse
                return;
            }

            base.WndProc(ref m);
        }

        protected virtual void OnNodeDoubleClickCustom(TreeNode node)
        {
            NodeDoubleClickCustom?.Invoke(this, node);
        }

        // --- Obsluha udalostí expand/collapse/select ---
        private void CustomTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (_userAction)
                Console.WriteLine($"▶ Používateľ rozbaľuje: {e.Node.Text}");
            else
                Console.WriteLine($"▶ Automatické rozbalenie: {e.Node.Text}");
        }

        private void CustomTreeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (_userAction)
            {
                ExpandAllChildren(e.Node);
            }
        }

        private void CustomTreeView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (_userAction)
                Console.WriteLine($"▼ Používateľ zbaľuje: {e.Node.Text}");
            else
                Console.WriteLine($"▼ Automatické zbalenie: {e.Node.Text}");
        }

        private void CustomTreeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            Console.WriteLine($"✔ AfterCollapse: {e.Node.Text}");
        }

        private void CustomTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Console.WriteLine($"✔ AfterSelect: {e.Node.Text}");
        }

        // --- Príklad: vlastná metóda na výber ďalšieho uzla ---
        public void SelectNextNode()
        {
            if (SelectedNode?.NextNode != null)
                SelectedNode = SelectedNode.NextNode;
        }
        public TreeNode? FindNodeByText(string text)
        {
            foreach (TreeNode node in this.Nodes)
            {
                var found = FindNodeByTextRecursive(node, text);
                if (found != null)
                    return found;
            }
            return null;
        }

        private TreeNode? FindNodeByTextRecursive(TreeNode parent, string text)
        {
            if (parent.Name == text)
                return parent;

            foreach (TreeNode child in parent.Nodes)
            {
                var result = FindNodeByTextRecursive(child, text);
                if (result != null)
                    return result;
            }

            return null;
        }

        public bool ExpandNodeByText(string nodeText)
        {
            var node = FindNodeByText(nodeText);
            if (node != null)
            {
                bool prev = _userAction;
                _userAction = false;

                ExpandNodeRecursive(node);

                _userAction = prev;
                return true;
            }
            return false;
        }

        private void ExpandNodeRecursive(TreeNode node)
        {
            node.Expand();
            foreach (TreeNode child in node.Nodes)
            {
                ExpandNodeRecursive(child);
            }
        }

        private void ExpandAllChildren(TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Expand();
                if (child.Nodes.Count > 0)
                    ExpandAllChildren(child);
            }
        }

        // --- Zbalí konkrétny uzol podľa názvu ---
        public bool CollapseNodeByText(string nodeText)
        {
            var node = FindNodeByText(nodeText);
            if (node != null)
            {
                bool prev = _userAction;
                _userAction = false;
                node.Collapse();
                _userAction = prev;
                return true;
            }
            return false;
        }
    }
}


