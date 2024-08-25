#include "mainmodel.h"
#include <QFileDialog>
#include <QJsonObject>
#include <QJsonArray>
#include <QJsonDocument>
#include <QBuffer>
#include <QVectorIterator>

MainModel::MainModel(QObject* parent, int size) : QObject(parent)
{
    // Initialize the drawing tool, and frame manager
    isNewFile = true;
    tool = new DrawingTool();
    canvasSize = size;
    frameManager = new FrameManager(canvasSize);

    // Set fps and animation playing to default values
    framesPerSecond = 2;
    animationPlaying = false;
}

void MainModel::setImageArray()
{
    QVector<QImage> prevStrokes;
    QVector<QImage> frames;
    for (auto itr = frameManager->frameList.begin(); itr != frameManager->frameList.end(); itr++)
    {
        prevStrokes = *itr;
        frames.push_back(prevStrokes.back());
    }
    animatedFrames = frames;
}

void MainModel::sendFrameCount(int size)
{
    frameManager->frameCount = size;
}

int MainModel::getSelectedFrame()
{
    return frameManager->selectedFrame;
}

void MainModel::addNewFrame()
{
    frameManager->addFrame();
    emit MainModel::imageChanged(frameManager->activelyChangingFrame);

    // Update the frame preview list
    setImageArray();
    emit sendUpdateFrameIcons(animatedFrames);

    // Update animation preview if it's playing
    if (animationPlaying)
    {
        pauseAnimation();
        playAnimation();
    }
}

void MainModel::displaySelectedFrame(int selected)
{
    frameManager->setSelectedFrame(selected);
    emit imageChanged(frameManager->activelyChangingFrame);
}

void MainModel::removeFrame()
{
    // Call frameManager to remove frame
    emit buttonToRemove(getSelectedFrame());
    frameManager->removeFrame();
    displaySelectedFrame(getSelectedFrame());

    // Update animation preview if it's playing
    if (animationPlaying)
    {
        pauseAnimation();
        playAnimation();
    }
}

FrameManager MainModel::getFrameManager()
{
    return *frameManager;
}

void MainModel::beginDrawingStroke(QPoint mouseLocation)
{
    frameManager->beginDrawingStroke(mouseLocation);
}

void MainModel::continueDrawingStroke(QPoint mouseLocation)
{
    // Call frameManager to do the drawing, and signal the view about the change
    frameManager->continueDrawingStroke(mouseLocation, tool->getActiveTool(), tool->getSize());
    emit imageChanged(frameManager->activelyChangingFrame);
}

void MainModel::endDrawingStroke()
{
    frameManager->endDrawingStroke();

    // Update animation preview if it's playing
    if (animationPlaying)
    {
        pauseAnimation();
        playAnimation();
    }

    emit previewImageChanged(*frameManager->activelyChangingFrame, frameManager->selectedFrame);
}

void MainModel::setToBrush()
{
    tool->setToBrush();
}

void MainModel::setToEraser()
{
    tool->setToEraser();
}

// Set the tool to appropriate color
void MainModel::setColor(QColor color)
{
    tool->setColor(color);
}

// Set brush size to user's chosen sizes
void MainModel::setBrushSizeToOne()
{
    tool->setSize(1);
}
void MainModel::setBrushSizeToTwo()
{
    tool->setSize(2);
}
void MainModel::setBrushSizeToThree()
{
    tool->setSize(3);
}

void MainModel::undo()
{
    // Call undo from frameManager
    frameManager->undoCurrentFrame();

    // Signal the view that the frame state changed
    emit imageChanged(frameManager->activelyChangingFrame);
    emit previewImageChanged(*frameManager->activelyChangingFrame, frameManager->selectedFrame);

    // Update animation preview if it's playing
    if (animationPlaying)
    {
        pauseAnimation();
        playAnimation();
    }
}

void MainModel::redo()
{
    // Call redo from frameManager
    frameManager->redoCurrentFrame();

    // Signal the view that the frame state changed
    emit imageChanged(frameManager->activelyChangingFrame);
    emit previewImageChanged(*frameManager->activelyChangingFrame, frameManager->selectedFrame);

    // Update animation preview if it's playing
    if (animationPlaying)
    {
        pauseAnimation();
        playAnimation();
    }
}

void MainModel::playAnimation()
{
    // Display the animation
    animationPlaying = true;
    setImageArray();
    timer = new QTimer(this);
    frameItr = animatedFrames.begin();
    connect(timer, &QTimer::timeout, this, &MainModel::setDisplayedFrame);
    timer->start(double(1000) / framesPerSecond);
}

void MainModel::setDisplayedFrame()
{
    timer->stop();

    if (frameItr == animatedFrames.end())
        frameItr = animatedFrames.begin();

    emit displayedFrameChanged(*frameItr);
    frameItr++;

    // Play based on the user's chosen fps
    timer->start(double(1000) / framesPerSecond);
}

void MainModel::pauseAnimation()
{
    animationPlaying = false;
    timer->stop();  // Stop the timer to stop the playing
}

void MainModel::setFps(int fps)
{
    framesPerSecond = fps;

    // Update animation preview if it's playing
    if (animationPlaying)
    {
        pauseAnimation();
        playAnimation();
    }
}

