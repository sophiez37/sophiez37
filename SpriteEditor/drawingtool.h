#ifndef DRAWINGTOOL_H
#define DRAWINGTOOL_H

#include <QColor>
#include <QPoint>

/**
 * @brief The DrawingTool class keeps track of the user's
 * selected drawing tool (brush or eraser) as well as its
 * size and color.
 */
class DrawingTool
{
public:
    /**
     * @brief The DrawingTool constructor.
     */
    DrawingTool();

    /**
     * @brief Given a color, set the current color to that color.
     * @param newColor The new color as a QColor.
     */
    void setColor(QColor newColor);

    /**
     * @brief Given a size, set the current size to that size.
     * @param newSize The new size as an integer.
     */
    void setSize(int newSize);

    /**
     * @brief Get the active tool represented as a QColor.
     * @return a QColor representing a brush with color with an eraser.
     */
    QColor getActiveTool();

    /**
     * @brief Get the current size of the current tool.
     * @return the size as an integer.
     */
    int getSize();

    /**
     * @brief Set the active tool to the brush tool.
     */
    void setToBrush();

    /**
     * @brief Set the active tool to the eraser tool.
     */
    void setToEraser();

    /**
     * @brief The destructor for DrawingTool.
     */
    ~DrawingTool();

private:
    int size;            // The size of the tool.
    QColor* color;       // The color of the brush.
    QColor* eraser;      // The color to be used as an eraser.
    QColor* activeTool;  // The current tool represented as a QColor.
};

#endif // DRAWINGTOOL_H
