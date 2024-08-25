using NetworkUtil;
using System.Text.RegularExpressions;
using Model;
using System.Text.Json;
using System.Diagnostics;

namespace SnakeGame
{
    /// <summary>
    /// Game Controller which communicates with the server
    /// </summary>
    public class GameController
    {
        private World theWorld = new(0);
        private int playerID;
        private SocketState? theServer;
        public string movement { get; set; } = "{\"movement\":\"none\"}";

        public delegate void ErrorOccured();
        public event ErrorOccured? ErrorOcurredHandler;

        public delegate void Connected();
        public event Connected? ConnectedHandler;

        public delegate void PlayerUpdated();
        public event PlayerUpdated? PlayerUpdatedHandler;

        public delegate void WorldUpdated();
        public event PlayerUpdated? WorldUpdatedHandler;

        /// <summary>
        /// Connect to the server
        /// </summary>
        /// <param name="addr">Server address</param>
        public void Connect(string addr)
        {
            Networking.ConnectToServer(OnConnect, addr, 11000);
        }

        /// <summary>
        /// Send message to the server
        /// </summary>
        /// <param name="message">The message</param>
        public void Send(string message)
        {
            if (theServer != null)
            {
                Networking.Send(theServer.TheSocket, message + "\n");
            }
        }

        /// <summary>
        /// Method to be invoked by the networking library when a connection is made
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                ErrorOcurredHandler?.Invoke();
                return;
            }

            ConnectedHandler?.Invoke();

            theServer = state;

            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveMessage;
            Networking.GetData(state);
        }

        /// <summary>
        /// Method to be invoked by the networking library when 
        /// a network action occurs
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessage(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                ErrorOcurredHandler?.Invoke();
                return;
            }

            ProcessMessages(state);

            // Send movement to the server after the message has been processed
            // and set it back to none
            Send(movement);
            movement = "{\"movement\":\"none\"}";

            Networking.GetData(state);
        }

        /// <summary>
        /// Process any buffered messages separated by '\n'
        /// Display them, then remove them from the buffer.
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state)
        {
            string totalData = state.GetData();

            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.
            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                // Parse message from server
                JsonDocument doc = JsonDocument.Parse(p);

                // Get playerID and world size
                if (int.TryParse(p, out _))
                {
                    playerID = int.Parse(parts[0]);
                    theWorld.size = int.Parse(parts[1]);
                    PlayerUpdatedHandler?.Invoke();
                    state.RemoveData(0, p.Length);
                    continue;
                }

                // Deserialize walls
                else if (doc.RootElement.TryGetProperty("wall", out _))
                {
                    Wall? wall = JsonSerializer.Deserialize<Wall>(doc);
                    if (wall != null)
                        lock (theWorld)
                            theWorld.Walls[wall.wall] = wall;
                }

                // Deserialize snakes
                else if (doc.RootElement.TryGetProperty("snake", out _))
                {
                    Snake? snake = JsonSerializer.Deserialize<Snake>(doc);
                    if (snake != null)
                    {
                        lock (theWorld)
                        {
                            theWorld.Snakes[snake.snake] = snake;

                            if (snake.dc)
                                theWorld.Snakes.Remove(snake.snake);
                        }
                    }
                }

                // Dserialize powerups
                else if (doc.RootElement.TryGetProperty("power", out _))
                {
                    Powerup? power = JsonSerializer.Deserialize<Powerup>(doc);
                    if (power != null)
                    {
                        lock (theWorld)
                            theWorld.Powerups[power.power] = power;
                    }
                }

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            // Notify view to draw
            WorldUpdatedHandler?.Invoke();
        }

        /// <summary>
        /// Returns theWorld
        /// </summary>
        public World GetWorld()
        {
            return theWorld;
        }

        /// <summary>
        /// Returns playerID
        /// </summary>
        public int GetPlayerID()
        {
            return playerID;
        }
    }
}