#ifndef FRAMEMANAGER_H
#define FRAMEMANAGER_H

#include <QImage>
#include <QDebug>
#include <QPoint>
#include <QPainter>
#include <QPen>
#include <QStack>

/**
 * @brief The FrameManager class stores all the frames of a SpriteEditor
 * project. It managers the editing, adding, and deleting of frames.
 */
class FrameManager
{
public:
    /**
     * @brief The FrameMangaer constructor.
     */
    FrameManager();

    /**
     * @brief The FrameManager constructor that takes in a
     * custom canvas size.
     * @param canvasSize The size of width and height of the canvas as an
     * integer.
     */
    FrameManager(int canvasSize);

    /**
     * @brief Set the selected frame to the frame the user has selected.
     * @param frameNum An integer representing the user's selected frame.
     */
    void setSelectedFrame(int frameNum);

    /**
     * @brief Begin to draw or erase at the appropriate pixel location.
     * This method must be followed by continueDrawingStroke and endDrawingStroke.
     * @param mouseLocation The mouse location as a QPoint.
     */
    void beginDrawingStroke(QPoint mouseLocation);

    /**
     * @brief Continue to draw or erase at the appropriate pixel location.
     * This method must be preceded by beginDrawingStroke and followed by endDrawingStroke.
     * @param mouseLocation the location of the cursor
     * @param color the current color of the tool
     * @param brushSize the size of the brush
     */
    void continueDrawingStroke(QPoint mouseLocation, QColor color, int brushSize);

    /**
     * @brief Handle when the user release the mouse press
     */
    void endDrawingStroke();

    /**
     * @brief Add another frame to the frame list,
     * after the currently selected frame
     */
    void addFrame();

    /**
     * @brief Remove the currently selected frame from the frame list
     */
    void removeFrame();

    /**
     * @brief Undo the stroke in the currently selected frame
     */
    void undoCurrentFrame();

    /**
     * @brief Redo the stroke in the currently selected frame
     */
    void redoCurrentFrame();

    /**
     * @brief Save the actively changing frame to the vector representing
     * it's version history.
     */
    void saveImageState();

    /**
     * @brief Given a mouse location, return the corresponding pixel position.
     * @param mouseLocation The locatin of the mouse as a QPoint.
     * @return The corresponding pixel location as a QPoint.
     */
    QPoint getPixelPosition(QPoint mouseLocation);

    QVector<QImage> currentFrameVersions;               // The current frame that the user is working on and its version history.
    QImage* activelyChangingFrame;                      // The frame that mouse events is affecting.
    QPoint lastPoint;                                   // The last pixel point that the mouse had drawn on.
    QVector<QVector<QImage>> frameList;                 // Holds all the frames in the project as well as each frame's history.
    int frameCount;                                     // The number of frames in the project.
    int selectedFrame;                                  // The current frame that the user has selected.

private:
    QVector<QStack<QImage>> redoStackList;              // A list of stacks to keep track of current frame states after undo is triggered

    int canvasSize;                                     // The width and height of the canvas.
};

#endif // FRAMEMANAGER_H
