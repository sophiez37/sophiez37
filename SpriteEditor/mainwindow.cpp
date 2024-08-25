#include "mainwindow.h"
#include "ui_mainwindow.h"
#include <QFileDialog>
#include "mainmodel.h"
#include <QCursor>
#include <QPixmap>

MainWindow::MainWindow(MainModel& model, QWidget *parent)
    : QMainWindow(parent)
    ,ui(new Ui::MainWindow),model(model)
{
    ui->setupUi(this);

    // Drawing events on the canvas, connect from Canvas to MainWindow
    connect(ui->panel, &Canvas::mousePressed, this, &MainWindow::beginDraw);
    connect(ui->panel, &Canvas::mouseMoved, this, &MainWindow::continueDraw);
    connect(ui->panel, &Canvas::mouseReleased, this, &MainWindow::endDraw);

    // Drawing events on the canvas, connect from MainWindow to MainModel
    connect(this, &MainWindow::modelStartDraw, &model, &MainModel::beginDrawingStroke);
    connect(this, &MainWindow::modelContinueDraw, &model, &MainModel::continueDrawingStroke);
    connect(this, &MainWindow::modelEndDraw, &model, &MainModel::endDrawingStroke);

    // Update the preview/animation/canvas to appropriate state
    connect(&model, &MainModel::imageChanged, this, &MainWindow::updateImage);
    connect(&model, &MainModel::displayedFrameChanged, this, &MainWindow::updatePreview);
    connect(&model, &MainModel::previewImageChanged, this, &MainWindow::changeButtonIcon);

    // Animation preview buttons
    connect(ui->playButton, &QPushButton::clicked, &model, &MainModel::playAnimation);
    connect(ui->pauseButton, &QPushButton::clicked, &model, &MainModel::pauseAnimation);
    connect(ui->playButton, &QPushButton::clicked, this, [this] { ui->playButton->setEnabled(false);
                                                                  ui->pauseButton->setEnabled(true); });
    connect(ui->pauseButton, &QPushButton::clicked, this, [this] { ui->pauseButton->setEnabled(false);
                                                                  ui->playButton->setEnabled(true); });

    // Connect drawing tool (brush/eraser), and color to appropriate slots
    connect(ui->brush, &QPushButton::clicked, &model, &MainModel::setToBrush);
    connect(ui->eraser, &QPushButton::clicked, &model, &MainModel::setToEraser);
    connect(ui->brush, &QPushButton::clicked, this, &MainWindow::onBrushButtonClicked);
    connect(ui->eraser, &QPushButton::clicked, this, &MainWindow::onEraserButtonClicked);
    connect(ui->color, &QPushButton::clicked, this, &MainWindow::onColorButtonClicked);

    // Connect brush size buttons to appropriate slots
    connect(ui->size1, &QPushButton::clicked, this, &MainWindow::sizeOneButtonClicked);
    connect(ui->size2, &QPushButton::clicked, this, &MainWindow::sizeTwoButtonClicked);
    connect(ui->size3, &QPushButton::clicked, this, &MainWindow::sizeThreeButtonClicked);

    connect(ui->size1, &QPushButton::clicked, &model, &MainModel::setBrushSizeToOne);
    connect(ui->size2, &QPushButton::clicked, &model, &MainModel::setBrushSizeToTwo);
    connect(ui->size3, &QPushButton::clicked, &model, &MainModel::setBrushSizeToThree);

    // Connect signal/slot to handle when brush color changed
    connect(this, &MainWindow::colorChanged, &model, &MainModel::setColor);

    // Import/Export
    connect(ui->actionOpen, &QAction::triggered, this, &MainWindow::openFile);
    connect(ui->actionSave_As, &QAction::triggered, this, &MainWindow::fileSaveAs);
    connect(ui->actionSave, &QAction::triggered, &model, &MainModel::saveToFile);
    connect(ui->actionNew, &QAction::triggered, &model, &MainModel::newFile);

    // Save/open file
    connect(this, &MainWindow::sendOpenFile, &model, &MainModel::getOpenFile);
    connect(this, &MainWindow::sendSaveAsFile, &model, &MainModel::saveFileAs);
    connect(&model, &MainModel::getFileName, this, &MainWindow::fileSaveAs);

    // Undo/redo
    connect(ui->actionUndo, &QAction::triggered, &model, &MainModel::undo);
    connect(ui->actionRedo, &QAction::triggered, &model, &MainModel::redo);

    // Add frame
    connect(ui->addFrameButton, &QPushButton::clicked, this, [&model,this](){addFrame(model);});
    connect(&model, &MainModel::loadFileFrame, this, [&model,this](){addFrame(model);});

    // Delete frame
    connect (ui->deleteFrameButton, &QPushButton::clicked, this , &MainWindow::deleteFrame);
    connect (this, &MainWindow::removeFrame, &model , &MainModel::removeFrame);
    connect (&model, &MainModel::buttonToRemove, this, &MainWindow::removeButton);

    // Update frame preview
    connect(this, &MainWindow::sendImageButton, &model, &MainModel::displaySelectedFrame);
    connect(this, &MainWindow::updateFrameCount, &model, &MainModel::sendFrameCount);
    connect(this, &MainWindow::newFrame, &model, &MainModel::addNewFrame);
    connect(&model, &MainModel::sendUpdateFrameIcons, this, &MainWindow::updateFrameIcons);

    // Frame preview
    connect(ui->fpsSlider, &QSlider::valueChanged, this, &MainWindow::displayFps);
    connect(ui->fpsSlider, &QSlider::valueChanged, &model, &MainModel::setFps);

    // Reset all components when "New" is clicked
    connect(&model, &MainModel::resetComponents, this, &MainWindow::resetComponents);

    // Create a custom cursor from the pixmap of a brush
    QPixmap brushShape(":/icons/brush.png");
    brushShape = brushShape.scaled(QSize(70, 70));
    QCursor customCursor(brushShape);
    setCursor(customCursor);

    // Frame list
    auto anotherFrame = new QPushButton("");
    anotherFrame->setFixedSize(175, 175);
    buttonFrame.append(anotherFrame);
    ui->framePlacement->addWidget(anotherFrame);
    connect(anotherFrame, &QPushButton::clicked, this, &MainWindow::getButton);

    // Set the minimum and maximum size of the sprite to 2x2 and 32x32 respectively
    ui->fpsSlider->setMinimum(2);
    ui->fpsSlider->setMaximum(32);

    // Set the text of the FPS of animation preview
    QString text = "FPS: " + QString::number(2);
    ui->fpsLabel->setText(text);

    // Set the default brush size to size 1
    ui->size1->setEnabled(false);
    ui->size2->setEnabled(true);
    ui->size3->setEnabled(true);
    ui->brush->setStyleSheet("background-color:rgb(100,100,100)");

    // Set the button color
    ui->color->setStyleSheet("QPushButton {background-color: rgb(0,0,0); color: white;}");

    // Set the flag indicate if the animation preview is scaled or in actual size
    actualSize = false;

    // Set up the buttons in the preview box
    ui->sizeButton->setText("Actual Size");
    ui->playButton->setEnabled(true);
    ui->pauseButton->setEnabled(false);

    // Toggle button for animation preview size
    connect(ui->sizeButton, &QPushButton::clicked, this, &MainWindow::togglePreviewSize);
}

