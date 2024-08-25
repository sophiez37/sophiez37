using SnakeGame;

namespace Model
{
    /// <summary>
    /// This class constructs a World object containing walls, powerups, and snakes
    /// </summary>
    public class World
    {
        public Dictionary<int, Wall> Walls { get; set; }
        public Dictionary<int, Powerup> Powerups { get; set; }
        public Dictionary<int, Snake> Snakes { get; set; }
        public int size { get; set; }
        public int powerCount { set; get; }
        public int powerID { set; get; }
        public int powerFrameCount { set; get; }
        private bool decreasePowerup { get; set; }

        public World(int size)
        {
            Walls = new Dictionary<int, Wall>();
            Powerups = new Dictionary<int, Powerup>();
            Snakes = new Dictionary<int, Snake>();
            this.size = size;
            this.powerCount = 0;
            this.powerID = 0;
            this.powerFrameCount = 0;
            this.decreasePowerup = false;
        }

        /// <summary>
        /// Generates a random snake. The new snake will not collide with any walls on the first frame
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Snake generateRandomSnake(int id, string name)
        {
            Random rng = new Random();
            
            // Random position for snake head
            double headX = rng.Next(-size / 2 + 50 + Snake.startingLengh, size / 2 - 50 - Snake.startingLengh);
            double headY = rng.Next(-size / 2 + 50 + Snake.startingLengh, size / 2 - 50 - Snake.startingLengh);

            // Make sure that snake head doesn't collide with any walls
            bool collideWalls = checkCollisionWithAllWalls(headX, headY, Snake.startingLengh + 5);
            while (collideWalls)
            {
                headX = rng.Next(-size / 2 + 50 + Snake.startingLengh, size / 2 - 50 - Snake.startingLengh);
                headY = rng.Next(-size / 2 + 50 + Snake.startingLengh, size / 2 - 50 - Snake.startingLengh);

                collideWalls = checkCollisionWithAllWalls(headX, headY, Snake.startingLengh + 5);
            }

            // Random direction
            int direction = rng.Next(0, 4);
            List<Vector2D> body;
            Vector2D dir;
            if (direction == 0) // Up
            {
                dir = new Vector2D(0, -1);
                body = new() { new Vector2D(headX, headY + Snake.startingLengh), new Vector2D(headX, headY) };
            }
            else if (direction == 1) // Down
            {
                dir = new Vector2D(0, 1);
                body = new() { new Vector2D(headX, headY - Snake.startingLengh), new Vector2D(headX, headY) };
            }
            else if (direction == 2) // Left
            {
                dir = new Vector2D(-1, 0);
                body = new() { new Vector2D(headX + Snake.startingLengh, headY), new Vector2D(headX, headY) };
            }
            else // Right
            {
                dir = new Vector2D(1, 0);
                body = new() { new Vector2D(headX - Snake.startingLengh, headY), new Vector2D(headX, headY) };
            }

            return new Snake(id, name, body, dir, 0, false, true, false, false);
        }

        /// <summary>
        /// Generates random powerups
        /// </summary>
        /// <param name="maxPowerup">Maximum number of powerups in the world</param>
        public void generatePowerups()
        {
            Random rng = new Random();
            if (powerCount < Powerup.maxPowerups)
            {
                // Random position of powerup
                double x = rng.Next(-size / 2 + 50, size / 2 - 50);
                double y = rng.Next(-size / 2 + 50, size / 2 - 50);

                // Make sure the new powerup doesn't collide with any walls
                bool collideWalls = checkCollisionWithAllWalls(x, y, 8);
                while (collideWalls)
                {
                    x = rng.Next(-size / 2 + 50, size / 2 - 50);
                    y = rng.Next(-size / 2 + 50, size / 2 - 50);

                    collideWalls = checkCollisionWithAllWalls(x, y, 8);
                }

                Powerup powerup = new Powerup(powerID, new Vector2D(x, y), false);
                Powerups.Add(powerID, powerup);
                powerID++;
                powerCount++;
            }
        }

        /// <summary>
        /// Grow snake after eating powerup,
        /// generate new powerup
        /// </summary>
        /// <param name="snake"></param>
        /// <param name="powerupDelay"></param>
        /// <param name="snakeGrowthRate"></param>
        public void checkSnakeEatPowerup(Snake snake)
        {
            // Grow snake after eating powerup
            if (snake.gotPowerup && snake.frameCounter < Snake.growth)
                snake.frameCounter++;

            else if (snake.gotPowerup && snake.frameCounter >= Snake.growth)
            {
                snake.gotPowerup = false;
                snake.frameCounter = 0;
            }

            // Generate new powerup after dekay time
            if (decreasePowerup && powerFrameCount < Powerup.maxPowerupDelay)
                powerFrameCount++;

            else if (decreasePowerup && powerFrameCount >= Powerup.maxPowerupDelay)
            {
                decreasePowerup = false;
                powerCount--;
                powerFrameCount = 0;
            }
        }

