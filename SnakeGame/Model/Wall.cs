using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// This class constructs the Wall object
    /// </summary>
    [DataContract (Namespace = "")]
    public class Wall
    {
        [DataMember(Name = "ID")]
        public int wall { get; set; }
        [DataMember]
        public Vector2D p1 { get; set; }
        [DataMember]
        public Vector2D p2 { get; set; }

        public Wall(int ID, Vector2D p1, Vector2D p2)
        {
            this.wall = ID;
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
