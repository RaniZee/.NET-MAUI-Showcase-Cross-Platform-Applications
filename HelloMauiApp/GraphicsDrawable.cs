using Microsoft.Maui.Graphics;

namespace HelloMauiApp
{
    public class GraphicsDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.DarkBlue;
            canvas.StrokeSize = 4;

            canvas.DrawLine(10, 10, 90, 100);

            canvas.StrokeDashPattern = new float[] { 2, 2 };
            canvas.DrawLine(10, 120, 90, 210);
            canvas.StrokeDashPattern = null;   

            canvas.DrawRectangle(10, 230, 100, 50);

            canvas.FillColor = Colors.IndianRed;
            canvas.FillRectangle(150, 230, 100, 50);

            canvas.StrokeColor = Colors.DarkSlateGray;
            canvas.FillColor = Colors.LightSlateGray;
            canvas.FillRoundedRectangle(150, 10, 100, 100, 12);
            canvas.DrawRoundedRectangle(150, 10, 100, 100, 12);

            canvas.StrokeColor = Colors.Teal;
            canvas.StrokeSize = 6;
            canvas.DrawEllipse(150, 130, 100, 100);

            canvas.FontColor = Colors.Purple;
            canvas.FontSize = 18;
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.DrawString("Hello, Graphics!", 10, 300, HorizontalAlignment.Left);

            PathF path = new PathF();
            path.MoveTo(50, 350);
            path.LineTo(150, 350);
            path.LineTo(100, 450);
            path.Close();
            canvas.StrokeColor = Colors.Green;
            canvas.DrawPath(path);

            canvas.StrokeColor = Colors.Orange;
            canvas.DrawArc(200, 350, 100, 100, 0, 180, true, false);
        }
    }
}