#ifndef DIALOG_H
#define DIALOG_H

#include <QDialog>
#include<QMessageBox>

/**
 * @brief The Dialog class serves as the starting point of the app,
 * load/create new pet, requires appropriate info of the pet from user
 */

namespace Ui {
class Dialog;
}

class Dialog : public QDialog
{
    Q_OBJECT

public:
    /**
     * @brief Dialog constructor
     * @param parent the QWidget parent
     */
    explicit Dialog(QWidget *parent = nullptr);
    ~Dialog();

public slots:
    void rejectClicked();  // Reject when there's false information
    void loadPet();        // Load the pet and the main window

private:
    Ui::Dialog *ui;                    // The dialog ui
    QMessageBox validInputMessageBox;  // the message box
    void accept() override;            // Accept the input

};

#endif // DIALOG_H
