#ifndef CANVAS_H
#define CANVAS_H

#include <QLabel>
#include <QMouseEvent>
#include <QPaintEvent>

/**
 * @brief The Canvas class is a part of the view
 * the Sprite Editor. It listens for mouse events that
 * occur within the canvas and emits signals to the model
 * accordingly.
 */
class Canvas : public QLabel
{
    Q_OBJECT
public:
    /**
     * @brief The Canvas constructor.
     * @param parent a nullptr
     */
    explicit Canvas(QWidget *parent = nullptr);

    /**
     * @brief Method that is called on mouse press events.
     * @param a QMouseEvent
     */
    void mousePressEvent(QMouseEvent* e);

    /**
     * @brief Method that is called on mouse press events.
     * @param a QMouseEvent
     */
    void mouseMoveEvent(QMouseEvent* e);

    /**
     * @brief Method that is called on mouse release events.
     * @param a QMouseEvent
     */
    void mouseReleaseEvent(QMouseEvent* e);

signals:
    /**
     * @brief The mousePressed signal is fired on a mouse press.
     */
    void mousePressed(QPoint);

    /**
     * @brief The mouseMoved signal is fired on a mouse move.
     */
    void mouseMoved(QPoint);

    /**
     * @brief The mouseReleased signal is fired on a mouse release.
     */
    void mouseReleased(QPoint);

private:
};

#endif // CANVAS_H
