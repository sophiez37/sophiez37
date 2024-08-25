using Windows.Gaming.Input;
using Windows.Media.Devices.Core;
using Model;
using System.Diagnostics;

namespace SnakeGame;

public partial class MainPage : ContentPage
{
    private GameController controller = new GameController();
    public MainPage()
    {
        InitializeComponent();
        controller.ErrorOcurredHandler += NetworkErrorHandler;
        controller.ConnectedHandler += DisplayConnect;
        controller.PlayerUpdatedHandler += SetPlayer;
        controller.WorldUpdatedHandler += OnFrame;
        graphicsView.Invalidate();
        worldPanel.SetWorld(controller.GetWorld());
    }

    /// <summary>
    /// Send the player's name to the server upon connection
    /// </summary>
    private void DisplayConnect()
    {
        Dispatcher.Dispatch(() => {
            controller.Send(nameText.Text);
        });
    }

    /// <summary>
    /// Sets player when playerID updated
    /// </summary>
    private void SetPlayer()
    {
        Dispatcher.Dispatch(() => {
            worldPanel.SetPlayer(controller.GetPlayerID());
        });
    }

    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }

    /// <summary>
    /// Send movement commands
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();
        if (text == "w")
        {
            // Move up
            controller.movement = "{\"moving\":\"up\"}";
        }
        else if (text == "a")
        {
            // Move left
            controller.movement = "{\"moving\":\"left\"}";
        }
        else if (text == "s")
        {
            // Move down
            controller.movement = "{\"moving\":\"down\"}";
        }
        else if (text == "d")
        {
            // Move right
            controller.movement = "{\"moving\":\"right\"}";
        }
        entry.Text = "";
    }

    /// <summary>
    /// Display alert when there is a connection error happens
    /// and enable the connect button to allow user to connect again
    /// </summary>
    private void NetworkErrorHandler()
    {
        Dispatcher.Dispatch(() => DisplayAlert("Error", "Disconnected from server", "OK"));
        Dispatcher.Dispatch(() => { connectButton.IsEnabled = true; });
    }


    /// <summary>
    /// Event handler for the connect button
    /// We will put the connection attempt interface here in the view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        if (serverText.Text == "")
        {
            DisplayAlert("Error", "Please enter a server address", "OK");
            return;
        }
        if (nameText.Text == "")
        {
            DisplayAlert("Error", "Please enter a name", "OK");
            return;
        }
        if (nameText.Text.Length > 16)
        {
            DisplayAlert("Error", "Name must be less than 16 characters", "OK");
            return;
        }

        // Disable the controls and try to connect
        connectButton.IsEnabled = false;
        serverText.IsEnabled = false;
        lock (controller)
        {
            controller.Connect(serverText.Text);
        }

        keyboardHack.Focus();
    }

    /// <summary>
    /// Use this method as an event handler for when the controller has updated the world
    /// </summary>
    public void OnFrame()
    {
        Dispatcher.Dispatch(() => graphicsView.Invalidate());
    }

    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W:\t\t Move up\n" +
                     "A:\t\t Move left\n" +
                     "S:\t\t Move down\n" +
                     "D:\t\t Move right\n",
                     "OK");
    }

    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by Phuong Anh Nguyen and Alimkhan Zhanaladin\n" +
        "CS 3500 Fall 2023, University of Utah", "OK");
    }

    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (!connectButton.IsEnabled)
            keyboardHack.Focus();
    }
}