void MainModel::clearData()
{
    // Clear the old panel, create a new one
    for (int i = frameManager->frameCount-1; i > 0; i--)
        emit buttonToRemove(i);

    int currentSize = canvasSize;
    delete frameManager;
    delete tool;
    tool = new DrawingTool();

    if (animationPlaying)
        pauseAnimation();

    frameManager = new FrameManager(currentSize);
    currentFilePath = "";
    framesPerSecond = 2;
    animatedFrames.clear();
    animationPlaying = false;
}

// Import/Export
void MainModel::newFile()
{
    isNewFile = true;
    delete frameManager->activelyChangingFrame;

    // Call frameManager to initialize new frame, and clear the old data
    frameManager->activelyChangingFrame = new QImage(canvasSize, canvasSize, QImage::Format_RGBA64);
    frameManager->activelyChangingFrame->fill(*(new QColor(0, 0, 0, 0)));

    emit previewImageChanged(*frameManager->activelyChangingFrame, 0);
    clearData();

    // Notice the view about the change
    emit imageChanged(frameManager->activelyChangingFrame);
    emit resetComponents();
}

void MainModel::getOpenFile(QString openedFile)
{
    newFile();
    currentFilePath = openedFile;
    readJSON();
    isNewFile = false;
}

void MainModel::saveToFile()
{
    // If that's a new file, ask the user to save it, else just save it
    if (isNewFile)
        emit getFileName();
    else
        toJSON();

    isNewFile = false;
}

void MainModel::saveFileAs(QString fileName)
{
    isNewFile = false;
    currentFilePath = fileName;
    toJSON();
}

void MainModel::toJSON()
{
    // Initialize Json properties
    QFile clearFile(currentFilePath);
    clearFile.resize(0);
    clearFile.close();
    QJsonArray jsonArray;
    QJsonObject jsonObject;
    QJsonObject jsonAnimationFrames;

    // Insert size of the canvas and current fps to the JSON
    jsonAnimationFrames.insert("size",canvasSize);
    jsonAnimationFrames.insert("fps",framesPerSecond);

    // Add the current frames to a temporary image array
    QJsonArray imageArray;
    setImageArray();
    animatedFrames.size();
    for (int i = 0; i  < animatedFrames.size(); i++)
    {
        QImage currentImage = animatedFrames.at(i);

        QByteArray byteArrayHolder;
        QBuffer buffer(&byteArrayHolder);
        buffer.open(QIODevice::WriteOnly);
        currentImage.save(&buffer, "PNG");
        QString imgData = byteArrayHolder.toBase64();
        if (!imgData.isEmpty())
            imageArray.push_back(imgData);
    }

    // Insert that image array to JSON
    jsonAnimationFrames.insert("frames",imageArray);

    // Insert the animation frames
    jsonObject.insert("Animation Frames", jsonAnimationFrames);
    jsonArray.append(jsonObject);

    // Setting up the JSON document
    QJsonDocument jsonDoc;
    jsonDoc.setArray(jsonArray);
    QString strJson(jsonDoc.toJson(QJsonDocument::Indented));
    QFile currentFile(currentFilePath);
    if (currentFile.open(QIODevice::ReadWrite))
    {
        QTextStream out(&currentFile);
        out << strJson;
    }
    currentFile.close();
}

void MainModel::readJSON()
{
    // Set the file path
    QString holdFilePath = currentFilePath;
    QString strJson;
    QFile currentFile(holdFilePath);
    if (currentFile.open(QIODevice::ReadOnly))
        strJson = currentFile.readAll();

    currentFile.close();
    QJsonDocument jsonDoc = QJsonDocument::fromJson(strJson.toUtf8());
    QJsonArray jsonArray = jsonDoc.array();
    QJsonObject mainObject = jsonArray.first().toObject();

    // Extract information about frame list, animation, size and fps
    QJsonObject animationFrameObject = mainObject.value(QString("Animation Frames")).toObject();
    QJsonArray frames = animationFrameObject.value("frames").toArray();
    QJsonValue loadCanvasSize = animationFrameObject.value("size");
    QJsonValue loadFps = animationFrameObject.value("fps");

    // Set the appropriate settings to the canvas
    canvasSize = loadCanvasSize.toInt();
    currentFilePath = holdFilePath;
    framesPerSecond = loadFps.toInt();
    for (int i = 0; i < frames.count(); i++)
    {
        QString data = frames.at(i).toString();
        QByteArray dataByteArray = data.toStdString().c_str();
        if (i != 0)
            emit loadFileFrame();

        frameManager->selectedFrame = i;
        delete frameManager->activelyChangingFrame;
        frameManager->activelyChangingFrame = new QImage(canvasSize, canvasSize, QImage::Format_RGBA64);
        frameManager->activelyChangingFrame->loadFromData(QByteArray::fromBase64(dataByteArray), "PNG");
        frameManager->saveImageState();
        emit previewImageChanged(*frameManager->activelyChangingFrame, i);
    }

    // Set default chosen frame when open is frame 0
    frameManager->selectedFrame = 0;
    delete frameManager->activelyChangingFrame;
    frameManager->activelyChangingFrame = new QImage(frameManager->frameList[frameManager->selectedFrame].last());
    frameManager->frameCount = frames.count();
    emit MainModel::imageChanged(frameManager->activelyChangingFrame);
}

