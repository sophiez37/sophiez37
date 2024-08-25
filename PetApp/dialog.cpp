#include "dialog.h"
#include "ui_dialog.h"
#include <string>
#include "mainmodel.h"
#include "mainwindow.h"
#include <QJsonDocument>
#include <QFileDialog>
#include <QJsonObject>
#include <QJsonArray>

using std::string;

Dialog::Dialog(QWidget *parent)
    : QDialog(parent)
    , ui(new Ui::Dialog)
{
    ui->setupUi(this);
    connect(ui->buttonBox, &QDialogButtonBox::rejected, this, &Dialog::rejectClicked);
    connect(ui->loadButton, &QPushButton::clicked, this, &Dialog::loadPet);
}

Dialog::~Dialog()
{
    delete ui;
}

void Dialog::rejectClicked()
{
    QApplication::quit();
}

void Dialog::accept()
{
    string name = ui->nameInput->text().toStdString();
    int age = ui->ageInput->text().toInt();
    double weight = ui->weightInput->text().toDouble();
    string animal = ui->petType->currentText().toStdString();

    // Check for the input validity
    if (name == "")
    {
        validInputMessageBox.setHidden(false);
        validInputMessageBox.setText("Please give your pet a name.");
    }
    else if (!(age > 0 && age < 30))
    {
        validInputMessageBox.setHidden(false);
        validInputMessageBox.setText("Please enter a valid age.");
        return;
    }
    else if (!(weight > 0 && weight < 200))
    {
        validInputMessageBox.setHidden(false);
        validInputMessageBox.setText("Please enter a valid weight.");
        return;
    }
    else
    {
        QString fileName = QFileDialog::getSaveFileName(this, "Save as .ssp file","C://", tr("Frames (*.ssp)"));
        if (fileName != "")
        {
            MainModel* model = new MainModel(name, age, weight, animal);
            model->setCurrentFilePath(fileName);
            MainWindow* window = new MainWindow(*model, name, age, weight, animal);
            window->show();
            QDialog::accept();
        }
    }
}

void Dialog::loadPet()
{
    // Get file name
    QString fileName = QFileDialog::getOpenFileName(this, "Open a .ssp file","C://", tr("Pet (*.ssp)"));
    if (fileName != "")
    {
        QString holdFilePath = fileName;
        QString strJson;
        QFile currentFile(holdFilePath);
        if (currentFile.open(QIODevice::ReadOnly))
            strJson = currentFile.readAll();

        currentFile.close();
        QJsonDocument jsonDoc = QJsonDocument::fromJson(strJson.toUtf8());
        QJsonArray jsonArray = jsonDoc.array();
        QJsonObject mainObject = jsonArray.first().toObject();

        // Extract information about frame list, animation, size and fps
        QJsonObject petObject = mainObject.value(QString("Pet")).toObject();
        QJsonArray progresses = petObject.value("progresses").toArray();
        QJsonValue petName = petObject.value("name");
        QJsonValue petWeight = petObject.value("weight");
        QJsonValue petAge = petObject.value("age");
        QJsonValue petType = petObject.value("animal");
        QJsonValue gameTime = petObject.value("gametime");

        // Parse info to string
        string name = petName.toString().toStdString();
        string animal = petType.toString().toStdString();
        int age = petAge.toInt();
        double weight = petWeight.toDouble();
        int gameTimeInt = gameTime.toInt();

        // Load the model and window
        MainModel* model = new MainModel(name, age, weight, animal);
        MainWindow* window = new MainWindow(*model, name, age, weight, animal);

        // Set the file path and progress bar and current game time to appropriate values
        model->setCurrentFilePath(fileName);
        model->loadFileProgressbars(progresses);
        model->setGameTime(gameTimeInt);

        window->show();
        this->close();
    }
}
