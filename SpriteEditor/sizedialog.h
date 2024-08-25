#ifndef SIZEDIALOG_H
#define SIZEDIALOG_H

#include <QDialog>

namespace Ui {
class SizeDialog;
}

/**
 * @brief The SizeDialog class is a part of the UI. SizeDialogue appears
 * at the beginning of execution. It serves as a landing point for user
 * events related to setting the size of the canvas.
 */
class SizeDialog : public QDialog
{
    Q_OBJECT

public:
    explicit SizeDialog(QWidget *parent = nullptr);
    ~SizeDialog();

private slots:
    void sliderValueChanged(int value);  // Record the value when slider changed
    void acceptButtonClicked();          // Handle when accept button is clicked
    void rejectButtonClicked();          // Handle when reject button is clicked

private:
    Ui::SizeDialog *ui;
    int size = 2;
};

#endif // SIZEDIALOG_H
