#include "petpanel.h"

PetPanel::PetPanel(QWidget *parent)
    : QGraphicsView(parent)
{
    this->setHidden(false);
}

void PetPanel::resizeEvent(QResizeEvent* e)
{
    // Set resize event
    QRectF rect = scene()->itemsBoundingRect();
    setSceneRect(rect);
    fitInView(sceneRect(),Qt::KeepAspectRatio);
    QGraphicsView::resizeEvent(e);
}

void PetPanel::mouseMoveEvent(QMouseEvent* e)
{
    emit mousePressed(e->pos());
}

void PetPanel::mousePressEvent(QMouseEvent* e)
{
    emit mouseMoved(e->pos());
}

void PetPanel::mouseReleaseEvent(QMouseEvent* e)
{
    emit mouseReleased(e->pos());
}
