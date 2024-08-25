#include "framemanager.h"

FrameManager::FrameManager()
{
}

FrameManager::FrameManager(int canvasSize) : canvasSize(canvasSize)
{
    // Set the actively changing frame to appropriate size
    activelyChangingFrame = new QImage(canvasSize, canvasSize, QImage::Format_RGBA64);
    // Make sure the created frame is clear
    activelyChangingFrame->fill(*(new QColor(0, 0, 0, 0)));

    // Set the default frame to 0 and the number of frame is 1
    selectedFrame = 0;
    frameCount = 1;

    // Add the current frame to the frame's state list
    currentFrameVersions.push_back(*activelyChangingFrame);

    // Add the redo stack corresponding to the frame number
    QStack<QImage> stack;
    redoStackList.push_back(stack);

    // Add the frame's state list to the frame list
    frameList.push_back(currentFrameVersions);
    saveImageState();
}

void FrameManager::beginDrawingStroke(QPoint mouseLocation)
{
    // Convert mouse location to pixel location
    QPoint pixelLocation = getPixelPosition(mouseLocation);
    lastPoint.setX(pixelLocation.x());
    lastPoint.setY(pixelLocation.y());
}

void FrameManager::continueDrawingStroke(QPoint mouseLocation, QColor color, int brushSize)
{
    QPoint pixelLocation = getPixelPosition(mouseLocation);
    QPainter painter(activelyChangingFrame);

    QPen pen;

    // If the current tool is an eraser
    if (color.alpha() == 0)
        painter.setCompositionMode(QPainter::CompositionMode_Clear);

    // Set the width, color and pen to appropriate settings
    pen.setWidth(brushSize);
    pen.setColor(color);
    painter.setPen(pen);
    painter.drawLine(lastPoint, pixelLocation);
    lastPoint = pixelLocation;
}

void FrameManager::endDrawingStroke()
{
    // Clear the redoStack,
    // to prevent unwanted redo when a new stroke is added
    redoStackList[selectedFrame].clear();
    // Save the image state
    saveImageState();
}

QPoint FrameManager::getPixelPosition(QPoint mouseLocation)
{
    // Convert x, y to appropriate location on frame
    int x = (mouseLocation.x() * canvasSize) / 600;
    int y = (mouseLocation.y() * canvasSize) / 600;
    QPoint pixelLocation(x, y);
    return pixelLocation;
}

void FrameManager::setSelectedFrame(int frameNum)
{
    selectedFrame = frameNum;

    // Delete the old existed image linked to activelyChangingFrame,
    // then assign it to another one from the frame list
    delete activelyChangingFrame;
    activelyChangingFrame = new QImage(frameList[selectedFrame].last());
}

void FrameManager::addFrame()
{
    selectedFrame = selectedFrame + 1;

    // Delete the old existed image linked to activelyChangingFrame,
    // then assign it to the new added frame to the frame list
    delete activelyChangingFrame;
    activelyChangingFrame = new QImage(canvasSize, canvasSize, QImage::Format_RGBA64);

    // Make sure the new frame is clear
    activelyChangingFrame->fill(*(new QColor(0, 0, 0, 0)));

    // Add the state and frame to frameList
    QVector<QImage> newStrokes;
    newStrokes.push_back(*activelyChangingFrame);
    frameList.insert(selectedFrame,newStrokes);

    // Add the redo stack corresponding to the frame number
    QStack<QImage> stack;
    redoStackList.insert(selectedFrame, stack);

    // Save the current image state
    saveImageState();
}

void FrameManager::removeFrame()
{
    // Remove selected frame from the frameList
    frameList.remove(selectedFrame);
    if (selectedFrame != 0)
        selectedFrame--;
}

void FrameManager::saveImageState()
{
    // Save image state
    QImage newInstance = *activelyChangingFrame;
    frameList[selectedFrame].append(newInstance);
}

void FrameManager::undoCurrentFrame()
{
    // Prevent calling undo on an empty frame
    if (frameList[selectedFrame].size() < 3)
        return;

    // Push the current state to the redoStack
    redoStackList[selectedFrame].push(frameList[selectedFrame].last());
    frameList[selectedFrame].pop_back();
    delete activelyChangingFrame;

    // Assign the activelyChangingFrame to the previous state
    activelyChangingFrame = new QImage(frameList[selectedFrame].last());
}

void FrameManager::redoCurrentFrame()
{
    // If the redoStack is empty, we don't redo
    if (redoStackList[selectedFrame].size() <= 0)
        return;

    // Push the state from last undo command back to frameList
    frameList[selectedFrame].push_back(redoStackList[selectedFrame].pop());
    delete activelyChangingFrame;

    // Assign the activelyChaningFrame to the state after redo
    activelyChangingFrame = new QImage(frameList[selectedFrame].last());
}
