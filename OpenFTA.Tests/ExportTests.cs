using System.Drawing.Imaging;
using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>Bitmap export: nothing is cut off and the view is not changed by the export.</summary>
public class ExportTests : SettingsTestBase
{
    [Theory]
    [MemberData(nameof(GoldenExampleTests.ExampleFiles), MemberType = typeof(GoldenExampleTests))]
    public void Bitmap_export_contains_the_whole_tree(string file)
    {
        var t = ExampleLoader.Load(file);
        var engine = t.MyDrawingEngine;
        engine.SetStructure(t.FTAStructure);
        engine.GlobalZoom = 2.5;
        engine.offsetX = 300;
        engine.offsetY = -150;
        var leaf = t.FTAStructure.Values.First(i => i.Children.Count == 0);
        engine.TopEvent = leaf;                         // e.g. set by the Sort button

        string path = Path.Combine(Path.GetTempPath(), $"openfta-export-{Guid.NewGuid():N}.png");
        try
        {
            engine.ExportToBMP(path, ImageFormat.Png);

            using var image = new Bitmap(path);
            var visible = t.FTAStructure.Values.Where(i => !i.IsHidden).ToList();
            int treeWidth = visible.Max(i => i.X) - visible.Min(i => i.X) + Constants.EventWidth;
            int treeHeight = visible.Max(i => i.Y) - visible.Min(i => i.Y) + Constants.EventHeight;
            Assert.True(image.Width >= treeWidth, $"Image width {image.Width} < tree width {treeWidth}");
            Assert.True(image.Height >= treeHeight, $"Image height {image.Height} < tree height {treeHeight}");

            // Margin: the border of the image is empty (nothing is cut off)
            for (int x = 0; x < image.Width; x++)
            {
                AssertWhite(image, x, 0);
                AssertWhite(image, x, image.Height - 1);
            }
            for (int y = 0; y < image.Height; y++)
            {
                AssertWhite(image, 0, y);
                AssertWhite(image, image.Width - 1, y);
            }

            // Something was drawn
            Assert.True(CountDarkPixels(image) > 100, "Exported image is empty");
        }
        finally
        {
            File.Delete(path);
        }

        // The view was restored
        Assert.Equal(2.5, engine.GlobalZoom);
        Assert.Equal(300, engine.offsetX);
        Assert.Equal(-150, engine.offsetY);
        Assert.Same(leaf, engine.TopEvent);
    }

    private static void AssertWhite(Bitmap image, int x, int y)
    {
        Color c = image.GetPixel(x, y);
        Assert.True(c.R > 240 && c.G > 240 && c.B > 240, $"Pixel ({x}, {y}) on the border is not white: {c}");
    }

    private static int CountDarkPixels(Bitmap image)
    {
        int count = 0;
        for (int y = 0; y < image.Height; y += 2)
            for (int x = 0; x < image.Width; x += 2)
            {
                Color c = image.GetPixel(x, y);
                if (c.R < 100 && c.G < 100 && c.B < 100)
                    count++;
            }
        return count;
    }
}
