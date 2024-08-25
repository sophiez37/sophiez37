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
    /// This class constructs the Powerup object
    /// </summary>
    public class Powerup
    {
        public int power { get; set; }
        public Vector2D loc { get; set; }
        public bool died { get; set; }

        [JsonIgnore]
        public static int maxPowerups { get; set; } = 20;
        [JsonIgnore]
        public static int maxPowerupDelay { get; set; } = 75;

        public Powerup(int power, Vector2D loc, bool died)
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
        }
    }
}
