#ifndef MAINMODEL_H
#define MAINMODEL_H

#include <QObject>
#include "pet.h"
#include "cat.h"
#include "dog.h"
#include <string>
#include <QString>
#include <QPoint>
#include <Box2D/Box2D.h>
#include <QTimer>
#include <QDebug>

using std::string;

/**
 * @brief The MainModel class, handling events occur in the app, updating element values
 * of view and send updates to MainWindow to display it to UI
 */
class MainModel : public QObject
{
    Q_OBJECT

private:
    QTimer gameTimer;    // The game timer
    QTimer timer;        // Timer for decay
    int gameTime = 360;  // Start game at 6 am.
    int decayWaitTime;   // Decay time

    bool decayAllowed = true;  // Set decay allowance, default true
    bool petNotSleep = true;   // Check if pet not sleeping, default true

    QString currentFilePath;  // Current file path
    b2World world;            // box2D world
    b2Body* body;             // box2D body
    const int ppm = 20;       // Setup ppm
    void decayAllBars();      // Set decay to all progress bars
    void setBox2dWorld();     // Set box2D world
    void toJSON();            // Convert current game state to JSON when save

public:
    /**
     * @brief MainModel constructor
     * @param name pet name
     * @param age pet age
     * @param weight pet weight
     * @param animalType pet type
     */
    MainModel(string name, int age , double weight, string animalType);

    Pet* pet;  // The pet
    enum Actions {Default, Food, Water, Bath, Bathroom, Play, Rest}; // Activities
    enum ProgressBars{Hunger, Thirst, Hygiene, Potty, Energy}; // Progress bars relative to activities
    Actions selectedAction; // The selected action

    void initializeProgressBars();          // Initialize the progress bar
    void loadFileProgressbars(QJsonArray);  // Load progress bar data from file
    void setCurrentFilePath(QString);       // Set the current file path
    void setGameTime(int);                  // Set the current game time
    bool feedingAllowed = true;             // Check feeding allowed, default true
    bool wateringAllowed = true;            // Check watering allowed, default true

signals:
    void setUpProgressBars(QVector<int>);          // Emit signals to setup the progress bar
    void updateProgressBar(double, ProgressBars);  // Emit signal to update progress bar
    void setCurrentModule(QString, Actions);       // Emit signal to set the current activity
    void progressBarComplete(string);              // Signal to tell a progress bar reached 100%

    void updateHygiene();                          // Emit to update to appropriate hygiene cursor
    void updateFood();                             // Emit to update to food cursor
    void updateWater();                            // Emit to update to water cursor

    void resetCursor();                            // Emit to update to default cursor
    void newGameTime(int, string);                 // Emit to set new game time

    void newBallAngle(double);     // Set new ball angle
    void newBallPos(int, int);     // Set position of new ball
    void newBallPosMap(int, int);  // Set position map of new ball

public slots:
    void getActionSelection(int);  // Get the currently selected action

    void foodButtonClicked();      // Handler when food button is clicked
    void waterButtonClicked();     // Handler when water button is clicked
    void bathButtonClicked();      // Handler when bath button is clicked
    void pottyButtonClicked();     // Handler when potty button is clicked
    void restButtonClicked();      // Handler when rest button is clicked
    void playButtonClicked();      // Handler when play button is clicked

    void updateGameTime();          // Handler to update the game time
    void stopGameTimer();           // Handler to stop the game time
    void startGameTimer();          // Handler to start the game time
    void stopTimer();               // Handler to stop the decay timer

    void produceNewBall(int, int);  // Produce a new ball
    void updateBall();              // Update the ball state (location, angle, etc.)


    void setMediumSpeed();  // Set the game to medium speed
    void setFastSpeed();    // Set the game to fast speed
    void setFasterSpeed();  // Set the game to faster speed

    void stopAllDecay();    // Stop the progress bars decay
    void resumeAllDecay();  // Resume the progress bars decay

    void petIsSleeping();     // Set pet to sleep
    void petIsNotSleeping();  // Set pet to not sleep
};

#endif // MAINMODEL_H
