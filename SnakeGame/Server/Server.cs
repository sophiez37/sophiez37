using System;
using System.Collections.Generic;
using NetworkUtil;
using System.Text.RegularExpressions;
using Model;
using SnakeGame;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;

namespace Server;
/// <summary>
/// The server maintains the state of the world, computes all game mechanics,
/// and communicates to the clients what is going on
/// </summary>
class Server
{
    // A map of clients that are connected, each with an ID
    private Dictionary<long, SocketState> clients;

    // Instance of the world
    private World theWorld;

    // GameSettings object containing information from settings.xml file
    private GameSettings settings;

    static void Main(string[] args)
    {
        Server server = new Server();
        server.StartServer();

        // Update the world per frame
        Stopwatch watch = new Stopwatch();
        while (true)
        {
            watch.Start();
            while (watch.ElapsedMilliseconds < server.settings.MSPerFrame) { }
            watch.Restart();
            server.UpdateWorld();
        }
    }

    /// <summary>
    /// Updates the world state by updating snakes' positions,
    /// powerups and sends all of those information to all clients
    /// </summary>
    private void UpdateWorld()
    {
        lock (theWorld)
        {
            UpdateSnakes();
            UpdatePowerups();
            SendWorld();
        }
    }

    /// <summary>
    /// Updates the positions of all snakes if they are still connected
    /// and removes if they are disconnected
    /// </summary>
    private void UpdateSnakes()
    {
        lock (theWorld)
        {
            foreach (Snake snake in theWorld.Snakes.Values)
            {
                // If the snake is dead then count the coolDown time
                // until it's time to generate a new snake
                if (!snake.dc && snake.died && snake.coolDown < settings.RespawnRate)
                {
                    snake.coolDown++;
                }

                else if (!snake.dc && snake.died && snake.coolDown >= settings.RespawnRate)
                {
                    theWorld.Snakes[snake.snake] = theWorld.generateRandomSnake(snake.snake, snake.name);
                    snake.coolDown = 0;
                    snake.alive = true;
                    snake.died = false;
                }

                // If the snake is alive then update its movement and checks for all collisions in the world
                else if (!snake.dc && snake.alive)
                {
                    theWorld.checkSnakeEatPowerup(snake);
                    snake.update(settings.UniverseSize);
                    theWorld.checkCollision(snake);
                }

                // If the client is disconnected then remove the client from the clients dictionary and
                // remove the snake
                else if (snake.dc)
                {
                    if (snake.frameAfterDisconnect >= 1)
                    {
                        int id = snake.snake;
                        theWorld.Snakes.Remove(snake.snake);
                        lock (clients) { clients.Remove(snake.snake); }
                        Console.WriteLine("Client " + id + " disconnected!");
                    }

                    snake.frameAfterDisconnect++;
                    continue;
                }

                else
                    snake.died = false;
            }
        }
    }

    /// <summary>
    /// Generates random powerups in the world
    /// </summary>
    private void UpdatePowerups()
    {
        lock (theWorld)
        {
            theWorld.generatePowerups();
        }
    }

    /// <summary>
    /// Send new state of snakes and powerups to clients
    /// </summary>
    private void SendWorld()
    {
        StringBuilder message = new StringBuilder();
        lock (theWorld)
        {
            foreach (Snake snake in theWorld.Snakes.Values)
                message.Append(JsonSerializer.Serialize(snake) + "\n");

            foreach (Powerup pow in theWorld.Powerups.Values)
                message.Append(JsonSerializer.Serialize(pow) + "\n");
        }

        lock (clients)
        {
            foreach (SocketState client in clients.Values)
                Networking.Send(client.TheSocket, message.ToString());
        }
    }

    /// <summary>
    /// Initialized the server's state
    /// </summary>
    public Server()
    {
        // Construct the GameSettings object from settings.xml file
        DataContractSerializer ser = new DataContractSerializer(typeof(GameSettings));
        string path = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
        path = Path.Combine(path, "settings.xml");
        XmlReader reader = XmlReader.Create(path);
        settings = (GameSettings)ser.ReadObject(reader)!;
        reader.Close();

        clients = new Dictionary<long, SocketState>();
        theWorld = new World(settings.UniverseSize);

        lock (theWorld)
        {
            foreach (Wall wall in settings.Walls)
            {
                if (!theWorld.Walls.ContainsKey(wall.wall))
                {
                    theWorld.Walls.Add(wall.wall, wall);
                }
            }
        }
    }

