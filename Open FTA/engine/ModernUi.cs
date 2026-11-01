using System.Runtime.InteropServices;

/// <summary>Flat, current look of the standard WinForms controls (tool strips, tree view, list view).</summary>
internal static class ModernUi
{
    public static readonly Color PanelBack = Color.FromArgb(245, 247, 250);
    public static readonly Color Accent = Color.FromArgb(0, 120, 215);

    public static readonly ToolStripRenderer ToolStripRenderer = new FlatToolStripRenderer();

    /// <summary>Flat renderer for all tool strips and status strips in the control tree (tool strips added later are not affected).</summary>
    public static void ApplyFlatToolStrips(Control root)
    {
        ToolStripManager.Renderer = ToolStripRenderer;   // drop-down menus of the tool strip buttons
        foreach (Control c in root.Controls)
        {
            if (c is ToolStrip strip)
            {
                strip.Renderer = ToolStripRenderer;
                if (strip is not StatusStrip)
                    strip.GripStyle = ToolStripGripStyle.Hidden;
            }
            ApplyFlatToolStrips(c);
        }
    }

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

    /// <summary>
    /// Look of the Windows Explorer for a tree or list view (arrows instead of +/-, hover and
    /// full-row highlight). Applied again when the window handle is recreated.
    /// </summary>
    public static void ApplyExplorerTheme(Control control)
    {
        void Apply() { try { SetWindowTheme(control.Handle, "Explorer", null); } catch (DllNotFoundException) { } }
        control.HandleCreated += (_, _) => Apply();
        if (control.IsHandleCreated)
            Apply();
    }

    /// <summary>Plain background, light hover and pressed state, thin separators, no rounded edges.</summary>
    private sealed class FlatToolStripRenderer : ToolStripProfessionalRenderer
    {
        public FlatToolStripRenderer() : base(new FlatColorTable()) => RoundedEdges = false;

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is ToolStripDropDown)
                base.OnRenderToolStripBorder(e);           // menus keep their frame
            else if (e.ToolStrip is not StatusStrip)
            {
                using var line = new Pen(Color.FromArgb(222, 226, 230));   // thin line below the bar
                e.Graphics.DrawLine(line, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width, e.ToolStrip.Height - 1);
            }
        }
    }

    private sealed class FlatColorTable : ProfessionalColorTable
    {
        private static readonly Color Back = PanelBack;
        private static readonly Color Hover = Color.FromArgb(229, 236, 245);
        private static readonly Color Pressed = Color.FromArgb(212, 224, 240);
        private static readonly Color Border = Color.FromArgb(196, 212, 232);

        public override Color ToolStripGradientBegin => Back;
        public override Color ToolStripGradientMiddle => Back;
        public override Color ToolStripGradientEnd => Back;
        public override Color ToolStripBorder => Back;
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color ToolStripContentPanelGradientBegin => Back;
        public override Color ToolStripContentPanelGradientEnd => Back;
        public override Color StatusStripGradientBegin => Back;
        public override Color StatusStripGradientEnd => Back;
        public override Color OverflowButtonGradientBegin => Back;
        public override Color OverflowButtonGradientMiddle => Back;
        public override Color OverflowButtonGradientEnd => Back;
        public override Color GripDark => Back;
        public override Color GripLight => Back;
        public override Color SeparatorDark => Color.FromArgb(214, 220, 228);
        public override Color SeparatorLight => Back;

        public override Color ButtonSelectedHighlight => Hover;
        public override Color ButtonSelectedGradientBegin => Hover;
        public override Color ButtonSelectedGradientMiddle => Hover;
        public override Color ButtonSelectedGradientEnd => Hover;
        public override Color ButtonSelectedBorder => Border;
        public override Color ButtonPressedHighlight => Pressed;
        public override Color ButtonPressedGradientBegin => Pressed;
        public override Color ButtonPressedGradientMiddle => Pressed;
        public override Color ButtonPressedGradientEnd => Pressed;
        public override Color ButtonPressedBorder => Border;
        public override Color ButtonCheckedHighlight => Pressed;
        public override Color ButtonCheckedGradientBegin => Pressed;
        public override Color ButtonCheckedGradientMiddle => Pressed;
        public override Color ButtonCheckedGradientEnd => Pressed;

        public override Color MenuItemSelected => Hover;
        public override Color MenuItemBorder => Border;
        public override Color MenuItemSelectedGradientBegin => Hover;
        public override Color MenuItemSelectedGradientEnd => Hover;
        public override Color MenuItemPressedGradientBegin => Pressed;
        public override Color MenuItemPressedGradientMiddle => Pressed;
        public override Color MenuItemPressedGradientEnd => Pressed;
        public override Color MenuBorder => Color.FromArgb(206, 212, 220);
        public override Color ImageMarginGradientBegin => Back;
        public override Color ImageMarginGradientMiddle => Back;
        public override Color ImageMarginGradientEnd => Back;
    }
}