// Delegate the drawing work to MainModel
void MainWindow::beginDraw(QPoint mouseLocation)
{
    emit modelStartDraw(mouseLocation);
}
void MainWindow::continueDraw(QPoint mouseLocation)
{
    emit modelContinueDraw(mouseLocation);
}
void MainWindow::endDraw(QPoint mouseLocation)
{
    emit modelEndDraw(mouseLocation);
}

// Update the canvas to current image state
void MainWindow::updateImage(QImage* image)
{
    QImage scaled = image->scaled(ui->panel->width(), ui->panel->height());
    ui->panel->setPixmap(QPixmap::fromImage(scaled));
}

// Update the preview to current image state
void MainWindow::updatePreview(QImage image)
{
    // Scale the image if the user chose so
    if (!actualSize)
        image = image.scaled(ui->previewLabel->width(), ui->previewLabel->height());

    ui->previewLabel->setPixmap(QPixmap::fromImage(image));
    ui->previewLabel->setAlignment(Qt::AlignHCenter | Qt::AlignVCenter);
}

void MainWindow::onColorButtonClicked()
{
    QColor color = QColorDialog::getColor(Qt::white, this, "Choose Color");
    QPixmap brushShape(":/icons/brush.png");
    brushShape = brushShape.scaled(QSize(70, 70));

    // Create a cursor from the pixmap
    QCursor customCursor(brushShape);
    setCursor(customCursor);

    // Highlight the brush is currently being used
    ui->brush->setEnabled(true);
    ui->brush->setStyleSheet("background-color:rgb(100,100,100)");
    ui->eraser->setStyleSheet("");

    // Set color of the button
    if (color.red() == 0 && color.blue() == 0 && color.green() == 0)
        ui->color->setStyleSheet("QPushButton {background-color: rgb(0,0,0); color: white;}");
    else
    {
        QString colorString = QString("background-color: %1").arg(color.name());
        ui->color->setStyleSheet(colorString);
    }

    emit colorChanged(color);
}

