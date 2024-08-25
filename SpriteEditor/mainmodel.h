#ifndef MAINMODEL_H
#define MAINMODEL_H

#include <QPoint>
#include "drawingtool.h"
#include "framemanager.h"
#include <QObject>
#include <QFile>
#include <QTimer>

/**
 * @brief The MainModel class facilitates communicatio between the UI classes (Canvas, MainWindow, SizeDialogue)
 * and the model classes (FrameManger,DrawingTool). It responds to button presses and mouse events by delegating
 * the tasks off to the other model classes.
 */
class MainModel : public QObject
{
    Q_OBJECT
public:
    /**
     * @brief MainModel constructor
     * @param parent the parent object
     * @param size (type int) the canvas size from user
     */
    explicit MainModel(QObject *parent, int);

    /**
     * @brief draw the pixel at the given mouse position
     * @param position the position of the mouse
     */
    void drawPixel(QPoint position);

    /**
     * @brief Get the current frame manager
     * @return the frame manager
     */
    FrameManager getFrameManager();

    /**
     * @brief Get the currently selected frame
     * @return the frame number of the currently selected frame
     */
    int getSelectedFrame();

public slots:
    // Drawing Actions
    void beginDrawingStroke(QPoint);     // Initiates a drawing stroke
    void continueDrawingStroke(QPoint);  // Continues the drawing stroke
    void endDrawingStroke();             // Ends the current drawing stroke

    // Tool Settings
    void setToBrush();                   // Sets the current tool to the brush
    void setToEraser();                  // Sets the current tool to the eraser
    void setColor(QColor);               // Sets the current color

    // Brush Size Settings
    void setBrushSizeToOne();            // Sets the brush size to smallest
    void setBrushSizeToTwo();            // Sets the brush size to medium
    void setBrushSizeToThree();          // Sets the brush size to larger

    // File Operations
    void getOpenFile(QString);           // Opens and loads the selected file
    void saveToFile();                   // Saves the current drawing or animation to the current file path
    void saveFileAs(QString);            // Save the current content under a new file path
    void newFile();                      // Start a new file
    void toJSON();                       // Exports the current drawing
    void readJSON();                     // Imports drawing

    // Animation Controls
    void setImageArray();                // Sets or updates the array holding the frame images
    void setDisplayedFrame();            // Updates the display
    void playAnimation();                // Starts playing the animation
    void pauseAnimation();               // Pauses the animation
    void setFps(int);                    // Sets the fps
    void sendFrameCount(int);            // Sends the current frame count

    // Undo/Redo functions
    void undo();                         // Process the undo command
    void redo();                         // Process the redo command

    // Frame Management
    void clearData();                    // Clears the drawing
    void addNewFrame();                  // Adds a new frame
    void displaySelectedFrame(int);      // Displays the selected frame
    void removeFrame();                  // Removes the currently selected frame

signals:
    void imageChanged(QImage *);         // Emitted on image change
    void getFileName();                  // Requests file name
    void displayedFrameChanged(QImage);  // Indicates frame display change
    void buttonToRemove(int);            // Requests removal of a button by index
    void loadFileFrame();                // Signal to load the frame file
    void previewImageChanged(QImage, int);       // Signals drawing stroke end
    void sendUpdateFrameIcons(QVector<QImage>);  // Updates UI frame icons
    void resetComponents();                 // Signal to reset all UI components

private:
    FrameManager *frameManager;          // Manages animation frames
    DrawingTool *tool;                   // Current drawing tool
    QString currentFilePath;             // Path of the current file
    bool isNewFile;                      // Indicates if the file is new
    int framesPerSecond;                 // Animation FPS
    int canvasSize;                      // Drawing canvas size
    QVector<QImage> animatedFrames;      // Stores animation frames
    QVector<QImage>::Iterator frameItr;  // Iterates through frames
    QTimer *timer;                       // Controls animation playback
    bool animationPlaying;               // Flag for animation playback state
};

#endif // MAINMODEL_H