        /// <summary>
        /// Checks the collision between snake and powerups, walls, and other snakes
        /// </summary>
        /// <param name="snake"></param>
        public void checkCollision(Snake snake)
        {
            Vector2D snakeHead = snake.body[snake.body.Count - 1];

            // Snake - Powerup collision
            foreach (Powerup powerup in Powerups.Values)
            {
                if (!powerup.died)
                {
                    if (checkSnakePowerupCollision(snake, powerup))
                    {
                        powerup.died = true;
                        decreasePowerup = true;

                        snake.gotPowerup = true;
                        snake.score++;
                    }
                }
            }

            // Snake - Wall collision
            if (checkCollisionWithAllWalls(snakeHead.GetX(), snakeHead.GetY(), 5))
            {
                snake.alive = false;
                snake.died = true;
            }

            // Snake - Snake collision
            foreach (Snake snakeCollision in Snakes.Values)
            {
                for (int i = 0; i < snakeCollision.body.Count - 1; i++)
                {
                    // If snakeCollision is our snake, skip 2 last segment
                    if (snakeCollision.snake == snake.snake)
                    {
                        if (i >= snake.body.Count - 3)
                            continue;
                    }

                    Vector2D segment1 = snakeCollision.body[i];
                    Vector2D segment2 = snakeCollision.body[i + 1];

                    if (checkSnakeSegmentCollision(snakeHead, segment1, segment2))
                    {
                        snake.alive = false;
                        snake.died = true;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the object at point (xPos, yPos) collides with wall
        /// </summary>
        /// <param name="xPos">x position</param>
        /// <param name="yPos">y position</param>
        /// <param name="objectSize">Half the width of the object</param>
        /// <param name="wall">wall to check</param>
        /// <returns>True if object collides with wall, false otherwise</returns>
        private bool checkCollisionWithWall(double xPos, double yPos, int objectSize, Wall wall)
        {
            double leftX = Math.Min(wall.p1.GetX(), wall.p2.GetX()) - 25 - objectSize;
            double rightX = Math.Max(wall.p1.GetX(), wall.p2.GetX()) + 25 + objectSize;
            double topY = Math.Min(wall.p1.GetY(), wall.p2.GetY()) - 25 - objectSize;
            double bottomY = Math.Max(wall.p1.GetY(), wall.p2.GetY()) + 25 + objectSize;

            return xPos > leftX && xPos < rightX && yPos > topY && yPos < bottomY;
        }

        /// <summary>
        /// Checks if the object at point (xPos, yPos) collides with any of the walls in world
        /// </summary>
        /// <param name="xPos">x position</param>
        /// <param name="yPos">y position</param>
        /// <param name="objectSize">Half the width of the object</param>
        /// <returns>True if object collides with any of the walls, false otherwise</returns>
        private bool checkCollisionWithAllWalls(double xPos, double yPos, int objectSize)
        {
            foreach (Wall wall in Walls.Values)
            {
                if (checkCollisionWithWall(xPos, yPos, objectSize, wall))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if snake collides with powerup
        /// </summary>
        /// <param name="snake"></param>
        /// <param name="powerup"></param>
        /// <returns>True if collides, false otherwise</returns>
        private bool checkSnakePowerupCollision(Snake snake, Powerup powerup)
        {
            Vector2D snakePos = snake.body[snake.body.Count - 1];
            Vector2D powerupPos = powerup.loc;
            double distance = (snakePos - powerupPos).Length();
            return distance < 13;
        }

        /// <summary>
        /// Checks if snake head collides with the segment starting at segment1, ending at segment2
        /// </summary>
        /// <param name="headPos"></param>
        /// <param name="segment1"></param>
        /// <param name="segment2"></param>
        /// <returns>True if collides, false otherwise</returns>
        private bool checkSnakeSegmentCollision(Vector2D headPos, Vector2D segment1, Vector2D segment2)
        {
            double leftX = Math.Min(segment1.GetX(), segment2.GetX()) - 10;
            double rightX = Math.Max(segment1.GetX(), segment2.GetX()) + 10;
            double topY = Math.Min(segment1.GetY(), segment2.GetY()) - 10;
            double bottomY = Math.Max(segment1.GetY(), segment2.GetY()) + 10;

            return headPos.GetX() > leftX && headPos.GetX() < rightX && headPos.GetY() > topY && headPos.GetY() < bottomY;
        }
    }
}