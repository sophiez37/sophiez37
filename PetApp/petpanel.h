#ifndef PETPANEL_H
#define PETPANEL_H

#include <QGraphicsView>
#include <QMouseEvent>
#include <QPaintEvent>

/**
 * @brief The PetPanel class, main panel displaying the app
 */
class PetPanel : public QGraphicsView
{
    Q_OBJECT

public:
    explicit PetPanel(QWidget *parent = nullptr);  // Constructor
    void resizeEvent(QResizeEvent* e);             // Resize event
    void mouseMoveEvent(QMouseEvent* e);           // Mouse move event
    void mousePressEvent(QMouseEvent* e);          // Mouse press event
    void mouseReleaseEvent(QMouseEvent* e);        // Mouse release event

signals:
    void mousePressed(QPoint);   // Send mouse press event to MainWindow
    void mouseMoved(QPoint);     // Send mouse move event to MainWindow
    void mouseReleased(QPoint);  // Send mouse release event to MainWindow
};
#endif // PETPANEL_H
