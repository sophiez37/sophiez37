Snake Game

SERVER

We didn't have time to implement any special features to out server.

The "settings.xml" file contains basic data items, including UniverseSize, MSPerFrame, RespawnRate, and Walls.
Changing the content of each items in the settings file will cause changes to the server.

TO CONNECT TO A SERVER

First, enter server IP address and player's name. Then click connect.
In case you don't enter any server IP address, the default server will be local host.
In case you don't enter any player's name, the default name will be "player".
After the connecting to a server successfully, the connect button will be disabled.

In case there is an error occurred with the connection to the server, an error message will be displayed
and the connect button will be enabled again so user can try to reconnect.

SNAKE'S DEATH

A snake dies when it collides with a wall or a snake's body.
When the snake dies, it's body will be drawn in black color with a white spine in the middle for one frame
and an explosion image will be drawn at the last position of its head for 10,000,000 ticks.

IMPLEMENTED BY

Phuong Anh Nguyen and Alimkhan Zhanaladin
Last updated: November 27 2023
