#include "drawingtool.h"

DrawingTool::DrawingTool()
{

    color = new QColor(0, 0, 0, 255);
    eraser = new QColor(Qt::transparent);

    activeTool = color;
    size = 1;
}

void DrawingTool::setColor(QColor newColor)
{
    // Set color to new color
    color = new QColor(newColor);
    activeTool = color;
}

void DrawingTool::setSize(int newSize)
{
    // Set size to desired size
    size = newSize;
}

QColor DrawingTool::getActiveTool() {
    // Return the current color
    QColor toReturn = *activeTool;
    return toReturn;
}

void DrawingTool::setToEraser()
{
    activeTool = eraser;
}

void DrawingTool::setToBrush()
{
    activeTool = color;
}

int DrawingTool::getSize()
{
    return size;
}

DrawingTool::~DrawingTool()
{
    delete color;
    delete eraser;
}