void MainWindow::onBrushButtonClicked()
{
    // Set the cursor to brush icon
    QPixmap brushShape(":/icons/brush.png");
    brushShape = brushShape.scaled(QSize(70, 70));

    // Create a cursor from the pixmap
    QCursor customCursor(brushShape);
    setCursor(customCursor);

    // Highlight the brush button
    ui->brush->setStyleSheet("background-color:rgb(100,100,100)");
    ui->eraser->setStyleSheet("");
}

void MainWindow::onEraserButtonClicked()
{
    // Set the cursor to eraser icon
    QPixmap eraserShape(":/icons/eraser.png");
    eraserShape = eraserShape.scaled(QSize(50, 50));

    // Create a cursor from the pixmap
    QCursor customCursor(eraserShape);
    setCursor(customCursor);

    // Highlight the eraser button
    ui->eraser->setStyleSheet("background-color:rgb(100,100,100)");
    ui->brush->setStyleSheet("");
}

void MainWindow::sizeOneButtonClicked()
{
    // Highlight the size 1 button
    ui->size1->setEnabled(false);
    ui->size2->setEnabled(true);
    ui->size3->setEnabled(true);
}

void MainWindow::sizeTwoButtonClicked()
{
    // Highlight the size 2 button
    ui->size1->setEnabled(true);
    ui->size2->setEnabled(false);
    ui->size3->setEnabled(true);
}

void MainWindow::sizeThreeButtonClicked()
{
    // Highlight the size 3 button
    ui->size1->setEnabled(true);
    ui->size2->setEnabled(true);
    ui->size3->setEnabled(false);
}

void MainWindow::openFile()
{
    // Get file name
    QString fileName = QFileDialog::getOpenFileName(this, "Open a .ssp file","C://", tr("Frames (*.ssp)"));
    if (fileName != "")
        // Delegate the work to MainModel
        emit sendOpenFile(fileName);
}

void MainWindow::fileSaveAs()
{
    // Get file name
    QString fileName = QFileDialog::getSaveFileName(this, "Save as .ssp file","C://", tr("Frames (*.ssp)"));
    if (fileName != "")
        // Delegate the work to MainModel
        emit sendSaveAsFile(fileName);
}

