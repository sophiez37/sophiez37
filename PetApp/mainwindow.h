#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include "mainmodel.h"
#include <QString>
#include <QVector>
#include <QPixmap>
#include <QGraphicsScene>
#include <QGraphicsPixmapItem>
#include "petAnimation.h"
#include <QMessageBox>
#include <QMediaPlayer>
#include <QAudioOutput>
#include <QUrl>

/**
 * @brief The MainWindow class is the main interface for the PetApp. It serves as a
 * landing point for user events.
 */

QT_BEGIN_NAMESPACE
namespace Ui {
class MainWindow;
}
QT_END_NAMESPACE

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    /**
     * @brief MainWindow constructor
     * @param model the model of the app
     * @param name name of the pet
     * @param age pet age
     * @param weight pet weight
     * @param animal cat or dog
     * @param parent the QWidget parent
     */
    MainWindow(MainModel& model, string name, int age, double weight, string animal, QWidget *parent = nullptr);
    ~MainWindow();

private:
    Ui::MainWindow *ui;               // The ui
    MainModel& model;                 // The model

    QGraphicsScene *scene;            // Storing the pet and ball of the room/activity
    QGraphicsPixmapItem *petObject;   // The pet
    QGraphicsPixmapItem *ballObject;  // The ball in play activity
    QGraphicsPixmapItem* background;  // Background of the room/activity

    PetAnimation animation;           // Animation of the pet
    string currentAnimal;             // Current pet: dog/cat

    bool ballOnScreen;                // Check if the ball is on screen
    bool touchedBallOnce = false;     // Check if the ball is touched once

    int actionIndex;                  // The current action
    int petXPosition=100;             // The X position of the pet
    bool animationNeedFLip = false;   // Check if pet need to turn around
    bool isMovingToNextRoom = false;  // Check if pet is going to the next room
    bool inNewRoom;                   // Check if pet is in the new room

    MainModel::Actions currentActivity = MainModel::Food; // Current activity of the game, default is food

    QMessageBox congratulationsMessageBox;     // The congratulation when finishing a module
    QMessageBox notificationMessageBox;        // Notify when its time to feed the pet

    QMediaPlayer* backgroundMusicSound;        // Background music sound
    QMediaPlayer* purrEffect;                  // Purr effect sound
    QMediaPlayer* brushEffect;                 // Brush effect sound
    QMediaPlayer* clickSoundEffect;            // Click effect sound
    QMediaPlayer* foodSoundEffect;             // Eating sound
    QMediaPlayer* waterSoundEffect;            // Drinking sound
    QMediaPlayer* congratulationsSoundEffect;  // Congratulation sound
    QMediaPlayer* fartSoundEffect;             // Fart sound

    QAudioOutput* backgroundMusicOutput;       // Background music sound output
    QAudioOutput* purrOutput;                  // Purr effect sound output
    QAudioOutput* brushOutput;                 // Brush effect sound output
    QAudioOutput* waterAudioOutput;            // Water sound output
    QAudioOutput* clickAudioOutput;            // Clicking sound output
    QAudioOutput* foodAudioOutput;             // Food sound output
    QAudioOutput* congratulationsAudioOutput;  // Congratulation sound output
    QAudioOutput* fartAudioOutput;             // Fart sound output

    void movingToNextRoom(MainModel::Actions action);  // Pet moving to the next room
    void disableAllButton();                           // Disable all button
    void reEnableAllButton();                          // Re-enable all button except current activity

public slots:
    void panelPressed(QPoint);                                // Handling when the main panel is pressed
    void panelReleased(QPoint);                               // Handling when the main panel is released from pressing

    void playPetAnimation(string pet);                        // Play the pet animation
    void displayInitialProgressBars(QVector<int>);            // Set up the initial state of progress bars
    void updateProgressBar(double, MainModel::ProgressBars);  // Update the progress bar
    void setCurrentModule(QString, MainModel::Actions);       // Set current action module
    void getNewXPosition();

    void setBallInvisible();           // Set the ball invisible
    void setBallVisible();             // Set ball to visible

    void showCongratulations(string);  // Show the congratulation box

    void playClickSound();             // Play click sound
    void playFoodSound();              // Play food sound

    void setHygieneDisplay();          // Set the cursor to hygiene
    void setFoodDisplay();             // Set the cursor to food
    void setWaterDisplay();            // Set the cursor to water
    void resetCursorDisplay();         // Reset the cursor display

    void replayBackgroundMusic(QMediaPlayer::MediaStatus);  // Replay the background music
    void displayGameTime(int, string);                      // Display the current game time

    void mediumSpeedClicked();       // Handler when medium time speed button clicked
    void fastSpeedClicked();         // Handler when fast time speed button clicked
    void fasterSpeedClicked();       // Handler when fater time speed button clicked

    void updateBallPos(int,int);     // Update the ball location
    void updateBallAngle(double);    // Update the ball angle
    void updateBallPosMap(int,int);  // Update the position map of ball

    void checkPetMovingOut(QTimer* timer, MainModel::Actions);  // Check if pet moved out of the current scene
    void checkPetMovingIn(QTimer* timer, MainModel::Actions);   // Check if pet moved in to the new scene

    void newPet();   // Initialize new pet
    void loadPet();  // Load the current pet

    void foodButtonClicked();   // Handler for food button clicked
    void waterButtonClicked();  // Handler for water button clicked
    void bathButtonClicked();   // Handler for bath button clicked
    void pottyButtonClicked();  // Handler for potty button clicked
    void restButtonClicked();   // Handler for rest button clicked
    void playButtonClicked();   // Handler for play button clicked

signals:
    void newClickBallPos(int,int);  // Emit the position where we create the ball
    void ballOutOfFrame();          // Emit signal to tell ball out of frame
    void ballInFrame();             // Emit signal to tell ball in frame

    void interactingWithPet();      // Emit signal to tell interaction with pet begin
    void stopInteractingWithPet();  // Emit signal to tell interaction with pet stop

    void petIsSleeping();           // Emit signal to tell pet is sleeping
    void petIsNotSleeping();        // Emit signal to tell pet is not sleeping

    void stopTimer();               // Emit signal to stop the timer (decaying progress bar)
    void stopGameTimer();           // Emit signal to stop game timer (whole game timer)

};
#endif // MAINWINDOW_H
