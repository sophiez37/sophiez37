using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace Server
{
    /// <summary>
    /// This class contains information from settings.xml file
    /// </summary>
    [DataContract(Namespace = "")]
    public class GameSettings
    {
        [DataMember]
        public int MSPerFrame { get; set; }
        [DataMember]
        public int RespawnRate { get; set; }
        [DataMember]
        public int UniverseSize { get; set; }
        [DataMember]
        public List<Wall> Walls { get; set; }

        public GameSettings(int MSPerFrame, int ReprawnRate, int UniverseSize)
        {
            this.MSPerFrame = MSPerFrame;
            this.RespawnRate = ReprawnRate;
            this.UniverseSize = UniverseSize;
            this.Walls = new List<Wall>();
        }
    }
}
