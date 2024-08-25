#include "canvas.h"
#include<QImage>

Canvas::Canvas(QWidget *parent)
    : QLabel(parent)
{
    this->setHidden(false);
}

void Canvas::mousePressEvent(QMouseEvent* e)
{
    emit mousePressed(e->pos());
    mouseMoveEvent(e);
}

void Canvas::mouseReleaseEvent(QMouseEvent* e)
{
    emit mouseReleased(e->pos());
}

void Canvas::mouseMoveEvent(QMouseEvent* e)
{
    emit mouseMoved(e->pos());
}
