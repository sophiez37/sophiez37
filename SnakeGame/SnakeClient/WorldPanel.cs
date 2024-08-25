using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using Model;
using Microsoft.Maui.Controls;
using Windows.UI.Input.Inking;
using static System.Net.Mime.MediaTypeNames;
using Windows.Web.UI;

namespace SnakeGame;
public class WorldPanel : IDrawable
{
    // A delegate for DrawObjectWithTransform
    // Methods matching this delegate can draw whatever they want onto the canvas
    public delegate void ObjectDrawer(object o, ICanvas canvas);

    private World theWorld;
    private int playerID;
    private IImage wall;
    private IImage background;
    private IImage explosion;
    private int viewSize = 900;
    private bool initializedForDrawing = false;

    // List recording time and position of snake deaths to draw explosions
    private List<(DateTime, Vector2D)> deaths = new();

    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeClient.Resources.Images";
        using (Stream stream = assembly.GetManifestResourceStream($"{path}.{name}"))
        {
#if MACCATALYST
            return PlatformImage.FromStream(stream);
#else
            return new W2DImageLoadingService().FromStream(stream);
#endif
        }
    }

    public WorldPanel()
    {
    }

    private void InitializeDrawing()
    {
        wall = loadImage( "wallsprite.png" );
        background = loadImage( "background.png" );
        explosion = loadImage( "explosion.png" );
        initializedForDrawing = true;
    }

    /// <summary>
    /// Modifies theWorld
    /// </summary>
    /// <param name="w"></param>
    public void SetWorld(World w)
    {
        theWorld = w;
    }

    /// <summary>
    /// Modifies playerID
    /// </summary>
    /// <param name="playerID"></param>
    public void SetPlayer(int playerID)
    {
        this.playerID = playerID;
    }

    /// <summary>
    /// This method performs a translation and rotation to draw an object.
    /// </summary>
    /// <param name="canvas">The canvas object for drawing onto</param>
    /// <param name="o">The object to draw</param>
    /// <param name="worldX">The X component of the object's position in world space</param>
    /// <param name="worldY">The Y component of the object's position in world space</param>
    /// <param name="angle">The orientation of the object, measured in degrees clockwise from "up"</param>
    /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
    {
        // "push" the current transform
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        canvas.Rotate((float)angle);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();
    }

    /// <summary>
    /// A method that can be used as an ObjectDrawer delegate
    /// </summary>
    /// <param name="o">The snake to draw</param>
    /// <param name="canvas"></param>
    private void SnakeSegmentDrawer(object o, ICanvas canvas)
    {
        canvas.StrokeSize = 10;
        canvas.StrokeLineCap = LineCap.Round;
        int snakeSegmentLength = (int)o;
        canvas.DrawLine(0, 0, 0, -snakeSegmentLength);
    }

    /// <summary>
    /// A method that can be used as an ObjectDrawer delegate
    /// </summary>
    /// <param name="o">The name to draw</param>
    /// <param name="canvas"></param>
    private void SnakeNameDrawer(object o, ICanvas canvas)
    {
        canvas.FontColor = Colors.White;
        canvas.FontSize = 10;
        string name = (string)o;
        canvas.DrawString(name, -25, -25, 50, 50, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    /// <summary>
    /// A method that can be used as an ObjectDrawer delegate
    /// </summary>
    /// <param name="o">The snake dead segment to draw</param>
    /// <param name="canvas"></param>
    private void SnakeDeadSegmentDrawer(object o, ICanvas canvas)
    {
        // Snake body turns black
        canvas.StrokeSize = 10;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeColor = Colors.Black;
        int snakeSegmentLength = (int)o;
        canvas.DrawLine(0, 0, 0, -snakeSegmentLength);

        // Snake skeleton
        canvas.StrokeSize = 2;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeColor = Colors.White;
        canvas.DrawLine(0, 0, 0, -snakeSegmentLength);
    }

    /// <summary>
    /// A method that can be used as an ObjectDrawer delegate
    /// </summary>
    /// <param name="o">The powerup to draw</param>
    /// <param name="canvas"></param>
    private void PowerupDrawer(object o, ICanvas canvas)
    {
        Powerup p = o as Powerup;

        // Outer ring of powerup is orange
        float width = 16;
        canvas.FillColor = Colors.DarkOrange;
        canvas.FillEllipse(-(width / 2), -(width / 2), width, width);

        // Inner ring of powerup is green
        width = 10;
        canvas.FillColor = Colors.DarkOliveGreen;
        canvas.FillEllipse(-(width / 2), -(width / 2), width, width);
    }

    /// <summary>
    /// A method that can be used as an ObjectDrawer delegate to draw one Wall
    /// </summary>
    /// <param name="o">The wall to draw</param>
    /// <param name="canvas"></param>
    private void WallDrawer(object o, ICanvas canvas)
    {
        float width = 50;
        canvas.DrawImage(wall, -(width / 2), -(width / 2), width, width);
    }

    /// <summary>
    /// A method that can be used as an ObjectDrawer delegate to draw one Wall
    /// </summary>
    /// <param name="o">The wall to draw</param>
    /// <param name="canvas"></param>
    private void ExplosionDrawer(object o, ICanvas canvas)
    {
        float width = 50;
        canvas.DrawImage(explosion, -(width / 2), -(width / 2), width, width);
    }

    /// <summary>
    /// Draw the world
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if ( !initializedForDrawing )
            InitializeDrawing();

        // undo previous transformations from last frame
        canvas.ResetState();

        lock(theWorld)
        {
            // Center the player's view
            if(theWorld.Snakes.ContainsKey(playerID))
            {
                Snake player = theWorld.Snakes[playerID];
                float playerX = (float)player.body[player.body.Count - 1].GetX();
                float playerY = (float)player.body[player.body.Count - 1].GetY();
                canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2));
                canvas.DrawImage(background, -(theWorld.size / 2), -(theWorld.size / 2), theWorld.size, theWorld.size);
            }

            // Draw all walls
            foreach(Wall w in theWorld.Walls.Values)
            {
                Vector2D distance = w.p1 - w.p2;
                int numberWalls = (int)(distance.Length() / 50) + 1;

                if (w.p1.GetX() == w.p2.GetX())
                {
                    double startY = Math.Min(w.p1.GetY(), w.p2.GetY());
                    for (int i = 0; i < numberWalls; i++)
                    {
                        DrawObjectWithTransform(canvas, w, w.p1.GetX(), startY, 0, WallDrawer);
                        startY += 50;
                    }
                }

                if(w.p1.Y == w.p2.Y)
                {
                    double startX = Math.Min(w.p1.GetX(), w.p2.GetX());
                    for (int i = 0; i < numberWalls; i++)
                    {
                        DrawObjectWithTransform(canvas, w, startX, w.p1.GetY(), 0, WallDrawer);
                        startX += 50;
                    }
                }
            }

            // Draw all powerups
            foreach(Powerup p in theWorld.Powerups.Values)
            {
                if(!p.died)
                {
                    DrawObjectWithTransform(canvas, p, p.loc.GetX(), p.loc.GetY(), 0, PowerupDrawer);
                }
            }

            // Draw all snakes
            foreach(Snake s in theWorld.Snakes.Values)
            {
                if(s.alive)
                {
                    // Set snake's color
                    if (s.snake % 8 == 0)
                        canvas.StrokeColor = Colors.Red;
                    else if (s.snake % 8 == 1)
                        canvas.StrokeColor = Colors.Orange;
                    else if (s.snake % 8 == 2)
                        canvas.StrokeColor = Colors.Yellow;
                    else if (s.snake % 8 == 3)
                        canvas.StrokeColor = Colors.Green;
                    else if (s.snake % 8 == 4)
                        canvas.StrokeColor = Colors.Blue;
                    else if (s.snake % 8 == 5)
                        canvas.StrokeColor = Colors.Purple;
                    else if (s.snake % 8 == 6)
                        canvas.StrokeColor = Colors.Violet;
                    else
                        canvas.StrokeColor = Colors.Magenta;

                    // Draw snakes's body
                    for (int i = 0; i < s.body.Count - 1; i++)
                    {
                        Vector2D snakeSegment = s.body[i + 1] - s.body[i];
                        int segmentLength = (int)snakeSegment.Length();
                        snakeSegment.Normalize();
                        DrawObjectWithTransform(canvas, segmentLength, s.body[i].GetX(), s.body[i].GetY(), snakeSegment.ToAngle(), SnakeSegmentDrawer);
                    }
                    
                    // Draw snakes's name and score
                    DrawObjectWithTransform(canvas, s.name + ": " + s.score, s.body[s.body.Count - 1].GetX(), s.body[s.body.Count - 1].GetY() - 15, 0, SnakeNameDrawer);
                }

                // Draw dead snake
                if(s.died)
                {
                    // Record the time where snake hits the wall
                    DateTime deathTime = DateTime.Now;

                    // Draw black skeleton snake
                    for (int i = 0; i < s.body.Count - 1; i++)
                    {
                        Vector2D snakeSegment = s.body[i + 1] - s.body[i];
                        int segmentLength = (int)snakeSegment.Length();
                        snakeSegment.Normalize();
                        DrawObjectWithTransform(canvas, segmentLength, s.body[i].GetX(), s.body[i].GetY(), snakeSegment.ToAngle(), SnakeDeadSegmentDrawer);
                    }

                    // Record the position where snake hits the wall
                    Vector2D headSegment = s.body[s.body.Count - 2] - s.body[s.body.Count - 1];
                    headSegment.Normalize();
                    Vector2D explosionPos = s.body[s.body.Count - 1] - (headSegment * 5);
                    deaths.Add((deathTime, explosionPos));
                }
            }

            // Draw explosions
            foreach((DateTime, Vector2D) death in deaths)
            {
                DateTime startTime = death.Item1;
                Vector2D explosionPos = death.Item2;
                DateTime endTime = DateTime.Now;
                TimeSpan timePassed = endTime.Subtract(startTime);
                TimeSpan explosionDuration = TimeSpan.FromTicks(10000000);
                if (!startTime.Equals(new DateTime(0)) && timePassed <= explosionDuration)
                {
                    DrawObjectWithTransform(canvas, new object(), explosionPos.GetX(), explosionPos.GetY(), 0, ExplosionDrawer);
                }
            }
        }
    }
}
