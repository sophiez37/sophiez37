using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SnakeGame;

namespace Model
{
    /// <summary>
    /// This class constructs the Snake object
    /// </summary>
    public class Snake
    {
        public int snake { get; set; }
        public string name { get; set; }
        public List<Vector2D> body { get; set; }
        public Vector2D dir { get; set; }
        public int score { get; set; }
        public bool died { get; set; }
        public bool alive { get; set; }
        public bool dc { get; set; }
        public bool join { get; set; }

        [JsonIgnore]
        public static int speed { get; set; } = 6;
        [JsonIgnore]
        public static int startingLengh { get; set; } = 120;
        [JsonIgnore]
        public static int growth { get; set; } = 24;

        // True if snake has changed direction
        [JsonIgnore]
        public bool changeDir { set; get; }

        // Velocity = dir * speed
        [JsonIgnore]
        public Vector2D velocity { set; get; }

        // Number of frames since death
        [JsonIgnore]
        public int coolDown { get; set; }

        // Number of frames after disconnecting
        [JsonIgnore]
        public int frameAfterDisconnect { get; set; }

        // True if snake ate a powerup
        [JsonIgnore]
        public bool gotPowerup { get; set; }

        // Count frames to calculate movement
        [JsonIgnore]
        public int frameCounter { get; set; }

        public Snake(int snake, string name, List<Vector2D> body, Vector2D dir, int score, bool died, bool alive, bool dc, bool join)
        {
            this.snake = snake;
            this.name = name;
            this.body = body;
            this.dir = dir;
            this.score = score;
            this.died = died;
            this.alive = alive;
            this.dc = dc;
            this.join = join;

            changeDir = false;
            velocity = dir * speed;
            coolDown = 0;
        }

        /// <summary>
        /// Updates the snake position
        /// </summary>
        /// <param name="worldSize">Size of the world</param>
        public void update(int worldSize)
        {
            double size = worldSize / 2;
            double snakeHeadX = body![body.Count - 1].GetX();
            double snakeHeadY = body[body.Count - 1].GetY();

            // When snake goes to the edge of the world, wrap around to appear at the opposite edge
            if (snakeHeadX >= size)
            {
                body.Add(new(-size, snakeHeadY));
                body.Add(new(-size, snakeHeadY));
            }
            else if (snakeHeadX <= -size)
            {
                body.Add(new(size, snakeHeadY));
                body.Add(new(size, snakeHeadY));
            }
            else if (snakeHeadY >= size)
            {
                body.Add(new(snakeHeadX, -size));
                body.Add(new(snakeHeadX, -size));
            }
            else if (snakeHeadY <= -size)
            {
                body.Add(new(snakeHeadX, size));
                body.Add(new(snakeHeadX, size));
            }

            double snakeTailX = body[0].GetX();
            double snakeTailY = body[0].GetY();

            if ((snakeTailX <= -size || snakeTailX >= size))
            {
                body[0].X = body[0].X * -1;

                if (body[0].GetX() <= -size || body[0].GetX() >= size)
                    body.Remove(body[0]);
            }

            else if (snakeTailY <= -size || snakeTailY >= size)
            {
                body[0].Y = body[0].Y * -1;

                if (body[0].GetY() <= -size || body[0].GetY() >= size)
                    body.Remove(body[0]);
            }

            // If snake changes direction
            if (changeDir)
            {
                body[body.Count - 1] = body[body.Count - 1] - velocity;

                velocity = dir! * speed;
                Vector2D newHead = body[body.Count - 1] + velocity;

                body.Add(newHead);

                changeDir = false;

            }

            // Snake movements
            Vector2D tailVelocity = body[1] - body[0];
            tailVelocity.Normalize();
            tailVelocity = tailVelocity * speed;

            // When snake ate powerup
            if (gotPowerup)
            {
                tailVelocity = new(0, 0);
            }

            //Update snake's head and tail postion 
            body![0] += tailVelocity;
            body[body.Count - 1] += velocity;

            if (body[0].Equals(body[1]))
                body.Remove(body[0]);
        }
    }
}
