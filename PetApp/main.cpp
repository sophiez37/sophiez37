
#include <QApplication>
#include "dialog.h"

/**
 * University of Utah Course: CS 3505-001 Spring 2024 - Software Practice II
 * Semester: Spring 2024
 * Assignment 9: Educational App
 *
 * The app teaches people how to take care of pets, specifically dog and cat
 *
 * @Author: Duy Khanh Tran, Phuong Anh Nguyen, Freda Shi, Hung Phan Quoc Viet, Dean Pham, Quang P Le
 * Created date: 04/05/2024
 */

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    Dialog d;
    d.show();
    QFont font("Comic Sans MS", 12);
    a.setFont(font);
    return a.exec();
}