    /// <summary>
    /// Start accepting Tcp sockets connections from clients
    /// </summary>
    public void StartServer()
    {
        // This begins an "event loop"
        Networking.StartServer(NewClientConnected, 11000);

        Console.WriteLine("Server is running. Accepting clients.");
    }

    /// <summary>
    /// Method to be invoked by the networking library
    /// when a new client connects
    /// </summary>
    /// <param name="state">The SocketState representing the new client</param>
    private void NewClientConnected(SocketState state)
    {
        if (state.ErrorOccurred)
            return;
        Console.WriteLine("Client " + state.ID + " connected.");

        // change the state's network action to the 
        // receive handler so we can process data when something
        // happens on the network
        state.OnNetworkAction = ReceivePlayerName;

        Networking.GetData(state);
    }

    /// <summary>
    /// Receives the player's name and complete the handshake;
    /// send the player it's ID, the world's size, and all walls;
    /// create a snake representing the client
    /// </summary>
    /// <param name="state">The SocketState representing the new client</param>
    private void ReceivePlayerName(SocketState state)
    {
        if (state.ErrorOccurred)
            return;

        string totalData = state.GetData();

        string[] parts = Regex.Split(totalData, @"(?<=[\n])");

        string playerName = "player";
        if (parts[0] != null)
        {
            playerName = parts[0];
        }

        // Save the client state
        // Need to lock here because clients can disconnect at any time
        lock (clients)
        {
            clients[state.ID] = state;
        }

        // Send client's ID, world size and walls
        string firstMessage = state.ID.ToString() + "\n" + theWorld.size.ToString() + "\n";
        Networking.Send(state.TheSocket, firstMessage);

        lock (theWorld)
        {
            foreach (Wall wall in settings.Walls)
            {
                string wallString = JsonSerializer.Serialize(wall);
                Networking.Send(state.TheSocket, wallString + '\n');
            }
        }

        // Generate random snake for the new client
        Snake playerSnake = theWorld.generateRandomSnake((int)state.ID, playerName);
        lock (theWorld)
        {
            if (!theWorld.Snakes.ContainsKey((int)state.ID))
            {
                theWorld.Snakes.Add((int)state.ID, playerSnake);
            }
        }

        state.OnNetworkAction = ReceiveMessage;
        Networking.GetData(state);
    }

    /// <summary>
    /// Method to be invoked by the networking library
    /// when a network action occurs
    /// </summary>
    /// <param name="state"></param>
    private void ReceiveMessage(SocketState state)
    {
        // Remove the client if they aren't still connected
        if (state.ErrorOccurred)
        {
            RemoveClient(state.ID);
            return;
        }

        ProcessMessage(state);
        // Continue the event loop that receives messages from this client
        Networking.GetData(state);
    }


    /// <summary>
    /// Given the data that has arrived so far, 
    /// potentially from multiple receive operations, 
    /// determine if we have enough to make a complete message,
    /// and process it (change the direction of the snake according to client's request).
    /// </summary>
    /// <param name="sender">The SocketState that represents the client</param>
    private void ProcessMessage(SocketState state)
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

            lock (theWorld)
            {
                Snake snake = theWorld.Snakes[(int)state.ID];
                Vector2D? dir = null;
                if (p.Equals("{\"moving\":\"up\"}\n"))
                {
                    dir = new Vector2D(0, -1);
                }
                else if (p.Equals("{\"moving\":\"down\"}\n"))
                {
                    dir = new Vector2D(0, 1);
                }
                else if (p.Equals("{\"moving\":\"left\"}\n"))
                {
                    dir = new Vector2D(-1, 0);
                }
                else if (p.Equals("{\"moving\":\"right\"}\n"))
                {
                    dir = new Vector2D(1, 0);
                }

                if (dir != null)
                {
                    if (!dir.Equals(snake.dir) && (dir + snake.dir).Length() != 0)
                    {
                        snake.changeDir = true;
                        snake.dir = dir;
                    }
                }
            }

            // Remove it from the SocketState's growable buffer
            state.RemoveData(0, p.Length);
        }
    }

    /// <summary>
    /// Removes a client from the clients dictionary
    /// </summary>
    /// <param name="id">The ID of the client</param>
    private void RemoveClient(long id)
    {
        Console.WriteLine("Client " + id + " disconnected");
        lock (clients)
        {
            clients.Remove(id);
        }
    }
}