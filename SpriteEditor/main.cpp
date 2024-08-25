#include "sizedialog.h"
#include <QApplication>

/**
 * University of Utah Course: CS 3505-001 Spring 2024 - Software Practice II
 * Semester: Spring 2024
 * Assignment 8: Sprite Editor Implementation
 *
 * @Author: Duy Khanh Tran, Phuong Anh Nguyen, Freda Shi, Hung Phan Quoc Viet, Dean Pham, Quang P Le
 * Created date: 03/03/2024
 *
 * The Sprite Editor, with base features and 2 extra features:
 *
 * Different brush size options:
 *      - User can choose different brush size to draw with
 *      - Brush size is also applicable for eraser size, in other words,
 *      if user chose larger brush size and using eraser, then the erased area is equally large
 *
 * Undo/Redo features:
 *      - User can undo the previous stroke and redo the stroke
 *      - Similar to any undo/redo implementations, if the user undo a stroke, but
 *      then added another stroke, then user can't do redo that stroke again
 *
 * Shortcuts note:
 *      Save: Ctrl + S
 *      Save As: F12
 *      New: Ctrl + N
 *      Open: Ctrl + O
 *      Undo: Ctrl + Z
 *      Redo: Ctrl + Y
 */

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    SizeDialog sizeDialog;
    sizeDialog.show();
    return a.exec();
}