void MainWindow::getButton()
{
    QPushButton* buttonSender = qobject_cast<QPushButton*>(sender()); // retrieve the button you have clicked
    QString buttonText = buttonSender->text();
    int frameNumber = buttonText.toInt();
    emit sendImageButton(frameNumber);
}

void MainWindow::addFrame(MainModel& model)
{
    // Add another frame button to the frame preview (right side of UI)
    int totalFrames = model.getFrameManager().frameCount + 1;
    QString frameNumber = QString::number(model.getFrameManager().frameCount);
    auto anotherFrame = new QPushButton(frameNumber);
    anotherFrame->setFixedSize(175, 175);
    buttonFrame.insert(frameNumber.toInt(), anotherFrame);
    ui->framePlacement->addWidget(anotherFrame);

    // Connect the appropriate slots to click event
    connect(anotherFrame, &QPushButton::clicked, this, &MainWindow::getButton);

    // Signal the model about the change
    emit updateFrameCount(totalFrames);
    emit newFrame();
}

void MainWindow::updateFrameIcons(QVector<QImage> currentFrames)
{
    // Update each frame button (right side of UI) to appropriate canvas linked to it
    // This is done after add frame is called
    for (int i = 0; i < currentFrames.size(); i++)
    {
        QImage scaled = currentFrames[i].scaled(175, 175);
        QIcon buttonIcon(QPixmap::fromImage(scaled));
        buttonFrame[i]->setIcon(buttonIcon);
        buttonFrame[i]->setIconSize(QSize(175, 175));
    }
}

void MainWindow::deleteFrame()
{
    // Not delete frame when there's only one frame
    if (model.getFrameManager().frameCount == 1)
        return;

    // Decrease the total frame, and signal MainModel about that
    int totalFrames = model.getFrameManager().frameCount - 1;
    emit updateFrameCount(totalFrames);
    emit removeFrame();
}

void MainWindow::removeButton(int buttonIndex)
{
    // Remove the current frame button when deleted
    auto buttonToRemove = buttonFrame[buttonIndex];
    buttonFrame.remove(buttonIndex);
    delete buttonToRemove;
    for (int index = buttonIndex; index < buttonFrame.count(); index++)
        buttonFrame[index]->setText(QString::number((buttonFrame[index]->text().toInt() - 1)));
}

void MainWindow::displayFps(int value)
{
    // Display the current FPS to view
    QString text = "FPS: " + QString::number(value);
    ui->fpsLabel->setText(text);
}

void MainWindow::changeButtonIcon(QImage image, int buttonIndex)
{
    // Change the button icon accordingly to the current state of canvas
    QImage scaled = image.scaled(175, 175);
    QIcon buttonIcon(QPixmap::fromImage(scaled));
    buttonFrame[buttonIndex]->setIcon(buttonIcon);
    buttonFrame[buttonIndex]->setIconSize(QSize(175, 175));
}

void MainWindow::togglePreviewSize()
{
    // Toggle flag
    actualSize = !actualSize;

    // Change text of the button after user click
    if (actualSize)
        ui->sizeButton->setText("Scaled Size");
    else
        ui->sizeButton->setText("Actual Size");
}

void MainWindow::resetComponents()
{
    actualSize = false;

    // Reset brush, eraser, color, and brush size buttons
    onBrushButtonClicked();
    sizeOneButtonClicked();
    ui->color->setStyleSheet("QPushButton {background-color: rgb(0,0,0); color: white;}");

    // Reset the buttons in the preview box
    ui->sizeButton->setText("Actual Size");
    ui->playButton->setEnabled(true);
    ui->pauseButton->setEnabled(false);

    // Reset the image displayed
    QImage* image = new QImage(ui->previewLabel->width(), ui->previewLabel->height(), QImage::Format_RGBA64);
    image->fill(*(new QColor(0, 0, 0, 0)));
    updatePreview(*image);
}

MainWindow::~MainWindow()
{
    delete ui;
}
