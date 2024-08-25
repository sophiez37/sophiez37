#ifndef MAINWINDOW_H

#define MAINWINDOW_H
#include "mainmodel.h"
#include <QMainWindow>
#include <QColorDialog>
#include <QColor>
QT_BEGIN_NAMESPACE
namespace Ui
{
    class MainWindow;
}
QT_END_NAMESPACE

/**
 * @brief The MainWindow class is the main interface for the SpriteEditor. It serves as a
 * landing point for user events. It also updates the Canvas accordingly based on signals
 * received from MainModel.
 */
class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    /**
     * @brief the constructor for MainWindow
     * @param model the associated model
     * @param parent the parent OWidget
     */
    MainWindow(MainModel &model, QWidget *parent = nullptr);

    /**
     * @brief update the canvas to the current state
     */
    void updateCanvas();

    /**
     * The destructor
     */
    ~MainWindow();

public slots:
    // Drawing events handlers
    void beginDraw(QPoint);              // Inform the MainModel to begin drawing on a mouse press.
    void continueDraw(QPoint);           // Inform the MainModel to continue drawing on a mouse move
    void endDraw(QPoint);                // Inform the MainModel to save the current frame on a mouse release.

    // Update the canvas and preview sections adther a stroke
    void updateImage(QImage*);           // Given a QImage, update the image displayed on the Canvas.
    void updatePreview(QImage);          // Given a QImage, update the image displayed on the image preview area.
    void changeButtonIcon(QImage, int);  // Set the frame button to change accordingly to the change on canvas

    // Drawing Tools (brush/eraser, color of brush)
    void onColorButtonClicked();         // Open the QColorDialogue and alert MainModel of color changes.
    void onBrushButtonClicked();         // Set the style of the brush button accordingly to indicate that it has been pressed.
    void onEraserButtonClicked();        // Set the style of the eraser button accordingly to indicate that it has been pressed.

    // Handle the brush sizes
    void sizeOneButtonClicked();         // Set the style of the size button to indiciate that it has been pressed.
    void sizeTwoButtonClicked();         // Set the style of the size button to indiciate that it has been pressed.
    void sizeThreeButtonClicked();       // Set the style of the size button to indiciate that it has been pressed.

    // Get and set the frane list buttons
    void getButton();                    // Get the frame button
    void removeButton(int buttonIndex);  // Remove the frame button when frame got deleted

    // Add and remove frames from the list
    void addFrame(MainModel &model);     // Add a new frame to frame list
    void deleteFrame();                  // Delete the current selected frame
    void updateFrameIcons(QVector<QImage> currentFrames);  // Update the frame buttons when add a new frame

    // Import/export
    void openFile();                     // Open the .ssp file
    void fileSaveAs();                   // Save the new file that has not been on disk

    // FPS and preview
    void displayFps(int);                // Display the current FPS to view
    void togglePreviewSize();            // Update the flag for preview size and button text

    // Reset all UI components
    void resetComponents();              // Reset all UI components when user clicks "New"

signals:
    // Mouse location for drawing event
    void modelStartDraw(QPoint);         // Signal that alerts the MainModel that the user has began drawing at a given point.
    void modelContinueDraw(QPoint);      // Signal that alerts the MainModel that the user has continued drawing at a given point.
    void modelEndDraw(QPoint);           // Signal that alerts the MainModel that the user has stopped drawing at a given opint.

    // Color of the pen
    void colorChanged(QColor);           // Signal that alerts the MainModel of the new color the user has selected.

    // Import/export
    void sendOpenFile(QString);          // Signal to alert the MainModel about the file that has been opened
    void sendSaveAsFile(QString);        // Signal the MainModel about the file path to save as

    // Update frame list buttons
    void sendImageButton(int);           // Send the current image button to MainModel
    void updateFrameCount(int);          // Send the updated frame count to MainModel

    // Add/Remove frame signals
    void newFrame();                     // Signal that there has been new frame added
    void removeFrame();                  // Signal that there is frame that's removed

private:
    Ui::MainWindow *ui;                  // MainWindow object
    QVector<QPushButton*> buttonFrame;   // the list of frames in preview section (right side of the UI)
    MainModel &model;                    // The Model of the SpriteEditor
    bool actualSize;                     // Flag for animation preview is in actual size or not
};
#endif // MAINWINDOW_H
